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
    public float rockStartSize;
    public float energyCost;
    public float damage;

    private PlayerEnergy playerEnergy;
    private static float actionTime;
    private GameObject spawnedRock;
    private float rockSize = 0;
    private int rockNum = 0;

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
        rockSize = rockStartSize;
    }

    // Update is called once per frame
    void Update ()
    {
        if (GrabPress ())
        {
            GetComponent<SpawnAndAttachToHand> ().SpawnAndAttach (null);
            GameObject[] allRocks = GameObject.FindGameObjectsWithTag ("Rock");
            rockNum = allRocks.Length - 1;
            this.useEnergy ();
            actionTime = Time.time;
        }
        else if (GrabHold ())
        {
            if (playerEnergy.energyIsNotZero ())
            {
                GameObject[] allRocks = GameObject.FindGameObjectsWithTag ("Rock");
                spawnedRock = allRocks[rockNum];
                rockSize += (0.01f * Time.deltaTime);
                spawnedRock.transform.localScale = new Vector3 (rockSize, rockSize, rockSize);
                actionTime = Time.time;
                this.useEnergy ();
            }

        }
        else
        {
            if(rockNum != 0) {
                GetComponent<SpawnAndAttachToHand> ().hand.DetachObject(GameObject.FindGameObjectsWithTag ("Rock")[rockNum]);
            }
            rockSize = rockStartSize;
            rockNum = 0;
            if ((Time.time - actionTime) > 1)
            {
                playerEnergy.regenEnergy ();
            }
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

    public void useEnergy ()
    {
        playerEnergy.useEnergy (energyCost);
    }

}