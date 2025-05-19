// WolfController.cs
using UnityEngine;
using System.Collections;
using System.Collections.Generic; // For List

public class WolfController : MonoBehaviour
{
    //Lane Position
    public float leadDistance = 5f;         // How far ahead of the player the wolf tries to stay
    public float laneSwitchSpeed = 12f;     // How quickly the wolf switches lanes
    private float wolfSpeed;                // Wolf's forward speed, will match player's

    //Lane finding
    private int currentLane = 1;
    private float[] lanePositions;          // Will be synced with player's lane positions
    private Vector3 targetPosition;
    private Transform playerTransform;
    
    public float lookAheadDistance = 15f;   // How far the wolf 'looks' for obstacles
    public LayerMask obstacleLayer;         
    public float pathfindingCheckInterval = 0.2f; // How often to check for a safe path
    
    //Visual trail
    public TrailRenderer trailRenderer;

    //Management variables
    private float activeDuration;
    private float activeTimer;
    private bool isWolfActive = false;
    private Coroutine pathfindingCoroutine;
    
    //Get the player movement script 
    public PlayerMovement playerMovementScript;

    void Awake()
    {
        if (trailRenderer == null)
        {
            trailRenderer = GetComponentInChildren<TrailRenderer>();
        }
        if (trailRenderer != null)
        {
            trailRenderer.emitting = false; // Start with trail off
        }
        gameObject.SetActive(false); // Start inactive, will be activated by player

    }

    public void Activate(Transform player, float duration, float playerCurrentSpeed, int playerCurrentLane, float[] playerLanePositions)
    {
        if (isWolfActive) // If already active, just reset timer
        {
            activeTimer = 0f;
            activeDuration = duration; // Update duration if re-triggered
            return;
        }

        playerTransform = player;
        activeDuration = duration;
        activeTimer = 0f;
        isWolfActive = true;
        this.wolfSpeed = playerCurrentSpeed; // Match player speed initially
        this.lanePositions = playerLanePositions;
        this.currentLane = playerCurrentLane;

        // Position the wolf: player's current lane, leadDistance units ahead
        transform.position = new Vector3(
            lanePositions[currentLane],
            playerTransform.position.y, // Match player's Y, making him level to the ground
            playerTransform.position.z + leadDistance
        );
        targetPosition = transform.position;

        //Set up tail render
        if (trailRenderer != null)
        {
            trailRenderer.Clear();
            trailRenderer.emitting = true;
        }
        gameObject.SetActive(true);

        if (pathfindingCoroutine != null)
        {
            StopCoroutine(pathfindingCoroutine);
        }
        pathfindingCoroutine = StartCoroutine(PathfindingCoroutine());
        Debug.Log("Wolf Activated. Speed: " + wolfSpeed + ", Duration: " + duration);
        
        // Get and store the PlayerMovement script component
        playerMovementScript = player.GetComponent<PlayerMovement>();
        if (playerMovementScript == null)
        {
            Debug.LogError("WolfController: PlayerMovement script not found on the provided player Transform.", player);
            // Optionally, decide if the wolf should still activate or not
            // For now, we'll let it activate but log the error.
        }
    }

    void Update()
    {
        if (!isWolfActive || playerTransform == null)
        {
            if (isWolfActive && playerTransform == null) Deactivate(); // Deactivate if player is gone
            return;
        }

        activeTimer += Time.deltaTime;
        if (activeTimer >= activeDuration)
        {
            Deactivate();
            return;
        }

        // Update wolf's speed if player's speed might change with berry pickup
        // This requires PlayerMovement to expose its current speed
        PlayerMovement pm = playerTransform.GetComponent<PlayerMovement>();
        if (pm != null) {
            wolfSpeed = pm.playerSpeed; // Continuously match player speed
        }


        // Maintain lead distance and forward movement
        float targetZ = playerTransform.position.z + leadDistance;
        transform.position = new Vector3(transform.position.x, transform.position.y, targetZ);

        // Smoothly move to target lane X position
        float newX = Mathf.Lerp(transform.position.x, targetPosition.x, Time.deltaTime * laneSwitchSpeed);
        transform.position = new Vector3(newX, transform.position.y, transform.position.z);
    }

