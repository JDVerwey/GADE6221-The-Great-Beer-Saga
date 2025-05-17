using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupSpawner : MonoBehaviour
{
    //Declarations
    public GameObject[] pickupPrefabs; // Stores an array of Pickup Prefabs
    public Transform player; // Stores Player instance
    public float spawnDistance = 20f; // Spawn distance from player
    public float spawnInterval = 5f; // Time for spawning the pickups
    private float timer; // Timer for spawning the pickup

    [Header("Overlap Prevention")]
    public float minClearanceRadius = 1f; // Radius to check for clearance around the pickup
    public LayerMask obstacleLayer;      // Layer(s) that obstacles are on

    void Start()
    {
        // Validate pickupPrefabs array
        if (pickupPrefabs == null || pickupPrefabs.Length == 0)
        {
            Debug.LogError("Pickup Prefabs array is not assigned or is empty in the PickupSpawner. Disabling spawner.");
            enabled = false; // Disable this script if no prefabs are set
            return;
        }

        // Validate if obstacleLayer is set, if not, it might not work as expected
        if (obstacleLayer == 0) // LayerMask value is 0 if nothing is selected
        {
            Debug.LogWarning("Obstacle Layer is not set in the PickupSpawner. Overlap check might not work correctly for pickups.");
        }


        // Start the timer to initially spawn a pickup
        timer = spawnInterval;
    }

    void Update()
    {
        // Count up the timer
        timer += Time.deltaTime;

        //Check if spawn should happen
        if (timer >= spawnInterval)
        {
            // Ensure player is assigned
            if (player == null)
            {
                Debug.LogWarning("Player transform not assigned to PickupSpawner. Cannot spawn pickup.");
                timer = 0f; // Reset timer to avoid constant warnings
                return;
            }

            //Random generation for the lane
            int lane = Random.Range(0, 3); // Assumes 3 lanes: 0, 1, 2
            //Create spawn position
            // The Y value (0.5f) might need adjustment based on your pickup's pivot point and size
            Vector3 spawnPosition = new Vector3(lane * 2f - 2f, 0.5f, player.position.z + spawnDistance);

            // --- Overlap Check ---
            // Check if the potential spawn position is clear of obstacles
            if (!Physics.CheckSphere(spawnPosition, minClearanceRadius, obstacleLayer, QueryTriggerInteraction.Ignore))
            {
                // If the area is clear, then proceed to spawn the pickup

                // Randomly select a pickup from the array
                int randomIndex = Random.Range(0, pickupPrefabs.Length);
                GameObject pickupToSpawn = pickupPrefabs[randomIndex];

                if (pickupToSpawn != null)
                {
                    //Spawn the selected pickup
                    Instantiate(pickupToSpawn, spawnPosition, Quaternion.identity);
                    // Debug.Log($"Spawned pickup: {pickupToSpawn.name} at {spawnPosition}"); // Optional: for testing
                }
                else
                {
                    Debug.LogWarning($"Pickup prefab at index {randomIndex} is null. Skipping spawn.");
                }
            }

            //Reset the timer (happens whether we spawned or not, to maintain spawn interval)
            timer = 0f;
        }
    }
}