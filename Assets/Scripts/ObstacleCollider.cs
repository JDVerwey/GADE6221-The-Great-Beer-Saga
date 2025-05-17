using UnityEngine;

public class ObstacleCollider : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        //Get Player component 
        PlayerMovement playerMovement = FindObjectOfType<PlayerMovement>();

        //Check if the player collided with obstacle
        if (other.CompareTag("Player"))
        {
            bool endGame = playerMovement.CheckShield();
            if (endGame)
            {
                GameManager.Instance.OnObstacleCollision();
            }
            
        }
    }
}