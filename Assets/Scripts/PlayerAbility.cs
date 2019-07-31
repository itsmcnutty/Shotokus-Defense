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
    public GameObject spikePrefab;

    private PlayerEnergy playerEnergy;
    private ControllerArc arc;
    private float rockSize = 0;
    private float spikeSize = 0;
    private GameObject rock;
    private GameObject spike;
    private Vector3 spikeEndPosition;

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
            CancelAbility ();
        }
        else if (GrabPress () && arc.CanUseAbility ())
        {
            TriggerNewAbility ();
        }
        else if (GrabHold () && !playerEnergy.HealAbilityIsActive ())
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
            playerEnergy.RegenEnergy ();
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

    public bool GrabRelease ()
    {
        return grabAction.GetStateUp (handType);
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
            GameObject[] allObjects = GameObject.FindGameObjectsWithTag ("Rock");
            rock = allObjects[allObjects.Length - 1];
            GetComponent<Hand> ().TriggerHapticPulse (800);
        }
        else
        {
            playerEnergy.SetActiveAbility (PlayerEnergy.AbilityType.Spike);
            spike = Instantiate (spikePrefab) as GameObject;
            spikeSize = 0;
            spike.transform.position = arc.GetEndPosition ();
            spikeEndPosition = spike.transform.position;
            spikeEndPosition.y += 1f;

        }
    }

    public void UpdateAbility ()
    {
        if (playerEnergy.AbilityIsActive (PlayerEnergy.AbilityType.Rock) && rock != null)
        {
                rockSize += (0.01f * Time.deltaTime);
                rock.transform.localScale = new Vector3 (rockSize, rockSize, rockSize);
                playerEnergy.UseEnergy (energyCost, PlayerEnergy.AbilityType.Rock);
                GetComponent<Hand> ().TriggerHapticPulse (800);
        }
        else if (playerEnergy.AbilityIsActive (PlayerEnergy.AbilityType.Spike) && spike != null)
        {
                spikeSize += (10f * Time.deltaTime);
                float spikeXY = spikeSize + spike.transform.localScale.x;
                float spikeZ = (spikeSize * 2) + spike.transform.localScale.z;
                spike.transform.localScale = new Vector3 (spikeXY, spikeXY, spikeZ);
                spikeEndPosition = spike.transform.position;
                spikeEndPosition.y += 1f;
                playerEnergy.UseEnergy (energyCost, PlayerEnergy.AbilityType.Spike);
        }
    }

    public void EndAbility ()
    {
        if (playerEnergy.AbilityIsActive (PlayerEnergy.AbilityType.Rock) && rock != null)
        {
            RemoveRockFromHand ();
        }
        else if (playerEnergy.AbilityIsActive (PlayerEnergy.AbilityType.Spike) && spike != null)
        {
            spike.GetComponent<SpikeMovement>().SetSpeed(0.1f);
            spike.GetComponent<SpikeMovement>().SetEndPosition(spikeEndPosition);
            spike = null;
        }
    }

    public void CancelAbility ()
    {
        if (playerEnergy.AbilityIsActive (PlayerEnergy.AbilityType.Rock))
        {
            RemoveRockFromHand ();
        }
        else if (playerEnergy.AbilityIsActive (PlayerEnergy.AbilityType.Spike))
        {
            Destroy(spike);
        }
        playerEnergy.SetActiveAbility (PlayerEnergy.AbilityType.Heal);
    }

    public void RemoveRockFromHand ()
    {
        GetComponent<SpawnAndAttachToHand> ().hand.DetachObject (rock);
        rockSize = rockStartSize;
    }
}