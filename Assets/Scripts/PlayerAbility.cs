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

    private const float ROCK_CREATE_DIST = 3f;

    private void Awake ()
    {
        GameObject player = GameObject.FindWithTag ("MainCamera");
        if (player != null)
        {
            playerEnergy = player.GetComponent<PlayerEnergy> ();
        }

        arc = GetComponentInChildren<ControllerArc> ();
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
            CancelAbility();
        }
        else if (GrabPress () && arc.CanUseAbility ())
        {
            TriggerNewAbility ();
        }
        else if (GrabHold () && !playerEnergy.AbilityIsActive (PlayerEnergy.AbilityType.Heal) && rockNum != -1)
        {
            if (playerEnergy.EnergyIsNotZero ())
            {
                UpdateAbility ();
            }
            else
            {
                playerEnergy.UpdateAbilityUseTime ();
            }

        }
        else
        {
            EndAbility ();
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

    public void TriggerNewAbility ()
    {
        if (arc.GetDistanceFromPlayer () <= ROCK_CREATE_DIST)
        {
            playerEnergy.SetActiveAbility (PlayerEnergy.AbilityType.Rock);
            GetComponent<SpawnAndAttachToHand> ().SpawnAndAttach (null);
            GameObject[] allRocks = GameObject.FindGameObjectsWithTag ("Rock");
            rockNum = allRocks.Length - 1;
            GetComponent<Hand> ().TriggerHapticPulse (800);
        }
        else
        {
            playerEnergy.SetActiveAbility (PlayerEnergy.AbilityType.Spike);
        }
    }

    public void UpdateAbility ()
    {
        if (playerEnergy.AbilityIsActive (PlayerEnergy.AbilityType.Rock))
        {
            GameObject[] allRocks = GameObject.FindGameObjectsWithTag ("Rock");
            spawnedRock = allRocks[rockNum];
            rockSize += (0.01f * Time.deltaTime);
            spawnedRock.transform.localScale = new Vector3 (rockSize, rockSize, rockSize);
            playerEnergy.UseEnergy (energyCost, PlayerEnergy.AbilityType.Rock);
            GetComponent<Hand> ().TriggerHapticPulse (800);
        }
    }

    public void EndAbility ()
    {
        if (playerEnergy.AbilityIsActive (PlayerEnergy.AbilityType.Rock))
        {
            RemoveRockFromHand ();
        }
    }

    public void CancelAbility()
    {
        if (playerEnergy.AbilityIsActive (PlayerEnergy.AbilityType.Rock))
        {
            playerEnergy.SetActiveAbility (PlayerEnergy.AbilityType.Heal);
            RemoveRockFromHand ();
        }
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