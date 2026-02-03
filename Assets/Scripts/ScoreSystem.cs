using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;

/// <summary>
/// ScoreSystem manages the game's scoring mechanics including points, combos, and multipliers
/// This is the second core game element demonstrating UI integration and game mechanics
/// </summary>
public class ScoreSystem : MonoBehaviour
{
    // Singleton pattern for easy access
    public static ScoreSystem Instance { get; private set; }
    
    [Header("Score Settings")]
    [SerializeField] private int basePointsPerCollectible = 100;
    [SerializeField] private int bonusPointsPerEnemy = 250;
    [SerializeField] private float comboTimeWindow = 2f;
    [SerializeField] private int maxComboMultiplier = 10;
    
    [Header("UI References")]
    [SerializeField] private Text scoreText;
    [SerializeField] private Text comboText;
    [SerializeField] private Text highScoreText;
    [SerializeField] private GameObject comboPopup;
    [SerializeField] private Animator scoreAnimator;
    
    [Header("Audio")]
    [SerializeField] private AudioClip scoreSound;
    [SerializeField] private AudioClip comboSound;
    [SerializeField] private AudioClip highScoreSound;
    private AudioSource audioSource;
    
    // Score tracking
    private int currentScore = 0;
    private int highScore = 0;
    private int comboCount = 0;
    private int currentMultiplier = 1;
    private float lastScoreTime;
    
    // Events for other systems to subscribe to
    public UnityEvent<int> OnScoreChanged;
    public UnityEvent<int> OnComboChanged;
    public UnityEvent OnNewHighScore;
    
    // Animation hashes
    private static readonly int ScorePulseHash = Animator.StringToHash("ScorePulse");
    private static readonly int ComboPopHash = Animator.StringToHash("ComboPop");
    
    private void Awake()
    {
        // Implement singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Initialize events
        if (OnScoreChanged == null) OnScoreChanged = new UnityEvent<int>();
        if (OnComboChanged == null) OnComboChanged = new UnityEvent<int>();
        if (OnNewHighScore == null) OnNewHighScore = new UnityEvent();
    }
    
    private void Start()
    {
        // Initialize audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Load high score from PlayerPrefs
        LoadHighScore();
        
        // Initialize UI
        UpdateScoreUI();
        UpdateComboUI();
    }
    
    private void Update()
    {
        // Check for combo timeout
        CheckComboTimeout();
    }
    
    /// <summary>
    /// Adds points for collecting an item
    /// </summary>
    public void AddCollectiblePoints()
    {
        AddPoints(basePointsPerCollectible, "Collectible");
    }
    
    /// <summary>
    /// Adds points for defeating an enemy
    /// </summary>
    public void AddEnemyPoints()
    {
        AddPoints(bonusPointsPerEnemy, "Enemy Defeated");
    }
    
    /// <summary>
    /// Adds custom points with a reason
    /// </summary>
    public void AddPoints(int basePoints, string reason = "")
    {
        // Check if within combo window
        if (Time.time - lastScoreTime <= comboTimeWindow)
        {
            // Increase combo
            comboCount++;
            currentMultiplier = Mathf.Min(comboCount, maxComboMultiplier);
            OnComboChanged?.Invoke(comboCount);
            
            // Play combo sound
            if (comboSound != null && audioSource != null)
            {
                audioSource.pitch = 1f + (comboCount * 0.1f); // Increase pitch with combo
                audioSource.PlayOneShot(comboSound);
            }
            
            // Show combo popup
            ShowComboPopup();
        }
        else
        {
            // Reset combo
            ResetCombo();
        }
        
        // Calculate final points with multiplier
        int finalPoints = basePoints * currentMultiplier;
        currentScore += finalPoints;
        lastScoreTime = Time.time;
        
        // Trigger events
        OnScoreChanged?.Invoke(currentScore);
        
        // Play score sound
        if (scoreSound != null && audioSource != null)
        {
            audioSource.pitch = 1f;
            audioSource.PlayOneShot(scoreSound);
        }
        
        // Check for new high score
        CheckHighScore();
        
        // Update UI
        UpdateScoreUI();
        UpdateComboUI();
        
        // Trigger score animation
        if (scoreAnimator != null)
        {
            scoreAnimator.SetTrigger(ScorePulseHash);
        }
        
        // Log for debugging
        Debug.Log($"[ScoreSystem] Added {finalPoints} points ({reason}). " +
                  $"Base: {basePoints}, Multiplier: x{currentMultiplier}, " +
                  $"Total Score: {currentScore}");
    }
    
