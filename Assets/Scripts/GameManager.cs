using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;    

public class GameManager : MonoBehaviour
{
    // Singleton instance for global access
    public static GameManager Instance { get; private set; }

    // Game state variables
    public static int score = 0;        // Player's current score
    public static bool gameOver = false;// True when run ends
    public static bool isPaused = false;
    
    // UI References
    public Button resetButton; // Restart button on death panel
    public GameObject deathPanel; // Game over panel
    public GameObject pausePanel; // Pause menu panel
    public Button pauseRestartButton; // Restart button on pause menu
    public Button resumeButton; // Resume button on pause menu
    public TMP_Text scoreText; // Variable for the score text in the menu
    

    // References (optional, set in Inspector)
    public PlayerMovement player;       // Reference to player for control disabling

    void Awake()
    {
        // Set up singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
            
            //sceneLoaded event to re-find references to objects on reset 
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject); // Ensure only one instance exists
        }
        // Initial setup (will be called again via OnSceneLoaded after reset)
        InitializeReferences();
    }
    
    // Called when the GameObject is destroyed (good practice to unsubscribe)
    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
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
        // This assumes your panel GameObject is NAMED "DeathPanel" in the Hierarchy.
        // Using tags or direct Inspector assignment in a scene-specific setup object
        // can be more robust than Find.
        GameObject deathPanelObj = GameObject.Find("DeathPanel");
        if (deathPanelObj != null)
        {
            deathPanel = deathPanelObj;
            // Ensure it starts deactivated in the new scene
        }
        else
        {
            Debug.LogError("GameManager: Could not find 'DeathPanel' GameObject in the scene!");
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
            Debug.LogError("GameManager: Could not find 'ResetButton'!");
        }

        // Pause Panel
        GameObject pausePanelObj = GameObject.Find("PausePanel");
        if (pausePanelObj != null)
        {
            pausePanel = pausePanelObj;
        }
        else
        {
            Debug.LogError("GameManager: Could not find 'PausePanel'!");
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
            Debug.LogError("GameManager: Could not find 'PauseRestartButton'!");
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
            Debug.LogError("GameManager: Could not find 'ResumeButton'!");
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
            Debug.LogError("GameManager: Could not find 'ScoreText'!");
            scoreText = null;
        }
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

    // Called by ObstacleSpawner.cs when an obstacle is passed
    public void OnObstaclePassed()
    {
        if (!gameOver)
        {
            AddScore(1); // Increment score
            UpdateScoreUI();
        }
    }

    // Called by BossFight.cs when boss is overcome or dodged
    public void OnBossOvercome()
    {
        if (!gameOver)
        {
            score += 10; // Add 10 points (Rule 22)
            Debug.Log("Boss overcome! Score: " + score);
        }
    }

    // Called by BossFight.cs when player loses boss fight
    public void OnBossLoss()
    {
        gameOver = true; // End run (Rule 5)
        Debug.Log("Boss fight lost! Game Over!");
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
            Debug.LogError("GameManager: Cannot show Death Panel - reference is missing!");
        }
    }
    
    void PauseGame()
    {
        if (pausePanel != null)
        {
            isPaused = true;
            Time.timeScale = 0f; // Pause time
            pausePanel.SetActive(true);
            Debug.Log("Game Paused.");
        }
        else
        {
            Debug.LogError("GameManager: Cannot show Pause Panel - reference is missing!");
        }
    }

    void ResumeGame()
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
            Debug.LogError("GameManager: ScoreText is null, cannot update score UI!");
        }
    }
}
