using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;
using UnityEngine.AI;

public class PlayerAbility : MonoBehaviour
{
    public SteamVR_Input_Sources handType;
    public SteamVR_Behaviour_Pose controllerPose;
    public SteamVR_Action_Boolean grabAction;
    public SteamVR_Action_Boolean gripAction;
    public SteamVR_Action_Boolean drawAction;
    public Hand otherHand;
    public float baseSpikeRadius = 0.5f;

    [Header("Prefabs")]
    public GameObject playerAbilityAreaPrefab;
    public GameObject areaOutlinePrefab;
    public GameObject wallOutlinePrefab;
    public GameObject rockPrefab;
    public GameObject spikePrefab;
    public GameObject quicksandPrefab;
    public GameObject wallPrefab;

    [Header("Outline Materials")]
    public Material validOutlineMat;
    public Material invalidOutlineMat;

    [Header("Ability Values")]
    public float rockCreationDistance = 3f;
    public float numberOfRocksInCluster = 4;
    public float minRockDiameter = 0.25f;
    public float maxRockDimater = 1.5f;
    public float rockMassScale = 100f;
    public float spikeSpeedReduction = 10f;
    public float spikeMinSpeed = .05f;
    public float spikeMaxHeight = 1.75f;
    public LayerMask spikeLayerMask;
    public float energyPerSpikeInChain = 50;
    public float maxSpikesInChain = 50;
    public float maxSpikeDiameter = 5f;
    public float wallMaxHeight = 2f;
    public float wallSizeMultiplier = 120f;
    public float wallSpeedReduction = 50f;
    public float wallButtonClickDelay = 0.05f;

    private Hand hand;
    private PlayerEnergy playerEnergy;
    private ControllerArc arc;
    private ControllerArc otherArc;
    private GameObject player;
    private GameObject rock;
    private List<GameObject> spikeQuicksandOutlines;
    private float startingSpikeHandHeight;
    private GameObject abilityRing;

    private static GameObject wallOutline;
    private static GameObject wall;
    private static Hand firstHandHeld;
    private static Hand firstHandReleased;
    private static float lastAngle;
    private static float startingHandHeight;
    private static float currentWallHeight;

    private Vector2 horizontalSpikeChainVelocity;
    private static List<Vector2> spikeLocations;
    private HashSet<Vector3> allSpikes;
    private static List<GameObject> availableSpikes = new List<GameObject>();
    private static List<GameObject> availableRocks = new List<GameObject>();

    private static bool rockClusterEnabled = false;
    private static bool wallPushEnabled = false;
    private static bool spikeChainEnabled = false;

    private NavMeshSurface surface;
    private NavMeshSurface surfaceLight;

