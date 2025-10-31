using FishNet.Object;
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
    [SerializeField] float groundDist;
    [SerializeField] Vector2 groundedBox;
    [SerializeField] float leftDist;
    [SerializeField] Vector2 leftBox;
    [SerializeField] float rightDist;
    [SerializeField] Vector2 rightBox;
    [SerializeField] float upDist;
    [SerializeField] Vector2 upBox;

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
    CollisionSide flags;
    private bool isOwner;

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
    }

    private void OnDestroy()
    {
        if (isOwner)
        {
            upACT.performed -= Jump;
            rollACT.performed -= RollAction;
        }
    }

    bool walljumping;

    void FixedUpdate()
    {
        isGrounded = CheckGrounded(8); //Layer 8 = 256 = Collidable objects (walls, floors, etc.)

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
            if (movement.x < 0) spriteRenderer.flipX = true;
            if (movement.x > 0) spriteRenderer.flipX = false;
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

        if (CheckAbove(8) && !isGrounded && curVel.y > 0)
        {
            curVel.y = 0f;
        }

        if ((CheckSides(CollisionSide.Left, 8) || CheckSides(CollisionSide.Right, 8)) && !isGrounded && !walljumping)
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
        else if (CheckSides(CollisionSide.Left, 8))
        {
            walljumping = true;
            curJumpCt = 0;
            airJumpPitch = origPitch;
            audSrc.pitch = airJumpPitch;

            curVel.y = wallJumpForce;
            curVel.x = wallJumpForce;

            spriteRenderer.flipX = false;
        }
        else if (CheckSides(CollisionSide.Right, 8))
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

    public bool CheckSides(CollisionSide side, int layerMask)
    {
        int mask = (int)Mathf.Pow(2, layerMask);

        Vector2 left = new Vector2(collBox.bounds.min.x + 0.2f, collBox.bounds.min.y + 0.2f);
        Vector2 right = new Vector2(collBox.bounds.max.x - 0.2f, collBox.bounds.min.y + 0.2f);

        float dist = 0.35f;
        RaycastHit2D raycastHit2D = default(RaycastHit2D);

        if (side == CollisionSide.Left)
        {
            Debug.DrawLine(left, left + Vector2.left * dist, Color.cyan, 0.15f);
            raycastHit2D = Physics2D.Raycast(left, Vector2.left, dist, mask);
        }
        else if (side == CollisionSide.Right)
        {
            Debug.DrawLine(right, right + Vector2.right * dist, Color.cyan, 0.15f);
            raycastHit2D = Physics2D.Raycast(right, Vector2.right, dist, mask);
        }

        return raycastHit2D.collider;
    }

    public bool CheckGrounded(int layer)
    {
        int mask = (int)MathF.Pow(2, layer);

        Vector2 left = new Vector2(collBox.bounds.min.x, collBox.bounds.center.y);
        Vector2 center = collBox.bounds.center;
        Vector2 right = new Vector2(collBox.bounds.max.x, collBox.bounds.center.y);

        float dist = collBox.bounds.extents.y + 0.16f;

        Debug.DrawRay(left, Vector2.down, Color.white);
        Debug.DrawRay(center, Vector2.down, Color.white);
        Debug.DrawRay(right, Vector2.down, Color.white);

        return Physics2D.Raycast(left, Vector2.down, dist, mask).collider || 
            Physics2D.Raycast(center, Vector2.down, dist, mask).collider || Physics2D.Raycast(right, Vector2.down, dist, mask).collider;
    }

    public bool CheckAbove(int layer)
    {
        int mask = (int)MathF.Pow(2, layer);
        Vector2 vec = new Vector2(collBox.bounds.min.x, collBox.bounds.center.y);
        Vector2 vec2 = collBox.bounds.center;
        Vector2 vec3 = new Vector2(collBox.bounds.max.x, collBox.bounds.center.y);

        float dist = collBox.bounds.extents.y + 0.16f;

        Debug.DrawRay(vec, Vector2.up, Color.blue);
        Debug.DrawRay(vec2, Vector2.up, Color.blue);
        Debug.DrawRay(vec3, Vector2.up, Color.blue);

        return Physics2D.Raycast(vec, Vector2.up, dist, mask).collider || 
            Physics2D.Raycast(vec2, Vector2.up, dist, mask).collider || Physics2D.Raycast(vec3, Vector2.up, dist, mask).collider;
    }
}
