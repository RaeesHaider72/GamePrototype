using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Input Type")]
    [SerializeField] private PlayerInput playerInput;

    [SerializeField] private Danda danda;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float rotationSpeed = 10f;

    [SerializeField ]private float lookSensitivity = 5f;


    private Vector2 rotation;


    [SerializeField]
    private CharacterController controller;

    public CinemachineOrbitalFollow freeLookCamera;

    public GameObject walkEffect;

    private Vector3 velocity;

    private void Awake()
    {
   

    }



    private void Update()
    {
        HandleMovement();
        HandleJump();
        ApplyGravity();
        HandleCameraRotation();

        HandleHit();
    }

   private void HandleMovement()
{
    Vector2 input = playerInput.Movement;

    Vector3 forward = freeLookCamera.transform.forward;
    Vector3 right = freeLookCamera.transform.right;

    forward.y = 0f;
    right.y = 0f;

    forward.Normalize();
    right.Normalize();

    Vector3 moveDirection =
        forward * input.y +
        right * input.x;

    if (moveDirection.magnitude > 0.1f)
    {

        controller.Move(
            moveDirection * moveSpeed * Time.deltaTime);


        walkEffect.SetActive(true);

        Quaternion targetRotation =
            Quaternion.LookRotation(moveDirection);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            10f * Time.deltaTime);
    }
    else
    {
        walkEffect.SetActive(false);
    }
}

    private void HandleJump()
    {
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        if (controller.isGrounded && playerInput.Jump)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    private void ApplyGravity()
    {
        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }


    private void HandleCameraRotation()
    {
        Vector2 look = playerInput.LookDirection;

        rotation.x += look.x * lookSensitivity * Time.deltaTime;
        rotation.y -= look.y * lookSensitivity * Time.deltaTime;

        rotation.y = Mathf.Clamp(rotation.y, -40f, 70f);

        freeLookCamera.HorizontalAxis.Value = rotation.x;
        freeLookCamera.VerticalAxis.Value = rotation.y;
    }


    private void HandleHit()
    {
        if (playerInput.Hit)
        {

            if (danda != null)
            {
                danda.Hit();
            }
        }
    }
}