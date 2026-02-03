using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// PlayerHealth manages player health, damage, and death
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    // Singleton instance
    public static PlayerHealth Instance { get; private set; }
    
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private float invincibilityDuration = 1.5f;
    
    [Header("Regeneration")]
    [SerializeField] private bool enableRegeneration = true;
    [SerializeField] private float regenDelay = 5f;
    [SerializeField] private int regenAmount = 5;
    [SerializeField] private float regenInterval = 1f;
    
    [Header("Effects")]
    [SerializeField] private GameObject damageEffect;
    [SerializeField] private AudioClip damageSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip healSound;
    
    // Events
    public UnityEvent<int, int> OnHealthChanged; // current, max
    public UnityEvent OnPlayerDeath;
    public UnityEvent OnPlayerDamaged;
    
    // Private variables
    private int currentHealth;
    private bool isInvincible = false;
    private float lastDamageTime;
    private float regenTimer;
    private AudioSource audioSource;
    
    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Initialize events
        if (OnHealthChanged == null) OnHealthChanged = new UnityEvent<int, int>();
        if (OnPlayerDeath == null) OnPlayerDeath = new UnityEvent();
        if (OnPlayerDamaged == null) OnPlayerDamaged = new UnityEvent();
    }
    
    private void Start()
    {
        currentHealth = maxHealth;
        audioSource = GetComponent<AudioSource>();
        
        // Notify UI of initial health
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    private void Update()
    {
        // Handle regeneration
        if (enableRegeneration && currentHealth < maxHealth && currentHealth > 0)
        {
            if (Time.time - lastDamageTime >= regenDelay)
            {
                regenTimer += Time.deltaTime;
                if (regenTimer >= regenInterval)
                {
                    Heal(regenAmount);
                    regenTimer = 0f;
                }
            }
        }
    }
    
    /// <summary>
    /// Takes damage from an attack
    /// </summary>
    public void TakeDamage(int damage)
    {
        if (isInvincible || currentHealth <= 0) return;
        
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        lastDamageTime = Time.time;
        regenTimer = 0f;
        
        // Trigger events
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnPlayerDamaged?.Invoke();
        
        // Play damage sound
        if (damageSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(damageSound);
        }
        
        // Show damage effect
        if (damageEffect != null)
        {
            Instantiate(damageEffect, transform.position, Quaternion.identity);
        }
        
        Debug.Log($"[PlayerHealth] Took {damage} damage. Health: {currentHealth}/{maxHealth}");
        
        // Check for death
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Start invincibility
            StartCoroutine(InvincibilityCoroutine());
        }
    }
    
    /// <summary>
    /// Heals the player
    /// </summary>
    public void Heal(int amount)
    {
        if (currentHealth <= 0) return;
        
        int previousHealth = currentHealth;
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        
        if (currentHealth > previousHealth)
        {
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            
            // Play heal sound
            if (healSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(healSound);
            }
            
            Debug.Log($"[PlayerHealth] Healed {currentHealth - previousHealth}. Health: {currentHealth}/{maxHealth}");
        }
    }
    
    /// <summary>
    /// Fully restores health
    /// </summary>
    public void FullHeal()
    {
        Heal(maxHealth);
    }
    
    /// <summary>
    /// Handles player death
    /// </summary>
    private void Die()
    {
        Debug.Log("[PlayerHealth] Player died!");
        
        // Play death sound
        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound);
        }
        
        // Trigger death event
        OnPlayerDeath?.Invoke();
        
        // Notify game manager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoseLife();
        }
    }
    
    /// <summary>
    /// Invincibility coroutine after taking damage
    /// </summary>
    private System.Collections.IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;
        
        // Flash effect (optional)
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        float flashInterval = 0.1f;
        float elapsed = 0f;
        
        while (elapsed < invincibilityDuration)
        {
            // Toggle visibility
            foreach (Renderer r in renderers)
            {
                r.enabled = !r.enabled;
            }
            
            yield return new WaitForSeconds(flashInterval);
            elapsed += flashInterval;
        }
        
        // Ensure visible at end
        foreach (Renderer r in renderers)
        {
            r.enabled = true;
        }
        
        isInvincible = false;
    }
    
    /// <summary>
    /// Gets current health
    /// </summary>
    public int GetCurrentHealth()
    {
        return currentHealth;
    }
    
    /// <summary>
    /// Gets max health
    /// </summary>
    public int GetMaxHealth()
    {
        return maxHealth;
    }
    
    /// <summary>
    /// Gets health percentage
    /// </summary>
    public float GetHealthPercent()
    {
        return (float)currentHealth / maxHealth;
    }
    
    /// <summary>
    /// Checks if player is alive
    /// </summary>
    public bool IsAlive()
    {
        return currentHealth > 0;
    }
    
    /// <summary>
    /// Resets health to full (for respawn)
    /// </summary>
    public void ResetHealth()
    {
        currentHealth = maxHealth;
        isInvincible = false;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
}
