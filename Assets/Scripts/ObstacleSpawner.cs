using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    public GameObject[] obstaclePrefabs;
    public Transform player;
    public float spawnDistance = 20f;
    public float spawnInterval = 5f;
    private float timer;

    void Start() { timer = spawnInterval; }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            int lane = Random.Range(0, 3);
            Vector3 spawnPosition = new Vector3(lane * 2f - 2f, 0f, player.position.z + spawnDistance);
            GameObject obstacle = Instantiate(obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)], spawnPosition, Quaternion.identity);
            obstacle.GetComponent<Obstacles>().Initialize(lane);
            timer = 0f;
        }
    }
}

