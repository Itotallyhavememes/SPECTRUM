using UnityEngine;
using UnityEngine.InputSystem;

public class SmoothCam : MonoBehaviour
{
    [SerializeField] public GameObject target;
    [SerializeField] float lerpSpeed;
    [SerializeField] Vector3 positionOffset;
    [SerializeField] float lookAmount;

    InputAction lookACT;

    void Start()
    {
        lookACT = InputSystem.actions.FindAction("Look");
    }

    void Update()
    {
        Vector2 look2D = lookACT.ReadValue<Vector2>();

        Vector3 targetPos = target.transform.position + positionOffset;
        targetPos.y += look2D.y * lookAmount;

        Vector3 move = Vector3.Lerp(transform.position, targetPos, lerpSpeed * Time.deltaTime);
        move.z = transform.position.z;
   
        transform.position = move;
    }
}
