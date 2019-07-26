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
    public float rockStartSize;
    public float energyCost;
    public float damage;

    private PlayerEnergy playerEnergy;
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
        if (GrabPress () && !GripHold ())
        {
            GetComponent<SpawnAndAttachToHand> ().SpawnAndAttach (null);
            GameObject[] allRocks = GameObject.FindGameObjectsWithTag ("Rock");
            rockNum = allRocks.Length - 1;
        }
        else if (GrabHold () && !GripHold ())
        {
            if (playerEnergy.EnergyIsNotZero ())
            {
                GameObject[] allRocks = GameObject.FindGameObjectsWithTag ("Rock");
                spawnedRock = allRocks[rockNum];
                rockSize += (0.01f * Time.deltaTime);
                spawnedRock.transform.localScale = new Vector3 (rockSize, rockSize, rockSize);
                playerEnergy.UseEnergy (energyCost);
            }

        }
        else
        {
            if (rockNum != 0)
            {
                GetComponent<SpawnAndAttachToHand> ().hand.DetachObject (GameObject.FindGameObjectsWithTag ("Rock") [rockNum]);
            }
            rockSize = rockStartSize;
            rockNum = 0;
            playerEnergy.RegenEnergy ();
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

    public bool GripHold ()
    {
        return gripAction.GetState (handType);
    }

}