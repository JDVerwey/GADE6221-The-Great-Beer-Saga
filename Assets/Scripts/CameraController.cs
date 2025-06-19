using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Get the player transform
    public Transform player;

    // the distance we want to keep from the player
    private Vector3 offset;

    void Start()
    {
        // Make sure the player is received
        if (player == null)
        {
            Debug.LogError("Player Not Found");
            this.enabled = false; //Turn the script to inactive 
            return;
        }

        // figure out the starting distance from the player
        offset = transform.position - player.position;
    }

    // Using LateUpdate is better for the camera, makes sure the player has moved first.
    void LateUpdate()
    {
        // if the player is gone (e.g., destroyed), just stop.
        if (player == null)
        {
            return;
        }

        // apply the offset to the player's current position to move the camera
        transform.position = player.position + offset;
    }
}