    /// <summary>
    /// Checks if combo has timed out
    /// </summary>
    private void CheckComboTimeout()
    {
        if (comboCount > 0 && Time.time - lastScoreTime > comboTimeWindow)
        {
            ResetCombo();
            UpdateComboUI();
        }
    }
    
    /// <summary>
    /// Resets the combo counter
    /// </summary>
    private void ResetCombo()
    {
        comboCount = 0;
        currentMultiplier = 1;
        OnComboChanged?.Invoke(0);
    }
    
    /// <summary>
    /// Shows the combo popup animation
    /// </summary>
    private void ShowComboPopup()
    {
        if (comboPopup != null)
        {
            comboPopup.SetActive(true);
            
            // Get animator and play animation
            Animator popupAnimator = comboPopup.GetComponent<Animator>();
            if (popupAnimator != null)
            {
                popupAnimator.SetTrigger(ComboPopHash);
            }
        }
    }
    
    /// <summary>
    /// Checks and updates high score
    /// </summary>
    private void CheckHighScore()
    {
        if (currentScore > highScore)
        {
            highScore = currentScore;
            SaveHighScore();
            OnNewHighScore?.Invoke();
            
            // Play high score sound
            if (highScoreSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(highScoreSound);
            }
            
            UpdateHighScoreUI();
        }
    }
    
    /// <summary>
    /// Updates the score display
    /// </summary>
    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {currentScore:N0}";
        }
    }
    
    /// <summary>
    /// Updates the combo display
    /// </summary>
    private void UpdateComboUI()
    {
        if (comboText != null)
        {
            if (comboCount > 1)
            {
                comboText.text = $"Combo x{currentMultiplier}";
                comboText.gameObject.SetActive(true);
            }
            else
            {
                comboText.gameObject.SetActive(false);
            }
        }
    }
    
    /// <summary>
    /// Updates the high score display
    /// </summary>
    private void UpdateHighScoreUI()
    {
        if (highScoreText != null)
        {
            highScoreText.text = $"High Score: {highScore:N0}";
        }
    }
    
    /// <summary>
    /// Saves high score to PlayerPrefs
    /// </summary>
    private void SaveHighScore()
    {
        PlayerPrefs.SetInt("HighScore", highScore);
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// Loads high score from PlayerPrefs
    /// </summary>
    private void LoadHighScore()
    {
        highScore = PlayerPrefs.GetInt("HighScore", 0);
        UpdateHighScoreUI();
    }
    
    /// <summary>
    /// Resets the current score (for new game)
    /// </summary>
    public void ResetScore()
    {
        currentScore = 0;
        ResetCombo();
        UpdateScoreUI();
        UpdateComboUI();
        OnScoreChanged?.Invoke(0);
    }
    
    /// <summary>
    /// Gets the current score
    /// </summary>
    public int GetCurrentScore()
    {
        return currentScore;
    }
    
    /// <summary>
    /// Gets the high score
    /// </summary>
    public int GetHighScore()
    {
        return highScore;
    }
    
    /// <summary>
    /// Gets the current combo count
    /// </summary>
    public int GetComboCount()
    {
        return comboCount;
    }
    
    /// <summary>
    /// Gets the current multiplier
    /// </summary>
    public int GetMultiplier()
    {
        return currentMultiplier;
    }
}
