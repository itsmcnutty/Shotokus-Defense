using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnEnemyButton : MonoBehaviour
{
    public int numberOfEnemies;
    private GameObject enemyProducerObject;
    private EnemyProducer enemyProducer;

    // Start is called before the first frame update
    void Start()
    {
        enemyProducerObject = GameObject.FindWithTag("EnemyProducer");
        enemyProducer = enemyProducerObject.GetComponent<EnemyProducer>();
        if (!enemyProducer)
        {
            Debug.Log("ERROR: Coundnt find the instance ENEMYPRODUCER");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void spawnEnemy()
    {
//        enemyProducer.SpawnEnemy(numberOfEnemies);
    }
    
}
