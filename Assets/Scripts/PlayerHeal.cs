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
    public SteamVR_Action_Boolean grabAction;
    public Hand hand;
    public float energyCost;
    private static bool healAvailable;
    private static Hand firstTriggerHeld;
    private PlayerHealth playerHealth;
    private PlayerEnergy playerEnergy;
    private const float HAND_DIST_Y = 0.1f;
    private const float HAND_DIST_XZ = 0.2f;

    private void Awake ()
    {
        GameObject player = GameObject.FindWithTag ("MainCamera");
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
        if (GrabPress ())
        {
            healAvailable = false;
        }
        else if(GripPress())
        {
            healAvailable = true;
        }
        else if (GripHold () && healAvailable)
        {
            if (firstTriggerHeld == null)
            {
                firstTriggerHeld = hand;
            }
            else if (firstTriggerHeld != hand && HandsAreClose ())
            {
                if (playerEnergy.EnergyIsNotZero ())
                {
                    GetComponent<Hand> ().TriggerHapticPulse (1500);
                    firstTriggerHeld.GetComponent<Hand> ().TriggerHapticPulse (1500);
                    playerHealth.RegenHealth ();
                    playerEnergy.DrainRealEnergy (energyCost);
                }
                else
                {
                    playerEnergy.UpdateAbilityUseTime ();
                }
            }
        }
        else
        {
            firstTriggerHeld = null;
        }
    }

    public bool GripPress ()
    {
        return gripAction.GetStateDown (handType);
    }

    public bool GripHold ()
    {
        return gripAction.GetState (handType);
    }

    public bool GrabPress ()
    {
        return grabAction.GetStateDown (handType);
    }

    public bool HandsAreClose ()
    {
        Vector3 handPos = hand.transform.position;
        Vector3 otherHandPos = firstTriggerHeld.transform.position;
        return (Math.Abs (otherHandPos.x - handPos.x) < HAND_DIST_XZ) &&
            (Math.Abs (otherHandPos.y - handPos.y) < HAND_DIST_Y) &&
            (Math.Abs (otherHandPos.z - handPos.z) < HAND_DIST_XZ);
    }
}