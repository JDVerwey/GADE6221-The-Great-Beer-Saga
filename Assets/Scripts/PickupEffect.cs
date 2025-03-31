using System.Collections;
using System.Collections.Generic;
using Unity.PlasticSCM.Editor.WebApi;
using Unity.VisualScripting;
using UnityEngine;

public class PickupEffects : MonoBehaviour
{
    //Fields
    PlayerMovement player;
    private float timer;
    public float slowTimer = 15f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //If the timer is greater than 0
        if (timer > 0)
        {
            //Then the timer couunts down to 0
            timer -= Time.deltaTime;
            //If the timer is less than or equal 0
            if (timer <= 0)
            {
                //then the player speed = 0
                player.playerSpeed = 4;
            }
        }
    }
    //Is called when an other game object collides with the player
    private void OnTriggerEnter(Collider other)
    {
        //If the other object is a Berry
        if (other.tag == "Berry") 
        {
            //Than the player speed is set to 2, timer is set to slow timer and the berry is destroyed 
            player.playerSpeed = 2;
            timer = slowTimer;
            Destroy(other.gameObject);
        }
    }
}
