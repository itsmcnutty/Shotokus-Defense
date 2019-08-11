using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class MenuUIController : MonoBehaviour
{
    public SteamVR_Input_Sources rightHandInput;
    public SteamVR_Input_Sources leftHandInput;
    public SteamVR_Action_Boolean pauseAction;

    public GameObject wallPrefab;

    private GameObject player; // get coordinates of player, to instate menu in front of hem
    private GameObject wall;

    private void Awake()
    {
        player = GameObject.FindWithTag("MainCamera");
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // todo - otherwise, close it
        if (PausePress())
        {
            Debug.Log("Im being PaUsEd");
        }
    }

    public bool PausePress()
    {
        return PausePressLeft() || PausePressRight();
    }
    
    public bool PausePressRight()
    {
        return pauseAction.GetState(rightHandInput);
    }    
    
    public bool PausePressLeft()
    {
        return pauseAction.GetState(leftHandInput);
    }

    // todo make this toggleable, so if not in menu access it
    // freezes the game, and instantiates the menu
    public void PauseGame()
    {

        Vector3 playerPos = player.transform.position;
        Quaternion playerRot = player.transform.rotation;
        Vector3 playerFor = player.transform.forward;
        Vector3 spawnPosition = playerPos + playerFor*5;
        
        
        wall = Instantiate(wallPrefab, spawnPosition, playerRot);
        wall.transform.position = new Vector3(5,0,5);
        
        
    }
    
    
}
