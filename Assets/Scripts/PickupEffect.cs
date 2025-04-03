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
        //Check if colider is with player
        if (other.CompareTag("Player"))
        {
            PlayerMovement playerMovement = other.GetComponent<PlayerMovement>(); // Reference to player
            if (playerMovement != null)
            {
                //Apply the berry Effect
                playerMovement.ApplyBerryEffect(0.5f, 5f); // Halve speed for 5 seconds
                Destroy(gameObject); // Remove berry
                Debug.Log("Berry collected, slowing player");
            }
        }
    }
}
