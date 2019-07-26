using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    
    public EnemyProducer enemyProducer;
    public int numOfEnemiesPerWave; // number of enemies per wave
    public int increaseOfEnePerWave; // how many more enemies per wave
    private int enemiesDestroyed; // number of enemies destroyed in current Wave
    
    // Start is called before the first frame update
    void Start()
    {
        enemiesDestroyed = 0;
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
    }
    
    // todo 
    void onEnemyDeath(EnemyHealth enemy)
    {
        enemiesDestroyed += 1;
        // enter here if all enemies have been destroyed, to start next wave
        if (enemiesDestroyed == numOfEnemiesPerWave)
        {
            numOfEnemiesPerWave += increaseOfEnePerWave;
            Invoke("StartWave", 3);
        }
    }
    
    
    // make function that calls StartWave when enemies die
    
    
}
