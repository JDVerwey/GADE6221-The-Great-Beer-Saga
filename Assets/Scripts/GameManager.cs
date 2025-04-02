using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;

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
    public PlayerMovement player;       // Reference to player for control disabling (optional)

    void Awake()
    {
        // Set up singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes (optional)
        }
        else
        {
            Destroy(gameObject); // Ensure only one instance exists
        }

        //Resets the game when the reset button is clicked
        if (resetButton != null)
            resetButton.onClick.AddListener(ResetGame);
        //deactivates the game over screen
        deathPanel.SetActive(false);
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
        // Handle game over state
        if (gameOver)
        {
            //Show Game Over screen 
            deathPanel.SetActive(true);
        }
    }

    // Called by ObstacleCollider.cs when player hits an obstacle
    public void OnObstacleCollision()
    {
        gameOver = true; // End run (Rule 5)
        Debug.Log("Game Over");
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

    // Reset game state (e.g., for restart)
    public void ResetGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
