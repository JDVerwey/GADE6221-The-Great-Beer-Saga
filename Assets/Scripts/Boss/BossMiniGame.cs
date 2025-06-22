using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Required for UI elements like Text

public class ButtonMasher : MonoBehaviour
{
    [Header("Minigame Configuration")]
    public float totalDuration = 15f; // Total time for the minigame
    public float keySwapInterval = 5f; // Time until the key to mash changes
    public int requiredMashRate = 3; // Required mashes per second

    [Header("UI Elements")]
    public Text keyToPressText; // UI Text to display the key to mash
    public Text timerText; // UI Text to display the remaining time
    public Text mashCountText; // UI Text to display the current mash count

    [Header("Screen Shake")]
    public float shakeDuration = 0.1f;
    public float shakeMagnitude = 0.1f;
    private Vector3 initialCameraPosition;
    private Camera mainCamera;

    private KeyCode currentKey;
    private int mashCount = 0;
    private float timeRemaining;
    private bool minigameActive = false;

    // List of keys that can be chosen for the minigame
    private readonly KeyCode[] possibleKeys = {
        KeyCode.A, KeyCode.B, KeyCode.C, KeyCode.D, KeyCode.E, KeyCode.F, KeyCode.G, KeyCode.H,
        KeyCode.I, KeyCode.J, KeyCode.K, KeyCode.L, KeyCode.M, KeyCode.N, KeyCode.O, KeyCode.P,
        KeyCode.Q, KeyCode.R, KeyCode.S, KeyCode.T, KeyCode.U, KeyCode.V, KeyCode.W, KeyCode.X,
        KeyCode.Y, KeyCode.Z
    };

    void Start()
    {
        // Disable the script on start, it will be enabled when the boss fight begins
        enabled = false;
        mainCamera = Camera.main;
    }

    // Call this method to start the minigame
    public void StartMinigame()
    {
        enabled = true;
        minigameActive = true;
        mashCount = 0;
        timeRemaining = totalDuration;
        initialCameraPosition = mainCamera.transform.position;
        StartCoroutine(MinigameRoutine());
        UpdateUI();
    }

    void Update()
    {
        if (!minigameActive) return;

        // Check for player input
        if (Input.GetKeyDown(currentKey))
        {
            mashCount++;
            StartCoroutine(ShakeScreen());
            UpdateUI();
        }

        // Update timer
        timeRemaining -= Time.deltaTime;
        if (timeRemaining <= 0)
        {
            timeRemaining = 0;
            EndMinigame();
        }
        UpdateUI();
    }

    private IEnumerator MinigameRoutine()
    {
        float keySwapTimer = keySwapInterval;

        while (minigameActive)
        {
            // Set a new random key at the start and every interval
            if (keySwapTimer >= keySwapInterval)
            {
                SetNewRandomKey();
                keySwapTimer = 0f;
            }

            keySwapTimer += Time.deltaTime;
            yield return null;
        }
    }

    private void SetNewRandomKey()
    {
        currentKey = possibleKeys[Random.Range(0, possibleKeys.Length)];
    }

    private void EndMinigame()
    {
        minigameActive = false;

        // Check for win/loss condition
        float averageMashRate = (float)mashCount / totalDuration;
        if (averageMashRate >= requiredMashRate)
        {
            Debug.Log("Minigame Won!");
            // Add your win logic here (e.g., boss defeated)
        }
        else
        {
            Debug.Log("Minigame Lost!");
            // Add your loss logic here (e.g., player takes damage)
        }

        // Reset UI and disable the script
        keyToPressText.text = "";
        timerText.text = "";
        mashCountText.text = "";
        enabled = false;
    }

    private void UpdateUI()
    {
        if(keyToPressText) keyToPressText.text = "Mash: " + currentKey.ToString();
        if(timerText) timerText.text = "Time: " + timeRemaining.ToString("F2");
        if(mashCountText) mashCountText.text = "Mashes: " + mashCount;
    }

    private IEnumerator ShakeScreen()
    {
        float elapsed = 0.0f;

        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            float y = Random.Range(-1f, 1f) * shakeMagnitude;

            mainCamera.transform.position = new Vector3(initialCameraPosition.x + x, initialCameraPosition.y + y, initialCameraPosition.z);
            elapsed += Time.deltaTime;

            yield return null;
        }

        mainCamera.transform.position = initialCameraPosition;
    }

    // Example of how to trigger the minigame
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Boss"))
        {
            StartMinigame();
        }
    }
}
