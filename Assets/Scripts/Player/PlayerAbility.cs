using System;
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
    public float baseSpikeRadius = 0.5f;

    [Header ("Prefabs")]
    public GameObject actionPlaceholderPrefab;
    public GameObject wallOutlinePrefab;
    public GameObject rockPrefab;
    public GameObject spikePrefab;
    public GameObject quicksandPrefab;
    public GameObject wallPrefab;

    [Header ("Outline Materials")]
    public Material validOutlineMat;
    public Material invalidOutlineMat;

    [Header ("Ability Values")]
    public float rockCreationDistance = 3f;
    public float minRockDiameter = 0.25f;
    public float maxRockDimater = 1.5f;
    public float spikeSpeedReduction = 10f;
    public float spikeMinSpeed = .05f;
    public float maxSpikeDiameter = 5f;
    public float wallSizeMultiplier = 200f;

    private Hand hand;
    private PlayerEnergy playerEnergy;
    private ControllerArc arc;
    private ControllerArc otherArc;
    private GameObject player;
    private GameObject rock;
    private GameObject spikeQuicksandOutline;
    private float startingSpikeHandHeight;

    private static GameObject wallOutline;
    private static GameObject wall;
    private static Hand firstHandHeld;
    private static float lastAngle;
    private static float startingHandHeight;
    private static float currentWallHeight;

    private static List<Vector2> spikeLocations;
    private HashSet<Vector3> allSpikes;
    private static List<GameObject> availableSpikes = new List<GameObject> ();

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
        arc = GetComponentInChildren<ControllerArc> ();
        otherArc = otherHand.GetComponentInChildren<ControllerArc> ();
        hand = GetComponent<Hand> ();

        allSpikes = new HashSet<Vector3> ();

        spikeLocations = new List<Vector2> ();
        spikeLocations.Add (new Vector2 (2, 0));
        spikeLocations.Add (new Vector2 (1, 1));
        spikeLocations.Add (new Vector2 (-1, 1));
        spikeLocations.Add (new Vector2 (-2, 0));
        spikeLocations.Add (new Vector2 (-1, -1));
        spikeLocations.Add (new Vector2 (1, -1));

        float maxRadius = (float) Math.Sqrt (playerEnergy.maxEnergy / 50);
        int numLayers = (int) Math.Floor ((maxRadius - baseSpikeRadius) / (baseSpikeRadius * 2));
        int numSpikes = numLayers * spikeLocations.Count + 1;

        for (int i = 0; i < numSpikes; i++)
        {
            GameObject spike = Instantiate (spikePrefab) as GameObject;
            spike.transform.position = new Vector3 (0, -10, 0);
            MakeSpikeAvailable (spike);
        }
    }

    // Update is called once per frame
    void Update ()
    {
        if (GripPress ())
        {
            CancelAbility ();
        }

        if (GrabPress ())
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

        if (DrawPress () && playerEnergy.EnergyAboveThreshold (100f))
        {
            EnterDrawMode ();
        }
        else if (DrawHold ())
        {
            playerEnergy.UpdateAbilityUseTime ();
            if (WallOutlineIsActive () && !WallIsActive ())
            {
                SetWallLocation ();
                SetOutlineMaterial (wallOutline, WallIsValid ());
            }
        }
        else if (DrawRelease ())
        {
            if (WallOutlineIsActive () && !WallIsActive ())
            {
                CancelAbility ();
            }
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
        if (WallOutlineIsActive ())
        {
            if (firstHandHeld != null && firstHandHeld != hand)
            {
                OutlineProperties properties = wallOutline.GetComponentInChildren<OutlineProperties> ();
                if (WallIsValid ())
                {
                    wall = Instantiate (wallPrefab) as GameObject;
                    wall.transform.position = wallOutline.transform.position;
                    wall.transform.localScale = wallOutline.transform.localScale;
                    wall.transform.rotation = wallOutline.transform.rotation;
                    startingHandHeight = Math.Min (hand.transform.position.y, otherHand.transform.position.y);
                    Destroy (wallOutline);
                }
                else
                {
                    playerEnergy.CancelEnergyUsage (firstHandHeld);
                    Destroy (wallOutline);
                    ResetWallInfo ();
                }

            }
            else
            {
                firstHandHeld = hand;
            }
        }
        else if (!WallIsActive () && arc.CanUseAbility ())
        {
            firstHandHeld = null;
            if (hand.currentAttachedObject != null)
            {
                if (hand.currentAttachedObject != otherHand.currentAttachedObject)
                {
                    rock = hand.currentAttachedObject;
                    Destroy (rock.GetComponent<RockProperties> ());
                }
            }
            else if (arc.GetDistanceFromPlayer () <= rockCreationDistance)
            {
                rock = Instantiate (rockPrefab) as GameObject;
                rock.transform.position = new Vector3 (arc.GetEndPosition ().x, arc.GetEndPosition ().y - 0.25f, arc.GetEndPosition ().z);
                hand.AttachObject (rock, GrabTypes.Scripted);
            }
            else if (hand.hoveringInteractable == null && playerEnergy.EnergyAboveThreshold (200f))
            {
                spikeQuicksandOutline = Instantiate (actionPlaceholderPrefab) as GameObject;
                spikeQuicksandOutline.transform.position = arc.GetEndPosition ();
                startingSpikeHandHeight = hand.transform.position.y;
            }
        }
    }

    private void UpdateAbility ()
    {

        if (RockIsActive ())
        {
            float range = maxRockDimater - minRockDiameter;
            float rockEnergyCost = (rock.transform.localScale.x - minRockDiameter) * playerEnergy.maxEnergy / range;
            rockEnergyCost = (rockEnergyCost < 0) ? 0 : rockEnergyCost;
            rock.GetComponent<Rigidbody> ().mass = 3200f * (float) Math.Pow (rockEnergyCost / 2.0, 3.0);
            playerEnergy.SetTempEnergy (hand, rockEnergyCost);
            hand.SetAllowResize (playerEnergy.EnergyIsNotZero ());
        }
        else if (SpikeQuicksandIsActive ())
        {
            float size = (float) Math.Pow ((Math.Abs (hand.transform.position.y - startingSpikeHandHeight)) + (baseSpikeRadius * 2), 3);
            Vector3 newSize = new Vector3 (size, 1f, size);
            if (playerEnergy.EnergyIsNotZero () || newSize.x < spikeQuicksandOutline.transform.localScale.x)
            {
                spikeQuicksandOutline.transform.localScale = newSize;
            }
            float energyCost = spikeQuicksandOutline.transform.localScale.x * playerEnergy.maxEnergy / maxSpikeDiameter;
            playerEnergy.SetTempEnergy (hand, energyCost);
            SetOutlineMaterial (spikeQuicksandOutline, SpikeQuicksandIsValid ());
        }
        else if (WallIsActive () && playerEnergy.EnergyIsNotZero ())
        {
            hand.TriggerHapticPulse (1500);
            float newHandHeight = (Math.Min (hand.transform.position.y, otherHand.transform.position.y) - startingHandHeight) * 2f;
            if (newHandHeight < 1 && currentWallHeight < newHandHeight)
            {
                currentWallHeight = newHandHeight;
                Vector3 newPos = new Vector3 (wall.transform.position.x, wall.transform.localScale.y * newHandHeight, wall.transform.position.z);
                wall.transform.position = Vector3.MoveTowards (wall.transform.position, newPos, 1f);
                float area = (float) Math.Round (wall.transform.localScale.x * wall.transform.localScale.y * newHandHeight, 2) * wallSizeMultiplier;
                playerEnergy.SetTempEnergy (firstHandHeld, area);
            }
        }
    }

    private void EndAbility ()
    {
        if (RockIsActive ())
        {
            hand.DetachObject (rock);
            hand.SetAllowResize (true);
            if (otherHand.currentAttachedObject == rock)
            {
                float rockSize = (float) Math.Pow (Math.Floor (rock.transform.localScale.x * rock.transform.localScale.y * rock.transform.localScale.z), 3);
                rock.GetComponent<Rigidbody> ().mass = 3200f * (float) Math.Pow (rockSize / 2.0, 3.0);
                playerEnergy.SetTempEnergy (hand, rockSize);
                playerEnergy.TransferHandEnergy (hand, otherHand);
                otherHand.GetComponent<PlayerAbility> ().rock = rock;
            }
            else
            {
                rock.AddComponent<RockProperties> ();
                playerEnergy.UseEnergy (hand);
                hand.TriggerHapticPulse (500);
            }
            rock = null;
        }
        else if (SpikeQuicksandIsActive ())
        {
            float controllerVelocity = controllerPose.GetVelocity ().y;
            float handPos = (hand.transform.position.y - startingSpikeHandHeight);
            if (handPos < 0 && SpikeQuicksandIsValid ())
            {
                GameObject quicksand = Instantiate (quicksandPrefab) as GameObject;
                quicksand.transform.position = spikeQuicksandOutline.transform.position;
                quicksand.transform.localScale = new Vector3 (spikeQuicksandOutline.transform.localScale.x, .01f, spikeQuicksandOutline.transform.localScale.z);
                quicksand.AddComponent<QuicksandProperties> ();
                Destroy (spikeQuicksandOutline);
                playerEnergy.UseEnergy (hand);
                hand.TriggerHapticPulse (800);
            }
            else if (handPos > 0 && controllerVelocity > 0 && SpikeQuicksandIsValid ())
            {
                float finalSpikeRadius = baseSpikeRadius;
                float size = spikeQuicksandOutline.transform.localScale.x / 2;
                float triangleDist = (baseSpikeRadius / (float) Math.Sqrt (3)) + baseSpikeRadius;
                if (size >= triangleDist && size < baseSpikeRadius * 3)
                {
                    finalSpikeRadius = GenerateSpikesTriangle (spikeQuicksandOutline.transform.position, size, triangleDist);
                }
                else
                {
                    float height = (float) Math.Sqrt (3) * baseSpikeRadius;
                    finalSpikeRadius = GenerateSpikesHex (spikeQuicksandOutline.transform.position, spikeQuicksandOutline.transform.position, height, size);
                    Debug.Log("count=" + allSpikes.Count);
                }
                float radiusIncrease = finalSpikeRadius - baseSpikeRadius;

                finalSpikeRadius = (finalSpikeRadius * 2) - 0.05f;
                Vector3 centerLoc = spikeQuicksandOutline.transform.position;

                Destroy (spikeQuicksandOutline);
                playerEnergy.UseEnergy (hand);

                foreach (Vector3 spikePos in allSpikes)
                {
                    GameObject spike;
                    if (availableSpikes.Count != 0)
                    {
                        spike = availableSpikes[0];
                        availableSpikes.Remove (spike);
                    }
                    else
                    {
                        spike = Instantiate (spikePrefab) as GameObject;
                    }

                    Vector3 spikeCorrection = (spikePos - centerLoc) * 0.33f;
                    Vector3 radiusCorrection = new Vector3 (Math.Sign (spikeCorrection.x) * radiusIncrease, 0, Math.Sign (spikeCorrection.z) * radiusIncrease);
                    spike.transform.position = (spikePos - spikeCorrection) + radiusCorrection;
                    
                    float layerNum = (float) Math.Floor (Vector3.Distance(spikePos, centerLoc) / (baseSpikeRadius * 2));
                    float heightScale = 0.75f;
                    float layerScale = (float) Math.Pow(.8, layerNum);
                    float finalSpikeHeight = finalSpikeRadius * heightScale * layerScale;
                    spike.transform.localScale = new Vector3 (finalSpikeRadius, finalSpikeHeight, finalSpikeRadius);

                    float spikeVelocity = (controllerVelocity / spikeSpeedReduction) + spikeMinSpeed;
                    Vector3 spikeEndPosition = spike.transform.position;
                    spikeEndPosition.y += spikePos.y + (finalSpikeHeight * 2);

                    SpikeMovement.CreateComponent (spike, spikeVelocity, spikeEndPosition);
                    hand.TriggerHapticPulse (1500);
                }
                allSpikes.Clear();
            }
            else
            {
                Destroy (spikeQuicksandOutline);
                playerEnergy.CancelEnergyUsage (hand);
            }
        }
        else if (WallIsActive ())
        {
            wall.AddComponent<WallProperties> ();
            playerEnergy.UseEnergy (firstHandHeld);
            ResetWallInfo ();
        }
    }

    private float GenerateSpikesTriangle (Vector3 centerLoc, float areaRadius, float triangleDist)
    {
        Vector3 vertex1 = new Vector3 (centerLoc.x, centerLoc.y, centerLoc.z + ((float) Math.Sqrt (3) * baseSpikeRadius / 3));
        Vector3 vertex2 = new Vector3 (centerLoc.x - (baseSpikeRadius / 2), centerLoc.y, centerLoc.z - ((float) Math.Sqrt (3) * baseSpikeRadius / 6));
        Vector3 vertex3 = new Vector3 (centerLoc.x + (baseSpikeRadius / 2), centerLoc.y, centerLoc.z - ((float) Math.Sqrt (3) * baseSpikeRadius / 6));

        allSpikes.Add (vertex1);
        allSpikes.Add (vertex2);
        allSpikes.Add (vertex3);
        return (areaRadius - triangleDist) + baseSpikeRadius;
    }

    private float GenerateSpikesHex (Vector3 position, Vector3 centerLoc, float height, float areaRadius)
    {
        float radius = areaRadius;
        allSpikes.Add (position);
        foreach (Vector2 locationOffset in spikeLocations)
        {
            float newX = position.x + (baseSpikeRadius * locationOffset.x);
            float newZ = position.z + (height * locationOffset.y);
            float newY = position.y; // TODO implement height checks
            Vector3 newPos = new Vector3 (newX, newY, newZ);
            if (!allSpikes.Contains (newPos))
            {
                float currentDistance = Vector3.Distance (newPos, centerLoc) + baseSpikeRadius;
                if (currentDistance > areaRadius)
                {
                    float layerNum = (float) Math.Floor ((areaRadius - baseSpikeRadius) / (baseSpikeRadius * 2));
                    return (layerNum != 0) ? (areaRadius - baseSpikeRadius) / (2 * layerNum) : areaRadius;
                }
                else
                {
                    Debug.Log("NewPos=" + newPos);
                    Debug.Log("NewPosDist=" + Vector3.Distance(newPos, centerLoc));
                    radius = GenerateSpikesHex (newPos, centerLoc, height, areaRadius);
                }
            }
        }
        return radius;
    }

    private void CancelAbility ()
    {
        if (WallIsActive ())
        {
            playerEnergy.UseEnergy (firstHandHeld);
            ResetWallInfo ();
        }
        else
        {
            playerEnergy.CancelEnergyUsage (hand);
            if (SpikeQuicksandIsActive ())
            {
                Destroy (spikeQuicksandOutline);
                spikeQuicksandOutline = null;
            }
            else if (WallOutlineIsActive ())
            {
                Destroy (wallOutline);
                ResetWallInfo ();
            }
        }
    }

    private void EnterDrawMode ()
    {
        if (firstHandHeld != null && firstHandHeld != hand)
        {
            wallOutline = Instantiate (wallOutlinePrefab) as GameObject;
            SetWallLocation ();
            firstHandHeld = null;
        }
        else
        {
            firstHandHeld = hand;
        }
    }

    private void ResetWallInfo ()
    {
        firstHandHeld = null;
        wallOutline = null;
        wall = null;
        lastAngle = 0;
        currentWallHeight = 0;
    }

    private Vector3 GetWallPosition ()
    {
        Vector3 thisArcPos = arc.GetEndPosition ();
        Vector3 otherArcPos = otherArc.GetEndPosition ();
        float wallPosX = (thisArcPos.x + otherArcPos.x) / 2;
        float wallPosY = Math.Min (thisArcPos.y, otherArcPos.y);
        float wallPosZ = (thisArcPos.z + otherArcPos.z) / 2;
        return new Vector3 (wallPosX, wallPosY, wallPosZ);
    }

    private void SetWallLocation ()
    {
        Vector3 wallPosition = GetWallPosition ();
        wallOutline.transform.position = new Vector3 (wallPosition.x, wallPosition.y, wallPosition.z);

        float remainingEnergy = playerEnergy.GetRemainingEnergy ();
        float maxHeight = remainingEnergy / (arc.GetEndPointsDistance (otherArc) * wallSizeMultiplier);
        float area = arc.GetEndPointsDistance (otherArc) * maxHeight;
        area = (float) Math.Round (area, 2) * wallSizeMultiplier;
        if (maxHeight <= 1f)
        {
            wallOutline.transform.localScale = new Vector3 (remainingEnergy / wallSizeMultiplier, 1f, 0.1f);
        }
        else if (arc.GetEndPointsDistance (otherArc) <= 1f)
        {
            wallOutline.transform.localScale = new Vector3 (1f, remainingEnergy / wallSizeMultiplier, 0.1f);
        }
        else
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

    private bool WallIsValid ()
    {
        OutlineProperties properties = wallOutline.GetComponentInChildren<OutlineProperties> ();
        return (arc.CanUseAbility () &&
            otherArc.CanUseAbility () &&
            !properties.CollisionDetected () &&
            Vector3.Distance (player.transform.position, wallOutline.transform.position) >= rockCreationDistance);
    }

    private bool SpikeQuicksandIsValid ()
    {
        OutlineProperties properties = spikeQuicksandOutline.GetComponentInChildren<OutlineProperties> ();
        return (arc.CanUseAbility () &&
            !properties.CollisionDetected ());
    }

    private bool RockIsActive ()
    {
        return rock != null;
    }

    private bool SpikeQuicksandIsActive ()
    {
        return spikeQuicksandOutline != null;
    }

    private bool WallIsActive ()
    {
        return wall != null;
    }

    private bool WallOutlineIsActive ()
    {
        return wallOutline != null;
    }

    private void SetOutlineMaterial (GameObject outlineObject, bool valid)
    {
        if (valid)
        {
            outlineObject.GetComponentInChildren<SkinnedMeshRenderer> ().material = validOutlineMat;
        }
        else
        {
            outlineObject.GetComponentInChildren<SkinnedMeshRenderer> ().material = invalidOutlineMat;
        }
    }

    public static void MakeSpikeAvailable (GameObject spike)
    {
        availableSpikes.Add (spike);
    }
}