using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float gravity = -9.81f;

    [Header("Mouse Settings")]
    public float mouseSensitivity = 2f;
    public Transform cameraTransform;

    private CharacterController characterController;
    private float verticalSpeed = 0f; // For gravity
    private float cameraVerticalAngle = 0f; // For smoother vertical camera control

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        MovePlayer();
        RotateCamera();
    }

    void MovePlayer()
    {
        // Get input
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Calculate movement direction
        Vector3 moveDirection = transform.right * horizontal + transform.forward * vertical;

        // Apply gravity
        if (characterController.isGrounded)
        {
            verticalSpeed = -1f; // Small negative value to keep grounded
        }
        else
        {
            verticalSpeed += gravity * Time.deltaTime;
        }
        moveDirection.y = verticalSpeed;

        // Move character
        characterController.Move(moveDirection * moveSpeed * Time.deltaTime);
    }

    void RotateCamera()
    {
        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Rotate player around the Y-axis
        transform.Rotate(Vector3.up * mouseX);

        // Adjust vertical camera rotation
        cameraVerticalAngle -= mouseY;
        cameraVerticalAngle = Mathf.Clamp(cameraVerticalAngle, -60f, 60f); // Limit vertical angle

        // Apply camera rotation
        cameraTransform.localRotation = Quaternion.Euler(cameraVerticalAngle, 0f, 0f);
    }
}
