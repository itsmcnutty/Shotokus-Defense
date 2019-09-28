using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class PlayerAbility : MonoBehaviour
{
    [Header("Steam VR")]
    public SteamVR_Input_Sources handType;
    public SteamVR_Behaviour_Pose controllerPose;
    public SteamVR_Action_Boolean grabAction;
    public SteamVR_Action_Boolean gripAction;
    public SteamVR_Action_Boolean drawAction;
    public Hand otherHand;

    [Header("Outline Prefabs")]
    public GameObject playerAbilityAreaPrefab;
    public GameObject areaOutlinePrefab;
    public GameObject wallOutlinePrefab;

    [Header("Outline Materials")]
    public Material invalidOutlineMat;

    [Header("Ability Parameters")]

    public float rockCreationDistance = 3f;
    public static float RockCreationDistance = 0f;
    public LayerMask outlineLayerMask;

    private Hand hand;
    private PlayerEnergy playerEnergy;
    private ControllerArc arc;
    private ControllerArc otherArc;
    private GameObject player;
    private GameObject abilityRing;

    private static bool rockAbilityEnabled;

    public static bool RockAbilityEnabled
    {
        get
        {
            return rockAbilityEnabled;
        }
    }
    private static bool spikeAbilityEnabled;
    public static bool SpikeAbilityEnabled
    {
        get
        {
            return spikeAbilityEnabled;
        }
    }
    private static bool quicksandAbilityEnabled;
    public static bool QuicksandAbilityEnabled
    {
        get
        {
            return quicksandAbilityEnabled;
        }
    }
    private static bool wallAbilityEnabled;
    public static bool WallAbilityEnabled
    {
        get
        {
            return wallAbilityEnabled;
        }
    }

    private static bool rockClusterEnabled;
    public static bool RockClusterEnabled
    {
        get
        {
            return rockClusterEnabled;
        }
    }
    private static bool wallPushEnabled;
    public static bool WallPushEnabled
    {
        get
        {
            return wallPushEnabled;
        }
    }
    private static bool spikeChainEnabled;
    public static bool SpikeChainEnabled
    {
        get
        {
            return spikeChainEnabled;
        }
    }
    private static bool earthquakeEnabled;
    public static bool EarthquakeEnabled
    {
        get
        {
            return earthquakeEnabled;
        }
    }

    private Rocks rocks;
    public GameObject activeRock;

    private SpikeQuicksand spikeQuicksand;
    private List<GameObject> spikeQuicksandOutlines = new List<GameObject>();
    private float startingSpikeHandHeight;
    private Vector2 horizontalSpikeChainVelocity;

    private Walls walls;

    private void Awake()
    {
        // Initialized the player and abilities
        player = GameObject.FindWithTag("MainCamera");
        if (player != null)
        {
            playerEnergy = player.GetComponent<PlayerEnergy>();
            rocks = Rocks.CreateComponent(player, playerEnergy);
            spikeQuicksand = SpikeQuicksand.CreateComponent(player, areaOutlinePrefab, playerEnergy, invalidOutlineMat, outlineLayerMask);
            walls = Walls.CreateComponent(player, wallOutlinePrefab, playerEnergy, invalidOutlineMat, rockCreationDistance, outlineLayerMask);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // Initializes the rocks and spikes available to the player from the start to avoid mid-game performance issues
        rocks.InitRocks();
        spikeQuicksand.InitSpikes();

        RockCreationDistance = rockCreationDistance;

        arc = GetComponentInChildren<ControllerArc>();
        otherArc = otherHand.GetComponentInChildren<ControllerArc>();
        hand = GetComponent<Hand>();

        // Sets up the ring around the player
        abilityRing = Instantiate(playerAbilityAreaPrefab);
        MeshRenderer meshRenderer = abilityRing.GetComponentInChildren<MeshRenderer>();
        float abilityRingSize = rockCreationDistance * 2f * (1 / meshRenderer.bounds.size.x);
        abilityRing.transform.localScale = new Vector3(abilityRingSize, 0.01f, abilityRingSize);
    }

    // Update is called once per frame
    void Update()
    {
        // Repositions the ability ring when the player moves
        abilityRing.transform.position = new Vector3(player.transform.position.x, abilityRing.transform.position.y, player.transform.position.z);

        // Grip is cancel
        if (GripPress())
        {
            CancelAbility();
        }

        // Grab is trigger
        if (GrabPress() && !IsUIInteractable())
        {
            TriggerNewAbility();
        }
        else if (GrabHold())
        {
            UpdateAbility();
        }
        else if (GrabRelease())
        {
            if(IsUIInteractable())
            {
                ActivateUIInteractable();
            }
            else
            {
                EndAbility();
            }
        }

        // Draw is trackpad: can only be activated with enough energy and no other active abilities
        if (DrawRelease() && wallAbilityEnabled && playerEnergy.EnergyAboveThreshold(100f) && !RockIsActive() && !SpikeQuicksandIsActive() && !walls.WallIsActive())
        {
            if (!walls.WallOutlineIsActive())
            {
                walls.EnterDrawMode(hand, otherHand);
            }
            else if (!walls.OneHandHeld())
            {
                walls.ExitDrawMode(hand);
            }
        }
        else if (walls.WallOutlineIsActive())
        {
            walls.ActiveDrawMode(arc, otherArc);
        }

        if (RockIsActive())
        {
            if (activeRock.GetComponent<RockProperties>().CollidedWithEnemy())
            {
                hand.TurnOffVelocityMovement();
            }
            else if (activeRock.GetComponent<RockProperties>().CollidedWithNonEnemy())
            {
                hand.TurnOnVelocityMovement();
            }
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
        if (walls.WallOutlineIsActive())
        {
            // Creates a new wall when wall outline is active
            walls.CreateNewWall(hand, otherHand);
        }
        else if (!walls.WallIsActive())
        {
            // Tries to perform another action when not the wall
            if (hand.currentAttachedObject != null)
            {
                // Picks up rock if one has been attached
                activeRock = rocks.PickupRock(hand.currentAttachedObject, hand, otherHand);
            }
            else if (hand.hoveringInteractable != null && hand.hoveringInteractable.tag == "Rock")
            {
                // Picks up rock if one has been hovered
                activeRock = rocks.PickupRock(hand.hoveringInteractable.gameObject, hand, otherHand);
            }
            else if (arc.CanUseAbility())
            {
                // Tries to create a new ability
                if (rockAbilityEnabled && arc.GetDistanceFromPlayer() <= rockCreationDistance)
                {
                    // Creates rock if within the radius
                    activeRock = rocks.CreateNewRock(hand, arc);
                }
                else if ((spikeAbilityEnabled || quicksandAbilityEnabled) && playerEnergy.EnergyAboveThreshold(200f))
                {
                    // Creates a spike / quicksand outline when above the threshold
                    spikeQuicksandOutlines.Add(spikeQuicksand.IntializeOutline(hand, player, out startingSpikeHandHeight, out horizontalSpikeChainVelocity));
                }
            }
        }
    }

    private void UpdateAbility()
    {

        if (RockIsActive())
        {
            if (hand.currentAttachedObject == otherHand.currentAttachedObject)
            {
                // Regrows the rock when both hands have grabbed the current rock
                rocks.UpdateRock(activeRock, hand);
            }
            else
            {
                rocks.StopRegrowthParticles(activeRock);
            }
            playerEnergy.UpdateAbilityUseTime();
        }
        else if (SpikeQuicksandIsActive())
        {
            // Regrows the spike / quicksand area
            spikeQuicksandOutlines = spikeQuicksand.UpdateOutline(spikeQuicksandOutlines, hand, controllerPose, startingSpikeHandHeight, horizontalSpikeChainVelocity);
        }
        else if (walls.WallIsActive() && playerEnergy.EnergyIsNotZero())
        {
            // Changes the height of the wall while the player still has the energy to do so
            walls.UpdateWallHeight(hand, otherHand, controllerPose);
        }
    }

    private void EndAbility()
    {
        if (RockIsActive())
        {
            // Throws and resets the current rock
            rocks.ThrowRock(activeRock, hand, otherHand);
            activeRock = null;
        }
        else if (SpikeQuicksandIsActive())
        {
            // Creates the spike / quicksand and removes all outlines created
            spikeQuicksand.TryCreateSpikesOrQuicksand(spikeQuicksandOutlines, hand, otherHand, controllerPose, startingSpikeHandHeight, horizontalSpikeChainVelocity);
            spikeQuicksand.ClearSpikeQuicksandOutlines(spikeQuicksandOutlines);
        }
        else if (walls.WallIsActive())
        {
            // Resets wall information and performs the power-up if enabled
            walls.EndCreateWall(hand, otherHand, controllerPose);
        }
    }

    public void CancelAbility()
    {
        if (hand.hoveringInteractable != null && hand.hoveringInteractable.gameObject.tag == "Rock")
        {
            // Prevents the player from picking up rocks with the grip button
            hand.hoveringInteractable = null;
        }
        else if (hand.currentAttachedObject != null && activeRock == null)
        {
            // Prevents the player from picking up rocks with the grip button
            hand.DetachObject(hand.currentAttachedObject);
        }
        else if (walls.WallIsActive())
        {
            // Cancels the wall outline
            walls.CancelWall(hand, otherHand);
        }
        else if (SpikeQuicksandIsActive())
        {
            // Cancels the ability areas
            playerEnergy.CancelEnergyUsage(hand);
            spikeQuicksand.CancelSpikes(spikeQuicksandOutlines);
        }
        else if (walls.WallOutlineIsActive())
        {
            // Stops the regrowth of the wall
            playerEnergy.CancelEnergyUsage(hand);
            walls.CancelWallOutline();
        }
        else if (arc.GetPointerHitObject() != null)
        {
            // Tries to destroy the pointed at object
            DestroyPointerHitObject();
        }
    }

    private void DestroyPointerHitObject()
    {
        GameObject hitObject = arc.GetPointerHitObject();
        if (hitObject.CompareTag("Wall"))
        {
            // Destroys the highlighted wall or quicksand
            Destroy(hitObject.transform.parent.gameObject);
        }
    }

    private bool IsUIInteractable()
    {
        GameObject hitObject = arc.GetPointerHitObject();
        if (hitObject && hitObject.GetComponent<FloatInteractable>())
        {
            return true;
        }
        return false;
    }

    private void ActivateUIInteractable()
    {
        GameObject hitObject = arc.GetPointerHitObject();
        if (hitObject && hitObject.name.Equals("Show Tutorial Sphere"))
        {
            TutorialController.Instance.ShowTutorial();
        }
        else if (hitObject && hitObject.name.Equals("Start Wave Sphere"))
        {
            TutorialController.Instance.StartWave();
        }
        else if (hitObject && hitObject.name.Equals("Start Tutorial Sphere"))
        {
            TutorialController.Instance.StartTutorial();
        }
        else if (hitObject && hitObject.name.Equals("Next Slide Star"))
        {
            TutorialController.Instance.NextSlide();
        }
        else if (hitObject && hitObject.name.Equals("Back Slide Star"))
        {
            TutorialController.Instance.PreviousSlide();
        }
        else if (hitObject && hitObject.name.Equals("Teleport Sphere"))
        {
            GameController.Instance.Teleport(true);
        }
    }

    public bool RockIsActive()
    {
        return activeRock != null;
    }

    public bool SpikeQuicksandIsActive()
    {
        return spikeQuicksandOutlines.Count != 0;
    }

    public static void ToggleRockAbility()
    {
        rockAbilityEnabled = !rockAbilityEnabled;
        PowerupController.Instance.rockClusterCanvas.SetActive(!PowerupController.Instance.rockClusterCanvas.activeSelf);
    }

    public static void ToggleSpikeAbility()
    {
        spikeAbilityEnabled = !spikeAbilityEnabled;
        PowerupController.Instance.spikeChainCanvas.SetActive(!PowerupController.Instance.spikeChainCanvas.activeSelf);
    }

    public static void ToggleWallAbility()
    {
        wallAbilityEnabled = !wallAbilityEnabled;
        PowerupController.Instance.wallPushCanvas.SetActive(!PowerupController.Instance.wallPushCanvas.activeSelf);
    }

    public static void ToggleQuicksandAbility()
    {
        quicksandAbilityEnabled = !quicksandAbilityEnabled;
        PowerupController.Instance.earthquakeCanvas.SetActive(!PowerupController.Instance.earthquakeCanvas.activeSelf);
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

    public static void ToggleEarthquake()
    {
        earthquakeEnabled = !earthquakeEnabled;
    }

    public static void TurnOffAllAbilities()
    {
        rockAbilityEnabled = false;
        spikeAbilityEnabled = false;
        wallAbilityEnabled = false;
        quicksandAbilityEnabled = false;
        TurnOffAllPowerups(true, true);
    }

    public static void TurnOffAllPowerups(bool resetCounter, bool turnCanvasOff)
    {
        rockClusterEnabled = false;
        spikeChainEnabled = false;
        wallPushEnabled = false;
        earthquakeEnabled = false;

        if(resetCounter)
        {
            PowerupController.Instance.ResetCounters();
        }

        if(turnCanvasOff)
        {
            PowerupController.Instance.TurnCanvasOff();
        }
    }

    public static IEnumerator LongVibration(Hand hand, float length, ushort strength)
    {
        for (float i = 0; i < length; i += Time.deltaTime)
        {
            hand.TriggerHapticPulse(strength);
            yield return new WaitForEndOfFrame(); //every single frame for the duration of "length" you will vibrate at "strength" amount
        }
    }

    public static float CalculateOutlineVerticleCorrection(GameObject outline, LayerMask outlineLayerMask, out bool outOfBounds)
    {
        float verticleCorrection = 0;
        RaycastHit hit;
        // Performs a raycast from above and below the current position
        if (Physics.Raycast(outline.transform.position + Vector3.up, Vector3.down, out hit, 2f, outlineLayerMask) ||
            Physics.Raycast(outline.transform.position, Vector3.down, out hit, 2f, outlineLayerMask))
        {
            if (hit.collider.tag == "Ground")
            {
                // Sets the difference in height if it collides with the ground
                verticleCorrection = hit.point.y - outline.transform.position.y;
            }

            // Hitting anything means that the spike is still in the map
            outOfBounds = false;
        }
        else
        {
            // Hitting nothing means that the spike is out of bounds, or that the next position on the earth was too far down for the spike to move
            outOfBounds = true;
        }
        return verticleCorrection;
    }

    public static void SetOutlineMaterial(GameObject outlineObject, bool valid, Material validOutlineMat, Material invalidOutlineMat)
    {
        // Sets the material of the outline based on whether it's a valid outline
        if (valid)
        {
            outlineObject.GetComponentInChildren<MeshRenderer>().material = validOutlineMat;
        }
        else
        {
            outlineObject.GetComponentInChildren<MeshRenderer>().material = invalidOutlineMat;
        }
    }

    public IEnumerator RepositionAbilityRing()
    {
        yield return new WaitForEndOfFrame();

        abilityRing.transform.position = new Vector3(player.transform.position.x, player.transform.position.y, player.transform.position.z);

        // Changes height of the ability ring to be at the players feet
        RaycastHit hit;
        if (Physics.Raycast(abilityRing.transform.position, Vector3.down, out hit, 3f, outlineLayerMask))
        {
            abilityRing.transform.position += new Vector3(0, hit.point.y - abilityRing.transform.position.y, 0);
        }
        else
        {
            abilityRing.transform.position += new Vector3(0, -abilityRing.transform.position.y, 0);
        }
    }
}