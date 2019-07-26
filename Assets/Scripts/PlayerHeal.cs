using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class PlayerHeal : MonoBehaviour
{
    public SteamVR_Input_Sources handType;
    public SteamVR_Action_Boolean gripAction;
    public float energyCost;
    private static SteamVR_Input_Sources firstTriggerHeld;
    private PlayerHealth playerHealth;
    private PlayerEnergy playerEnergy;

    private void Awake ()
    {
        GameObject player = GameObject.FindWithTag ("Player");
        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth> ();
            playerEnergy = player.GetComponent<PlayerEnergy> ();
        }
    }

    // Start is called before the first frame update
    void Start ()
    {

    }

    // Update is called once per frame
    void Update ()
    {
        if (HandClose () && GripHold ())
        {
            if (firstTriggerHeld == SteamVR_Input_Sources.Any)
            {
                firstTriggerHeld = handType;
            }
            else if (firstTriggerHeld != handType)
            {
                playerHealth.RegenHealth ();
                playerEnergy.UseEnergy (energyCost);
            }
        }
        else
        {
            firstTriggerHeld = SteamVR_Input_Sources.Any;
        }
    }

    public bool GripHold ()
    {
        return gripAction.GetState (handType);
    }

    public bool HandClose ()
    {
        //Debug.Log ("Pos x = " + hand.transform.position.x);
        //Debug.Log ("Pos y = " + hand.transform.position.y);
        //Debug.Log ("Pos z = " + hand.transform.position.z);
        return true;
    }
}