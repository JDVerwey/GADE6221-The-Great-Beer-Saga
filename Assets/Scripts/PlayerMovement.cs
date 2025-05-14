using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    //Fields for player movement
    public float playerSpeed = 14f;
    public float movementDistance = 2f; // This field seems unused. Consider removing if not needed.
    public float jumpForce = 6f;

    //Fields for our lane system of movement
    private int currentLane = 1; // 0 = left (-2), 1 = middle (0), 2 = right (2)
    private float[] lanePositions = { -2f, 0f, 2f }; // Explicit lane X positions
    private Vector3 targetPosition;
    public float laneSwitchSpeed = 10f; // Speed of lane transition

    //Fields for the jumping of our player
    private Rigidbody rb;
    private bool isGrounded = true;

    // Jump physics improvements
    [Header("Jump Physics Control")]
    public float fallMultiplier = 2.5f;

    //Effect variables 
    private float normalSpeed; // Store original speed
    private float slowTimer; // Timer for slow effect
    private bool isSlowed; // Track slow state

    private ObstacleSpawner obstacleSpawnerIns;

    // Animator reference
    private Animator playerAnimator;
    
    [Header("PowerUps")]
    public GameObject wolfPrefab; // Assign your Wolf Prefab in the Inspector
    private WolfController activeWolfInstance;
    

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody component not found on the player");
            enabled = false;
            return;
        }

        rb.isKinematic = true; // Set to kinematic to avoid physics interference with Translate
        targetPosition = transform.position = new Vector3(lanePositions[currentLane], transform.position.y, transform.position.z);
        normalSpeed = playerSpeed; // Initialize normal speed

        // Get ObstacleSpawner instance
        GameObject spawnerObject = GameObject.Find("ObstacleSpawner");
        if (spawnerObject != null)
        {
            obstacleSpawnerIns = spawnerObject.GetComponent<ObstacleSpawner>();
            if (obstacleSpawnerIns == null)
            {
                Debug.Log("ObstacleSpawner component not found on the 'ObstacleSpawner' GameObject");
            }
        }
        else
        {
            Debug.Log("GameObject with name 'ObstacleSpawner' not found");
        }

        // Get the Animator component
        playerAnimator = GetComponent<Animator>();
        if (playerAnimator == null)
        {
            Debug.Log("Player Animator component not found");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Move forward continuously
        transform.Translate(Vector3.forward * Time.deltaTime * playerSpeed, Space.World);

        // Lane switching
        if (Input.GetKeyDown(KeyCode.A) && currentLane > 0)
        {
            currentLane--;
            UpdateTargetPosition();
        }
        if (Input.GetKeyDown(KeyCode.D) && currentLane < 2)
        {
            currentLane++;
            UpdateTargetPosition();
        }

        // Smoothly move to target lane position
        // Only Lerp the X position to avoid interfering with jump's Y movement
        Vector3 currentPos = transform.position;
        float newX = Mathf.Lerp(currentPos.x, targetPosition.x, Time.deltaTime * laneSwitchSpeed);
        transform.position = new Vector3(newX, currentPos.y, currentPos.z);


        // Jumping when space is pressed
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            if (rb != null)
            {
                rb.isKinematic = false; // Temporarily disable kinematic for jump
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                isGrounded = false;

                // Trigger jump animation
                if (playerAnimator != null)
                {
                    playerAnimator.SetBool("IsJumping", true);
                }
            }
        }

        // Handle slow timer
        if (isSlowed)
        {
            //Start Timer
            slowTimer -= Time.deltaTime;
            if (slowTimer <= 0f)
            {
                //Reset Speed
                playerSpeed = normalSpeed;
                if (obstacleSpawnerIns != null)
                {
                    //Reset Spawn interval
                    obstacleSpawnerIns.spawnInterval *= 0.5f;
                }
                isSlowed = false;
                Debug.Log("Player speed restored to: " + playerSpeed);
            }
        }
    }

    // FixedUpdate is called at a fixed interval and is good for physics calculations
    void FixedUpdate()
    {
        if (rb == null || rb.isKinematic) // Do nothing if no rigidbody or if it's kinematic
            return;

        if (!isGrounded) // Only apply when in the air
        {
            // Apply fall multiplier
            if (rb.linearVelocity.y < 0) // If the player is falling (velocity is negative)
            {
                // We multiply by (fallMultiplier - 1) because gravity is already applying 1x force
                // ForceMode.Acceleration applies an acceleration that ignores mass, good for our gravity-like fall
                rb.AddForce(Vector3.up * Physics.gravity.y * (fallMultiplier - 1), ForceMode.Acceleration);
            }

        }
    }

    private void UpdateTargetPosition()
    {
        // Target position should only care about the X for lanes. Y is handled by jump/gravity.
        targetPosition = new Vector3(lanePositions[currentLane], transform.position.y, transform.position.z);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            if (playerAnimator != null)
            {
                playerAnimator.SetBool("IsJumping", false);
            }
        }
    }

    public void ApplyBerryEffect(float slowMultiplier, float duration)
    {
        if (!isSlowed)
        {
            //Slow the player speed
            playerSpeed *= slowMultiplier;
            slowTimer = duration;
            if (obstacleSpawnerIns != null)
            {
                obstacleSpawnerIns.spawnInterval *= 2f; // Clear way to write "times 2"
            }
            isSlowed = true;
            Debug.Log("Player slowed to: " + playerSpeed);
        }
    }
    
    //Implementation for Wolf Pathfinding pickup 
    public void ActivateWolfPowerUp(float duration)
    {
        Debug.Log("Player picked up Wolf PowerUp");
        if (wolfPrefab == null)
        {
            Debug.LogError("Wolf Prefab not assigned in PlayerMovement script");
            return;
        }

        // Instantiate the wolf if it doesn't exist or isn't active
        // Or, if you implement object pooling, get one from the pool.
        if (activeWolfInstance == null || !activeWolfInstance.gameObject.activeInHierarchy)
        {
            GameObject wolfGO = Instantiate(wolfPrefab); // Position will be set by WolfController
            activeWolfInstance = wolfGO.GetComponent<WolfController>();

            if (activeWolfInstance == null)
            {
                Debug.LogError("WolfController component not found");
                Destroy(wolfGO); // Clean up
                return;
            }
        }
    
        // Activate/Re-activate the wolf
        // Pass necessary player info: transform, duration, current speed, current lane, and lane positions array
        activeWolfInstance.Activate(transform, duration, this.playerSpeed, this.currentLane, this.lanePositions);
    }

}