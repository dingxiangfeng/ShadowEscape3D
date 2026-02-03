using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// GameManager handles overall game state, level management, and game flow
/// </summary>
public class GameManager : MonoBehaviour
{
    // Singleton instance
    public static GameManager Instance { get; private set; }
    
    [Header("Game State")]
    [SerializeField] private GameState currentState = GameState.MainMenu;
    
    [Header("Level Settings")]
    [SerializeField] private string[] levelNames;
    [SerializeField] private int currentLevelIndex = 0;
    
    [Header("UI References")]
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private GameObject levelCompleteScreen;
    [SerializeField] private Text levelText;
    [SerializeField] private Text timerText;
    
    [Header("Game Settings")]
    [SerializeField] private float levelTimeLimit = 300f; // 5 minutes per level
    [SerializeField] private int playerLives = 3;
    
    // Private variables
    private float currentLevelTime;
    private int currentLives;
    private bool isPaused = false;
    
    // Game state enum
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver,
        LevelComplete,
        Victory
    }
    
    private void Awake()
    {
        // Singleton pattern
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
    }
    
    private void Start()
    {
        InitializeGame();
    }
    
    private void Update()
    {
        // Handle pause input
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
        
        // Update timer during gameplay
        if (currentState == GameState.Playing)
        {
            UpdateTimer();
        }
    }
    
    /// <summary>
    /// Initializes game settings
    /// </summary>
    private void InitializeGame()
    {
        currentLives = playerLives;
        currentLevelTime = levelTimeLimit;
        UpdateLevelUI();
    }
    
    /// <summary>
    /// Starts a new game
    /// </summary>
    public void StartNewGame()
    {
        currentLevelIndex = 0;
        currentLives = playerLives;
        
        if (ScoreSystem.Instance != null)
        {
            ScoreSystem.Instance.ResetScore();
        }
        
        LoadLevel(currentLevelIndex);
        SetGameState(GameState.Playing);
    }
    
    /// <summary>
    /// Loads a specific level
    /// </summary>
    public void LoadLevel(int levelIndex)
    {
        if (levelIndex >= 0 && levelIndex < levelNames.Length)
        {
            currentLevelIndex = levelIndex;
            currentLevelTime = levelTimeLimit;
            
            StartCoroutine(LoadLevelAsync(levelNames[levelIndex]));
        }
        else
        {
            Debug.LogError($"[GameManager] Invalid level index: {levelIndex}");
        }
    }
    
    /// <summary>
    /// Asynchronously loads a level
    /// </summary>
    private IEnumerator LoadLevelAsync(string levelName)
    {
        // Show loading screen here if needed
        
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(levelName);
        
        while (!asyncLoad.isDone)
        {
            // Update loading progress here if needed
            yield return null;
        }
        
        // Level loaded
        SetGameState(GameState.Playing);
        UpdateLevelUI();
    }
    
    /// <summary>
    /// Updates the level timer
    /// </summary>
    private void UpdateTimer()
    {
        currentLevelTime -= Time.deltaTime;
        
        if (currentLevelTime <= 0)
        {
            currentLevelTime = 0;
            OnTimeUp();
        }
        
        UpdateTimerUI();
    }
    
    /// <summary>
    /// Called when time runs out
    /// </summary>
    private void OnTimeUp()
    {
        LoseLife();
    }
    
    /// <summary>
    /// Player loses a life
    /// </summary>
    public void LoseLife()
    {
        currentLives--;
        
        if (currentLives <= 0)
        {
            GameOver();
        }
        else
        {
            // Respawn player
            RespawnPlayer();
        }
    }
    
    /// <summary>
    /// Respawns the player at checkpoint
    /// </summary>
    private void RespawnPlayer()
    {
        // Find player and respawn point
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        GameObject respawnPoint = GameObject.FindGameObjectWithTag("Respawn");
        
        if (player != null && respawnPoint != null)
        {
            player.transform.position = respawnPoint.transform.position;
            player.transform.rotation = respawnPoint.transform.rotation;
        }
        
        // Reset level timer
        currentLevelTime = levelTimeLimit;
    }
    
    /// <summary>
    /// Completes the current level
    /// </summary>
    public void CompleteLevel()
    {
        SetGameState(GameState.LevelComplete);
        
        // Calculate time bonus
        int timeBonus = Mathf.RoundToInt(currentLevelTime * 10);
        if (ScoreSystem.Instance != null)
        {
            ScoreSystem.Instance.AddPoints(timeBonus, "Time Bonus");
        }
        
        // Show level complete screen
        if (levelCompleteScreen != null)
        {
            levelCompleteScreen.SetActive(true);
        }
        
        // Check if there are more levels
        if (currentLevelIndex < levelNames.Length - 1)
        {
            // More levels available
            StartCoroutine(LoadNextLevelAfterDelay(3f));
        }
        else
        {
            // Game complete!
            Victory();
        }
    }
    
    /// <summary>
    /// Loads next level after a delay
    /// </summary>
    private IEnumerator LoadNextLevelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (levelCompleteScreen != null)
        {
            levelCompleteScreen.SetActive(false);
        }
        
        LoadLevel(currentLevelIndex + 1);
    }
    
    /// <summary>
    /// Game over state
    /// </summary>
    public void GameOver()
    {
        SetGameState(GameState.GameOver);
        
        if (gameOverScreen != null)
        {
            gameOverScreen.SetActive(true);
        }
        
        Time.timeScale = 0f;
    }
    
    /// <summary>
    /// Victory state - all levels completed
    /// </summary>
    private void Victory()
    {
        SetGameState(GameState.Victory);
        Debug.Log("[GameManager] Congratulations! You've completed all levels!");
    }
    
    /// <summary>
    /// Toggles pause state
    /// </summary>
    public void TogglePause()
    {
        if (currentState == GameState.Playing)
        {
            Pause();
        }
        else if (currentState == GameState.Paused)
        {
            Resume();
        }
    }
    
    /// <summary>
    /// Pauses the game
    /// </summary>
    public void Pause()
    {
        SetGameState(GameState.Paused);
        Time.timeScale = 0f;
        isPaused = true;
        
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(true);
        }
        
        // Unlock cursor for menu
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    /// <summary>
    /// Resumes the game
    /// </summary>
    public void Resume()
    {
        SetGameState(GameState.Playing);
        Time.timeScale = 1f;
        isPaused = false;
        
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(false);
        }
        
        // Lock cursor for gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    /// <summary>
    /// Restarts the current level
    /// </summary>
    public void RestartLevel()
    {
        Time.timeScale = 1f;
        LoadLevel(currentLevelIndex);
    }
    
    /// <summary>
    /// Returns to main menu
    /// </summary>
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SetGameState(GameState.MainMenu);
        SceneManager.LoadScene("MainMenu");
    }
    
    /// <summary>
    /// Quits the game
    /// </summary>
    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
    
    /// <summary>
    /// Sets the current game state
    /// </summary>
    private void SetGameState(GameState newState)
    {
        currentState = newState;
        Debug.Log($"[GameManager] Game state changed to: {newState}");
    }
    
    /// <summary>
    /// Updates level UI elements
    /// </summary>
    private void UpdateLevelUI()
    {
        if (levelText != null)
        {
            levelText.text = $"Level {currentLevelIndex + 1}";
        }
    }
    
    /// <summary>
    /// Updates timer UI
    /// </summary>
    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(currentLevelTime / 60);
            int seconds = Mathf.FloorToInt(currentLevelTime % 60);
            timerText.text = $"{minutes:00}:{seconds:00}";
        }
    }
    
    /// <summary>
    /// Gets current game state
    /// </summary>
    public GameState GetCurrentState()
    {
        return currentState;
    }
    
    /// <summary>
    /// Gets remaining lives
    /// </summary>
    public int GetLives()
    {
        return currentLives;
    }
    
    /// <summary>
    /// Gets current level index
    /// </summary>
    public int GetCurrentLevelIndex()
    {
        return currentLevelIndex;
    }
}
