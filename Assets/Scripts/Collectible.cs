using UnityEngine;

/// <summary>
/// Collectible handles pickup items that give the player points
/// </summary>
public class Collectible : MonoBehaviour
{
    [Header("Collectible Settings")]
    [SerializeField] private int pointValue = 100;
    [SerializeField] private CollectibleType type = CollectibleType.Coin;
    
    [Header("Animation")]
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.25f;
    
    [Header("Effects")]
    [SerializeField] private GameObject collectEffect;
    [SerializeField] private AudioClip collectSound;
    
    // Private variables
    private Vector3 startPosition;
    private AudioSource audioSource;
    
    public enum CollectibleType
    {
        Coin,
        Gem,
        PowerUp,
        Key,
        HealthPack
    }
    
    private void Start()
    {
        startPosition = transform.position;
        audioSource = GetComponent<AudioSource>();
    }
    
    private void Update()
    {
        // Rotate the collectible
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        
        // Bob up and down
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Collect();
        }
    }
    
    /// <summary>
    /// Handles the collection of this item
    /// </summary>
    private void Collect()
    {
        // Add points to score system
        if (ScoreSystem.Instance != null)
        {
            ScoreSystem.Instance.AddPoints(pointValue, type.ToString());
        }
        
        // Play collect sound
        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position);
        }
        
        // Spawn collect effect
        if (collectEffect != null)
        {
            Instantiate(collectEffect, transform.position, Quaternion.identity);
        }
        
        // Destroy the collectible
        Destroy(gameObject);
    }
    
    /// <summary>
    /// Gets the point value of this collectible
    /// </summary>
    public int GetPointValue()
    {
        return pointValue;
    }
    
    /// <summary>
    /// Gets the type of this collectible
    /// </summary>
    public CollectibleType GetCollectibleType()
    {
        return type;
    }
}
