using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProducer : MonoBehaviour
{
    // Start is called before the first frame update

    public GameObject prefab; // prefab to be spawned. Set prefab on the editor

    private Bounds spawnArea;
    private int numOfEnemies; // max number of enemies per round
    private int spawnedEnemies = 0; // number of enemies spawned already // todo remove

    // Start is called before the first frame update
    void Start()
    {
//        numOfEnemies = 10;
//        spawnArea = this.GetComponent<BoxCollider>().bounds;
//        InvokeRepeating("spawnEnemy", 5f, 2f);
    }
    
    

    // Returns: Vector3 random coordinates inside the spawnArea
    Vector3 randomSpawnPosition() {
        float x = Random.Range(spawnArea.min.x, spawnArea.max.x);
        float z = Random.Range(spawnArea.min.z, spawnArea.max.z);
        float y = 0.5f;
        return new Vector3(x, y, z);
    }
    
    // this will be called by GameController.cs
    // self explanatory??
    // TODO add stop spawning based on how many enemies are already there or on a maximum number of enemies
    public void spawnEnemy(int maxNumOfEnemies)
    {
        numOfEnemies = maxNumOfEnemies;
        spawnArea = this.GetComponent<BoxCollider>().bounds;
        InvokeRepeating("Spawner", 3f, 2f);
    }

    // todo document
    private void Spawner()
    {
        if (spawnedEnemies == numOfEnemies)
        {
            return;
        }
        Instantiate(prefab, randomSpawnPosition(), Quaternion.identity);
        spawnedEnemies += 1; // unnecessary
    }
    

    // Update is called once per frame
    void Update()
    {
        
    }
}
