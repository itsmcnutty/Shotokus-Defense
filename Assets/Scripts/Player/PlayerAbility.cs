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
    public Material validOutlineMat;
    public Material invalidOutlineMat;

    [Header("Ability Parameters")]

    public float rockCreationDistance = 3f;
    public LayerMask outlineLayerMask;

    private Hand hand;
    private PlayerEnergy playerEnergy;
    private ControllerArc arc;
    private ControllerArc otherArc;
    private GameObject player;
    private GameObject abilityRing;

    private static bool rockClusterEnabled = false;
    private static bool wallPushEnabled = false;
    private static bool spikeChainEnabled = false;
    private static bool earthquakeEnabled = false;

    private Rocks rocks;
    public GameObject activeRock;

    private SpikeQuicksand spikeQuicksand;
    private List<GameObject> spikeQuicksandOutlines = new List<GameObject>();
    private float startingSpikeHandHeight;
    private Vector2 horizontalSpikeChainVelocity;

    private Walls walls;


    private void Awake()
    {
        player = GameObject.FindWithTag("MainCamera");
        if (player != null)
        {
            playerEnergy = player.GetComponent<PlayerEnergy>();
            rocks = Rocks.CreateComponent(player, playerEnergy);
            spikeQuicksand = SpikeQuicksand.CreateComponent(player, areaOutlinePrefab, playerEnergy, validOutlineMat, invalidOutlineMat, outlineLayerMask);
            walls = Walls.CreateComponent(player, wallOutlinePrefab, playerEnergy, validOutlineMat, invalidOutlineMat, rockCreationDistance, outlineLayerMask);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        rocks.InitRocks();
        spikeQuicksand.InitSpikes();

        arc = GetComponentInChildren<ControllerArc>();
        otherArc = otherHand.GetComponentInChildren<ControllerArc>();
        hand = GetComponent<Hand>();

        abilityRing = Instantiate(playerAbilityAreaPrefab);
        MeshRenderer meshRenderer = abilityRing.GetComponentInChildren<MeshRenderer>();
        abilityRing.transform.localScale = new Vector3(rockCreationDistance * 2f * (1 / meshRenderer.bounds.size.x), 0.01f, rockCreationDistance * 2f * (1 / meshRenderer.bounds.size.x));
        abilityRing.transform.position = new Vector3(player.transform.position.x, player.transform.position.y, player.transform.position.z);

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

    // Update is called once per frame
    void Update()
    {
        abilityRing.transform.position = new Vector3(player.transform.position.x, abilityRing.transform.position.y, player.transform.position.z);

        if (GripPress())
        {
            CancelAbility();
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

        if (DrawRelease() && playerEnergy.EnergyAboveThreshold(100f) && !RockIsActive() && !SpikeQuicksandIsActive() && !walls.WallIsActive())
        {
            if (!walls.WallOutlineIsActive())
            {
                walls.EnterDrawMode(hand, otherHand);
            }
            else
            {
                walls.ExitDrawMode(hand);
            }
        }
        else if (walls.WallOutlineIsActive())
        {
            walls.ActiveDrawMode(arc, otherArc);
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
            walls.CreateNewWall(hand, otherHand);
        }
        else if (!walls.WallIsActive())
        {
            if (hand.currentAttachedObject != null)
            {
                activeRock = rocks.PickupRock(hand.currentAttachedObject, hand, otherHand);
            }
            else if(hand.hoveringInteractable != null && hand.hoveringInteractable.tag == "Rock")
            {
                activeRock = rocks.PickupRock(hand.hoveringInteractable.gameObject, hand, otherHand);
            }
            else if (arc.CanUseAbility())
            {
                if (arc.GetDistanceFromPlayer() <= rockCreationDistance)
                {
                    activeRock = rocks.CreateNewRock(hand, arc);
                }
                else if (playerEnergy.EnergyAboveThreshold(200f))
                {
                    spikeQuicksandOutlines.Add(spikeQuicksand.IntializeOutline(hand, player, out startingSpikeHandHeight, out horizontalSpikeChainVelocity));
                }
            }
        }
    }

    private void UpdateAbility()
    {

        if (RockIsActive())
        {
            rocks.UpdateRock(activeRock, hand);
        }
        else if (SpikeQuicksandIsActive())
        {
            spikeQuicksandOutlines = spikeQuicksand.UpdateOutline(spikeQuicksandOutlines, hand, controllerPose, startingSpikeHandHeight, horizontalSpikeChainVelocity);
        }
        else if (walls.WallIsActive() && playerEnergy.EnergyIsNotZero())
        {
            walls.UpdateWallHeight(hand, otherHand, controllerPose);
        }
    }

    private void EndAbility()
    {
        if (RockIsActive())
        {
            rocks.ThrowRock(activeRock, hand, otherHand);
            activeRock = null;
        }
        else if (SpikeQuicksandIsActive())
        {
            spikeQuicksand.TryCreateSpikesOrQuicksand(spikeQuicksandOutlines, hand, controllerPose, startingSpikeHandHeight, horizontalSpikeChainVelocity);
            spikeQuicksand.ClearSpikeQuicksandOutlines(spikeQuicksandOutlines);
        }
        else if (walls.WallIsActive())
        {
            walls.EndCreateWall(hand, otherHand, controllerPose);
        }
    }

    private void CancelAbility()
    {
        if (hand.hoveringInteractable != null && hand.hoveringInteractable.gameObject.tag == "Rock")
        {
            hand.hoveringInteractable = null;
        }
        else if(hand.currentAttachedObject != null && activeRock == null)
        {
            hand.DetachObject(hand.currentAttachedObject);
        }
        else if (walls.WallIsActive())
        {
            walls.CancelWall(hand, otherHand);
        }
        else if (SpikeQuicksandIsActive())
        {
            playerEnergy.CancelEnergyUsage(hand);
            spikeQuicksand.CancelSpikes(spikeQuicksandOutlines);
        }
        else if (walls.WallOutlineIsActive())
        {
            playerEnergy.CancelEnergyUsage(hand);
            walls.CancelWallOutline();
        }
        else if (arc.GetPointerHitObject() != null)
        {
            DestroyPointerHitObject();
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

    public bool RockIsActive()
    {
        return activeRock != null;
    }

    public bool SpikeQuicksandIsActive()
    {
        return spikeQuicksandOutlines.Count != 0;
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

    public static bool EarthquakeEnabled()
    {
        return earthquakeEnabled;
    }
}