using UnityEngine;

/// <summary>
/// CameraController handles third-person camera following and collision avoidance
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0f, 2f, -5f);
    
    [Header("Follow Settings")]
    [SerializeField] private float followSpeed = 10f;
    [SerializeField] private float rotationSpeed = 5f;
    
    [Header("Collision Settings")]
    [SerializeField] private float minDistance = 1f;
    [SerializeField] private float maxDistance = 10f;
    [SerializeField] private LayerMask collisionLayers;
    [SerializeField] private float collisionOffset = 0.2f;
    
    [Header("Look Settings")]
    [SerializeField] private float mouseSensitivity = 3f;
    [SerializeField] private float minVerticalAngle = -30f;
    [SerializeField] private float maxVerticalAngle = 60f;
    
    // Private variables
    private float currentDistance;
    private float horizontalAngle = 0f;
    private float verticalAngle = 20f;
    private Vector3 currentVelocity;
    
    private void Start()
    {
        // Find player if target not set
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }
        
        currentDistance = offset.magnitude;
        
        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    private void LateUpdate()
    {
        if (target == null) return;
        
        HandleInput();
        UpdateCameraPosition();
        HandleCollision();
    }
    
    /// <summary>
    /// Handles mouse input for camera rotation
    /// </summary>
    private void HandleInput()
    {
        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        
        // Update angles
        horizontalAngle += mouseX;
        verticalAngle -= mouseY;
        
        // Clamp vertical angle
        verticalAngle = Mathf.Clamp(verticalAngle, minVerticalAngle, maxVerticalAngle);
        
        // Handle zoom with scroll wheel
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        currentDistance -= scroll * 2f;
        currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);
    }
    
    /// <summary>
    /// Updates the camera position based on angles and distance
    /// </summary>
    private void UpdateCameraPosition()
    {
        // Calculate rotation
        Quaternion rotation = Quaternion.Euler(verticalAngle, horizontalAngle, 0f);
        
        // Calculate desired position
        Vector3 direction = rotation * Vector3.back;
        Vector3 desiredPosition = target.position + Vector3.up * offset.y + direction * currentDistance;
        
        // Smoothly move camera
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, 1f / followSpeed);
        
        // Look at target
        Vector3 lookTarget = target.position + Vector3.up * offset.y * 0.5f;
        transform.LookAt(lookTarget);
    }
    
    /// <summary>
    /// Handles camera collision with environment
    /// </summary>
    private void HandleCollision()
    {
        Vector3 targetPosition = target.position + Vector3.up * offset.y;
        Vector3 direction = transform.position - targetPosition;
        float distance = direction.magnitude;
        
        // Raycast to check for obstacles
        RaycastHit hit;
        if (Physics.Raycast(targetPosition, direction.normalized, out hit, distance, collisionLayers))
        {
            // Move camera in front of obstacle
            transform.position = hit.point - direction.normalized * collisionOffset;
        }
    }
    
    /// <summary>
    /// Sets a new target for the camera to follow
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
    
    /// <summary>
    /// Resets camera angles
    /// </summary>
    public void ResetCamera()
    {
        horizontalAngle = 0f;
        verticalAngle = 20f;
        currentDistance = offset.magnitude;
    }
    
    /// <summary>
    /// Shakes the camera (for impacts)
    /// </summary>
    public void ShakeCamera(float intensity, float duration)
    {
        StartCoroutine(CameraShakeCoroutine(intensity, duration));
    }
    
    /// <summary>
    /// Camera shake coroutine
    /// </summary>
    private System.Collections.IEnumerator CameraShakeCoroutine(float intensity, float duration)
    {
        Vector3 originalPosition = transform.localPosition;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * intensity;
            float y = Random.Range(-1f, 1f) * intensity;
            
            transform.localPosition = originalPosition + new Vector3(x, y, 0f);
            
            elapsed += Time.deltaTime;
            intensity = Mathf.Lerp(intensity, 0f, elapsed / duration);
            
            yield return null;
        }
        
        transform.localPosition = originalPosition;
    }
}
