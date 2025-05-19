using UnityEngine;

public class BossSpawner : MonoBehaviour
{
    [Header("Boss Configuration")]
    public GameObject bossPrefab;
    public Transform player;

    [Header("Spawning Settings")]
    public float spawnDistance = 50f;
    private float timer;
    public float spawnYPosition = 1.9f; // Added for Y-position control

    [Header("Size Settings")]
    public float initialBossSize = 1.0f;
    public float sizeIncreasePerSpawn = 0.5f;
    private float currentBossSizeMultiplier;

    private GameObject activeBossInstance;

    // Lane positions, similar to PlayerMovement and ObstacleSpawner
    private readonly float[] laneXPositions = { 0f, 1f, 2f };


    void Start()
    {
        currentBossSizeMultiplier = initialBossSize;

        if (bossPrefab == null)
        {
            Debug.LogError("Boss Prefab is not assigned in the BossSpawner. Disabling spawner.", this);
            enabled = false;
            return;
        }
        if (player == null)
        {
            Debug.LogWarning("Player Transform is not assigned in the BossSpawner. Attempting to find player by tag.", this);
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                Debug.Log("BossSpawner: Player Transform found via tag 'Player'.", this);
            }
            else
            {
                Debug.LogError("BossSpawner: Player Transform could not be found. Boss spawning might not work as expected.", this);
            }
        }
    }

    void Update()
    {
    }

    public void SpawnBoss()
    {
        if (bossPrefab == null) // Added safety check
        {
            Debug.LogError("Boss Prefab is null, cannot spawn boss.", this);
            return;
        }

        if (activeBossInstance != null)
        {
            Debug.Log("Destroying previous boss instance.", this);
            Destroy(activeBossInstance);
            activeBossInstance = null;
        }

        //WCalculate Spawn Position with Randomized Lane
        float spawnZ = (player != null) ? player.position.z + spawnDistance : transform.position.z + spawnDistance;

        // Randomly select a lane index (0, 1, or 2)
        int laneIndex = Random.Range(0, laneXPositions.Length);
        // Get the X position for the selected lane
        float spawnX = laneXPositions[laneIndex];

        // Use the configurable spawnYPosition
        Vector3 spawnPosition = new Vector3(spawnX, spawnYPosition, spawnZ);

        // Increase the size multiplier for the new boss
        // If size should only increase after a boss is *replaced*, this condition might need adjustment.
        currentBossSizeMultiplier += sizeIncreasePerSpawn;
        // Ensure size doesn't go below initial size if sizeIncreasePerSpawn was negative or currentBossSizeMultiplier started lower.
        currentBossSizeMultiplier = Mathf.Max(currentBossSizeMultiplier, initialBossSize);
        Debug.Log($"Spawning Boss in lane {laneIndex} (X: {spawnX}). New size multiplier: {currentBossSizeMultiplier}", this);


        activeBossInstance = Instantiate(bossPrefab, spawnPosition, Quaternion.identity);
        activeBossInstance.transform.localScale = Vector3.one * currentBossSizeMultiplier;
    }
}