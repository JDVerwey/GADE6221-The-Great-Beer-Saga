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
        //Timer for spawning obstacle
        timer += Time.deltaTime;
        
        //Check if we need to spawn
        if (timer >= spawnInterval)
        {
            //Choose lane to spawn
            int lane = Random.Range(0, 3);
            //Create position to spawn
            Vector3 spawnPosition = new Vector3(lane * 2f - 2f, 0.75f, player.position.z + spawnDistance);
            //Spawn the obstacle 
            GameObject obstacle = Instantiate(obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)], spawnPosition, Quaternion.identity);
            //Ensures obstacle knows in which lane to spawn
            obstacle.GetComponent<Obstacles>().Initialize(lane);
            //Reset timer to spawn new obstsacle
            timer = 0f;
        }
    }
}

