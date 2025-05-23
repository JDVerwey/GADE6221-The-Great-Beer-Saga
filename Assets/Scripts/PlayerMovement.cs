using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    //Fields for player movement
    public float playerSpeed = 14f;
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
    private bool isShieldActive = false;
    private Coroutine flashingCoroutine; 
    public Renderer[] playerRenderers; 
    public float flashInterval = 0.15f; 
    
    [Header("Pickup Tags")]
    public string berryPickupTag = "BerryPickup";
    public string wolfPickupTag = "WolfPickup";
    public string shieldPickupTag = "ShieldPickup";
    
    private GameObject lastTouchedPickup;
    
    [Header("UI Elements")] 
    public GameObject berryGuiElement;
    public GameObject ShieldGuiElement;
    public GameObject WolfGuiElement;
    
    private bool isWolfPowerUpActive = false;
    
    GameManager gameManagerComp;
    BossSpawner BossSpawnerComp;
    private int nextBossSpawnScoreThreshold = 40;


    
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
        targetPosition = transform.position =
            new Vector3(lanePositions[currentLane], transform.position.y, transform.position.z);
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

        //Disable GUI components 
        berryGuiElement.SetActive(false);
        ShieldGuiElement.SetActive(false);
        WolfGuiElement.SetActive(false);

        //Get Game Manager component
        GameObject gameManagerObject = GameObject.Find("GameManager");
        if (gameManagerObject != null)
        {
            gameManagerComp = gameManagerObject.GetComponent<GameManager>();

        }
        
        //Get the BossSpawner component 
        GameObject BossSpawnerObject = GameObject.Find("BossSpawner");
        if (BossSpawnerObject != null)
        {
            BossSpawnerComp = BossSpawnerObject.GetComponent<BossSpawner>();
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
                
                //Hide Berry GUI 
                berryGuiElement.SetActive(false);
            }
        }
    }

    // FixedUpdate is called at a fixed interval and is good for physics calculations
    void FixedUpdate()
    {
        if (rb == null) // Do nothing if no rigidbody or if it's kinematic
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
        
        // Process the last touched pickup, if any
        if (lastTouchedPickup != null)
        {
            GameObject pickupToProcess = lastTouchedPickup;
            lastTouchedPickup = null; // Clear it immediately to handle the next potential pickup

            if (pickupToProcess == null) // Should not happen if collider was disabled, but good check
            {
                return;
            }

            // Identify and process the pickup
            if (pickupToProcess.CompareTag(berryPickupTag))
            {
                ApplyBerryEffect(0.5f, 5f);
                Debug.Log("Berry collected, slowing player (processed in FixedUpdate)", this);
            }
            else if (pickupToProcess.CompareTag(wolfPickupTag))
            {
                WolfPickup wolfPickupScript = pickupToProcess.GetComponent<WolfPickup>();
                if (wolfPickupScript != null)
                {
                    ActivateWolfPowerUp(wolfPickupScript.powerUpDuration);
                }
            }
            else if (pickupToProcess.CompareTag(shieldPickupTag))
            {
                ActivateShieldPowerUp();
            }

            Destroy(pickupToProcess); // Destroy the pickup after processing
        }
        
        //Check score to spawn the boss 
        if (gameManagerComp.GetScore() >= nextBossSpawnScoreThreshold)
        {
            //Spawn the boss 
            BossSpawnerComp.SpawnBoss();
            // Set the next threshold for the boss spawn
            nextBossSpawnScoreThreshold += 40;
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
    
    //For the pickup collection
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(berryPickupTag) ||
            other.CompareTag(wolfPickupTag) ||
            other.CompareTag(shieldPickupTag))
        {
            
            lastTouchedPickup = other.gameObject;
            other.enabled = false; // Disable the collider of the pickup to prevent re-triggering
        }
    }

    public bool CheckShield()
        {
                if (isShieldActive)
                {
                    Debug.Log("Shield absorbed obstacle hit!");
                    isShieldActive = false; // Consume the shield
                    StopFlashingEffect();   // Stop the visual effect
                    //Hide GUI 
                    ShieldGuiElement.SetActive(false);
                    // Indicate to not end the game 
                    return false; 
                }
                else
                {
                    // Player hit an obstacle without a shield - Handle Game Over
                    Debug.Log("Player hit obstacle! Game Over.");
                    // End Game
                    return true;
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
            //Show Berry GUI 
            berryGuiElement.SetActive(true);
            Debug.Log("Player slowed to: " + playerSpeed);
        }
    }
    
//Implementation for Wolf Pathfinding pickup
    public void ActivateWolfPowerUp(float duration)
    {
        Debug.Log("Player picked up Wolf PowerUp", this);
        if (wolfPrefab == null)
        {
            Debug.LogError("Wolf Prefab not assigned in PlayerMovement script", this);
            return;
        }

        // Instantiate the wolf if it doesn't exist or isn't active
        if (activeWolfInstance == null || !activeWolfInstance.gameObject.activeInHierarchy)
        {
            GameObject wolfGO = Instantiate(wolfPrefab);
            activeWolfInstance = wolfGO.GetComponent<WolfController>();

            if (activeWolfInstance == null)
            {
                Debug.Log("WolfController component not found on instantiated wolfPrefab", wolfGO);
                Destroy(wolfGO); // Clean up
                return;
            }
        }

        // Activate/Re-activate the wolf
        activeWolfInstance.Activate(transform, duration, this.playerSpeed, this.currentLane, this.lanePositions);
        isWolfPowerUpActive = true;
        if (WolfGuiElement != null) WolfGuiElement.SetActive(true);
    }


    public void ActivateShieldPowerUp()
    {
        if (!isShieldActive) // Only activate if not already active, or re-activate
        {
            Debug.Log("Shield PowerUp Activated!");
            isShieldActive = true;
            //Show Shield GUI 
            ShieldGuiElement.SetActive(true);
            StartFlashingEffect();
        }
    }

    private void StartFlashingEffect()
    {
        //Error check if player renderer not found
        if (playerRenderers == null || playerRenderers.Length == 0) return;

        if (flashingCoroutine != null)
        {
            StopCoroutine(flashingCoroutine);
        }
        flashingCoroutine = StartCoroutine(FlashingEffectCoroutine());
    }

    private void StopFlashingEffect()
    {
        if (playerRenderers == null || playerRenderers.Length == 0) return;

        if (flashingCoroutine != null)
        {
            StopCoroutine(flashingCoroutine);
            flashingCoroutine = null;
        }
        // Ensure all renderers are visible when effect stops
        foreach (Renderer rend in playerRenderers)
        {
            if (rend != null) rend.enabled = true;
        }
        //Hide Shield GUI 
        ShieldGuiElement.SetActive(true);
    }

    IEnumerator FlashingEffectCoroutine()
    {
        if (playerRenderers == null || playerRenderers.Length == 0)
        {
            Debug.Log("Cannot start flashing effect: Player Renderers not assigned or found.");
            yield break; // Exit coroutine if no renderers
        }

        // Ensure renderers are initially visible
        foreach (Renderer rend in playerRenderers)
        {
            if (rend != null) rend.enabled = true;
        }

        while (isShieldActive)
        {
            // Toggle visibility
            foreach (Renderer rend in playerRenderers)
            {
                if (rend != null) rend.enabled = !rend.enabled;
            }
            yield return new WaitForSeconds(flashInterval);
        }

        // Ensure renderers are visible once the shield is no longer active
        foreach (Renderer rend in playerRenderers)
        {
            if (rend != null) rend.enabled = true;
        }
        flashingCoroutine = null; // Clear the coroutine reference
    }
}