using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Singleton instance for global access
    public static GameManager Instance { get; private set; }

    // Game state variables
    public static int score = 0;        // Player's current score
    public static bool gameOver = false;// True when run ends
    public UnityEngine.UI.Button resetButton; //instance of the restart button
    public GameObject deathPanel;

    // References (optional, set in Inspector)
    public PlayerMovement player;       // Reference to player for control disabling

    void Awake()
    {
        // Set up singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }
        else
        {
            Destroy(gameObject); // Ensure only one instance exists
        }

        //Setup for the reset button for when the game needs to rest
        if (resetButton != null)
        {
            resetButton.onClick.RemoveAllListeners(); // Good practice before adding
            resetButton.onClick.AddListener(ResetGame);
        }
        if(deathPanel != null)
        {
            deathPanel.SetActive(false);
        }
        
    }

    void Start()
    {
        // Initialize game state
        score = 0;
        gameOver = false;
    }

    //Method to add score 
    public void AddScore(int points)
    {
        if (!gameOver)
        {
            score += points;
            //Update the UI in the method as well
        }
    }

    void Update()
    {

    }

    // Called by ObstacleCollider.cs when player hits an obstacle
    public void OnObstacleCollision()
    {
        gameOver = true; // End run (Rule 5)
        
        //Show Game Over screen 
        deathPanel.SetActive(true);
    }

    // Called by ObstacleSpawner.cs when an obstacle is passed
    public void OnObstaclePassed()
    {
        if (!gameOver)
        {
            score += 1; // Increment score (Rule 11)
            Debug.Log("Score: " + score);
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
        //Reload the scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
