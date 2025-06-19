using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;    
using System;

public class GameManager : MonoBehaviour
{
    // Singleton instance for global access
    public static GameManager Instance { get; private set; }

    // Game state variables
    public static int score = 0;        // Player's current score
    public static bool gameOver = false;// True when run ends
    public static bool isPaused = false;
    public static int levelsBeaten = 0;
    
    // UI References
    public Button resetButton; // Restart button on death panel
    public GameObject deathPanel; // Game over panel
    public GameObject pausePanel; // Pause menu panel
    public Button pauseRestartButton; // Restart button on pause menu
    public Button resumeButton; // Resume button on pause menu
    public TMP_Text scoreText; // Variable for the score text in the menu
    public TMP_Text levelsBeatenText; // Variable for the levels beaten text in the menu

    // --- Event Definitions ---
    public static event Action OnObstaclePassedEvent; // Invoked when an obstacle is passed
    
    // Enum to identify pickup types
    public enum PickupType { Berry, Wolf, Shield }
    public static event Action<PickupType> OnPickupActivatedEvent; // Invoked when a pickup is activated

    public static event Action OnBossSpawnedEvent; // Invoked when a boss spawns
    public static event Action OnBossBeatenEvent;  // Invoked when a boss is beaten
    // --- End of Event Definitions ---

    public int GetScore()
    {
        return score;
    }

    public int GetLevelsBeaten()
    {
        return levelsBeaten;
    }
    
    void SubscribeToEvents()
    {
        OnObstaclePassedEvent += HandleObstaclePassedScore;
        OnBossBeatenEvent += HandleBossBeatenScore;
    }
    
    void UpdateLevelsBeatenUI()
    {
        if (levelsBeatenText != null)
        {
            levelsBeatenText.text = "Bosses Beaten: " + levelsBeaten;
        }
    }
    
