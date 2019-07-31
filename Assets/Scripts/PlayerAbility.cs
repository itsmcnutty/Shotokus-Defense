using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class PlayerAbility : MonoBehaviour
{
    public SteamVR_Input_Sources handType;
    public SteamVR_Behaviour_Pose controllerPose;
    public SteamVR_Action_Boolean grabAction;
    public SteamVR_Action_Boolean gripAction;
    public float rockStartSize;
    public float energyCost;

    private PlayerEnergy playerEnergy;
    private ControllerArc arc;
    private GameObject spawnedRock;
    private float rockSize = 0;
    private int rockNum = -1;

    private void Awake ()
    {
        GameObject player = GameObject.FindWithTag ("MainCamera");
        if (player != null)
        {
            playerEnergy = player.GetComponent<PlayerEnergy> ();
        }

        arc = GetComponentInChildren<ControllerArc>();
    }

    // Start is called before the first frame update
    void Start ()
    {
        rockSize = rockStartSize;
    }

    // Update is called once per frame
    void Update ()
    {
        if (GripPress ())
        {
            playerEnergy.SetActiveAbility (PlayerEnergy.AbilityType.Heal);
            RemoveRockFromHand();
        }
        else if (GrabPress () && arc.CanUseAbility())
        {
            playerEnergy.SetActiveAbility (PlayerEnergy.AbilityType.Rock);
            GetComponent<SpawnAndAttachToHand> ().SpawnAndAttach (null);
            GameObject[] allRocks = GameObject.FindGameObjectsWithTag ("Rock");
            rockNum = allRocks.Length - 1;
			GetComponent<Hand>().TriggerHapticPulse( 800 );
        }
        else if (GrabHold () && !playerEnergy.AbilityIsActive (PlayerEnergy.AbilityType.Heal) && rockNum != -1)
        {
            if (playerEnergy.EnergyIsNotZero ())
            {
                GameObject[] allRocks = GameObject.FindGameObjectsWithTag ("Rock");
                spawnedRock = allRocks[rockNum];
                rockSize += (0.01f * Time.deltaTime);
                spawnedRock.transform.localScale = new Vector3 (rockSize, rockSize, rockSize);
                playerEnergy.UseEnergy (energyCost, PlayerEnergy.AbilityType.Rock);
			    GetComponent<Hand>().TriggerHapticPulse( 800 );
            }
            else
            {
                playerEnergy.UpdateAbilityUseTime();
            }

        }
        else
        {
            RemoveRockFromHand();
        }
    }

    public bool GrabHold ()
    {
        return grabAction.GetState (handType);
    }

    public bool GrabPress ()
    {
        return grabAction.GetStateDown (handType);
    }

    public bool GripPress ()
    {
        return gripAction.GetStateDown (handType);
    }

    public void RemoveRockFromHand ()
    {
        if (rockNum != -1)
        {
            GetComponent<SpawnAndAttachToHand> ().hand.DetachObject (GameObject.FindGameObjectsWithTag ("Rock") [rockNum]);
            rockNum = -1;
        }
        rockSize = rockStartSize;
        playerEnergy.RegenEnergy ();
    }

}