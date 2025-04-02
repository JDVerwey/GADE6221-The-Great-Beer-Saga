using System;
using System.Collections;
using System.Collections.Generic;
using Unity.PlasticSCM.Editor.WebApi;
using Unity.VisualScripting;
using UnityEngine;

public class PickupEffects : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.ApplyBerryEffect(0.5f, 5f); // Halve speed for 15 seconds
                Destroy(gameObject); // Remove berry
                Debug.Log("Berry collected, slowing player");
            }
        }
    }
}