    void Awake()
    {
        // Set up singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
            SceneManager.sceneLoaded += OnSceneLoaded;

            // Subscribe GameManager methods to its own events
            SubscribeToEvents();
            
            // Call InitializeReferences() only for the true singleton instance on its first Awake.
            // OnSceneLoaded will handle re-initialization for subsequent scene loads.
            InitializeReferences();
        }
        else
        {
            // If an Instance already exists, this is a duplicate.
            Destroy(gameObject); // Destroy this duplicate GameObject.
            return; // Exit Awake early for this duplicate instance.
        }
        // Initial setup (will be called again via OnSceneLoaded after reset)
        InitializeReferences();
    }
    
    // Called when the GameObject is destroyed (good practice to unsubscribe)
    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        
        UnsubscribeFromEvents(); // Unsubscribe when destroyed
        
        // Clean up button listener if it exists
        if (resetButton != null)
            resetButton.onClick.RemoveListener(ResetGame);
        if (pauseRestartButton != null)
            pauseRestartButton.onClick.RemoveListener(ResetGame);
        if (resumeButton != null)
            resumeButton.onClick.RemoveListener(ResumeGame);
    }
    //Called when scene is loaded. 
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Re-initialize references for the newly loaded scene
        InitializeReferences();

        // Reset game state here, as this runs after the scene is ready
        ResetGameState();
    }
    
    // Helper method to find and set up references
    void InitializeReferences()
    {
        // --- Find UI Elements ---
        GameObject deathPanelObj = GameObject.Find("DeathPanel");
        if (deathPanelObj != null)
        {
            deathPanel = deathPanelObj;
            // Ensure it starts deactivated in the new scene
        }
        else
        {
            Debug.LogError("GameManager: Could not find 'DeathPanel' GameObject in the scene");
            deathPanel = null; // Set to null so we don't try to use a bad reference
        }

        // Reset Button (Death Panel)
        GameObject resetButtonObj = GameObject.Find("ResetButton");
        if (resetButtonObj != null)
        {
            resetButton = resetButtonObj.GetComponent<Button>();
            if (resetButton != null)
            {
                resetButton.onClick.RemoveAllListeners();
                resetButton.onClick.AddListener(ResetGame);
            }
        }
        else
        {
            Debug.LogError("GameManager: Could not find 'ResetButton'");
        }

        // Pause Panel
        GameObject pausePanelObj = GameObject.Find("PausePanel");
        if (pausePanelObj != null)
        {
            pausePanel = pausePanelObj;
        }
        else
        {
            Debug.LogError("GameManager: Could not find 'PausePanel'");
            pausePanel = null;
        }

        // Pause Restart Button
        GameObject pauseRestartButtonObj = GameObject.Find("PauseRestartButton");
        if (pauseRestartButtonObj != null)
        {
            pauseRestartButton = pauseRestartButtonObj.GetComponent<Button>();
            if (pauseRestartButton != null)
            {
                pauseRestartButton.onClick.RemoveAllListeners();
                pauseRestartButton.onClick.AddListener(ResetGame);
            }
        }
        else
        {
            Debug.LogError("GameManager: Could not find 'PauseRestartButton'");
        }

        // Resume Button
        GameObject resumeButtonObj = GameObject.Find("ResumeButton");
        if (resumeButtonObj != null)
        {
            resumeButton = resumeButtonObj.GetComponent<Button>();
            if (resumeButton != null)
            {
                resumeButton.onClick.RemoveAllListeners();
                resumeButton.onClick.AddListener(ResumeGame);
            }
        }
        else
        {
            Debug.LogError("GameManager: Could not find 'ResumeButton'");
        }
        
        // Score Text
        GameObject scoreTextObj = GameObject.Find("ScoreText");
        if (scoreTextObj != null)
        {
            scoreText = scoreTextObj.GetComponent<TMP_Text>();
            if (scoreText != null)
            {
                UpdateScoreUI(); // Initialize score display
            }
        }
        else
        {
            Debug.LogError("GameManager: Could not find 'ScoreText'");
            scoreText = null;
        }
        UpdateScoreUI();
        UpdateLevelsBeatenUI(); // Initialize new score display
    }
    
    // Reset game state variables
    void ResetGameState()
    {
        score = 0;
        gameOver = false;
        isPaused = false;
        Time.timeScale = 1f; // Ensure time is running
        if (deathPanel != null) deathPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        UpdateScoreUI(); // Reset score display
        UpdateLevelsBeatenUI();
    }
    void UnsubscribeFromEvents()
    {
        OnObstaclePassedEvent -= HandleObstaclePassedScore;
        OnBossBeatenEvent -= HandleBossBeatenScore;
    }
    
    private void HandleObstaclePassedScore()
    {
        if (!gameOver && !isPaused)
        {
            score += 1;
            UpdateScoreUI();
        }
    }

    private void HandleBossBeatenScore()
    {
        if (!gameOver && !isPaused)
        {
            levelsBeaten += 1;
            // You could also add points to the main score here if desired
            // score += 10; // Example: Add 10 points to main score for beating a boss
            UpdateLevelsBeatenUI();
            UpdateScoreUI(); // If main score was also affected
            Debug.Log("Boss Beaten! Levels Beaten: " + levelsBeaten);
        }
    }
    
    public void ReportObstaclePassed()
    {
        OnObstaclePassedEvent?.Invoke();
        // Debug.Log("OnObstaclePassedEvent Invoked");
    }

    public void ReportPickupActivated(PickupType type)
    {
        OnPickupActivatedEvent?.Invoke(type);
        Debug.Log($"OnPickupActivatedEvent Invoked for: {type}");
    }

    public void ReportBossSpawned()
    {
        OnBossSpawnedEvent?.Invoke();
        Debug.Log("OnBossSpawnedEvent Invoked");
    }

    public void ReportBossBeaten() // Renamed from OnBossOvercome for clarity with event
    {
        OnBossBeatenEvent?.Invoke();
        // Debug.Log("OnBossBeatenEvent Invoked via ReportBossBeaten");
    }
    
    //Method to add score 
    public void AddScore(int points)
    {
        if (!gameOver && !isPaused)
        {
            score += points;
            UpdateScoreUI(); // Update UI whenever score changes
        }
    }

    void Update()
    {
        // Toggle pause with Escape key
        if (Input.GetKeyDown(KeyCode.Escape) && !gameOver)
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    // Called by ObstacleCollider.cs when player hits an obstacle
    public void OnObstacleCollision()
    {
        gameOver = true; // End run (Rule 5)
        
        //Show Game Over screen 
        ShowGameOverScreen();
    }
    
    public void OnObstaclePassed() // Called by ObstacleSpawner.cs
    {
        ReportObstaclePassed();
    }

    // This method is now simplified
    public void OnBossOvercome() // Called by BossFight.cs
    {
        ReportBossBeaten();
    }

    public void OnBossLoss()
    {
        gameOver = true;
        Debug.Log("Boss fight lost! Game Over");
        ShowGameOverScreen(); // Show game over screen on boss loss too
    }


    // Reset game state for restart
    public void ResetGame()
    {
        // Resume time before reset
        Time.timeScale = 1f; 
        // OnSceneLoaded and its helper methods will handle resetting state and references AFTER the scene loads.
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    void ShowGameOverScreen()
    {
        if (deathPanel != null)
        {
            Time.timeScale = 0f; // Pause time on game over
            deathPanel.SetActive(true); // Show Game Over screen
            Debug.Log("Death Panel Activated.");
        }
        else
        {
            // This error means InitializeReferences failed to find the panel earlier
            Debug.LogError("GameManager: Cannot show Death Panel - reference is missing");
        }
    }
    
    void PauseGame()
    {
        //Check if the pause panel
        if (pausePanel != null)
        {
            isPaused = true;
            Time.timeScale = 0f; // Pause time
            pausePanel.SetActive(true);
            Debug.Log("Game Paused.");
        }
        else
        {
            Debug.LogError("GameManager: Cannot show Pause Panel - reference is missing");
        }
    }

    public void ResumeGame()
    {
        if (pausePanel != null)
        {
            isPaused = false;
            Time.timeScale = 1f; // Resume time
            pausePanel.SetActive(false);
            Debug.Log("Game Resumed.");
        }
    }
    
    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }
        else
        {
            Debug.LogError("GameManager: ScoreText is null, cannot update score UI");
        }
    }

    public void CloseGame()
    {
        //Close the game 
        Application.Quit();
    }
}
