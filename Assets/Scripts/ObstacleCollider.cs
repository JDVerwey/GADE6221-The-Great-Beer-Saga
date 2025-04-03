using UnityEngine;

public class ObstacleCollider : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        //Check if the player collided with obstacle
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.OnObstacleCollision();
        }
    }
}