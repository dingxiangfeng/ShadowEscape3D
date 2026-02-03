using UnityEngine;

/// <summary>
/// PlayerController handles all player movement, jumping, and camera control
/// This is one of the core game elements demonstrating 3D character control
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float rotationSpeed = 10f;
    
    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask groundLayer;
    
    [Header("Camera Settings")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float minVerticalAngle = -60f;
    [SerializeField] private float maxVerticalAngle = 60f;
    
    // Private variables
    private CharacterController characterController;
    private Animator animator;
    private Vector3 velocity;
    private float verticalRotation = 0f;
    private bool isGrounded;
    private bool isSprinting;
    
    // Animation parameter hashes for performance
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int JumpHash = Animator.StringToHash("Jump");
    
    private void Start()
    {
        // Initialize components
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        
        // Lock cursor for first-person/third-person control
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // Initialize camera reference if not set
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }
    }
    
    private void Update()
    {
        // Check ground status
        CheckGrounded();
        
        // Handle input
        HandleMovement();
        HandleJump();
        HandleCameraRotation();
        
        // Apply gravity
        ApplyGravity();
        
        // Update animations
        UpdateAnimations();
    }
    
    /// <summary>
    /// Checks if the player is grounded using a raycast
    /// </summary>
    private void CheckGrounded()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);
        
        // Reset vertical velocity when grounded
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Small negative value to keep grounded
        }
    }
    
    /// <summary>
    /// Handles player movement based on input
    /// </summary>
    private void HandleMovement()
    {
        // Get input
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        
        // Check for sprint
        isSprinting = Input.GetKey(KeyCode.LeftShift) && isGrounded;
        float currentSpeed = isSprinting ? sprintSpeed : moveSpeed;
        
        // Calculate movement direction relative to camera
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        
        // Keep movement on horizontal plane
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();
        
        // Calculate movement vector
        Vector3 moveDirection = (forward * vertical + right * horizontal).normalized;
        
        // Apply movement
        if (moveDirection.magnitude > 0.1f)
        {
            // Rotate player to face movement direction
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            
            // Move player
            characterController.Move(moveDirection * currentSpeed * Time.deltaTime);
        }
    }
    
    /// <summary>
    /// Handles jumping mechanics
    /// </summary>
    private void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            
            // Trigger jump animation
            if (animator != null)
            {
                animator.SetTrigger(JumpHash);
            }
        }
    }
    
    /// <summary>
    /// Handles camera rotation for looking around
    /// </summary>
    private void HandleCameraRotation()
    {
        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        
        // Horizontal rotation (player body)
        transform.Rotate(Vector3.up * mouseX);
        
        // Vertical rotation (camera only)
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, minVerticalAngle, maxVerticalAngle);
        
        if (cameraTransform != null)
        {
            cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        }
    }
    
    /// <summary>
    /// Applies gravity to the player
    /// </summary>
    private void ApplyGravity()
    {
        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }
    
    /// <summary>
    /// Updates animator parameters based on current state
    /// </summary>
    private void UpdateAnimations()
    {
        if (animator == null) return;
        
        // Calculate current speed for blend tree
        float currentSpeed = new Vector3(characterController.velocity.x, 0, characterController.velocity.z).magnitude;
        animator.SetFloat(SpeedHash, currentSpeed);
        animator.SetBool(IsGroundedHash, isGrounded);
    }
    
    /// <summary>
    /// Public method to apply external force (e.g., from explosions)
    /// </summary>
    public void ApplyExternalForce(Vector3 force)
    {
        velocity += force;
    }
    
    /// <summary>
    /// Toggle cursor lock state (for menus)
    /// </summary>
    public void ToggleCursorLock(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}
