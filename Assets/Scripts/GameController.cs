using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{

    private static GameController instance; // instance for singleton pattern
    public EnemyProducer enemyProducer;
    public int numOfEnemiesPerWave; // number of enemies to be spawned in one wave
    public int increaseOfEnePerWave; // how many more enemies per wave
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
        // CAREFUL might return an array
//        var enemy = GameObject.FindGameObjectWithTag("Enemy").GetComponent<EnemyHealth>();
//        Debug.Log(enemy);
//        enemy.OnEnemyDeath += OnEnemyDeath;

        StartWave(numOfEnemiesPerWave);
    }

    // Start is called before the first frame update
    void Start()
    {

        
    }

    // Update is called once per frame
    void Update()
    {
        OnEnemyDeathClear();
    }

    // PARAM: how many enemies on each wave
    // 
    void StartWave(int numOfEnemies)
    {
        enemiesDestroyed = 0;
        enemyProducer.spawnEnemy(numOfEnemies);
    }
    
    // This function will be called when the player eliminates all the enemies in the wave
    // It starts a new wave, while incrementing the number of enemies that will appear
    void OnEnemyDeathClear()
    {
        // enter here if all enemies have been destroyed, to start next wave
        if (enemiesDestroyed != numOfEnemiesPerWave)
        {
            // do nothing
            return;
        }
        numOfEnemiesPerWave += increaseOfEnePerWave;
        Debug.Log("Starting new Wave!!");
        Invoke("StartWave", 3);
    }
    
    // To be called when an enemey is destroyed
    // This function keeps track of destroyed enemies by updating enemiesDestroyed variable
    public void enemyGotDestroyed()
    {
        enemiesDestroyed++;
    }
    
    
    
}
