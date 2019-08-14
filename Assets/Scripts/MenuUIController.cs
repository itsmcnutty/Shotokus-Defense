﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class MenuUIController : MonoBehaviour
{
    public SteamVR_Input_Sources rightHandInput;
    public SteamVR_Input_Sources leftHandInput;
    public SteamVR_Action_Boolean pauseAction;

    public GameObject menuPrefab;

    private GameObject player; // get coordinates of player, to instate menu in front of hem
    private GameObject pauseMenu;

    private bool isPauseMenuActive; // true if player is on pause menu, false if not

    private Vector3 playerPos; // player transform position
    private Quaternion playerRot; // player transform rotation
    private Vector3 playerFor; // player transform forward

    private InteractLaserButton laserPointer;


    private void Awake()
    {
        player = GameObject.FindWithTag("MainCamera");
        laserPointer = this.GetComponent<InteractLaserButton>();
        isPauseMenuActive = false;
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
            laserPointer.toggleLaser();
            if (!isPauseMenuActive)
            {
                // menu is not active, so open it
                isPauseMenuActive = true;
                PauseGame();
                Time.timeScale = 0;
            }
            else
            {
                // menu is active, so close it
                isPauseMenuActive = false;
                Destroy(pauseMenu);
                Time.timeScale = 1;
            }
        }
        
    }

    public bool PausePress()
    {
        return PausePressLeft() || PausePressRight();
    }
    
    public bool PausePressRight()
    {
        return pauseAction.GetStateDown(rightHandInput);
    }    
    
    public bool PausePressLeft()
    {
        return pauseAction.GetStateDown(leftHandInput);
    }

    // todo make this toggleable, so if not in menu access it
    // freezes the game, and instantiates the menu
    public void PauseGame()
    {
        playerPos = player.transform.position;
        playerRot = player.transform.rotation;
        playerFor = player.transform.forward;
        Vector3 spawnPosition = playerPos + playerFor*5;
//        spawnPosition = new Vector3(playerPos.x+6,(float)1.9,playerPos.z+6);
//        spawnPosition.y =  (float)0;
//        pauseMenu = Instantiate(menuPrefab, spawnPosition, playerRot);
//        Vector3 targetPosition =  new Vector3(playerPos.x, transform.position.y, playerPos.z);
//        pauseMenu.transform.LookAt(targetPosition);
//        spawnPosition.y =  (float)1.9407;


//        pauseMenu = Instantiate(menuPrefab);

        pauseMenu = Instantiate(menuPrefab);
        spawnPosition = new Vector3(playerPos.x+6,(float)1.9,playerPos.z+6);
        pauseMenu.transform.position = spawnPosition;
        Vector3 targetPosition =  new Vector3(playerPos.x, (float)1.9, playerPos.z);
        pauseMenu.transform.LookAt(targetPosition);
    }
    
    
    
    
}