    private void Awake()
    {
        surface = GameObject.FindGameObjectWithTag("NavMesh").GetComponent<NavMeshSurface>();
        surfaceLight = GameObject.FindGameObjectWithTag("NavMesh Light").GetComponent<NavMeshSurface>();
        player = GameObject.FindWithTag("MainCamera");
        if (player != null)
        {
            playerEnergy = player.GetComponent<PlayerEnergy>();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        arc = GetComponentInChildren<ControllerArc>();
        otherArc = otherHand.GetComponentInChildren<ControllerArc>();
        hand = GetComponent<Hand>();

        abilityRing = Instantiate(playerAbilityAreaPrefab);
        MeshRenderer meshRenderer = abilityRing.GetComponentInChildren<MeshRenderer>();
        abilityRing.transform.localScale = new Vector3(rockCreationDistance * 2f * (1 / meshRenderer.bounds.size.x), 0.01f, rockCreationDistance * 2f * (1 / meshRenderer.bounds.size.x));
        abilityRing.transform.position = new Vector3(player.transform.position.x, player.transform.position.y, player.transform.position.z);

        RaycastHit hit;
        if (Physics.Raycast(abilityRing.transform.position, Vector3.down, out hit, 3f, spikeLayerMask))
        {
            abilityRing.transform.position += new Vector3(0, hit.point.y - abilityRing.transform.position.y, 0);
        }
        else
        {
            abilityRing.transform.position += new Vector3(0, -abilityRing.transform.position.y, 0);
        }


        allSpikes = new HashSet<Vector3>();
        spikeQuicksandOutlines = new List<GameObject>();

        spikeLocations = new List<Vector2>();
        spikeLocations.Add(new Vector2(2, 0));
        spikeLocations.Add(new Vector2(1, 1));
        spikeLocations.Add(new Vector2(-1, 1));
        spikeLocations.Add(new Vector2(-2, 0));
        spikeLocations.Add(new Vector2(-1, -1));
        spikeLocations.Add(new Vector2(1, -1));

        float maxRadius = (float) Math.Sqrt(playerEnergy.maxEnergy / 50);
        int numLayers = (int) Math.Floor((maxRadius - baseSpikeRadius) / (baseSpikeRadius * 2));
        int numSpikes = numLayers * spikeLocations.Count + 1;

        for (int i = 0; i < numSpikes; i++)
        {
            GameObject spike = Instantiate(spikePrefab) as GameObject;
            spike.transform.position = new Vector3(0, -10, 0);
            spike.SetActive(false);
            MakeSpikeAvailable(spike);
        }

        float numRocks = (numberOfRocksInCluster + 1) * RockProperties.GetRockLifetime() * 25;

        for (int i = 0; i < numRocks; i++)
        {
            GameObject rock = Instantiate(rockPrefab) as GameObject;
            rock.transform.position = new Vector3(0, -10, 0);
            rock.SetActive(false);
            MakeRockAvailable(rock);
        }
    }

    // Update is called once per frame
    void Update()
    {
        abilityRing.transform.position = new Vector3(player.transform.position.x, abilityRing.transform.position.y, player.transform.position.z);

        if (GripPress())
        {
            CancelAbility();
            if (arc.GetPointerHitObject() != null)
            {
                DestroyPointerHitObject();
            }
        }

        if (GrabPress())
        {
            TriggerNewAbility();
        }
        else if (GrabHold())
        {
            UpdateAbility();
        }
        else if (GrabRelease())
        {
            EndAbility();
        }

        if (DrawRelease() && playerEnergy.EnergyAboveThreshold(100f) && !RockIsActive() && !SpikeQuicksandIsActive() && !WallIsActive())
        {
            if (!WallOutlineIsActive())
            {
                EnterDrawMode();
            }
            else
            {
                ExitDrawMode();
            }
        }
        else if (WallOutlineIsActive())
        {
            playerEnergy.UpdateAbilityUseTime();
            SetWallLocation();
            SetOutlineMaterial(wallOutline, WallIsValid());
        }
    }

    public bool GrabHold()
    {
        return grabAction.GetState(handType);
    }

    private bool GrabPress()
    {
        return grabAction.GetStateDown(handType);
    }

    private bool GrabRelease()
    {
        return grabAction.GetStateUp(handType);
    }

    private bool DrawRelease()
    {
        return drawAction.GetStateUp(handType);
    }

    private bool GripPress()
    {
        return gripAction.GetStateDown(handType);
    }

    private void TriggerNewAbility()
    {
        if (WallOutlineIsActive())
        {
            if (firstHandHeld != null && firstHandHeld != hand)
            {
                OutlineProperties properties = wallOutline.GetComponentInChildren<OutlineProperties>();
                if (WallIsValid())
                {
                    wall = Instantiate(wallPrefab) as GameObject;
                    wall.transform.position = wallOutline.transform.position;
                    wall.transform.localScale = wallOutline.transform.localScale;
                    wall.transform.rotation = wallOutline.transform.rotation;
                    startingHandHeight = Math.Min(hand.transform.position.y, otherHand.transform.position.y);
                    playerEnergy.SetTempEnergy(firstHandHeld, 0);
                    Destroy(wallOutline);
                }
                else
                {
                    Destroy(wallOutline);
                    ResetWallInfo();
                }
                firstHandReleased = null;
            }
            else
            {
                firstHandHeld = hand;
            }
        }
        else if (!WallIsActive() && arc.CanUseAbility())
        {
            firstHandHeld = null;
            if (hand.currentAttachedObject != null)
            {
                if (hand.currentAttachedObject != otherHand.currentAttachedObject && GetRockEnergyCost(hand.currentAttachedObject) < playerEnergy.GetRemainingEnergy())
                {
                    rock = hand.currentAttachedObject;
                    Destroy(rock.GetComponent<RockProperties>());
                }
            }
            else if (arc.GetPointerHitObject().tag == "Rock")
            {
                if (GetRockEnergyCost(arc.GetPointerHitObject()) < playerEnergy.GetRemainingEnergy())
                {
                    rock = arc.GetPointerHitObject();
                    Destroy(rock.GetComponent<RockProperties>());
                    hand.AttachObject(rock, GrabTypes.Scripted);
                }
            }
            else if (arc.GetDistanceFromPlayer() <= rockCreationDistance)
            {
                rock = GetNewRock();
                rock.transform.position = new Vector3(arc.GetEndPosition().x, arc.GetEndPosition().y - 0.25f, arc.GetEndPosition().z);
                hand.AttachObject(rock, GrabTypes.Scripted);
            }
            else if (hand.hoveringInteractable == null && playerEnergy.EnergyAboveThreshold(200f))
            {
                spikeQuicksandOutlines.Add(Instantiate(areaOutlinePrefab));
                spikeQuicksandOutlines[0].transform.position = arc.GetEndPosition();
                startingSpikeHandHeight = hand.transform.position.y;

                if (spikeChainEnabled)
                {
                    Vector3 heading = spikeQuicksandOutlines[0].transform.position - player.transform.position;

                    float distance = heading.magnitude;
                    Vector3 velocity = (heading / distance);
                    horizontalSpikeChainVelocity = new Vector2(velocity.x, velocity.z).normalized;
                    playerEnergy.SetTempEnergy(hand, baseSpikeRadius * 2 * playerEnergy.maxEnergy / maxSpikeDiameter);
                }
            }
        }
    }

    private void UpdateAbility()
    {

        if (RockIsActive())
        {
            float rockEnergyCost = GetRockEnergyCost(rock);
            rockEnergyCost = (rockEnergyCost < 0) ? 0 : rockEnergyCost;
            rock.GetComponent<Rigidbody>().mass = rockMassScale * rock.transform.localScale.x;
            playerEnergy.SetTempEnergy(hand, rockEnergyCost);
            hand.SetAllowResize(playerEnergy.GetRemainingEnergy() > 0);
        }
        else if (SpikeQuicksandIsActive())
        {
            float handDistance = hand.transform.position.y - startingSpikeHandHeight;
            float size = (float) Math.Pow((Math.Abs(handDistance)) + (baseSpikeRadius * 2), 3);
            if (handDistance < 0 || !spikeChainEnabled)
            {
                GameObject spikeQuicksandOutline = spikeQuicksandOutlines[0];
                while (spikeQuicksandOutlines.Count > 1)
                {
                    GameObject outline = spikeQuicksandOutlines[1];
                    Destroy(outline);
                    spikeQuicksandOutlines.Remove(outline);
                }
                Vector3 newSize = new Vector3(size, 1f, size);
                float energyCost = spikeQuicksandOutline.transform.localScale.x * playerEnergy.maxEnergy / maxSpikeDiameter;
                playerEnergy.SetTempEnergy(hand, energyCost);
                if ((playerEnergy.EnergyIsNotZero() && energyCost <= playerEnergy.maxEnergy) || newSize.x < spikeQuicksandOutline.transform.localScale.x)
                {
                    spikeQuicksandOutline.transform.localScale = newSize;
                }
                SetOutlineMaterial(spikeQuicksandOutline, SpikeQuicksandIsValid(spikeQuicksandOutline));
            }
            else
            {
                foreach (GameObject outline in spikeQuicksandOutlines)
                {
                    SetOutlineMaterial(outline, SpikeQuicksandIsValid(outline));
                }

                float outlineSize = baseSpikeRadius * 2;
                Vector2 spikeChainOffset = Vector2.Perpendicular(horizontalSpikeChainVelocity);

                Vector3 arcPos = arc.GetEndPosition();
                int numOutlines = 1;
                float energyCost = outlineSize * playerEnergy.maxEnergy / maxSpikeDiameter;
                for (float i = outlineSize; i < (size - outlineSize); i += outlineSize)
                {
                    numOutlines++;
                    energyCost += energyPerSpikeInChain;
                    if (numOutlines > spikeQuicksandOutlines.Count && playerEnergy.EnergyIsNotZero() && energyCost <= playerEnergy.maxEnergy)
                    {
                        CorrectSpikeChainOutline(spikeChainOffset, true);

                        GameObject newOutline = Instantiate(areaOutlinePrefab) as GameObject;

                        float posX = arcPos.x + (i * spikeChainOffset.x) - (spikeChainOffset.x * spikeQuicksandOutlines.Count) / 2;
                        float posZ = arcPos.z + (i * spikeChainOffset.y) - (spikeChainOffset.y * spikeQuicksandOutlines.Count) / 2;
                        GameObject lastOutlinePlaced = spikeQuicksandOutlines[spikeQuicksandOutlines.Count - 1];

                        newOutline.transform.position = new Vector3(posX, lastOutlinePlaced.transform.position.y, posZ);
                        float verticleCorrection = CalculateOutlineVerticleCorrection(newOutline, out bool outOfBounds);
                        newOutline.transform.position += new Vector3(0, verticleCorrection, 0);

                        spikeQuicksandOutlines.Add(newOutline);
                        playerEnergy.SetTempEnergy(hand, energyCost);
                    }
                }

                while (numOutlines < spikeQuicksandOutlines.Count)
                {
                    GameObject removedOutline = spikeQuicksandOutlines[numOutlines];
                    Destroy(removedOutline);
                    spikeQuicksandOutlines.Remove(removedOutline);
                    energyCost -= energyPerSpikeInChain;
                    playerEnergy.SetTempEnergy(hand, energyCost);

                    CorrectSpikeChainOutline(spikeChainOffset, false);
                }
            }
        }
        else if (WallIsActive() && playerEnergy.EnergyIsNotZero())
        {
            hand.TriggerHapticPulse(1500);
            float newHandHeight = (Math.Min(hand.transform.position.y, otherHand.transform.position.y) - startingHandHeight) * 2f;
            if (newHandHeight < 1 && currentWallHeight < newHandHeight)
            {
                MeshRenderer meshRenderer = wallPrefab.GetComponentInChildren<MeshRenderer>();
                currentWallHeight = newHandHeight;
                Vector3 newPos = new Vector3(wall.transform.position.x, wallMaxHeight * newHandHeight, wall.transform.position.z);
                wall.transform.position = Vector3.MoveTowards(wall.transform.position, newPos, 1f);
                float area = (float) Math.Round(wall.transform.localScale.x * meshRenderer.bounds.size.x * wallMaxHeight * newHandHeight, 2) * wallSizeMultiplier;
                playerEnergy.SetTempEnergy(firstHandHeld, area);
            }
        }
    }

    private void CorrectSpikeChainOutline(Vector3 spikeChainOffset, bool addSpike)
    {
        if (addSpike)
        {
            spikeChainOffset *= -1;
        }
        foreach (GameObject outline in spikeQuicksandOutlines)
        {
            Vector3 outlinePos = outline.transform.position;
            float correctionX = spikeChainOffset.x / 2;
            float correctionZ = spikeChainOffset.y / 2;
            outline.transform.position = new Vector3(outlinePos.x + correctionX, outlinePos.y, outlinePos.z + correctionZ);
            float verticleCorrection = CalculateOutlineVerticleCorrection(outline, out bool outOfBounds);
            outline.transform.position += new Vector3(0, verticleCorrection, 0);
        }
    }

    private void EndAbility()
    {
        if (RockIsActive())
        {
            hand.DetachObject(rock);
            hand.SetAllowResize(true);
            if (otherHand.currentAttachedObject == rock)
            {
                float rockSize = (float) Math.Pow(Math.Floor(rock.transform.localScale.x * rock.transform.localScale.y * rock.transform.localScale.z), 3);
                playerEnergy.SetTempEnergy(hand, rockSize);
                playerEnergy.TransferHandEnergy(hand, otherHand);
                otherHand.GetComponent<PlayerAbility>().rock = rock;
            }
            else
            {
                rock.AddComponent<RockProperties>();
                rock.GetComponent<Rigidbody>().mass = rockMassScale * rock.transform.localScale.x;
                playerEnergy.UseEnergy(hand);
                hand.TriggerHapticPulse(500);

                if (rockClusterEnabled)
                {
                    for (int i = 0; i < numberOfRocksInCluster; i++)
                    {
                        GameObject newRock = GetNewRock();
                        newRock.AddComponent<RockProperties>();
                        Vector3 velocity, angularVelocity;
                        rock.GetComponent<Throwable>().GetReleaseVelocities(hand, out velocity, out angularVelocity);

                        newRock.transform.position = rock.transform.position;
                        newRock.transform.localScale = rock.transform.localScale;
                        newRock.GetComponent<Rigidbody>().velocity = velocity;
                        newRock.GetComponent<Rigidbody>().velocity = Vector3.ProjectOnPlane(UnityEngine.Random.insideUnitSphere, velocity) * (.75f + rock.transform.localScale.x) + velocity;
                        newRock.GetComponent<Rigidbody>().angularVelocity = newRock.transform.forward * angularVelocity.magnitude;
                    }
                }
                else
                {
                    PowerupController.IncrementRockClusterCounter();
                }
            }
            rock = null;
        }
        else if (SpikeQuicksandIsActive())
        {
            float controllerVelocity = controllerPose.GetVelocity().y;
            float handPos = (hand.transform.position.y - startingSpikeHandHeight);
            bool allOutlinesValid = true;
            foreach (GameObject outline in spikeQuicksandOutlines)
            {
                if (!SpikeQuicksandIsValid(outline))
                {
                    allOutlinesValid = false;
                }
            }
            if (handPos < 0 && allOutlinesValid)
            {
                GameObject spikeQuicksandOutline = spikeQuicksandOutlines[0];
                GameObject quicksand = Instantiate(quicksandPrefab) as GameObject;
                quicksand.transform.position = spikeQuicksandOutline.transform.position;
                quicksand.transform.localScale = new Vector3(spikeQuicksandOutline.transform.localScale.x, 1f, spikeQuicksandOutline.transform.localScale.z);
                quicksand.AddComponent<QuicksandProperties>();
                Destroy(spikeQuicksandOutline);
                spikeQuicksandOutlines.Remove(spikeQuicksandOutline);
                playerEnergy.UseEnergy(hand);
                hand.TriggerHapticPulse(800);
            }
            else if (handPos > 0 && controllerVelocity > 0 && allOutlinesValid)
            {
                if (spikeChainEnabled)
                {
                    playerEnergy.UseEnergy(hand);
                    float spikeVelocity = (controllerPose.GetVelocity().y / spikeSpeedReduction) + spikeMinSpeed;
                    while (spikeQuicksandOutlines.Count > 0)
                    {
                        GameObject outline = spikeQuicksandOutlines[0];
                        StartCoroutine(CreateChainSpike(outline, horizontalSpikeChainVelocity, spikeVelocity));
                        spikeQuicksandOutlines.Remove(outline);
                    }
                }
                else
                {
                    PowerupController.IncrementSpikeChainCounter();
                    GameObject spikeQuicksandOutline = spikeQuicksandOutlines[0];
                    float finalSpikeRadius = baseSpikeRadius;
                    float size = spikeQuicksandOutline.transform.localScale.x / 2;
                    float triangleDist = (2 * baseSpikeRadius / (float) Math.Sqrt(3)) + baseSpikeRadius;
                    if (size >= triangleDist && size < baseSpikeRadius * 3)
                    {
                        finalSpikeRadius = GenerateSpikesTriangle(spikeQuicksandOutline.transform.position, size, triangleDist);
                    }
                    else
                    {
                        float height = (float) Math.Sqrt(3) * baseSpikeRadius;
                        finalSpikeRadius = GenerateSpikesHex(spikeQuicksandOutline.transform.position, spikeQuicksandOutline.transform.position, height, size);
                    }
                    float radiusIncrease = finalSpikeRadius - baseSpikeRadius;

                    finalSpikeRadius = (finalSpikeRadius * 2) - 0.05f;
                    Vector3 centerLoc = spikeQuicksandOutline.transform.position;

                    Destroy(spikeQuicksandOutline);
                    spikeQuicksandOutlines.Remove(spikeQuicksandOutline);
                    playerEnergy.UseEnergy(hand);

                    foreach (Vector3 spikePos in allSpikes)
                    {
                        GameObject spike = GetNewSpike();

                        Vector3 spikeCorrection = (spikePos - centerLoc) * 0.33f;
                        Vector3 radiusCorrection = new Vector3(Math.Sign(spikeCorrection.x) * radiusIncrease, 0, Math.Sign(spikeCorrection.z) * radiusIncrease);
                        spike.transform.position = (spikePos - spikeCorrection) + radiusCorrection;

                        float layerNum = (float) Math.Floor(Vector3.Distance(spikePos, centerLoc) / (baseSpikeRadius * 2));
                        float layerScale = (float) Math.Pow(.8, layerNum);
                        float finalSpikeHeight = spikeMaxHeight * layerScale * UnityEngine.Random.Range(0.9f, 1f);
                        spike.transform.localScale = new Vector3(finalSpikeRadius, finalSpikeHeight, finalSpikeRadius);

                        float spikeVelocity = (controllerVelocity / spikeSpeedReduction) + spikeMinSpeed;
                        Vector3 spikeEndPosition = spike.transform.position;
                        spikeEndPosition.y += (finalSpikeHeight * spikeMaxHeight);

                        SpikeMovement.CreateComponent(spike, spikeVelocity, spikeEndPosition);
                        hand.TriggerHapticPulse(1500);
                    }
                    allSpikes.Clear();
                }
            }
            else
            {
                ClearSpikeQuicksandOutlines();
                playerEnergy.CancelEnergyUsage(hand);
            }
        }
        else if (WallIsActive())
        {
            float finalHandHeight = (Math.Min(hand.transform.position.y, otherHand.transform.position.y) - startingHandHeight) * 2f;
            if (finalHandHeight < 0.01f)
            {
                Destroy(wall);
                playerEnergy.CancelEnergyUsage(firstHandHeld);
            }
            else
            {
                wall.AddComponent<WallProperties>();
                wall.GetComponent<WallProperties>().wallHeightPercent = (Math.Min (hand.transform.position.y, otherHand.transform.position.y) - startingHandHeight) * 2f;
                playerEnergy.UseEnergy(firstHandHeld);
                if (wallPushEnabled)
                {
                    Vector3 velocity = new Vector3(controllerPose.GetVelocity().x, 0, controllerPose.GetVelocity().z);

                    wall.GetComponent<WallProperties>().direction = velocity.normalized;
                    wall.GetComponent<WallProperties>().wallMoveSpeed = velocity.magnitude / wallSpeedReduction;
                }
                else
                {
                    PowerupController.IncrementWallPushCounter();
                }

            }
            wall.GetComponent<CreateNavLink>().createLinks(wallMaxHeight);
            surface.BuildNavMesh();
            surfaceLight.BuildNavMesh();
            ResetWallInfo();
        }
    }

    private IEnumerator CreateChainSpike(GameObject outline, Vector2 spikeMoveDirection, float spikeVelocity)
    {
        int numSpikes = 0;
        float verticleCorrection = 0;
        while (true)
        {
            verticleCorrection = 0;
            GameObject spike = GetNewSpike();
            spike.transform.position = outline.transform.position;
            numSpikes++;

            float finalSpikeHeight = spikeMaxHeight * UnityEngine.Random.Range(0.9f, 1f);
            spike.transform.localScale = new Vector3(baseSpikeRadius * 2, finalSpikeHeight, baseSpikeRadius * 2);

            Vector3 spikeEndPosition = spike.transform.position;
            spikeEndPosition.y += (finalSpikeHeight * spikeMaxHeight);

            SpikeMovement.CreateComponent(spike, spikeVelocity, spikeEndPosition);
            hand.TriggerHapticPulse(1500);

            outline.transform.position += new Vector3(spikeMoveDirection.x, 0, spikeMoveDirection.y);

            bool outOfBounds;
            verticleCorrection = CalculateOutlineVerticleCorrection(outline, out outOfBounds);
            outline.transform.position += new Vector3(0, verticleCorrection, 0);
            if (!SpikeChainIsValid(outline) || numSpikes > maxSpikesInChain || outOfBounds)
            {
                Destroy(outline);
                break;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    private float CalculateOutlineVerticleCorrection(GameObject outline, out bool outOfBounds)
    {
        float verticleCorrection = 0;
        RaycastHit hit;
        if (Physics.Raycast(outline.transform.position + Vector3.up, Vector3.down, out hit, 1f, spikeLayerMask) ||
            Physics.Raycast(outline.transform.position, Vector3.down, out hit, 1f, spikeLayerMask))
        {
            if (hit.collider.tag == "Ground")
            {
                verticleCorrection = hit.point.y - outline.transform.position.y;
            }
            outOfBounds = false;
        }
        else
        {
            outOfBounds = true;
        }
        return verticleCorrection;
    }

    private float GenerateSpikesTriangle(Vector3 centerLoc, float areaRadius, float triangleDist)
    {
        Vector3 vertex1 = new Vector3(centerLoc.x, centerLoc.y, centerLoc.z + ((float) Math.Sqrt(3) * baseSpikeRadius / 3));
        Vector3 vertex2 = new Vector3(centerLoc.x - (baseSpikeRadius / 2), centerLoc.y, centerLoc.z - ((float) Math.Sqrt(3) * baseSpikeRadius / 6));
        Vector3 vertex3 = new Vector3(centerLoc.x + (baseSpikeRadius / 2), centerLoc.y, centerLoc.z - ((float) Math.Sqrt(3) * baseSpikeRadius / 6));

        allSpikes.Add(vertex1);
        allSpikes.Add(vertex2);
        allSpikes.Add(vertex3);
        return ((areaRadius - triangleDist) / 2) + baseSpikeRadius;
    }

    private float GenerateSpikesHex(Vector3 position, Vector3 centerLoc, float height, float areaRadius)
    {
        float radius = areaRadius;
        allSpikes.Add(position);
        foreach (Vector2 locationOffset in spikeLocations)
        {
            float newX = position.x + (baseSpikeRadius * locationOffset.x);
            float newZ = position.z + (height * locationOffset.y);
            float newY = position.y;
            Vector3 newPos = new Vector3(newX, newY, newZ);
            if (!SpikeApproximatelyEqual(newPos))
            {
                float currentDistance = Vector3.Distance(newPos, centerLoc) + baseSpikeRadius;
                if (currentDistance > areaRadius)
                {
                    float layerNum = (float) Math.Floor((areaRadius - baseSpikeRadius) / (baseSpikeRadius * 2));
                    return (layerNum != 0) ? (areaRadius - baseSpikeRadius) / (2 * layerNum) : areaRadius;
                }
                else
                {
                    radius = GenerateSpikesHex(newPos, centerLoc, height, areaRadius);
                }
            }
        }
        return radius;
    }

    private bool SpikeApproximatelyEqual(Vector3 newPos)
    {
        foreach (Vector3 spike in allSpikes)
        {
            if (Vector3.SqrMagnitude(spike - newPos) < 0.0001)
            {
                return true;
            }
        }
        return false;
    }

    private void CancelAbility()
    {
        if (WallIsActive())
        {
            wall.AddComponent<WallProperties>();
            wall.GetComponent<WallProperties>().wallHeightPercent = (Math.Min (hand.transform.position.y, otherHand.transform.position.y) - startingHandHeight) * 2f;
            playerEnergy.UseEnergy(firstHandHeld);
            ResetWallInfo();
        }
        else
        {
            playerEnergy.CancelEnergyUsage(hand);
            if (SpikeQuicksandIsActive())
            {
                ClearSpikeQuicksandOutlines();
            }
            else if (WallOutlineIsActive())
            {
                Destroy(wallOutline);
                ResetWallInfo();
            }
        }
    }

    private void DestroyPointerHitObject()
    {
        GameObject hitObject = arc.GetPointerHitObject();
        if (hitObject.tag == "Wall" || hitObject.tag == "Quicksand")
        {
            Destroy(hitObject);
        }
    }

    private void WallButtonsNotSimultaneous()
    {
        firstHandHeld = null;
        firstHandReleased = null;
    }

    private void EnterDrawMode()
    {
        if (firstHandHeld != null && firstHandHeld != hand)
        {
            firstHandHeld.GetComponent<PlayerAbility>().CancelInvoke("WallButtonsNotSimultaneous");
            wallOutline = Instantiate(wallOutlinePrefab) as GameObject;
            SetWallLocation();
            firstHandHeld = null;
        }
        else
        {
            firstHandHeld = hand;
            Invoke("WallButtonsNotSimultaneous", wallButtonClickDelay);
        }
    }

    private void ExitDrawMode()
    {
        if (firstHandReleased != null && firstHandReleased != hand)
        {
            firstHandReleased.GetComponent<PlayerAbility>().CancelInvoke("WallButtonsNotSimultaneous");
            Destroy(wallOutline);
            ResetWallInfo();
            firstHandReleased = null;
        }
        else
        {
            firstHandReleased = hand;
            Invoke("WallButtonsNotSimultaneous", wallButtonClickDelay);
        }
    }

    private void ResetWallInfo()
    {
        firstHandHeld = null;
        wallOutline = null;
        wall = null;
        lastAngle = 0;
        currentWallHeight = 0;
    }

    private void SetWallPosition()
    {
        Vector3 thisArcPos = arc.GetEndPosition();
        Vector3 otherArcPos = otherArc.GetEndPosition();

        float wallPosX = (thisArcPos.x + otherArcPos.x) / 2;
        float wallPosY = Math.Min(thisArcPos.y, otherArcPos.y);
        float wallPosZ = (thisArcPos.z + otherArcPos.z) / 2;
        wallOutline.transform.position = new Vector3(wallPosX, wallPosY, wallPosZ);

        float verticleCorrection = CalculateOutlineVerticleCorrection(wallOutline, out bool outOfBounds);
        wallOutline.transform.position += new Vector3(0, verticleCorrection, 0);
    }

    private void SetWallLocation()
    {
        SetWallPosition();
        
        MeshRenderer meshRenderer = wallPrefab.GetComponentInChildren<MeshRenderer>();

        float remainingEnergy = playerEnergy.GetRemainingEnergy();
        float maxHandDist = remainingEnergy / (wallSizeMultiplier * wallMaxHeight);
        float handDistance = (arc.GetEndPointsDistance(otherArc) < maxHandDist)
            ? arc.GetEndPointsDistance(otherArc)
            : maxHandDist;
        float wallWidth = ((handDistance - meshRenderer.bounds.size.x) / meshRenderer.bounds.size.x) + 1;
;
        wallOutline.transform.localScale = new Vector3(wallWidth, wallOutline.transform.localScale.y, wallOutline.transform.localScale.z);

        float angle = Vector3.SignedAngle(arc.GetEndPosition() - otherArc.GetEndPosition(), wallOutline.transform.position, new Vector3(0, -1, 0));
        angle += Vector3.SignedAngle(wallOutline.transform.position, new Vector3(1, 0, 0), new Vector3(0, -1, 0));
        float newAngle = angle;
        angle -= lastAngle;
        if (Math.Abs(angle) >= 0.5f)
        {
            lastAngle = newAngle;
            wallOutline.transform.Rotate(0, angle, 0, Space.Self);
        }
    }

    private bool WallIsValid()
    {
        OutlineProperties properties = wallOutline.GetComponentInChildren<OutlineProperties>();
        return (arc.CanUseAbility() &&
            otherArc.CanUseAbility() &&
            !properties.CollisionDetected() &&
            Vector3.Distance(player.transform.position, wallOutline.transform.position) >= rockCreationDistance);
    }

    private bool SpikeQuicksandIsValid(GameObject spikeQuicksandOutline)
    {
        OutlineProperties properties = spikeQuicksandOutline.GetComponentInChildren<OutlineProperties>();
        return (arc.CanUseAbility() &&
            !properties.CollisionDetected());
    }

    private bool SpikeChainIsValid(GameObject spikeInChain)
    {
        OutlineProperties properties = spikeInChain.GetComponentInChildren<OutlineProperties>();
        return (!properties.CollisionDetected());
    }

    private bool RockIsActive()
    {
        return rock != null;
    }

    private bool SpikeQuicksandIsActive()
    {
        return spikeQuicksandOutlines.Count != 0;
    }

    private bool WallIsActive()
    {
        return wall != null;
    }

    private bool WallOutlineIsActive()
    {
        return wallOutline != null;
    }

    private void SetOutlineMaterial(GameObject outlineObject, bool valid)
    {
        if (valid)
        {
            outlineObject.GetComponentInChildren<MeshRenderer>().material = validOutlineMat;
        }
        else
        {
            outlineObject.GetComponentInChildren<MeshRenderer>().material = invalidOutlineMat;
        }
    }

    private GameObject GetNewRock()
    {
        GameObject newRock;
        if (availableRocks.Count != 0)
        {
            newRock = availableRocks[0];
            newRock.SetActive(true);
            availableRocks.Remove(newRock);
        }
        else
        {
            newRock = Instantiate(rockPrefab) as GameObject;
        }
        return newRock;
    }

    private GameObject GetNewSpike()
    {
        GameObject spike;
        if (availableSpikes.Count != 0)
        {
            spike = availableSpikes[0];
            spike.SetActive(true);
            availableSpikes.Remove(spike);
        }
        else
        {
            spike = Instantiate(spikePrefab) as GameObject;
        }
        return spike;
    }

    private void ClearSpikeQuicksandOutlines()
    {
        foreach (GameObject outline in spikeQuicksandOutlines)
        {
            Destroy(outline);
        }
        spikeQuicksandOutlines.Clear();
    }

    private float GetRockEnergyCost(GameObject rock)
    {
        float range = maxRockDimater - minRockDiameter;
        return (rock.transform.localScale.x - minRockDiameter) * playerEnergy.maxEnergy / range;
    }

    public static void MakeSpikeAvailable(GameObject spike)
    {
        availableSpikes.Add(spike);
    }

    public static void MakeRockAvailable(GameObject spike)
    {
        availableRocks.Add(spike);
    }

    public static void ToggleRockCluster()
    {
        rockClusterEnabled = !rockClusterEnabled;
    }

    public static void ToggleSpikeChain()
    {
        spikeChainEnabled = !spikeChainEnabled;
    }

    public static void ToggleWallPush()
    {
        wallPushEnabled = !wallPushEnabled;
    }

    public static bool RockClusterEnabled()
    {
        return rockClusterEnabled;
    }

    public static bool SpikeChainEnabled()
    {
        return spikeChainEnabled;
    }

    public static bool WallPushEnabled()
    {
        return wallPushEnabled;
    }
}