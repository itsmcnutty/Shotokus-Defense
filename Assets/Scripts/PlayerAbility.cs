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
    public GameObject rockPrefab;
    public float energyCost;
    public float damage;

    private PlayerEnergy playerEnergy;
    private float actionTime;
    private GameObject spawnedRock;
    private float rockSize = 0.1f;

    public Hand hand;
    private SpawnAndAttachToHand spawnRock;

    private void Awake()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerEnergy = player.GetComponent<PlayerEnergy>();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        spawnRock = new GameObject().AddComponent<SpawnAndAttachToHand>() as SpawnAndAttachToHand;
        spawnRock.prefab = rockPrefab;
    }

    // Update is called once per frame
    void Update()
    {
        if (GetGrab())
        {
            if (playerEnergy.energyIsNotZero())
            {
                int getRockCount = GameObject.FindGameObjectsWithTag("Rock").Length;
                if (getRockCount < 1)
                {
                    spawnRock.SpawnAndAttach(hand);
                    //spawnedRock = Instantiate(rockPrefab) as GameObject;
                    //spawnedRock.transform.position = controllerPose.transform.position;
                    //GrabObject();
                }
                spawnedRock = GameObject.FindWithTag("Rock");
                if(spawnedRock != null) {
                    rockSize += (0.01f * Time.deltaTime);
                    spawnedRock.transform.localScale = new Vector3(rockSize, rockSize, rockSize);
                }
                this.useEnergy();
            }
            actionTime = Time.time;
        }
        else if (!GetGrab())
        {
            ReleaseObject();
            if ((Time.time - actionTime) > 1)
            {
                playerEnergy.regenEnergy();
            }
        }
    }

    private void GrabObject()
    {
        var joint = AddFixedJoint();
        joint.connectedBody = spawnedRock.GetComponent<Rigidbody>();
    }

    private FixedJoint AddFixedJoint()
    {
        FixedJoint fx = gameObject.AddComponent<FixedJoint>();
        fx.breakForce = 20000;
        fx.breakTorque = 20000;
        return fx;
    }

    private void ReleaseObject()
    {
        spawnedRock = GameObject.FindWithTag("Rock");
        if (GetComponent<FixedJoint>())
        {
            GetComponent<FixedJoint>().connectedBody = null;
            Destroy(GetComponent<FixedJoint>());

            spawnedRock.GetComponent<Rigidbody>().velocity = controllerPose.GetVelocity();
            spawnedRock.GetComponent<Rigidbody>().angularVelocity = controllerPose.GetAngularVelocity();

        }
        spawnedRock = null;
    }

    public bool GetGrab()
    {
        return grabAction.GetState(handType);
    }

    public void useEnergy()
    {
        playerEnergy.useEnergy(energyCost);
    }

}
