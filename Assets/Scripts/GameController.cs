using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    
    public EnemyProducer enemyProducer;
    public int numOfEnemiesPerWave; // number of enemies per wave
    public int increaseOfEnePerWave; // how many more enemies per wave
    
    // Start is called before the first frame update
    void Start()
    {
        StartWave(numOfEnemiesPerWave);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // PARAM: how many enemies on each wave
    // 
    void StartWave(int numOfEnemies)
    {
        enemyProducer.spawnEnemy(numOfEnemies);
        numOfEnemiesPerWave += increaseOfEnePerWave;
    }
    
    
    // make function that calls StartWave when enemies die
    
    
}
