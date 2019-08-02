using System;
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
    public Hand otherHand;
    public float rockStartSize;
    public float energyCost;

    [Header( "Prefabs" )]
    public GameObject actionPlaceholderPrefab;
    public GameObject spikePrefab;
    public GameObject quicksandPrefab;
    public GameObject wallPrefab;

    private Hand hand;
    private PlayerEnergy playerEnergy;
    private ControllerArc arc;
    private ControllerArc otherArc;
    private float rockSize = 0;
    private float placeholderSize = 0;
    private GameObject player;
    private GameObject rock;
    private GameObject placeholderInstance;
    private Vector3 spikeEndPosition;

    private static GameObject wall;
    private static Hand firstHandHeld;
    private static float lastAngle;
    private static float initialHandHeight;

    private const float ROCK_CREATE_DIST = 3f;
    private const float ROCK_SIZE_INCREASE_RATE = 0.01f;
    private const float SPIKE_SIZE_INCREASE_RATE = 0.001f;
    private const float SPIKE_SPEED_REDUCTION = 10f;
    private const float SPIKE_BASE_SPEED = .05f;
    private const float WALL_OVERLAP_DISTANCE = 2f;

    private void Awake ()
    {
        player = GameObject.FindWithTag ("MainCamera");
        if (player != null)
        {
            playerEnergy = player.GetComponent<PlayerEnergy> ();
        }
    }

    // Start is called before the first frame update
    void Start ()
    {
        rockSize = rockStartSize;

        arc = GetComponentInChildren<ControllerArc> ();
        otherArc = otherHand.GetComponentInChildren<ControllerArc> ();
        hand = GetComponent<Hand> ();
    }

    // Update is called once per frame
    void Update ()
    {
        if (GripPress ())
        {
            CancelAbility ();
            playerEnergy.RemoveHandFromActive(hand);
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
            playerEnergy.RemoveHandFromActive(hand);
            playerEnergy.RegenEnergy ();
        }
    }

    public bool GrabHold ()
    {
        return grabAction.GetState (handType);
    }

    private bool GrabPress ()
    {
        return grabAction.GetStateDown (handType);
    }

    private bool GripPress ()
    {
        return gripAction.GetStateDown (handType);
    }

    private void TriggerNewAbility ()
    {
        playerEnergy.AddHandToActive(hand);
        if (arc.GetDistanceFromPlayer () <= ROCK_CREATE_DIST)
        {
            playerEnergy.AddActiveAbility (PlayerEnergy.AbilityType.Rock);
            GetComponent<SpawnAndAttachToHand> ().SpawnAndAttach (null);
            GameObject[] allObjects = GameObject.FindGameObjectsWithTag ("Rock");
            rock = allObjects[allObjects.Length - 1];
            hand.TriggerHapticPulse (800);
        }
        else
        {
            if (arc.GetEndPointsDistance (otherArc) < WALL_OVERLAP_DISTANCE)
            {
                if (firstHandHeld != null && firstHandHeld != hand)
                {
                    playerEnergy.AddActiveAbility (PlayerEnergy.AbilityType.Wall);
                    wall = Instantiate (wallPrefab) as GameObject;

                    SetWallLocation ();
                    initialHandHeight = Math.Min (hand.transform.position.y, otherHand.transform.position.y);
                }
                else
                {
                    firstHandHeld = hand;
                }
            }
            else
            {
                firstHandHeld = null;
                playerEnergy.AddActiveAbility (PlayerEnergy.AbilityType.Spike);
                placeholderInstance = Instantiate (actionPlaceholderPrefab) as GameObject;
                placeholderSize = 0;
                placeholderInstance.transform.position = arc.GetEndPosition ();
            }
        }
    }

    private void UpdateAbility ()
    {
        if (playerEnergy.AbilityIsActive (PlayerEnergy.AbilityType.Rock) && rock != null)
        {
            rockSize += (ROCK_SIZE_INCREASE_RATE * Time.deltaTime);
            rock.transform.localScale = new Vector3 (rockSize, rockSize, rockSize);
            playerEnergy.DrainTempEnergy (hand, energyCost);
            hand.TriggerHapticPulse (800);
        }
        else if (playerEnergy.AbilityIsActive (PlayerEnergy.AbilityType.Spike) && placeholderInstance != null)
        {
            placeholderSize += (SPIKE_SIZE_INCREASE_RATE * Time.deltaTime);
            float sizeXZ = placeholderSize + placeholderInstance.transform.localScale.x;
            placeholderInstance.transform.localScale = new Vector3 (sizeXZ, 0.5f, sizeXZ);
            spikeEndPosition = placeholderInstance.transform.position;
            spikeEndPosition.y += placeholderInstance.transform.localScale.y + 1f;
            playerEnergy.DrainTempEnergy (hand, energyCost);
        }
        else if (playerEnergy.AbilityIsActive (PlayerEnergy.AbilityType.Wall) && wall != null)
        {
            SetWallLocation ();
        }
    }

    private void EndAbility ()
    {
        if (playerEnergy.AbilityIsActive (PlayerEnergy.AbilityType.Rock) && rock != null)
        {
            playerEnergy.UseEnergy(PlayerEnergy.AbilityType.Rock, hand);
            RemoveRockFromHand ();
        }
        else if (playerEnergy.AbilityIsActive (PlayerEnergy.AbilityType.Spike) && placeholderInstance != null)
        {
            playerEnergy.UseEnergy(PlayerEnergy.AbilityType.Spike, hand);
            float controllerVelocity = controllerPose.GetVelocity ().y;
            if (controllerVelocity <= 0)
            {
                GameObject quicksand = Instantiate (quicksandPrefab) as GameObject;
                quicksand.transform.position = placeholderInstance.transform.position;
                quicksand.transform.localScale = new Vector3 (placeholderInstance.transform.localScale.x, .01f, placeholderInstance.transform.localScale.z);
            }
            else
            {
                GameObject spike = Instantiate (spikePrefab) as GameObject;
                spike.transform.position = placeholderInstance.transform.position;
                spike.transform.localScale = new Vector3 (placeholderInstance.transform.localScale.x, placeholderInstance.transform.localScale.z, placeholderInstance.transform.localScale.z * 2);

                float spikeVelocity = (controllerVelocity / SPIKE_SPEED_REDUCTION) + SPIKE_BASE_SPEED;
                spike.GetComponent<SpikeMovement> ().SetSpeed (spikeVelocity);
                spike.GetComponent<SpikeMovement> ().SetEndPosition (spikeEndPosition);
            }

            Destroy (placeholderInstance);
        }
        else if (playerEnergy.AbilityIsActive (PlayerEnergy.AbilityType.Wall) && wall != null)
        {
            playerEnergy.UseEnergy(PlayerEnergy.AbilityType.Wall, hand);
            ResetWallInfo ();
        }
    }

    private void CancelAbility ()
    {
        if (rock != null)
        {
            playerEnergy.CancelEnergyUsage(PlayerEnergy.AbilityType.Rock, hand);
            RemoveRockFromHand ();
        }
        else if (placeholderInstance != null)
        {
            playerEnergy.CancelEnergyUsage(PlayerEnergy.AbilityType.Spike, hand);
            Destroy (placeholderInstance);
            placeholderSize = 0;
        }
        else if (wall != null)
        {
            playerEnergy.CancelEnergyUsage(PlayerEnergy.AbilityType.Wall, hand);
            Destroy (wall);
            ResetWallInfo ();
        }
        playerEnergy.AddActiveAbility (PlayerEnergy.AbilityType.Heal);
    }

    private void RemoveRockFromHand ()
    {
        GetComponent<SpawnAndAttachToHand> ().hand.DetachObject (rock);
        rockSize = rockStartSize;
        rock = null;
    }

    private void ResetWallInfo ()
    {
        firstHandHeld = null;
        wall = null;
        lastAngle = 0;
    }

    private Vector3 GetWallPosition ()
    {
        Vector3 thisArcPos = arc.GetEndPosition ();
        Vector3 otherArcPos = otherArc.GetEndPosition ();
        float wallPosX = (thisArcPos.x + otherArcPos.x) / 2;
        float wallPosY = (thisArcPos.y + otherArcPos.y) / 2;
        float wallPosZ = (thisArcPos.z + otherArcPos.z) / 2;
        return new Vector3 (wallPosX, wallPosY, wallPosZ);
    }

    private void SetWallLocation ()
    {
        Vector3 wallPosition = GetWallPosition ();

        float wallHeight = Math.Max (initialHandHeight, Math.Min (hand.transform.position.y, otherHand.transform.position.y));

        wall.transform.position = new Vector3 (wallPosition.x, wallPosition.y, wallPosition.z);
        wall.transform.localScale = new Vector3 (arc.GetEndPointsDistance (otherArc), wallHeight, 0.1f);

        float angle = Vector3.SignedAngle (arc.GetEndPosition () - otherArc.GetEndPosition (), wall.transform.position, new Vector3 (0, -1, 0));
        angle += Vector3.SignedAngle (wall.transform.position, new Vector3 (1, 0, 0), new Vector3 (0, -1, 0));
        float newAngle = angle;
        angle -= lastAngle;
        if (Math.Abs (angle) >= 0.5f)
        {
            lastAngle = newAngle;
            wall.transform.Rotate (0, angle, 0, Space.Self);
        }
    }

    public bool IsNotUsingWall ()
    {
        return wall == null;
    }
}