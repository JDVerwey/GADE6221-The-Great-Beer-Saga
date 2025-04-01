using UnityEngine;

public class Obstacles : MonoBehaviour
{
    public int lane;
    private Transform player;
    private bool scored = false;

    void Start()
    {
        player = FindObjectOfType<PlayerMovement>().transform;
        transform.position = new Vector3(lane * 2f - 2f, transform.position.y, transform.position.z);
    }

    void Update()
    {
        if (transform.position.z < player.position.z && !scored)
        {
            GameManager.Instance.OnObstaclePassed();
            scored = true;
        }
        if (transform.position.z < player.position.z - 10f)
            Destroy(gameObject);
    }

    public void Initialize(int assignedLane) { lane = assignedLane; }
}