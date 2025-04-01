using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    //Fields
    public float playerSpeed = 4;
    public float movementDistance = 2;
    public float leftMaximum = -2;
    public float rightMaximum = 2;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //Moves the player forwards continiously
        transform.Translate(Vector3.forward * Time.deltaTime * playerSpeed, Space.World);

        //if the A key is pressed on the keyboard
        if (Input.GetKeyDown(KeyCode.A))
        {
            //If the position fo the player - the distance the player moves is more than the maximum distance left
            if (transform.position.x - movementDistance >= leftMaximum)
            {
                //move the player to the left 
                transform.Translate(Vector3.left * movementDistance);
            }
        }
        //if the D key is pressed on the keyboard
        if (Input.GetKeyDown(KeyCode.D))
        {
            //If the position fo the player + the distance the player moves is less than the maximum distance Right
            if (transform.position.x + movementDistance <= rightMaximum)
            {
                //move the player to the Right
                transform.Translate(Vector3.right * movementDistance);
            }
        }
    }
}
