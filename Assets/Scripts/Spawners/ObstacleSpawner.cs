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
    public float minClearanceRadius = 4f;
    public LayerMask obstacleLayer;

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

            // --- Overlap Check using SphereCast ---
            // Check if the potential spawn position is clear of other obstacles on the specified layer
            // QueryTriggerInteraction.Ignore ensures that trigger colliders don't block spawning
            if (!Physics.CheckSphere(spawnPosition, minClearanceRadius, obstacleLayer, QueryTriggerInteraction.Ignore))
            {
                // If the area is clear, then spawn the obstacle
                // First, select the prefab
                GameObject prefabToSpawn = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];

                // Then, instantiate it using its own rotation
                GameObject obstacle = Instantiate(prefabToSpawn, spawnPosition, prefabToSpawn.transform.rotation);
                
                //Ensures obstacle knows in which lane to spawn
                Obstacles obstacleScript = obstacle.GetComponent<Obstacles>();
                if (obstacleScript != null)
                {
                    obstacleScript.Initialize(lane);
                }

            }
            // If Physics.CheckSphere returns true, it means the area is NOT clear,
            // so we simply do nothing and don't spawn an obstacle in this attempt.

            //Reset timer for the next spawn attempt interval (happens whether we spawned or not)
            timer = 0f;
        }
    }
}

