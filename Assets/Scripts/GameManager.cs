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
    public static int score = 0;        // Player's current score for obstacles passed
    public static int levelsBeaten = 0; // New score component for bosses beaten
    public static bool gameOver = false;// True when run ends
    public static bool isPaused = false;

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

    // --- Level Transition Fields ---
    [Header("Level Transition Settings")]
    public string startSceneName = "StartScene";
    public string wildernessSceneName = "WildernessScene";
    public string longhouseSceneName = "LonghouseScene";
    public string[] randomPlayableLevelNames;
    
    public int startSceneScoreThreshold = 5;
    public int wildernessScoreThreshold = 20;
    public int longhouseScoreThreshold = 30;

    private string currentActiveSceneName; // Keeps track of the scene currently loaded
    
    //Database fields 
    [Header("Database Fields:")]
    private int currentHighScore = 0;
    private string playerName = "Player"; //Also overwriting the value from the edit in Main Value

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
            InitializeReferences();
            // Initialize currentActiveSceneName for the very first scene load
            currentActiveSceneName = SceneManager.GetActiveScene().name;
        }
        else
        {
            // If an Instance already exists, this is a duplicate.
            Destroy(gameObject); // Destroy this duplicate GameObject.
            return; // Exit Awake early for this duplicate instance.
        }
    }

    // Called when the GameObject is destroyed
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
        currentActiveSceneName = scene.name; // Update current scene name
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
        }
        else
        {
            Debug.LogError("GameManager: Could not find 'DeathPanel' GameObject in the scene");
            deathPanel = null;
        }

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

        GameObject scoreTextObj = GameObject.Find("ScoreText");
        if (scoreTextObj != null)
        {
            scoreText = scoreTextObj.GetComponent<TMP_Text>();
        }
        else
        {
            Debug.LogError("GameManager: Could not find 'ScoreText'");
            scoreText = null;
        }

        // Find UI for levels beaten
        GameObject levelsBeatenTextObj = GameObject.Find("LevelsBeatenText");
        if (levelsBeatenTextObj != null)
        {
            levelsBeatenText = levelsBeatenTextObj.GetComponent<TMP_Text>();
        }
        else
        {
            Debug.LogWarning("GameManager: Could not find 'LevelsBeatenText' UI element. Levels beaten score will not be displayed.");
            levelsBeatenText = null;
        }

        UpdateScoreUI(); // Initialize score display
        UpdateLevelsBeatenUI(); // Initialize new score display
    }

    // Reset game state variables
    void ResetGameState()
    {
        score = 0;
        levelsBeaten = 0; // Reset new score
        gameOver = false;
        isPaused = false;
        Time.timeScale = 1f; // Ensure time is running
        if (deathPanel != null) deathPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        UpdateScoreUI(); // Reset score display
        UpdateLevelsBeatenUI(); // Reset new score display
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
            CheckForLevelTransition(); // Check for transition after score updates
        }
    }

    private void HandleBossBeatenScore()
    {
        if (!gameOver && !isPaused)
        {
            levelsBeaten += 1;
            UpdateLevelsBeatenUI();
            UpdateScoreUI(); // If main score was also affected
            Debug.Log("Boss Beaten! Levels Beaten: " + levelsBeaten);
        }
    }

    public void ReportObstaclePassed()
    {
        OnObstaclePassedEvent?.Invoke();
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

    public void ReportBossBeaten()
    {
        OnBossBeatenEvent?.Invoke();
    }

    //Method to add score
    public void AddScore(int points)
    {
        if (!gameOver && !isPaused)
        {
            score += points;
            UpdateScoreUI();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !gameOver)
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    // This method is called when the player collides with an obstacle.
    public void OnObstacleCollision()
    {
        gameOver = true;
        // Check if the final score is a new high score.
        if (score > currentHighScore)
        {
            Debug.Log("New High Score! Saving to cloud...");
            currentHighScore = score;
            // Use the singleton to save the new high score.
            CloudSave.Instance.SaveData(playerName, currentHighScore);
        }
        ShowGameOverScreen();
    }

    public void OnObstaclePassed() // Called by ObstacleSpawner.cs
    {
        ReportObstaclePassed();
    }

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
        Time.timeScale = 1f;
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
            Debug.LogError("GameManager: Cannot show Death Panel - reference is missing");
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
    }

    //Level Transition Methods
    private void CheckForLevelTransition()
    {
        if (gameOver || isPaused) return; // Don't transition if game is over or paused

        // Transition 1 -> 2 (Start Scene to Wilderness)
        if (currentActiveSceneName == startSceneName && score >= startSceneScoreThreshold)
        {
            TransitionToLevel(wildernessSceneName);
        }
        
        // Transition from Wilderness to a random level
        else if (currentActiveSceneName == wildernessSceneName && score >= wildernessScoreThreshold)
        {
            TransitionToRandomPlayableLevel();
        }
        // Transition from Longhouse to a random level
        else if (currentActiveSceneName == longhouseSceneName && score >= longhouseScoreThreshold)
        {
            TransitionToRandomPlayableLevel();
        }
    }

    private void TransitionToLevel(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("GameManager: Attempted to transition to an empty scene name.");
            return;
        }
        Debug.Log($"GameManager: Transitioning to scene: {sceneName}");
        SceneManager.LoadScene(sceneName);
    }

    private void TransitionToRandomPlayableLevel()
    {
        if (randomPlayableLevelNames == null || randomPlayableLevelNames.Length == 0)
        {
            Debug.LogError("GameManager: No random playable levels defined in 'randomPlayableLevelNames' array. Please populate it in the Inspector.");
            return;
        }

        int randomIndex = UnityEngine.Random.Range(0, randomPlayableLevelNames.Length);
        string sceneToLoad = randomPlayableLevelNames[randomIndex];
        TransitionToLevel(sceneToLoad);
    }
    // --- End Level Transition Methods ---

    void UpdateLevelsBeatenUI()
    {
        if (levelsBeatenText != null)
        {
            levelsBeatenText.text = "Levels Beaten: " + levelsBeaten;
        }
    }
    
    //Database Logic 
    async void LoadHighScore()
    {
        // Use the singleton to load data.
        PlayerData loadedData = await CloudSave.Instance.LoadData();
        if (loadedData != null)
        {
            currentHighScore = loadedData.HighestScore;
            playerName = loadedData.PlayerName;
            // You could update a UI element here with the high score.
        }
    }

    public void CloseGame()
    {
        Application.Quit();
    }
}