using UnityEngine;

public class LostShieldPickup : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        //Make sure collider is player
        if (other.CompareTag("Player")) 
        {
            PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.ActivateShieldPowerUp();
                Destroy(gameObject); // Destroy the wolf pickup
               
            }
        }
    }
}
