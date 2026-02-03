using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// UIManager handles all user interface elements and HUD updates
/// </summary>
public class UIManager : MonoBehaviour
{
    // Singleton instance
    public static UIManager Instance { get; private set; }
    
    [Header("HUD Elements")]
    [SerializeField] private Text scoreText;
    [SerializeField] private Text highScoreText;
    [SerializeField] private Text comboText;
    [SerializeField] private Text livesText;
    [SerializeField] private Text timerText;
    [SerializeField] private Text levelText;
    [SerializeField] private Slider healthBar;
    [SerializeField] private Image healthFill;
    
    [Header("Panels")]
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject levelCompletePanel;
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;
    
    [Header("Popup Elements")]
    [SerializeField] private GameObject scorePopupPrefab;
    [SerializeField] private Transform popupContainer;
    [SerializeField] private GameObject comboPopup;
    
    [Header("Colors")]
    [SerializeField] private Color healthHighColor = Color.green;
    [SerializeField] private Color healthMediumColor = Color.yellow;
    [SerializeField] private Color healthLowColor = Color.red;
    
    [Header("Animation")]
    [SerializeField] private Animator hudAnimator;
    [SerializeField] private float popupDuration = 1f;
    
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
    }
    
    private void Start()
    {
        // Subscribe to score system events
        if (ScoreSystem.Instance != null)
        {
            ScoreSystem.Instance.OnScoreChanged.AddListener(UpdateScoreDisplay);
            ScoreSystem.Instance.OnComboChanged.AddListener(UpdateComboDisplay);
            ScoreSystem.Instance.OnNewHighScore.AddListener(OnNewHighScore);
        }
        
        // Initialize UI
        HideAllPanels();
        ShowHUD();
    }
    
    /// <summary>
    /// Updates the score display
    /// </summary>
    public void UpdateScoreDisplay(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score:N0}";
            
            // Trigger score animation
            StartCoroutine(PulseText(scoreText));
        }
    }
    
    /// <summary>
    /// Updates the combo display
    /// </summary>
    public void UpdateComboDisplay(int combo)
    {
        if (comboText != null)
        {
            if (combo > 1)
            {
                comboText.text = $"x{combo} COMBO!";
                comboText.gameObject.SetActive(true);
                
                // Show combo popup
                if (comboPopup != null)
                {
                    StartCoroutine(ShowComboPopup());
                }
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
    public void UpdateHighScoreDisplay(int highScore)
    {
        if (highScoreText != null)
        {
            highScoreText.text = $"Best: {highScore:N0}";
        }
    }
    
    /// <summary>
    /// Updates the lives display
    /// </summary>
    public void UpdateLivesDisplay(int lives)
    {
        if (livesText != null)
        {
            livesText.text = $"Lives: {lives}";
        }
    }
    
    /// <summary>
    /// Updates the timer display
    /// </summary>
    public void UpdateTimerDisplay(float timeRemaining)
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(timeRemaining / 60);
            int seconds = Mathf.FloorToInt(timeRemaining % 60);
            timerText.text = $"{minutes:00}:{seconds:00}";
            
            // Change color when low on time
            if (timeRemaining < 30)
            {
                timerText.color = healthLowColor;
            }
            else if (timeRemaining < 60)
            {
                timerText.color = healthMediumColor;
            }
            else
            {
                timerText.color = Color.white;
            }
        }
    }
    
    /// <summary>
    /// Updates the level display
    /// </summary>
    public void UpdateLevelDisplay(int level)
    {
        if (levelText != null)
        {
            levelText.text = $"Level {level}";
        }
    }
    
    /// <summary>
    /// Updates the health bar
    /// </summary>
    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (healthBar != null)
        {
            float healthPercent = currentHealth / maxHealth;
            healthBar.value = healthPercent;
            
            // Update health bar color
            if (healthFill != null)
            {
                if (healthPercent > 0.6f)
                {
                    healthFill.color = healthHighColor;
                }
                else if (healthPercent > 0.3f)
                {
                    healthFill.color = healthMediumColor;
                }
                else
                {
                    healthFill.color = healthLowColor;
                }
            }
        }
    }
    
    /// <summary>
    /// Shows a score popup at a position
    /// </summary>
    public void ShowScorePopup(int points, Vector3 worldPosition)
    {
        if (scorePopupPrefab != null && popupContainer != null)
        {
            // Convert world position to screen position
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPosition);
            
            // Create popup
            GameObject popup = Instantiate(scorePopupPrefab, popupContainer);
            popup.transform.position = screenPos;
            
            // Set text
            Text popupText = popup.GetComponent<Text>();
            if (popupText != null)
            {
                popupText.text = $"+{points}";
            }
            
            // Destroy after duration
            Destroy(popup, popupDuration);
        }
    }
    
    /// <summary>
    /// Shows the combo popup animation
    /// </summary>
    private IEnumerator ShowComboPopup()
    {
        if (comboPopup != null)
        {
            comboPopup.SetActive(true);
            
            yield return new WaitForSeconds(0.5f);
            
            comboPopup.SetActive(false);
        }
    }
    
    /// <summary>
    /// Pulses a text element for emphasis
    /// </summary>
    private IEnumerator PulseText(Text text)
    {
        Vector3 originalScale = text.transform.localScale;
        Vector3 targetScale = originalScale * 1.2f;
        
        // Scale up
        float elapsed = 0f;
        float duration = 0.1f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            text.transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / duration);
            yield return null;
        }
        
        // Scale back down
        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            text.transform.localScale = Vector3.Lerp(targetScale, originalScale, elapsed / duration);
            yield return null;
        }
        
        text.transform.localScale = originalScale;
    }
    
    /// <summary>
    /// Called when a new high score is achieved
    /// </summary>
    private void OnNewHighScore()
    {
        // Show celebration effect
        Debug.Log("[UIManager] New High Score!");
        
        if (highScoreText != null)
        {
            StartCoroutine(FlashText(highScoreText, Color.yellow, 3));
        }
    }
    
    /// <summary>
    /// Flashes a text element
    /// </summary>
    private IEnumerator FlashText(Text text, Color flashColor, int times)
    {
        Color originalColor = text.color;
        
        for (int i = 0; i < times; i++)
        {
            text.color = flashColor;
            yield return new WaitForSeconds(0.15f);
            text.color = originalColor;
            yield return new WaitForSeconds(0.15f);
        }
    }
    
    /// <summary>
    /// Shows the HUD
    /// </summary>
    public void ShowHUD()
    {
        if (hudPanel != null)
        {
            hudPanel.SetActive(true);
        }
    }
    
    /// <summary>
    /// Hides the HUD
    /// </summary>
    public void HideHUD()
    {
        if (hudPanel != null)
        {
            hudPanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// Shows the pause menu
    /// </summary>
    public void ShowPauseMenu()
    {
        HideAllPanels();
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }
    }
    
    /// <summary>
    /// Shows the game over screen
    /// </summary>
    public void ShowGameOver(int finalScore)
    {
        HideAllPanels();
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            
            // Update final score display
            Text finalScoreText = gameOverPanel.GetComponentInChildren<Text>();
            if (finalScoreText != null)
            {
                finalScoreText.text = $"Final Score: {finalScore:N0}";
            }
        }
    }
    
    /// <summary>
    /// Shows the level complete screen
    /// </summary>
    public void ShowLevelComplete(int levelScore, int timeBonus)
    {
        HideAllPanels();
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(true);
        }
    }
    
    /// <summary>
    /// Shows the main menu
    /// </summary>
    public void ShowMainMenu()
    {
        HideAllPanels();
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }
    }
    
    /// <summary>
    /// Shows the settings panel
    /// </summary>
    public void ShowSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
    }
    
    /// <summary>
    /// Hides the settings panel
    /// </summary>
    public void HideSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// Hides all UI panels
    /// </summary>
    private void HideAllPanels()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (levelCompletePanel != null) levelCompletePanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        if (ScoreSystem.Instance != null)
        {
            ScoreSystem.Instance.OnScoreChanged.RemoveListener(UpdateScoreDisplay);
            ScoreSystem.Instance.OnComboChanged.RemoveListener(UpdateComboDisplay);
            ScoreSystem.Instance.OnNewHighScore.RemoveListener(OnNewHighScore);
        }
    }
}
