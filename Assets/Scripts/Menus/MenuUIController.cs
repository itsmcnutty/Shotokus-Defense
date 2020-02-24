using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Valve.VR;

public class MenuUIController : MonoBehaviour
{
    private static MenuUIController instance; // instance for singleton pattern
    public SteamVR_Input_Sources rightHandInput;
    public SteamVR_Input_Sources leftHandInput;
    public SteamVR_Action_Boolean pauseAction;

    public GameObject menuPrefab;

    [Header("Audio Sources")]
    public AudioSource menuShow;
    public AudioSource menuHide;
    
    // External control keys
    private KeyCode KEY_RESTART_GAME = KeyCode.Backspace;

    private GameObject cameraRig; // this is the Player game object with tag "Player"
    private GameObject player; // get coordinates of player, to instate menu in front of them. THIS IS NOT THE PLAYER, THIS IS THE PLAYER'S HEAD
    private Camera vrCamera;
    private GameObject pauseMenu;

    private bool isPauseMenuActive; // true if player is on pause menu, false if not

    private Vector3 playerPos; // player transform position
    private Quaternion playerRot; // player transform rotation
    private Vector3 playerFor; // player transform forward

    private InteractLaserButton laserPointer;

    // Instance getter and initialization
    public static MenuUIController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = GameObject.FindObjectOfType(typeof(MenuUIController)) as MenuUIController;
            }
            return instance;
        }
    }

    private void Awake()
    {
        // Instantiate
        player = GameObject.FindWithTag("MainCamera");
        cameraRig = GameObject.FindWithTag("CameraRig");
        vrCamera = player.GetComponent<Camera>();

        laserPointer = this.GetComponent<InteractLaserButton>();
        isPauseMenuActive = false;
    }

    // Start is called before the first frame update
    void Start()
    { }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KEY_RESTART_GAME))
        {
            if (isPauseMenuActive)
            {
                pauseToggle();
                GameController.Instance.RestartGame();
            }
            else
            {
                GameController.Instance.RestartGame();
            }

            return;
        }

        if (PausePress())
        {
            pauseToggle();
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

    // Instantiates pause menu in the direction the player looks at
    public void PauseGame()
    {
        // calculate location in which player looks at, to instantiate menu
        playerPos = player.transform.position;
        playerRot = player.transform.rotation;
        playerFor = player.transform.forward;
        Vector3 spawnPosition = playerPos + playerFor * 6.5f;
        spawnPosition.y =  2.8f + cameraRig.transform.position.y; // todo test this out

        pauseMenu = Instantiate(menuPrefab, spawnPosition, playerRot);
        pauseMenu.GetComponentInChildren<Canvas>().worldCamera = vrCamera;
        pauseMenu.transform.LookAt(player.transform.position);

        // todo fixes camera rotation but buttons are not detectable - do not delete
//        pauseMenu = Instantiate(menuPrefab);
//        spawnPosition = new Vector3(spawnPosition.x,(float)1.9,spawnPosition.z);
//        pauseMenu.transform.position = spawnPosition;
//        Vector3 targetPosition =  new Vector3(playerPos.x, (float)1.9, playerPos.z);
//        pauseMenu.transform.LookAt(targetPosition);
    }

    // if pause menu is not active, instantiate it and pause game
    // if pause menu active, delete it and unpause game
    public void pauseToggle()
    {
        ToggleLaser();
        if (!isPauseMenuActive)
        {
            // menu is not active, so open it and play sonud effect
            menuShow.Play();
            isPauseMenuActive = true;
            GameController.Instance.hideEnemies(true); // hide enemies
            PauseGame();
            Time.timeScale = 0;
        }
        else
        {
            // menu is active, so close it and play sound effect
            menuHide.Play();
            isPauseMenuActive = false;
            GameController.Instance.hideEnemies(false); // show enemies
            Destroy(pauseMenu);
            Time.timeScale = 1;
        }

        PlayerAbility.ToggleSpikeAbility();
        PlayerAbility.ToggleWallAbility();
        PlayerAbility.ToggleQuicksandAbility();
        PlayerAbility.ToggleRockAbility();
    }

    public void ToggleLaser()
    {
        laserPointer.toggleLaser();
    }

}