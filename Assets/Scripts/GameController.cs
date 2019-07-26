using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    
    public EnemyProducer enemyProducer;
    public int numOfEnemiesPerWave; // number of enemies per wave
    public int increaseOfEnePerWave; // how many more enemies per wave
    private int enemiesDestroyed; // number of enemies destroyed in current Wave

    private void Awake()
    {
        // CAREFUL might return an array
        var enemy = GameObject.FindGameObjectWithTag("Enemy").GetComponent<EnemyHealth>();
        Debug.Log(enemy);
        enemy.OnEnemyDeath += OnEnemyDeath;
        StartWave(numOfEnemiesPerWave);
    }

    // Start is called before the first frame update
    void Start()
    {

        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // PARAM: how many enemies on each wave
    // 
    void StartWave(int numOfEnemies)
    {
        enemiesDestroyed = 0;
        enemyProducer.spawnEnemy(numOfEnemies);
    }
    
    // todo 
    void OnEnemyDeath()
    {
        enemiesDestroyed += 1;
        // enter here if all enemies have been destroyed, to start next wave
        if (enemiesDestroyed == numOfEnemiesPerWave)
        {
            numOfEnemiesPerWave += increaseOfEnePerWave;
            Debug.Log("Starting new Wave!!");
            Invoke("StartWave", 3);
        }
    }
    
    
    
    // work around
//    public int getEnemiesDestroyed()
//    {
//        return this.enemiesDestroyed;
//    }
    
    
    
}
