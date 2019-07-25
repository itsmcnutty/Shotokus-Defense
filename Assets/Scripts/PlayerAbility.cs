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

    private void Awake ()
    {
        GameObject player = GameObject.FindWithTag ("Player");
        if (player != null)
        {
            playerEnergy = player.GetComponent<PlayerEnergy> ();
        }
    }

    // Start is called before the first frame update
    void Start ()
    {
        spawnRock = new GameObject ().AddComponent<SpawnAndAttachToHand> () as SpawnAndAttachToHand;
        spawnRock.prefab = rockPrefab;
    }

    // Update is called once per frame
    void Update ()
    {
        if (GrabPress ())
        {
            if (playerEnergy.energyIsNotZero ())
            {
                spawnRock.SpawnAndAttach (hand);
                GrabObject ();
                this.useEnergy ();
            }
            actionTime = Time.time;
        }
        else if (GrabHold ())
        {

            if (playerEnergy.energyIsNotZero ())
            {
                GameObject[] allRocks = GameObject.FindGameObjectsWithTag ("Rock");
                int numRocks = allRocks.Length;
                spawnedRock = allRocks[numRocks - 1];
                if (spawnedRock != null)
                {
                    rockSize += (0.01f * Time.deltaTime);
                    spawnedRock.transform.localScale = new Vector3 (rockSize, rockSize, rockSize);
                }
                actionTime = Time.time;
                this.useEnergy ();
            }

        }
        else
        {
            GameObject[] allRocks = GameObject.FindGameObjectsWithTag ("Rock");
            if (allRocks != null && allRocks.Length > 1)
            {
                spawnedRock = allRocks[allRocks.Length - 1];
                ReleaseObject ();
                //hand.DetachObject(spawnedRock, false);
            
            }
            if ((Time.time - actionTime) > 1)
            {
                playerEnergy.regenEnergy ();
            }
        }
    }

    private void GrabObject ()
    {
        var joint = AddFixedJoint ();
        joint.connectedBody = spawnedRock.GetComponent<Rigidbody> ();
    }

    private FixedJoint AddFixedJoint ()
    {
        FixedJoint fx = gameObject.AddComponent<FixedJoint> ();
        fx.breakForce = 20000;
        fx.breakTorque = 20000;
        return fx;
    }

    private void ReleaseObject ()
    {
        spawnedRock = GameObject.FindWithTag ("Rock");
        if (GetComponent<FixedJoint> ())
        {
            GetComponent<FixedJoint> ().connectedBody = null;
            Destroy (GetComponent<FixedJoint> ());

            spawnedRock.GetComponent<Rigidbody> ().velocity = controllerPose.GetVelocity ();
            spawnedRock.GetComponent<Rigidbody> ().angularVelocity = controllerPose.GetAngularVelocity ();

        }
        spawnedRock = null;
    }

    public bool GrabHold ()
    {
        return grabAction.GetState (handType);
    }

    public bool GrabPress ()
    {
        return grabAction.GetStateDown (handType);
    }

    public void useEnergy ()
    {
        playerEnergy.useEnergy (energyCost);
    }

}