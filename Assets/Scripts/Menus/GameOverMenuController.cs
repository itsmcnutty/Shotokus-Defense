using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class GameOverMenuController : MonoBehaviour
{

    public GameObject gameoverMenuPrefab;

    public SteamVR_Action_Boolean pauseAction;
    public SteamVR_Input_Sources rightHandInput;
    public SteamVR_Input_Sources leftHandInput;


    private GameObject player; // get coordinates of player, to instate menu in front of them
    private Camera vrCamera;
    private GameObject gameoverMenu;

    private Vector3 playerPos; // player transform position
    private Quaternion playerRot; // player transform rotation
    private Vector3 playerFor; // player transform forward

    private InteractLaserButton laserPointer;

    private void Awake()
    {
        player = GameObject.FindWithTag("MainCamera");
        vrCamera = player.GetComponent<Camera>();
        laserPointer = this.GetComponent<InteractLaserButton>();
        

    }

    // Start is called before the first frame update
    void Start()
    { }

    // Update is called once per frame
    void Update()
    {
        // if inside game over menu
        // toggle like the pause menu with button press
        if (PausePress() && GameObject.FindGameObjectWithTag("Menu").name == "GameOverMenu")
        {
            GameController.Instance.RestartWave();
        }
    }

    // Creates the game over screen and pauses the game
    public void GameOverScreen()
    {
        // calculate location in which player looks at, to instantiate menu
        playerPos = player.transform.position;
        playerRot = player.transform.rotation;
        playerFor = player.transform.forward;
        Vector3 spawnPosition = playerPos + playerFor * 5;
        spawnPosition.y = (float) 2.5; // todo test this out

        // instantiate menu
        gameoverMenu = Instantiate(gameoverMenuPrefab, spawnPosition, playerRot);
        gameoverMenu.GetComponentInChildren<Canvas>().worldCamera = vrCamera;
        gameoverMenu.transform.LookAt(player.transform.position);

        // freeze time, active menu laser pointers and cancel abilities
        if (TutorialController.Instance.TutorialWaveInProgress())
        {
            TutorialController.Instance.ToggleTutorialAbilities();
        }
        else
        {
            PlayerAbility.ToggleSpikeAbility();
            PlayerAbility.ToggleWallAbility();
            PlayerAbility.ToggleQuicksandAbility();
            PlayerAbility.ToggleRockAbility();
        }
        PlayerEnergy.Instance.CancelAllEnergyUsage();

        Time.timeScale = 0;
        laserPointer.toggleLaser();
        // disable pause menu
        GetComponent<MenuUIController>().enabled = false;

        // when menu is destroyed, time becomes normal and lasers are toggled again
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

}