using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupSpawner : MonoBehaviour
{
    //Declarations
    public GameObject berry; // Stores Berry Prefab
    public Transform player; // Stores Player instance
    public float spawnDistance = 20f; // Spawn distance from player
    public float spawnInterval = 5f; // Time for spawning the pickups 
    private float timer; // Timer for spawning the pickup  

    void Start()
    {
        // Start the timer to initially spawn a pickup
        timer = spawnInterval;
    }

    void Update()
    {
        // Count down the timer 
        timer += Time.deltaTime;
        
        //Check if spawn should happen
        if (timer >= spawnInterval)
        {
            //Random generation for the pickups
            int lane = Random.Range(0, 3);
            //Create spawn positon
            Vector3 spawnPosition = new Vector3(lane * 2f - 2f, 0.5f, player.position.z + spawnDistance);
            //Spawn the berry/pickup 
            Instantiate(berry, spawnPosition, Quaternion.identity);
            //Reset the timer
            timer = 0f;
        }
    }
}
