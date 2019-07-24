using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class PlayerAbility : MonoBehaviour
{
    public SteamVR_Input_Sources handType;
    public SteamVR_Action_Boolean grabAction;
    public PlayerEnergy playerEnergy;
    public float energyCost;
    public float damage;

    private float actionTime;

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
    }

    // Update is called once per frame
    void Update()
    {
        if (GetGrab())
        {
            this.useEnergy();
            actionTime = Time.time;
        }
        else if (!GetGrab() && (Time.time - actionTime) > 1)
        {
            playerEnergy.regenEnergy();
        }
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
