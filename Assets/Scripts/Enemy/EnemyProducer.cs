using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

// Struct to keep track of information for pointsAround() function
struct EnemyInfo
{
    public GameObject Prefab; // prefab to spawn
    public string SpawnLocation; // location to spawn the prefab

    public EnemyInfo(GameObject prefab, string spawnLocation)
    {
        Prefab = prefab;
        SpawnLocation = spawnLocation;
    }
}


public class EnemyProducer : MonoBehaviour
{
    private GameObject spawnLocationObject; // gameobject from spawner that will be used 
    
    public bool shouldSpawn; // boolean to activate spawning
    public GameObject heavyEnemyPrefab; // prefab to be spawned. Set prefab on the editor
    public GameObject mediumEnemyPrefab; // prefab to be spawned. Set prefab on the editor
    public GameObject lightEnemyPrefab; // prefab to be spawned. Set prefab on the editor
    private Bounds spawnArea;
    private Queue<EnemyInfo> enemyQueue; // queue containing all enemies. This queue will take care of making enemies wait to spawn if Enemies alive is bigger than spawn limit

    private void Awake()
    {
        enemyQueue = new Queue<EnemyInfo>();
    }
    
    // Function for spawning enemies in random location inside spawnArea
    // Returns: Vector3 coordinates
    Vector3 randomSpawnPosition() {
        float x = Random.Range(spawnArea.min.x, spawnArea.max.x);
        float z = Random.Range(spawnArea.min.z, spawnArea.max.z);
        float y = 0;
//        float y = 0.5f;
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
        
        // Play random beats
        spawnLocationObject.GetComponentInChildren<WaveCuePlayer>().PlayCue();
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
    }

    // this function takes a spawninfo and then add an EnemyInfo to the enemy queue
    // 1 enemy is 1 enemy info, therefore if there is a spawnInfo with 3 Heavy enemies, all 3 will be added separately to queue
    public void addToQueue(SpawnInfo spawnInfo)
    {
        string spawningLocation = spawnInfo.Location.ToString();
        
        for (int i = 0; i < spawnInfo.NumHeavyEnemies; i++)
        {
            EnemyInfo enemyInfo = new EnemyInfo(heavyEnemyPrefab, spawningLocation);
            enemyQueue.Enqueue(enemyInfo);
        }
        for (int i = 0; i < spawnInfo.NumMedEnemies; i++)
        {
            EnemyInfo enemyInfo = new EnemyInfo(mediumEnemyPrefab, spawningLocation);
            enemyQueue.Enqueue(enemyInfo);
        }
        for (int i = 0; i < spawnInfo.NumLightEnemies; i++)
        {
            EnemyInfo enemyInfo = new EnemyInfo(lightEnemyPrefab, spawningLocation);
            enemyQueue.Enqueue(enemyInfo);
        }
    }
    
    // Spawn function that takes an number of enemies to be spawned from the queue
    public void SpawnFromQueue(float spawningAmount)
    {
        // check that we dont try to spawn more enemies than the number of enemies queued (ex. lenght of queue = 2 and available spots = 3, that could break the loop)
        if (enemyQueue.Count < spawningAmount)
        {
            spawningAmount = enemyQueue.Count;
        }
        
        for(float i = 0; i < spawningAmount;i++)
        {
            
            EnemyInfo enemy = enemyQueue.Dequeue();
            spawnLocationObject = GameObject.FindWithTag(enemy.SpawnLocation);
            SpawnEnemy(1, enemy.Prefab);
            // update enemiesAlive counter
            GameController.Instance.EnemyAddNumAlive(1);
        }
    }
    
    // reset enemy queue
    public void ResetEnemyQueue()
    {
        enemyQueue = new Queue<EnemyInfo>();
    }
    
    
    // Update is called once per frame
    void Update()
    {
        
    }
}