    IEnumerator PathfindingCoroutine()
    {
        while (isWolfActive)
        {
            ChooseSafeLane();
            yield return new WaitForSeconds(pathfindingCheckInterval);
        }
    }

    void ChooseSafeLane()
    {
        if (lanePositions == null || lanePositions.Length == 0) return;

        List<int> potentialSafeLanes = new List<int>();
        // Store distances to obstacles for lanes that are not perfectly clear
        Dictionary<int, float> obstructedLaneDistances = new Dictionary<int, float>();

        for (int i = 0; i < lanePositions.Length; i++)
        {
            // Raycast origin: In the middle of the lane, slightly ahead of the wolf, and slightly above ground
            Vector3 rayOrigin = new Vector3(lanePositions[i], transform.position.y + 0.5f, transform.position.z + 0.2f);
            RaycastHit hit;

            // Visualize the ray in the editor
            Debug.DrawRay(rayOrigin, Vector3.forward * lookAheadDistance, Color.green, pathfindingCheckInterval);

            if (Physics.Raycast(rayOrigin, Vector3.forward, out hit, lookAheadDistance, obstacleLayer))
            {
                // Obstacle detected
                obstructedLaneDistances[i] = hit.distance;
                // Debug.Log($"Wolf: Lane {i} has obstacle '{hit.collider.name}' at {hit.distance}m.");
            }
            else
            {
                // No obstacle detected within lookAheadDistance
                potentialSafeLanes.Add(i);
                // Debug.Log($"Wolf: Lane {i} is clear.");
            }
        }

        int bestLane = currentLane;

        if (potentialSafeLanes.Count > 0)
        {
            // Prefer current lane if it's safe
            if (potentialSafeLanes.Contains(currentLane))
            {
                bestLane = currentLane;
            }
            else
            {
                // Current lane is not safe, pick the closest safe lane
                int closestSafeLane = -1;
                int minLaneDifference = int.MaxValue;
                foreach (int safeLane in potentialSafeLanes)
                {
                    int diff = Mathf.Abs(safeLane - currentLane);
                    if (diff < minLaneDifference)
                    {
                        minLaneDifference = diff;
                        closestSafeLane = safeLane;
                    }
                }
                if (closestSafeLane != -1) bestLane = closestSafeLane;
                else bestLane = potentialSafeLanes[0]; // Fallback to first safe lane
            }
        }
        else if (obstructedLaneDistances.Count > 0)
        {
            // All lanes have obstacles, pick the one with the furthest obstacle
            float maxDistance = -1f;
            foreach (var pair in obstructedLaneDistances)
            {
                if (pair.Value > maxDistance)
                {
                    maxDistance = pair.Value;
                    bestLane = pair.Key;
                }
            }
        }

        //Move to clearn lane if not in it already 
        if (currentLane != bestLane)
        {
            currentLane = bestLane;
            UpdateTargetLanePosition();
        }
    }

    void UpdateTargetLanePosition()
    {
        // Only update the X component for the target position, moving to another lane
        targetPosition = new Vector3(lanePositions[currentLane], transform.position.y, transform.position.z);
    }

    void Deactivate()
    {
        isWolfActive = false;
        if (trailRenderer != null)
        {
            trailRenderer.emitting = false;
        }
        if (pathfindingCoroutine != null)
        {
            StopCoroutine(pathfindingCoroutine);
            pathfindingCoroutine = null;
        }
        gameObject.SetActive(false);
        Debug.Log("Wolf Deactivated.");
        //Hide Wolf GUI 
        playerMovementScript.WolfGuiElement.SetActive(false);
    }
}