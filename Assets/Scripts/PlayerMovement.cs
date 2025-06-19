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

    private bool isWolfPowerUpActive = false; // This might be redundant if activeWolfInstance is checked

    // GameManager gameManagerComp; // Can be removed if only using GameManager.Instance
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

        // gameManagerComp = GameManager.Instance; // Access via singleton if needed, but direct calls are fine

        GameObject BossSpawnerObject = GameObject.Find("BossSpawner");
        if (BossSpawnerObject != null)
            BossSpawnerComp = BossSpawnerObject.GetComponent<BossSpawner>();
        else
            Debug.LogError("PlayerMovement: Could not find 'BossSpawner' GameObject.");


        targetRotation = Quaternion.identity;
    }

    void Update()
    {
        transform.Translate(Vector3.forward * Time.deltaTime * playerSpeed, Space.World);

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

        Vector3 currentPos = transform.position;
        float newX = Mathf.Lerp(currentPos.x, targetPosition.x, Time.deltaTime * laneSwitchSpeed);
        transform.position = new Vector3(newX, currentPos.y, currentPos.z);

        if (Mathf.Abs(transform.position.x - targetPosition.x) < 0.05f)
        {
            if (targetRotation != Quaternion.identity)
            {
                targetRotation = Quaternion.identity;
            }
        }
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                isGrounded = false;
                if (playerAnimator != null) playerAnimator.SetBool("IsJumping", true);
            }
        }

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

        if (!isGrounded && !rb.isKinematic) // Apply fall multiplier only when jumping/falling
        {
            if (rb.linearVelocity.y < 0)
            {
                rb.AddForce(Vector3.up * Physics.gravity.y * (fallMultiplier - 1), ForceMode.Acceleration);
            }
        }

        if (lastTouchedPickup != null)
        {
            GameObject pickupToProcess = lastTouchedPickup;
            lastTouchedPickup = null;

            if (pickupToProcess == null) return;

            if (pickupToProcess.CompareTag(berryPickupTag))
            {
                ApplyBerryEffect(0.5f, 5f); // This will now also call GameManager
            }
            else if (pickupToProcess.CompareTag(wolfPickupTag))
            {
                WolfPickup wolfPickupScript = pickupToProcess.GetComponent<WolfPickup>();
                if (wolfPickupScript != null)
                {
                    ActivateWolfPowerUp(wolfPickupScript.powerUpDuration); // This will now also call GameManager
                }
            }
            else if (pickupToProcess.CompareTag(shieldPickupTag))
            {
                ActivateShieldPowerUp(); // This will now also call GameManager
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
        targetPosition = new Vector3(lanePositions[currentLane], transform.position.y, transform.position.z);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            if (rb != null && !rb.isKinematic) rb.isKinematic = true; // Make kinematic again
            if (playerAnimator != null) playerAnimator.SetBool("IsJumping", false);
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
        // isWolfPowerUpActive = true; // activeWolfInstance can determine its own active state
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
        // ShieldGuiElement should be set based on isShieldActive, not always true here
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