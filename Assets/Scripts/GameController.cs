using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class GameController : MonoBehaviour
{
    public int initialNumOfEnemies; // initial number of enemies to start a round with
    public int increaseOfEnePerWave; // how many more enemies per wave
    
    private static GameController instance; // instance for singleton pattern
    private GameObject enemyProducerObject; // EnemyProducer Object Instance
    private EnemyProducer enemyProducer; // EnemyProducer script functionality
    private PlayerHealth playerHealth; // controller for player health once round ends

    public int numOfEnemiesPerWave; // number of enemies to be spawned in one wave 
    public int enemiesDestroyed; // number of enemies destroyed in current Wave
    
    private int caseSwitch;
    private GameObject playerObj; // gameObject that contains cameraRig, vrCamera, hands
    // todo change the whole code where vrCamera is player
    private GameObject player; // vrCamera reference, contains all player scripts
    private GameObject vrCamera; // referenced as our player, contains player scripts
    private GameObject cameraRig; // this is the steamVRObjects object 


    private GameObject UIControllerObj;
    private GameOverMenuController gameOverController;
    

    // Constructor
    private GameController(){}
    
    // Instance getter and initialization
    public static GameController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = GameObject.FindObjectOfType(typeof(GameController)) as GameController;
            }
            return instance;
        }
    }


    private void Awake()
    {
        // teleport script
        caseSwitch = 1;
        playerObj = GameObject.FindGameObjectWithTag("Player");
        vrCamera = GameObject.FindGameObjectWithTag("MainCamera");
        cameraRig = GameObject.FindGameObjectWithTag("CameraRig");
        
        
        player = GameObject.FindWithTag ("MainCamera");
        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth> ();
            // todo maybe restart enemy energy when restarting game
//            playerEnergy = player.GetComponent<PlayerEnergy> ();
        }

        enemyProducerObject = GameObject.FindWithTag("EnemyProducer");
        enemyProducer = enemyProducerObject.GetComponent<EnemyProducer>();
        
        UIControllerObj = GameObject.FindWithTag("UIController");
        gameOverController = UIControllerObj.GetComponent<GameOverMenuController>();
        
        numOfEnemiesPerWave = initialNumOfEnemies;
        StartWave(initialNumOfEnemies);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // This function starts a round and spawns the corresponding number of enemies
    // Future: this function should keep track of which types enemies to spawn and how many
    // Future: this function should keep track of the round number
    void StartWave(int numOfEnemies)
    {
        enemiesDestroyed = 0;
        enemyProducer.SpawnEnemy(numOfEnemies);
    }
    
    // This function will be called when the player eliminates all the enemies in the wave
    // It starts a new wave, while incrementing the number of enemies that will appear
    // this function should be called everytime an enemy dies
    public void OnEnemyDeathClear()
    {
        if (enemiesDestroyed != numOfEnemiesPerWave)
        {
            // not all enemies have been destroyed, so don't do anything
            return;
        }
        numOfEnemiesPerWave += increaseOfEnePerWave;
        Debug.Log("Starting new Wave!!");
        StartWave(numOfEnemiesPerWave);
    }

    // Future: delete all other instances of objects in the scene
    // delete walls, spikes, rocks
    public void RestartGame()
    {
        // reactivate pause functionality
        UIControllerObj.GetComponent<MenuUIController>().enabled = true;

        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (var enemy in enemies)
        {
            Destroy(enemy);
        }
        
        // destroy menu screens and unfreeze game 
        var menus = GameObject.FindGameObjectsWithTag("Menu");
        foreach (var menu in menus)
        {
            Destroy(menu);
        }
        
        // todo translate player to beginning position
        
        Debug.Log("Restarting game");
        
        // Reset values of wave
        playerHealth.RecoverAllHealth();
        StartWave(initialNumOfEnemies);
    }
    
    // This function is called when player looses
    // It will instantiate the game over menu and give the option to restart the game
    public void playerLost()
    {
        gameOverController.GameOverScreen();
    }

    // This function keeps track of destroyed enemies by updating enemiesDestroyed variable
    // To be called when an enemey is destroyed
    public void EnemyGotDestroyed()
    {
        enemiesDestroyed += 1;
    }
    
    
    // This function moves the player around the 5 wave zones
    // todo update player object position too
        public void Teleport()
    {
        Vector3 destinationPos;
        int temp = caseSwitch;
        caseSwitch += 1;
        
        // Get camera rig and head position
//        Transform cameraRig = SteamVR_Render.Top().origin;
        Transform cameraRigT = cameraRig.transform;
//        Vector3 headPosition = SteamVR_Render.Top().head.position;
        Vector3 headPosition = vrCamera.transform.position;
        

        Debug.Log("Teleport!");
        temp = temp % 5;
        Debug.Log(temp);
        Debug.Log(caseSwitch);
        switch (temp)
        {
            case 0:
//                playerObj.transform.position = new Vector3(9,0.25f,33);
                destinationPos = new Vector3(9,0.25f,33);
                break;
            case 1:
//                playerObj.transform.position = new Vector3(22.6f,0.25f,18.8f);
                destinationPos = new Vector3(22.6f,0.5f,18.8f);
                break;
            case 2:
//                playerObj.transform.position = new Vector3(-3f,0.25f,3.1f);
                destinationPos = new Vector3(-3f,0.75f,3.1f);
                break;
            case 3:
//                playerObj.transform.position = new Vector3(26,0.25f,-22.8f);
                destinationPos = new Vector3(26,1f,-22.8f);
                break;           
            case 4:
//                playerObj.transform.position = new Vector3(-1.5f,0.25f,-31.5f);
                destinationPos = new Vector3(-1.5f,0.75f,-31.5f);
                break;
            default:
//                playerObj.transform.position = new Vector3(0,0,0);
                destinationPos = new Vector3(0,0,0);
                break;
        }
        
        // Calculate translation
        Vector3 groundPosition = new Vector3(headPosition.x,cameraRigT.position.y, headPosition.z);
        Vector3 translateVector = destinationPos - groundPosition;

        // move
        cameraRigT.position += translateVector;
//        playerObj.transform.position = destinationPos;

    }
    
    
    
    
}
