using UnityEngine;

public class ObstacleCollider : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //GameManager.Instance.AddScore(1); // Score increases by 1 when passing
            GameManager.Instance.OnObstacleCollision();
        }
    }
}