using System.Collections;
using System.Collections.Generic;
using Unity.PlasticSCM.Editor.WebApi;
using Unity.VisualScripting;
using UnityEngine;

public class PickupEffects : MonoBehaviour
{
    public static bool isSlowed = false; // Public for GameManager access
    private float normalSpeed;
    private PlayerMovement playerMovement;
    private float slowTimer;

    void Start()
    {
        playerMovement = FindObjectOfType<PlayerMovement>();
        normalSpeed = playerMovement.playerSpeed;
    }

    void Update()
    {
        if (isSlowed)
        {
            slowTimer -= Time.deltaTime;
            if (slowTimer <= 0f)
            {
                playerMovement.playerSpeed = normalSpeed;
                isSlowed = false;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerMovement.playerSpeed *= 0.5f; // Halve speed
            slowTimer = 15f; // 15 seconds (Rule 24)
            isSlowed = true;
            Destroy(gameObject); // Remove berry
        }
    }
}
