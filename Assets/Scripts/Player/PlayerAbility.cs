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
    private SpikeQuicksand spikeQuicksand;
    private Walls walls;

    private void Awake()
    {
        player = GameObject.FindWithTag("MainCamera");
        if (player != null)
        {
            playerEnergy = player.GetComponent<PlayerEnergy>();
        }
        rocks = Rocks.CreateComponent(gameObject, rockPrefab, playerEnergy);
        spikeQuicksand = SpikeQuicksand.CreateComponent(gameObject, spikePrefab, quicksandPrefab, areaOutlinePrefab, playerEnergy, validOutlineMat,
            invalidOutlineMat, outlineLayerMask);
        walls = Walls.CreateComponent(gameObject, wallPrefab, wallOutlinePrefab, playerEnergy, validOutlineMat, invalidOutlineMat, rockCreationDistance,
            outlineLayerMask, player);
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

        if (DrawRelease() && playerEnergy.EnergyAboveThreshold(100f) && !rocks.RockIsActive() && !spikeQuicksand.SpikeQuicksandIsActive() && !walls.WallIsActive())
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
            if (hand.hoveringInteractable != null && hand.hoveringInteractable.gameObject.tag == "Rock")
            {
                rocks.PickupRock(hand, otherHand);
            }
            else if (arc.CanUseAbility())
            {
                if (arc.GetDistanceFromPlayer() <= rockCreationDistance)
                {
                    rocks.CreateNewRock(hand, arc);
                }
                else if (playerEnergy.EnergyAboveThreshold(200f))
                {
                    spikeQuicksand.IntializeOutline(hand, player);
                }
            }
        }
    }

    private void UpdateAbility()
    {

        if (rocks.RockIsActive())
        {
            rocks.UpdateRock(hand);
        }
        else if (spikeQuicksand.SpikeQuicksandIsActive())
        {
            spikeQuicksand.UpdateOutline(hand, controllerPose);
        }
        else if (walls.WallIsActive() && playerEnergy.EnergyIsNotZero())
        {
            walls.UpdateWallHeight(hand, otherHand);
        }
    }

    private void EndAbility()
    {
        if (rocks.RockIsActive())
        {
            rocks.ThrowRock(hand, otherHand);
        }
        else if (spikeQuicksand.SpikeQuicksandIsActive())
        {
            spikeQuicksand.TryCreateSpikesOrQuicksand(hand, controllerPose);
        }
        else if (walls.WallIsActive())
        {
            walls.EndCreateWall(hand, otherHand, controllerPose);
        }
    }

    private void CancelAbility()
    {
        if(hand.hoveringInteractable != null && hand.hoveringInteractable.gameObject.tag == "Rock")
        {
            hand.hoveringInteractable = null;
        }
        else if (walls.WallIsActive())
        {
            walls.CancelWall(hand, otherHand);
        }
        else if (spikeQuicksand.SpikeQuicksandIsActive())
        {
            playerEnergy.CancelEnergyUsage(hand);
            spikeQuicksand.CancelSpikes();
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