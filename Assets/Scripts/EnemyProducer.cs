using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProducer : MonoBehaviour
{
    public bool shouldSpawn; // boolean to activate spawning
    public GameObject[] prefab; // prefab to be spawned. Set prefab on the editor
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
    public void SpawnEnemy(int numOfEnemies)
    {
        if (!shouldSpawn)
        {
            return;
        }
        spawnArea = this.GetComponent<BoxCollider>().bounds;
        
        // todo maybe use invoke for delay in between enemy spawning
        for (int i = 0; i < numOfEnemies; i++)
        {
            Instantiate(prefab[i%prefab.Length], randomSpawnPosition(), Quaternion.identity);
        }
        
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }
}
