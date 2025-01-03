using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float mouseSensitivity = 2f;
    public Transform cameraTransform; 

    private CharacterController characterController;
    private float verticalSpeed = 0f; // for gravity

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
        
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 moveDirection = transform.right * horizontal + transform.forward * vertical;

        
        if (characterController.isGrounded)
            verticalSpeed = -1f; 
        else
            verticalSpeed += Physics.gravity.y * Time.deltaTime;

        moveDirection.y = verticalSpeed;

        
        characterController.Move(moveDirection * moveSpeed * Time.deltaTime);
    }

    void RotateCamera()
    {
        
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        
        transform.Rotate(Vector3.up * mouseX);

        
        Vector3 cameraRotation = cameraTransform.localEulerAngles;
        cameraRotation.x -= mouseY;
        cameraRotation.x = Mathf.Clamp(cameraRotation.x, -80f, 80f); // camera sinirlari simdilik kalsin
        cameraTransform.localEulerAngles = cameraRotation;
    }
}
