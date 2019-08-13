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

    public GameObject menuPrefab;

    private GameObject player; // get coordinates of player, to instate menu in front of hem
    private GameObject pauseMenu;

    private bool isPauseMenuActive; // true if player is on pause menu, false if not

    private Vector3 playerPos; // player transform position
    private Quaternion playerRot; // player transform rotation
    private Vector3 playerFor; // player transform forward


    private void Awake()
    {
        player = GameObject.FindWithTag("MainCamera");
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
            if (!isPauseMenuActive)
            {
                // menu is not active, so open it
                isPauseMenuActive = true;
                Debug.Log("I want to be PaUsEd");
                PauseGame();
//                Time.timeScale = 0;
            }
            else
            {
                // menu is active, so close it
                isPauseMenuActive = false;
                Destroy(pauseMenu);
//                Time.timeScale = 1;
            }
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
        playerPos = player.transform.position;
        playerRot = player.transform.rotation;
        playerFor = player.transform.forward;
        Vector3 spawnPosition = playerPos + playerFor*5;
        
        pauseMenu = Instantiate(menuPrefab, spawnPosition, Quaternion.Inverse(playerRot));
    }
    
    
    
    
}
