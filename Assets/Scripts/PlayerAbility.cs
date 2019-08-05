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
    public SteamVR_Action_Boolean drawAction;
    public Hand otherHand;
    public float rockStartSize;
    public float energyCost;

    [Header ("Prefabs")]
    public GameObject actionPlaceholderPrefab;
    public GameObject wallOutlinePrefab;
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
    private GameObject spikeQuicksandOutline;
    private Vector3 spikeEndPosition;

    private static GameObject wallOutline;
    private static GameObject wall;
    private static Hand firstHandHeld;
    private static float lastAngle;
    private static float startingHandHeight;

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
        }
        else if (GrabPress ())
        {
            TriggerNewAbility ();
        }
        else if (GrabHold ())
        {
            UpdateAbility ();
        }
        else if (GrabRelease ())
        {
            EndAbility ();
        }
        else if (DrawPress ())
        {
            EnterDrawMode ();
        }
        else if (DrawHold ())
        {
            if (playerEnergy.AbilityIsActive (PlayerEnergy.AbilityType.Wall) && wallOutline != null && arc.CanUseAbility () && otherArc.CanUseAbility ())
            {
                SetWallLocation ();
            }
        }
        else if (DrawRelease ())
        {
            if (playerEnergy.AbilityIsActive (PlayerEnergy.AbilityType.Wall) && wallOutline != null && wall == null)
            {
                CancelAbility ();
            }
        }
        else
        {
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

    private bool GrabRelease ()
    {
        return grabAction.GetStateUp (handType);
    }

    public bool DrawHold ()
    {
        return drawAction.GetState (handType);
    }

    private bool DrawPress ()
    {
        return drawAction.GetStateDown (handType);
    }

    private bool DrawRelease ()
    {
        return drawAction.GetStateUp (handType);
    }

    private bool GripPress ()
    {
        return gripAction.GetStateDown (handType);
    }

    private void TriggerNewAbility ()
    {
        if (wallOutline != null)
        {
            wall = Instantiate (wallPrefab) as GameObject;
            wall.transform.position = wallOutline.transform.position;
            wall.transform.localScale = wallOutline.transform.localScale;
            wall.transform.rotation = wallOutline.transform.rotation;
            startingHandHeight = Math.Min (hand.transform.position.y, otherHand.transform.position.y);
            Destroy (wallOutline);
        }
        else if (!playerEnergy.AbilityIsActive (PlayerEnergy.AbilityType.Wall) && arc.CanUseAbility ())
        {
            playerEnergy.AddHandToActive (hand);
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
                firstHandHeld = null;
                playerEnergy.AddActiveAbility (PlayerEnergy.AbilityType.Spike);
                spikeQuicksandOutline = Instantiate (actionPlaceholderPrefab) as GameObject;
                placeholderSize = 0;
                spikeQuicksandOutline.transform.position = arc.GetEndPosition ();
            }
        }
    }

    private void UpdateAbility ()
    {
        if (playerEnergy.EnergyIsNotZero ())
        {

            if (playerEnergy.AbilityIsActive (PlayerEnergy.AbilityType.Rock) && rock != null)
            {
                rockSize += (ROCK_SIZE_INCREASE_RATE * Time.deltaTime);
                rock.transform.localScale = new Vector3 (rockSize, rockSize, rockSize);
                playerEnergy.DrainTempEnergy (hand, energyCost);
                hand.TriggerHapticPulse (800);
            }
            else if (playerEnergy.AbilityIsActive (PlayerEnergy.AbilityType.Spike) && spikeQuicksandOutline != null)
            {
                placeholderSize += (SPIKE_SIZE_INCREASE_RATE * Time.deltaTime);
                float sizeXZ = placeholderSize + spikeQuicksandOutline.transform.localScale.x;
                spikeQuicksandOutline.transform.localScale = new Vector3 (sizeXZ, 0.5f, sizeXZ);
                spikeEndPosition = spikeQuicksandOutline.transform.position;
                spikeEndPosition.y += spikeQuicksandOutline.transform.localScale.y + 1f;
                playerEnergy.DrainTempEnergy (hand, energyCost);
            }
            else if (playerEnergy.AbilityIsActive (PlayerEnergy.AbilityType.Wall) && wall != null)
            {
                float currentHandHeight = Math.Min (hand.transform.position.y, otherHand.transform.position.y) - startingHandHeight;
                if (currentHandHeight > wall.transform.position.y)
                {
                    Vector3 newPos = new Vector3 (wall.transform.position.x, currentHandHeight, wall.transform.position.z);
                    wall.transform.position = Vector3.MoveTowards (wall.transform.position, newPos, 1f);
                    playerEnergy.SetTempEnergy (hand, (wall.transform.position.x * currentHandHeight) * 200f);
                }
            }
        }
        else
        {
            playerEnergy.UpdateAbilityUseTime ();
        }
    }

    private void EndAbility ()
    {
        Debug.Log ("Ability ended");
        if (playerEnergy.AbilityIsActive (PlayerEnergy.AbilityType.Rock) && rock != null)
        {
            playerEnergy.UseEnergy (PlayerEnergy.AbilityType.Rock, hand);
            RemoveRockFromHand ();
        }
        else if (playerEnergy.AbilityIsActive (PlayerEnergy.AbilityType.Spike) && spikeQuicksandOutline != null)
        {
            playerEnergy.UseEnergy (PlayerEnergy.AbilityType.Spike, hand);
            float controllerVelocity = controllerPose.GetVelocity ().y;
            if (controllerVelocity <= 0)
            {
                GameObject quicksand = Instantiate (quicksandPrefab) as GameObject;
                quicksand.transform.position = spikeQuicksandOutline.transform.position;
                quicksand.transform.localScale = new Vector3 (spikeQuicksandOutline.transform.localScale.x, .01f, spikeQuicksandOutline.transform.localScale.z);
            }
            else
            {
                GameObject spike = Instantiate (spikePrefab) as GameObject;
                spike.transform.position = spikeQuicksandOutline.transform.position;
                spike.transform.localScale = new Vector3 (spikeQuicksandOutline.transform.localScale.x, spikeQuicksandOutline.transform.localScale.z, spikeQuicksandOutline.transform.localScale.z * 2);

                float spikeVelocity = (controllerVelocity / SPIKE_SPEED_REDUCTION) + SPIKE_BASE_SPEED;
                spike.GetComponent<SpikeMovement> ().SetSpeed (spikeVelocity);
                spike.GetComponent<SpikeMovement> ().SetEndPosition (spikeEndPosition);
            }

            Destroy (spikeQuicksandOutline);
        }
        else if (playerEnergy.AbilityIsActive (PlayerEnergy.AbilityType.Wall) && wall != null)
        {
            playerEnergy.UseEnergy (PlayerEnergy.AbilityType.Wall, hand);
            ResetWallInfo ();
        }
        playerEnergy.RemoveHandFromActive (hand);
    }

    private void CancelAbility ()
    {
        Debug.Log ("Ability Canceled");
        if (rock != null)
        {
            playerEnergy.CancelEnergyUsage (PlayerEnergy.AbilityType.Rock, hand);
            RemoveRockFromHand ();
        }
        else if (spikeQuicksandOutline != null)
        {
            playerEnergy.CancelEnergyUsage (PlayerEnergy.AbilityType.Spike, hand);
            Destroy (spikeQuicksandOutline);
            placeholderSize = 0;
        }
        else if (wallOutline != null)
        {
            playerEnergy.CancelEnergyUsage (PlayerEnergy.AbilityType.Wall, hand);
            Destroy (wallOutline);
            ResetWallInfo ();
        }
        else if (wall != null)
        {
            ResetWallInfo ();
        }
        playerEnergy.RemoveHandFromActive (hand);
        playerEnergy.AddActiveAbility (PlayerEnergy.AbilityType.Heal);
    }

    private void EnterDrawMode ()
    {
        if (firstHandHeld != null && firstHandHeld != hand)
        {
            playerEnergy.AddActiveAbility (PlayerEnergy.AbilityType.Wall);
            wallOutline = Instantiate (wallOutlinePrefab) as GameObject;

            SetWallLocation ();
        }
        else
        {
            firstHandHeld = hand;
        }
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
        wallOutline = null;
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
        wallOutline.transform.position = new Vector3 (wallPosition.x, wallPosition.y, wallPosition.z);

        float remainingEnergy = playerEnergy.GetRemainingEnergy ();
        float maxHeight = remainingEnergy / (arc.GetEndPointsDistance (otherArc) * 200f);
        float area = arc.GetEndPointsDistance (otherArc) * maxHeight;
        area = (float) Math.Round (area, 2) * 200f;
        if (maxHeight > 1f)
        {
            wallOutline.transform.localScale = new Vector3 (arc.GetEndPointsDistance (otherArc), maxHeight, 0.1f);
        }

        float angle = Vector3.SignedAngle (arc.GetEndPosition () - otherArc.GetEndPosition (), wallOutline.transform.position, new Vector3 (0, -1, 0));
        angle += Vector3.SignedAngle (wallOutline.transform.position, new Vector3 (1, 0, 0), new Vector3 (0, -1, 0));
        float newAngle = angle;
        angle -= lastAngle;
        if (Math.Abs (angle) >= 0.5f)
        {
            lastAngle = newAngle;
            wallOutline.transform.Rotate (0, angle, 0, Space.Self);
        }
    }

    public bool IsNotUsingWall ()
    {
        return wallOutline == null && wall != null;
    }
}