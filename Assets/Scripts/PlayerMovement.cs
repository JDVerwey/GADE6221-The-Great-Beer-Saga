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

    //Fields for the jumping of our player, using the collision-based method
    private Rigidbody rb;
    private bool isGrounded = true; // Start as grounded

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
    public GameObject wolfPrefab;
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
    
    BossSpawner BossSpawnerComp;
    private int nextBossSpawnScoreThreshold = 40;

    // Fields for lane switch rotation
    [Header("Lane Switch Rotation")]
    public float maxRotationAngle = 20f; // Max angle to rotate during lane switch
    public float rotationSpeed = 10f;   // Speed of rotation
    private Quaternion targetRotation;  // Target rotation for the player



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

        // The rigidbody is kinematic by default, physics only take over during a jump.
        rb.isKinematic = true;
        targetPosition = transform.position =
            new Vector3(lanePositions[currentLane], transform.position.y, transform.position.z);
        normalSpeed = playerSpeed;

        GameObject spawnerObject = GameObject.Find("ObstacleSpawner");
        if (spawnerObject != null)
            obstacleSpawnerIns = spawnerObject.GetComponent<ObstacleSpawner>();
        else
            Debug.Log("GameObject with name 'ObstacleSpawner' not found");

        playerAnimator = GetComponent<Animator>();
        if (playerAnimator == null) Debug.Log("Player Animator component not found");

        if (berryGuiElement != null) berryGuiElement.SetActive(false);
        if (ShieldGuiElement != null) ShieldGuiElement.SetActive(false);
        if (WolfGuiElement != null) WolfGuiElement.SetActive(false);

        GameObject BossSpawnerObject = GameObject.Find("BossSpawner");
        if (BossSpawnerObject != null)
            BossSpawnerComp = BossSpawnerObject.GetComponent<BossSpawner>();
        else
            Debug.LogError("PlayerMovement: Could not find 'BossSpawner' GameObject.");


        targetRotation = Quaternion.identity;
    }

    void Update()
    {
        // --- Player Input ---

        // Jumping when space is pressed (using the collision-based isGrounded check)
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            if (rb != null)
            {
                rb.isKinematic = false; // Temporarily disable kinematic for jump
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                isGrounded = false; // We are now in the air

                // Trigger jump animation
                if (playerAnimator != null)
                {
                    playerAnimator.SetBool("IsJumping", true);
                }
            }
        }
        
        // Lane switching input
        if (Input.GetKeyDown(KeyCode.A) && currentLane > 0)
        {
            currentLane--;
            UpdateTargetPosition();
            targetRotation = Quaternion.Euler(0, -maxRotationAngle, 0);
        }
        if (Input.GetKeyDown(KeyCode.D) && currentLane < 2)
        {
            currentLane++;
            UpdateTargetPosition();
            targetRotation = Quaternion.Euler(0, maxRotationAngle, 0);
        }

        // --- Movement and Rotation ---

        // Move forward continuously
        transform.Translate(Vector3.forward * Time.deltaTime * playerSpeed, Space.World);
        
        // Smoothly move to target lane position
        Vector3 currentPos = transform.position;
        float newX = Mathf.Lerp(currentPos.x, targetPosition.x, Time.deltaTime * laneSwitchSpeed);
        
        // The Y-axis is now controlled by the Rigidbody during jumps, so we don't need special handling here.
        // We can directly set the position because the Rigidbody is kinematic when on the ground.
        transform.position = new Vector3(newX, transform.position.y, currentPos.z);

        // Logic to straighten rotation when in lane
        if (Mathf.Abs(transform.position.x - targetPosition.x) < 0.05f)
        {
            if (targetRotation != Quaternion.identity)
            {
                targetRotation = Quaternion.identity;
            }
        }
        // Smoothly rotate player towards the targetRotation
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);


        // --- Effects ---
        
        // Handle slow timer
        if (isSlowed)
        {
            slowTimer -= Time.deltaTime;
            if (slowTimer <= 0f)
            {
                playerSpeed = normalSpeed;
                if (obstacleSpawnerIns != null) obstacleSpawnerIns.spawnInterval *= 0.5f;
                isSlowed = false;
                if (berryGuiElement != null) berryGuiElement.SetActive(false);
            }
        }
    }

    void FixedUpdate()
    {
        if (rb == null) return;

        // Apply fall multiplier only when in the air
        if (!isGrounded) 
        {
            if (rb.linearVelocity.y < 0)
            {
                rb.AddForce(Vector3.up * Physics.gravity.y * (fallMultiplier - 1), ForceMode.Acceleration);
            }
        }

        // Handle pickup logic
        if (lastTouchedPickup != null)
        {
            GameObject pickupToProcess = lastTouchedPickup;
            lastTouchedPickup = null;

            if (pickupToProcess == null) return;

            if (pickupToProcess.CompareTag(berryPickupTag))
            {
                ApplyBerryEffect(0.5f, 5f);
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
            Destroy(pickupToProcess);
        }

        //Check score to spawn the boss
        if (GameManager.Instance != null && BossSpawnerComp != null &&
            GameManager.Instance.GetScore() >= nextBossSpawnScoreThreshold)
        {
            BossSpawnerComp.SpawnBoss();
            GameManager.Instance.ReportBossSpawned(); // Invoke the event
            nextBossSpawnScoreThreshold += 40; // Or some other logic for next threshold
        }
    }

    private void UpdateTargetPosition()
    {
        // We only update the target for the X position (lanes). Y is handled by physics.
        targetPosition = new Vector3(lanePositions[currentLane], transform.position.y, transform.position.z);
    }

    // This method handles all ground detection logic.
    void OnCollisionEnter(Collision collision)
    {
        // Check if the player has landed on an object tagged "Ground".
        if (collision.gameObject.CompareTag("Ground"))
        {
            // Only consider it a true landing if the player is not moving upwards.
            // This prevents the jump from being cancelled in the same frame it's initiated.
            if (rb.linearVelocity.y <= 0.1f)
            {
                isGrounded = true;
                rb.isKinematic = true; // Return to being kinematic upon landing.

                if (playerAnimator != null)
                {
                    playerAnimator.SetBool("IsJumping", false);
                }
            }
        }
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(berryPickupTag) ||
            other.CompareTag(wolfPickupTag) ||
            other.CompareTag(shieldPickupTag))
        {
            lastTouchedPickup = other.gameObject;
            other.enabled = false;
        }
    }

    public bool CheckShield()
    {
        if (isShieldActive)
        {
            isShieldActive = false;
            StopFlashingEffect();
            if (ShieldGuiElement != null) ShieldGuiElement.SetActive(false);
            return false;
        }
        return true;
    }

    public void ApplyBerryEffect(float slowMultiplier, float duration)
    {
        if (!isSlowed)
        {
            playerSpeed *= slowMultiplier;
            slowTimer = duration;
            if (obstacleSpawnerIns != null) obstacleSpawnerIns.spawnInterval *= 2f;
            isSlowed = true;
            if (berryGuiElement != null) berryGuiElement.SetActive(true);
            GameManager.Instance?.ReportPickupActivated(GameManager.PickupType.Berry); // Invoke event
        }
    }

    public void ActivateWolfPowerUp(float duration)
    {
        if (wolfPrefab == null)
        {
            Debug.LogError("Wolf Prefab not assigned!");
            return;
        }

        if (activeWolfInstance == null || !activeWolfInstance.gameObject.activeInHierarchy)
        {
            GameObject wolfGO = Instantiate(wolfPrefab);
            activeWolfInstance = wolfGO.GetComponent<WolfController>();
            if (activeWolfInstance == null)
            {
                Destroy(wolfGO);
                return;
            }
        }
        activeWolfInstance.Activate(transform, duration, this.playerSpeed, this.currentLane, this.lanePositions);
        if (WolfGuiElement != null) WolfGuiElement.SetActive(true);
        GameManager.Instance?.ReportPickupActivated(GameManager.PickupType.Wolf); // Invoke event
    }

    public void ActivateShieldPowerUp()
    {
        if (!isShieldActive)
        {
            isShieldActive = true;
            if (ShieldGuiElement != null) ShieldGuiElement.SetActive(true);
            StartFlashingEffect();
            GameManager.Instance?.ReportPickupActivated(GameManager.PickupType.Shield); // Invoke event
        }
    }

    private void StartFlashingEffect()
    {
        if (playerRenderers == null || playerRenderers.Length == 0) return;
        if (flashingCoroutine != null) StopCoroutine(flashingCoroutine);
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
        foreach (Renderer rend in playerRenderers)
        {
            if (rend != null) rend.enabled = true;
        }
        if (ShieldGuiElement != null) ShieldGuiElement.SetActive(isShieldActive);
    }

    IEnumerator FlashingEffectCoroutine()
    {
        if (playerRenderers == null || playerRenderers.Length == 0) yield break;
        foreach (Renderer rend in playerRenderers)
        {
            if (rend != null) rend.enabled = true;
        }
        while (isShieldActive)
        {
            foreach (Renderer rend in playerRenderers)
            {
                if (rend != null) rend.enabled = !rend.enabled;
            }
            yield return new WaitForSeconds(flashInterval);
        }
        foreach (Renderer rend in playerRenderers)
        {
            if (rend != null) rend.enabled = true;
        }
        flashingCoroutine = null;
    }
}
