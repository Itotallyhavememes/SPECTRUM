using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public enum CollisionSide
{
    None = 0,
    Left = 1,
    Right = 2,
    Sides = 3,
    Above = 4,
    Below = 8,
}

public class PlayerController : NetworkBehaviour
{
    //Components
    // private CharacterController charCtrl;
    private Rigidbody2D charRB;
    [SerializeField] private BoxCollider2D collBox;
    SpriteRenderer spriteRenderer;
    Animator animator;
    AudioSource audSrc;

    //Player Properties
    [Header("--- Properties ---")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float rollSpeed;
    [SerializeField] private float rollCooldown;
    [SerializeField] private float gravity;
    [SerializeField] private float terminalVelocity;
    [SerializeField] private float jumpStrength;
    [SerializeField] private float wallJumpForce;
    [SerializeField] private int airJumpCount;
    [SerializeField] private float friction;

    [Header("--- Collision ---")]
    [SerializeField] float sideOffset;
    [SerializeField] float sideDist;
    [SerializeField] float verticalDist;

    [Header("--- Sounds ---")]
    [SerializeField] AudioClip jumpSFX;
    [SerializeField] AudioClip wallJumpSFX;
    [SerializeField] AudioClip walkSFX;
    [SerializeField] AudioClip hurtSFX;
    [SerializeField] float airJumpPitchMultiplier;
    private float airJumpPitch;
    private float origPitch;

    //Utility Values
    bool isGrounded;
    private int curJumpCt;
    private Vector2 curVel;
    private bool runSoundPlaying;
    AudioSource audRunSrc;
    AudioSource audJumpSrc;

    InputAction moveACT, upACT, rollACT;
    private bool canRoll;
    private bool isRolling;
    private bool isOwner;
    bool walljumping;
    private readonly SyncVar<bool> spriteFlipped = new SyncVar<bool>(new SyncTypeSettings(WritePermission.ClientUnsynchronized, ReadPermission.ExcludeOwner));

    public override void OnStartClient()
    {
        base.OnStartClient();
        isOwner = base.IsOwner;

        if (base.IsOwner)
        {
            var sc = Camera.main.GetComponent<SmoothCam>();

            if (sc == null)
            {
                Debug.LogWarning("Camera is not compatible with character. Please add SmoothCam component to Camera.main");
            }
            else
                sc.target = gameObject;
        }
        else
        {
            gameObject.GetComponent<Rigidbody2D>().gravityScale = 2f;
            gameObject.GetComponent<PlayerController>().enabled = false;
        }

        Start();

    }

    void Start()
    {

        charRB = GetComponent<Rigidbody2D>();
        collBox = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audSrc = GetComponent<AudioSource>();
        origPitch = airJumpPitch = audSrc.pitch;

        moveACT = InputSystem.actions.FindAction("Move");
        upACT = InputSystem.actions.FindAction("Up");
        rollACT = InputSystem.actions.FindAction("Roll");

        if (isOwner)
        {
            upACT.performed += Jump;
            rollACT.performed += RollAction;
        }

        audJumpSrc = this.AddComponent<AudioSource>();
        audRunSrc = this.AddComponent<AudioSource>();
        audRunSrc.outputAudioMixerGroup = audJumpSrc.outputAudioMixerGroup = audSrc.outputAudioMixerGroup;

        canRoll = true;

        spriteFlipped.OnChange += OnSpriteFlipChanged;
    }

    [ServerRpc(RunLocally = true)]
    private void OnSpriteFlipChanged(bool prev, bool next, bool asServer) => spriteRenderer.flipX = next;
    private void SetSpriteFlipped(bool value) => spriteFlipped.Value = value;

    void FixedUpdate()
    {
        isGrounded = CheckVerticalCollision(8, false); //Layer 8 = 256 = Collidable objects (walls, floors, etc.)

        if (!isGrounded)
        {
            curVel.y -= gravity * Time.deltaTime;
            curVel.y = Mathf.Max(curVel.y, terminalVelocity);
        }

        if (isGrounded && curVel.y < 0)
        {
            curVel.y = -2f;
            curJumpCt = 0;
            airJumpPitch = origPitch;
            audSrc.pitch = airJumpPitch;
        }

        Vector2 moveActionValue = moveACT.ReadValue<Vector2>();
        Vector3 movement = new Vector3(moveActionValue.x, moveActionValue.y, 0f);

        movement *= moveSpeed;

        if (!isRolling)
        {
            if (movement.x < 0)
            {
                spriteRenderer.flipX = true;
                SetSpriteFlipped(true);
            }
            if (movement.x > 0)
            {
                spriteRenderer.flipX = false;
                SetSpriteFlipped(false);
            }
        }
        else
            curVel.x = movement.x = rollSpeed * (spriteRenderer.flipX ? -1 : 1);

        float yVal = curVel.y;

        curVel = Vector3.Lerp(curVel, movement, friction * Time.deltaTime);

        curVel.y = yVal;

        bool running = (isGrounded && movement.magnitude > 0.1f);

        animator.SetBool("IsRunning", running);
        animator.SetBool("IsJumping", !isGrounded);

        if (running && !runSoundPlaying && !isRolling)
            StartCoroutine(PlayRun());

        charRB.gravityScale = curVel.y;
        charRB.velocity = (curVel * Time.deltaTime) / 0.02f;

        if (CheckVerticalCollision(8, true) && !isGrounded && curVel.y > 0)
        {
            curVel.y = 0f;
        }

        if ((CheckSideCollision(CollisionSide.Left, 8) || CheckSideCollision(CollisionSide.Right, 8)) && !isGrounded && !walljumping)
        {
            curVel.x = 0f;
        }

        if (!walljumping) walljumping = true;
    }

    private void Jump(InputAction.CallbackContext context)
    {
        if (isGrounded)
        {
            audSrc.PlayOneShot(jumpSFX);
            curVel.y = jumpStrength;
        }
        else if (CheckSideCollision(CollisionSide.Left, 8))
        {
            walljumping = true;
            curJumpCt = 0;
            airJumpPitch = origPitch;
            audSrc.pitch = airJumpPitch;

            curVel.y = wallJumpForce;
            curVel.x = wallJumpForce;

            spriteRenderer.flipX = false;
        }
        else if (CheckSideCollision(CollisionSide.Right, 8))
        {
            walljumping = true;
            curJumpCt = 0;
            airJumpPitch = origPitch;
            audSrc.pitch = airJumpPitch;

            curVel.y = wallJumpForce;
            curVel.x = -wallJumpForce;

            spriteRenderer.flipX = true;
        }
        else if (!isGrounded && curJumpCt <= airJumpCount - 1)
        {
            curJumpCt++;
            airJumpPitch += airJumpPitchMultiplier;

            audJumpSrc.pitch = airJumpPitch;
            audJumpSrc.PlayOneShot(jumpSFX);

            curVel.y = jumpStrength;

        }
    }
    private void RollAction(InputAction.CallbackContext context)
    {
        if (canRoll)
        {
            canRoll = false;
            isRolling = true;

            animator.SetBool("Rolling", true);
        }
    }

    private IEnumerator RollFinished()
    {
        animator.SetBool("Rolling", false);

        isRolling = false;

        yield return new WaitForSeconds(rollCooldown);

        canRoll = true;
    }

    private IEnumerator PlayRun()
    {
        runSoundPlaying = true;

        audRunSrc.pitch = UnityEngine.Random.Range(0.75f, 1.25f);
        audRunSrc.PlayOneShot(walkSFX);

        yield return new WaitForSeconds(walkSFX.length);

        runSoundPlaying = false;
    }

    public bool CheckSideCollision(CollisionSide side, int layerMask)
    {
        int mask = (int)Mathf.Pow(2, layerMask);

        Vector2 left = new Vector2(collBox.bounds.min.x, collBox.bounds.min.y + sideOffset);
        Vector2 right = new Vector2(collBox.bounds.max.x, collBox.bounds.min.y + sideOffset);

        if (side == CollisionSide.Left)
        {
            Debug.DrawLine(left, left + Vector2.left * sideDist, Color.cyan);
            return Physics2D.Raycast(left, Vector2.left, sideDist, mask).collider;
        }
        else if (side == CollisionSide.Right)
        {
            Debug.DrawLine(right, right + Vector2.right * sideDist, Color.cyan);
            return Physics2D.Raycast(right, Vector2.right, sideDist, mask).collider;
        }

        return false;
    }

    public bool CheckVerticalCollision(int layer, bool up)
    {
        int mask = (int)MathF.Pow(2, layer);

        Vector2 left = new Vector2(collBox.bounds.min.x, (up) ? collBox.bounds.max.y : collBox.bounds.min.y);
        Vector2 center = new Vector2(collBox.bounds.center.x, (up) ? collBox.bounds.max.y : collBox.bounds.min.y);
        Vector2 right = new Vector2(collBox.bounds.max.x, (up) ? collBox.bounds.max.y : collBox.bounds.min.y);

        Vector2 direction = up ? Vector2.up : Vector2.down;

        Debug.DrawLine(left, left + direction * verticalDist, Color.white);
        Debug.DrawLine(center, center + direction * verticalDist, Color.white);
        Debug.DrawLine(right, right + direction * verticalDist, Color.white);

        return Physics2D.Raycast(left, direction, verticalDist, mask).collider ||
            Physics2D.Raycast(center, direction, verticalDist, mask).collider || Physics2D.Raycast(right, direction, verticalDist, mask).collider;
    }

    private void OnDestroy()
    {
        if (isOwner)
        {
            upACT.performed -= Jump;
            rollACT.performed -= RollAction;
        }
    }
}
