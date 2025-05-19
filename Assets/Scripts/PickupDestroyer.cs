using UnityEngine;

public class PickupDestroyer : MonoBehaviour
{
    private Transform player; // Player reference

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
        
    }

    void Update()
    {
        // Only proceed if player exists
        if (player != null)
        {
            if (transform.position.z < player.position.z - 7f)
            {
                Destroy(gameObject); // Destroy object when player is 7 units away from the previos object
            }
        }
    }
}
