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

    private int numOfEnemiesPerWave; // number of enemies to be spawned in one wave 
    private int enemiesDestroyed; // number of enemies destroyed in current Wave


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
        GameObject player = GameObject.FindWithTag ("MainCamera");
        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth> ();
            // todo maybe restart enemy energy when restarting game
//            playerEnergy = player.GetComponent<PlayerEnergy> ();
        }

        // Get enemyProducer functionality
        enemyProducerObject = GameObject.FindWithTag("EnemyProducer");
        enemyProducer = enemyProducerObject.GetComponent<EnemyProducer>();
        if (enemyProducer == null)
        {
            Debug.Log("ERROR: Couldn't find enemy producer");
        }

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
    public void RestartGame()
    {
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (var enemy in enemies)
        {
            Destroy(enemy);
        }
        
        // todo translate player to beginning position
        
        Debug.Log("Restarting game");
        
        // Reset values of wave
        playerHealth.RecoverAllHealth();
        StartWave(initialNumOfEnemies);
    }
    
    // todo make function for losing game

    // This function keeps track of destroyed enemies by updating enemiesDestroyed variable
    // To be called when an enemey is destroyed
    public void EnemyGotDestroyed()
    {
        enemiesDestroyed += 1;
    }
    
    
    
}
