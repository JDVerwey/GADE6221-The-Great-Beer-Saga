using UnityEngine;

public class Obstacles : MonoBehaviour
{
    public int lane;
    private Transform player;
    private bool scored = false;

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
            Debug.LogError("PlayerMovement not found in scene!");
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
                    GameManager.Instance.OnObstaclePassed();
                    scored = true;
                }
                else
                {
                    Debug.LogError("GameManager.Instance is null!");
                }
            }
            if (transform.position.z < player.position.z - 10f)
            {
                Destroy(gameObject);
            }
        }
    }

    public void Initialize(int assignedLane)
    {
        lane = assignedLane;
    }
}