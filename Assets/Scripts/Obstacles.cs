using UnityEngine;

public class Obstacles : MonoBehaviour
{
    public int lane; // Lane to keep track of the position of obstacle
    private Transform player; // Player reference
    private bool scored = false; // boolean to keep track of score

    void Start()
    {
        // Attempt to find PlayerMovement, log if not found
        PlayerMovement playerMovement = FindObjectOfType<PlayerMovement>();
        if (playerMovement != null)
        {
            player = playerMovement.transform;
        }
        else
        {
            Debug.LogError("PlayerMovement not found in scene");
        }

        // Set initial position based on lane
        transform.position = new Vector3(lane * 2f - 2f, transform.position.y, transform.position.z);
    }

    void Update()
    {
        // Only proceed if player exists
        if (player != null)
        {
            if (transform.position.z < player.position.z && !scored)
            {
                // Check if GameManager exists before calling
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.OnObstaclePassed(); //Add score when passed
                    scored = true;
                }
                else
                {
                    Debug.LogError("GameManager.Instance is null");
                }
            }
            if (transform.position.z < player.position.z - 7f)
            {
                Destroy(gameObject); // Destroy object when player is 7 units away from the previos object
            }
        }
    }

    public void Initialize(int assignedLane)
    {
        lane = assignedLane;
    }
}