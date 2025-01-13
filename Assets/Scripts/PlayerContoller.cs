using System.Collections;
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

    [Header("Camera Settings")]
    public Camera mainCamera; // Main player camera
    public Camera cardGameCamera; // Card game camera

    private CharacterController characterController;
    private float verticalSpeed = 0f; // For gravity
    private float cameraVerticalAngle = 0f; // For smoother vertical camera control
    public bool isPlayingCardGame = false;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;

        // Ensure only the main camera is active at start
        mainCamera.enabled = true;
        cardGameCamera.enabled = false;
        Time.timeScale = 1; // Keep game running at normal speed
    }

    void Update()
    {
        // Toggle card game mode with E key
        if (Input.GetKeyDown(KeyCode.E))
        {
            ToggleCardGameMode();
        }

        // Player movement and camera rotation only when not in card game mode
        if (!isPlayingCardGame && !ObjectManager.isSelectingObject)
        {
            MovePlayer();
            RotateCamera();
        }
    }

    void MovePlayer()
    {
        if (!characterController.enabled)
        {
            Debug.LogWarning("CharacterController is disabled, cannot move player.");
            return;
        }

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

    void ToggleCardGameMode()
    {
        isPlayingCardGame = !isPlayingCardGame;

        if (isPlayingCardGame)
        {
            mainCamera.enabled = false;
            cardGameCamera.enabled = true;

            if (characterController != null)
            {
                characterController.enabled = false; // Hareketi devre dışı bırak
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            mainCamera.enabled = true;
            cardGameCamera.enabled = false;

            if (characterController != null)
            {
                characterController.enabled = true; // Hareketi etkinleştir
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }


    void EnterCardGameMode()
    {
        // Switch to card game camera
        mainCamera.enabled = false;
        cardGameCamera.enabled = true;

        // Disable player movement
        if (characterController != null)
        {
            characterController.enabled = false;
        }

        // Unlock and show cursor for card game
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void ExitCardGameMode()
    {
        // Switch back to main camera
        mainCamera.enabled = true;
        cardGameCamera.enabled = false;

        
           characterController.enabled = true;
        

        // Lock cursor for normal gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public bool IsCardGameMode()
    {
        return isPlayingCardGame;
    }
}

