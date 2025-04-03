using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundManager : MonoBehaviour
{
    //fields
    // Ground segment prefab (assign in Inspector)
    public GameObject groundPrefab;
    // Length of each ground segment along Z-axis
    public float segmentLength = 27.8f;
    // Number of segments to keep ahead of player
    public int segmentsAhead = 3;
    // Number of segments to keep behind player
    public int segmentsBehind = 1;
    // Reference to player
    private Transform player;
    // Z-position of the last spawned segment
    private float lastSpawnZ;
    // Player�s starting Z-position
    private float playerStartZ;

    // Start is called before the first frame update
    void Start()
    {
        //Find the player
        player = FindObjectOfType<PlayerMovement>().transform;
        if (player == null)
        {
            Debug.LogError("Player not found for GroundManager");
            return;
        }
        //assign the players z position in player start z
        playerStartZ = player.position.z;
        //assign player start z to last spawn z
        lastSpawnZ = playerStartZ;

        // Spawn initial segments
        for (int i = 0; i < segmentsAhead; i++)
        {
            SpawnGroundSegment();
        }
    }

    // Update is called once per frame
    void Update()
    {//If the game is over or paused then it stops the update method from running
        if (GameManager.gameOver || GameManager.isPaused) return;

        // Spawn new segment if player is close to the end of the current ground
        if (player.position.z + (segmentsAhead * segmentLength) > lastSpawnZ)
        {
            SpawnGroundSegment();
        }

        // Destroy segments too far behind
        foreach (Transform segment in transform)
        {
            //if the position of the ground segment is less than the position fo the player - segment behind* the length of the segment
            if (segment.position.z < player.position.z - (segmentsBehind * segmentLength))
            {
                //destroys the segment behind the player
                Destroy(segment.gameObject);
            }
        }
    }
    //Spawns the ground segments
    void SpawnGroundSegment()
    {
        //set the spawn position
        Vector3 spawnPos = new Vector3(0f, 0f, lastSpawnZ);
        //Rotate the platform to be in the correct orientation according to a rotation Quaternion 
        Quaternion spawnRotation = Quaternion.Euler(0f, 90f, 0f);
        //instantiate the ground and save it as the newSegment game object
        GameObject newSegment = Instantiate(groundPrefab, spawnPos, spawnRotation);
        // Organize under GroundManager
        newSegment.transform.SetParent(transform);
        // Update the last spawn position
        lastSpawnZ += segmentLength;
    }
}
