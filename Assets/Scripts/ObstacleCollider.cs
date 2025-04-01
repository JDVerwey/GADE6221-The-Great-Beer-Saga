using UnityEngine;

public class ObstacleCollider : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            GameManager.Instance.OnObstacleCollision();
    }
}
