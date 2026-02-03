using UnityEngine;

/// <summary>
/// LevelTrigger handles level completion and checkpoint triggers
/// </summary>
public class LevelTrigger : MonoBehaviour
{
    [Header("Trigger Type")]
    [SerializeField] private TriggerType triggerType = TriggerType.LevelEnd;
    
    [Header("Visual Feedback")]
    [SerializeField] private GameObject activationEffect;
    [SerializeField] private AudioClip activationSound;
    [SerializeField] private Material activeMaterial;
    [SerializeField] private Material inactiveMaterial;
    
    [Header("Requirements")]
    [SerializeField] private bool requireAllCollectibles = false;
    [SerializeField] private int requiredCollectibles = 0;
    
    // Private variables
    private bool isActivated = false;
    private Renderer triggerRenderer;
    private AudioSource audioSource;
    
    public enum TriggerType
    {
        LevelEnd,
        Checkpoint,
        SecretArea,
        BossRoom
    }
    
    private void Start()
    {
        triggerRenderer = GetComponent<Renderer>();
        audioSource = GetComponent<AudioSource>();
        
        // Set initial material
        if (triggerRenderer != null && inactiveMaterial != null)
        {
            triggerRenderer.material = inactiveMaterial;
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (isActivated) return;
        
        // Check requirements
        if (requireAllCollectibles)
        {
            // Check if player has collected enough items
            // This would integrate with a collectible manager
            // For now, we'll skip this check
        }
        
        ActivateTrigger();
    }
    
    /// <summary>
    /// Activates the trigger
    /// </summary>
    private void ActivateTrigger()
    {
        isActivated = true;
        
        // Play activation effect
        if (activationEffect != null)
        {
            Instantiate(activationEffect, transform.position, Quaternion.identity);
        }
        
        // Play activation sound
        if (activationSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(activationSound);
        }
        
        // Change material
        if (triggerRenderer != null && activeMaterial != null)
        {
            triggerRenderer.material = activeMaterial;
        }
        
        // Handle trigger type
        switch (triggerType)
        {
            case TriggerType.LevelEnd:
                HandleLevelEnd();
                break;
            case TriggerType.Checkpoint:
                HandleCheckpoint();
                break;
            case TriggerType.SecretArea:
                HandleSecretArea();
                break;
            case TriggerType.BossRoom:
                HandleBossRoom();
                break;
        }
        
        Debug.Log($"[LevelTrigger] {triggerType} triggered!");
    }
    
    /// <summary>
    /// Handles level end trigger
    /// </summary>
    private void HandleLevelEnd()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CompleteLevel();
        }
    }
    
    /// <summary>
    /// Handles checkpoint trigger
    /// </summary>
    private void HandleCheckpoint()
    {
        // Update respawn point
        GameObject respawnPoint = GameObject.FindGameObjectWithTag("Respawn");
        if (respawnPoint != null)
        {
            respawnPoint.transform.position = transform.position;
            respawnPoint.transform.rotation = transform.rotation;
        }
        
        // Award bonus points
        if (ScoreSystem.Instance != null)
        {
            ScoreSystem.Instance.AddPoints(50, "Checkpoint");
        }
    }
    
    /// <summary>
    /// Handles secret area trigger
    /// </summary>
    private void HandleSecretArea()
    {
        // Award bonus points for finding secret
        if (ScoreSystem.Instance != null)
        {
            ScoreSystem.Instance.AddPoints(500, "Secret Area");
        }
        
        // Could trigger special events or unlock content
    }
    
    /// <summary>
    /// Handles boss room trigger
    /// </summary>
    private void HandleBossRoom()
    {
        // Change music to boss music
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusic(MusicType.Boss);
        }
        
        // Could spawn boss or trigger boss fight
    }
    
    /// <summary>
    /// Resets the trigger (for replayability)
    /// </summary>
    public void ResetTrigger()
    {
        isActivated = false;
        
        if (triggerRenderer != null && inactiveMaterial != null)
        {
            triggerRenderer.material = inactiveMaterial;
        }
    }
}
