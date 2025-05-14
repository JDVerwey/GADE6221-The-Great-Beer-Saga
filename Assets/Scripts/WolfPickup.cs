using UnityEngine;

public class WolfPickup : MonoBehaviour
{
    //Time duration of wolf path
    public float powerUpDuration = 15f;
    void OnTriggerEnter(Collider other)
    {
        //Make sure collider is player
        if (other.CompareTag("Player")) 
        {
            PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.ActivateWolfPowerUp(powerUpDuration);
                Destroy(gameObject); // Destroy the wolf pickup
            }
        }
    }
}