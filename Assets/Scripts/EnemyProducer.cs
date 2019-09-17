using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyProducer : MonoBehaviour
{
    private GameObject spawnLocationObject; // gameobject from spawner that will be used 
    
    public bool shouldSpawn; // boolean to activate spawning
    public GameObject heavyEnemyPrefab; // prefab to be spawned. Set prefab on the editor
    public GameObject mediumEnemyPrefab; // prefab to be spawned. Set prefab on the editor
    public GameObject lightEnemyPrefab; // prefab to be spawned. Set prefab on the editor
    private Bounds spawnArea;

    // Start is called before the first frame update
    void Start()
    {
        
    }
    
    // Function for spawning enemies in random location inside spawnArea
    // Returns: Vector3 coordinates
    Vector3 randomSpawnPosition() {
        float x = Random.Range(spawnArea.min.x, spawnArea.max.x);
        float z = Random.Range(spawnArea.min.z, spawnArea.max.z);
        float y = 0.5f;
        return new Vector3(x, y, z);
    }
    
    // Function that instantiates enemies in spawn area
    // PARAM: Int numOfEnemies - specifies how many enemies to be spawned
    // this will be called by GameController.cs
    public void SpawnEnemy(int numOfEnemies, GameObject enemyPrefab)
    {
        if (!shouldSpawn)
        {
            return;
        }
        spawnArea = spawnLocationObject.GetComponent<BoxCollider>().bounds;
        
        // todo maybe use invoke for delay in between enemy spawning
        for (int i = 0; i < numOfEnemies; i++)
        {
            Instantiate(enemyPrefab, randomSpawnPosition(), Quaternion.identity);
        }
    }

    public void Spawn(SpawnInfo spawnInfo)
    {
        string spawningLocation = spawnInfo.Location.ToString();
        spawnLocationObject = GameObject.FindWithTag(spawningLocation);
        SpawnEnemy(spawnInfo.NumHeavyEnemies, heavyEnemyPrefab);
        SpawnEnemy(spawnInfo.NumMedEnemies, mediumEnemyPrefab);
        SpawnEnemy(spawnInfo.NumLightEnemies, lightEnemyPrefab);
        
        // update counter of enemies alive
        GameController.Instance.EnemyAddNumAlive(spawnInfo.NumHeavyEnemies + spawnInfo.NumMedEnemies + spawnInfo.NumLightEnemies);
        
        //todo enemyProducer to singleton and own game object
        //todo spawners shouldnt have the enemyProducen script
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }
}
