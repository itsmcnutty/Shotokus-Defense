using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    private static GameController instance; // instance for singleton pattern
    private GameObject enemyProducerObject; // EnemyProducer Object Instance
    private EnemyProducer enemyProducer; // EnemyProducer script functionality
    private PlayerHealth playerHealth; // controller for player health once round ends
    //    private PlayerEnergy playerEnergy; // controller for player energy

    private int enemiesAlive; // number of enemies alive in current Wave

    [Header("Wait Times between Waves")]
    public float BEFORE_WAVE1;
    public float BETWEEN_WAVES;
    public float BETWEEN_LOCATIONS;

    [Header("Wave Files")]
    public TextAsset[] locationWaveFiles; // array containing location wave files

    [Header("Miscellaneous")]
    public float limitAmountEnemies; // maximum amount of enemies at one time in the game
    public GameObject teleportPillar;

    // variables for teleport function
    private int caseSwitch;
    private GameObject playerObj; // gameObject that contains cameraRig, vrCamera, hands
    // todo change the whole code where vrCamera is player
    private GameObject player; // vrCamera reference, contains all player scripts
    private GameObject vrCamera; // referenced as our player, contains player scripts
    private GameObject cameraRig; // this is the steamVRObjects object 

    private GameObject UIControllerObj;
    private GameOverMenuController gameOverController;

    private Queue<LocationWaves> allLocationWaves = new Queue<LocationWaves>();
    private LocationWaves currentLocation;
    private int currentLocationCounter; // used to keep track of teleportation 
    private Queue<LocationWaves> resetLocation;
    private Wave currentWave;
    private float currentWaveIndex = 0;

    private float currentTime;
    private bool pauseWaveSystem = true;
    private float availableSpots; // keeps track of how many more enemies can be spawned in the scene

    // Constructor
    private GameController() { }

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
        // teleport script
        caseSwitch = 0;
        playerObj = GameObject.FindGameObjectWithTag("Player");
        vrCamera = GameObject.FindGameObjectWithTag("MainCamera");
        cameraRig = GameObject.FindGameObjectWithTag("CameraRig");

        player = GameObject.FindWithTag("MainCamera");
        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
        }
        enemyProducerObject = GameObject.FindWithTag("EnemyProducer");
        enemyProducer = enemyProducerObject.GetComponent<EnemyProducer>();

        UIControllerObj = GameObject.FindWithTag("UIController");
        gameOverController = UIControllerObj.GetComponent<GameOverMenuController>();

        // Parse json to get waves information &  fill up AlllocationWave queue
        restartQueue();
    }

    // Start is called before the first frame update
    void Start()
    {
        currentTime = 0;
        currentLocation = allLocationWaves.Dequeue();
        currentWave = currentLocation.GetNextWave();
        enemiesAlive = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (pauseWaveSystem)
        {
            return;
        }

        currentTime += Time.deltaTime;

        if (currentWave != null)
        {
            SpawnInfo spawnInfo = currentWave.GetSpawnAtTime(currentTime);
            if (spawnInfo != null && spawnInfo.Location != SpawnInfo.SpawnLocation.None)
            {
                // add spawn info to enemy queue in enemy producer
                enemyProducer.addToQueue(spawnInfo);
                // check how many enemies are alive right now
                availableSpots = limitAmountEnemies - enemiesAlive;
                // count how enemies are going to be spawned
                float count = spawnInfo.NumHeavyEnemies + spawnInfo.NumMedEnemies + spawnInfo.NumLightEnemies;
                // select amount to spawn
                if (availableSpots < count)
                {
                    enemyProducer.SpawnFromQueue(availableSpots);
                }
                else
                {
                    enemyProducer.SpawnFromQueue(count);
                }
            }
        }
    }

    // This function will be called when the player eliminates all the enemies in the wave
    // It starts a new wave, while incrementing the number of enemies that will appear
    // this function should be called everytime an enemy dies
    public void OnEnemyDeathClear()
    {
        if (enemiesAlive != 0)
        {
            // not all enemies have been destroyed, so check queue to spawn more enemies
            availableSpots = limitAmountEnemies - enemiesAlive;
            enemyProducer.SpawnFromQueue(availableSpots);
            return;
        }

        // if all enemies have been killed before next wave time, then spawn early
        SpawnInfo spawnInfo = currentWave.GetNextSpawnTimeInfo(out float? newTime);
        if (spawnInfo != null)
        {
            currentTime = newTime.Value; // this moves foward time to spawnInfo time
            enemyProducer.Spawn(spawnInfo);
            return;
        }

        // if round has been completed, start new wave
        currentTime = 0;
        currentWave = currentLocation.GetNextWave();

        // dont continue in the function if there are more waves in the current location
        if (currentWave != null)
        {
            TogglePauseWaveSystem();

            if (TutorialController.Instance.TutorialWaveInProgress())
            {
                currentWaveIndex++;
                TutorialController.Instance.SetNextTutorial();
                return;
            }
            Invoke("TogglePauseWaveSystem", BETWEEN_WAVES);

            return;
        }

        // all waves have been completed, so start new location
        if (TutorialController.Instance.TutorialWaveInProgress())
        {
            TutorialController.Instance.EndTutorial();
        }

        // if there are no waves left, check if there are more locations
        if (allLocationWaves.Count != 0)
        {
            currentTime = 0;
            currentLocationCounter++;
            currentLocation = allLocationWaves.Dequeue();
            currentWave = currentLocation.GetNextWave();
            TogglePauseWaveSystem();
            SpawnTeleportPillar();
            return;
        }
        // no more locations left so you win
        Debug.Log("YOU WIN");
    }

    public void StartGameWithTutorial()
    {
        Teleport(false);
        TutorialController.Instance.SelectTutorial(TutorialController.TutorialSections.Rock);
    }

    public void StartGameWithoutTutorial()
    {
        Teleport(true);
        PlayerAbility.ToggleRockAbility();
        PlayerAbility.ToggleSpikeAbility();
        PlayerAbility.ToggleWallAbility();
        PlayerAbility.ToggleQuicksandAbility();
    }

    // this function restarts the allLocationWaves queue by enqueue all the location json files based on the current location postion counter
    public void restartQueue()
    {
        TutorialController.tutorialWaveInProgress = false;
        allLocationWaves = new Queue<LocationWaves>();
        for (int i = currentLocationCounter; i < locationWaveFiles.Length; i++)
        {
            allLocationWaves.Enqueue(JsonParser.parseJson(locationWaveFiles[i]));
        }
    }

    // deletes walls, spikes, rocks and restart the current wave in location
    public void RestartWave()
    {
        bool restartTutorialWave = false;
        if(TutorialController.Instance.TutorialWaveInProgress())
        {
            restartTutorialWave = true;
        }
        // reactivate pause functionality
        UIControllerObj.GetComponent<MenuUIController>().enabled = true;

        //        Debug.Log("Restarting wave");

        // Reset values of wave (queue, timer, enemies counter)
        enemiesAlive = 0;
        currentTime = 0;
        restartQueue();
        
        // destroy all objects in scene before restarting
        destroyAll(true);

        currentLocation = allLocationWaves.Dequeue();
        if (restartTutorialWave)
        {
            TutorialController.tutorialWaveInProgress = true;
            for (int i = 0; i < currentWaveIndex; i++)
            {
                currentLocation.GetNextWave();
            }
            currentWave = currentLocation.GetNextWave();
        }
        else
        {
            currentWave = currentLocation.GetNextWave();
        }
        playerHealth.RecoverAllHealth();
    }

    // delete walls, spikes, rocks
    // put player on location 1 and restart all the waves again
    public void RestartGame()
    {
        // destroy all objects in scene before restarting
        destroyAll(true);

        //        Debug.Log("Restarting wave");

        // Reset values of wave (queue, timer, enemies counter)
        enemiesAlive = 0;
        currentTime = 0;

        // teleport the player
        Teleport(false, 0);

        // restart queue to initial state (all waves from location 1)
        allLocationWaves = new Queue<LocationWaves>();
        for (int i = 0; i < locationWaveFiles.Length; i++)
        {
            allLocationWaves.Enqueue(JsonParser.parseJson(locationWaveFiles[i]));
        }

        currentLocation = allLocationWaves.Dequeue();
        currentWave = currentLocation.GetNextWave();
        playerHealth.RecoverAllHealth();
    }

    // this function destroys all the following game objects instances:
    // rocks, walls, spikes, quicksand, menus, enemies, particles of the abilities
    public void destroyAll(bool destroyEnemies)
    {
        if (destroyEnemies)
        {
            var enemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (var enemy in enemies)
                Destroy(enemy);
        }

        // destroy menu screens and unfreeze game 
        var menus = GameObject.FindGameObjectsWithTag("Menu");
        foreach (var menu in menus)
            Destroy(menu);

        // destroy rocks, spikes, walls, quicksand 
        var rocks = GameObject.FindGameObjectsWithTag("Rock");
        var spikes = GameObject.FindGameObjectsWithTag("Spike");
        var walls = GameObject.FindGameObjectsWithTag("Wall");
        var quicksands = GameObject.FindGameObjectsWithTag("Quicksand");
        foreach (var rock in rocks)
            Destroy(rock);
        foreach (var spike in spikes)
            Destroy(spike);
        foreach (var wall in walls)
            Destroy(wall);
        foreach (var quicksand in quicksands)
            Destroy(quicksand);

        // destroy particles
        var particles = GameObject.FindGameObjectsWithTag("Particle");
        foreach (var particle in particles)
            Destroy(particle);
    }

    // This function is called when player looses
    // It will instantiate the game over menu and give the option to restart the game
    public void playerLost()
    {
        destroyAll(true);
        gameOverController.GameOverScreen();
    }

    // This function keeps track of destroyed enemies by updating enemiesDestroyed variable
    // To be called when an enemey is destroyed
    public void EnemyGotDestroyed(GameObject enemyDestroyed)
    {
        if (enemyDestroyed.name != "Target Dummy")
        {
            enemiesAlive--;
            // Check if round is over or not
            OnEnemyDeathClear();
        }
        else
        {
            TutorialController.Instance.SpawnNewDummy();
        }
    }

    // Called in EnemyProducer, updates number of enemies alive
    public void EnemyAddNumAlive(int amount)
    {
        enemiesAlive += amount;
    }

    public void SpawnTeleportPillar()
    {
        Vector3 playerPos = vrCamera.transform.position;
        Vector3 playerDirection = new Vector3(player.transform.forward.x, 0, player.transform.forward.z);
        Quaternion playerRotation = new Quaternion(0, player.transform.rotation.y, 0, player.transform.rotation.w);
        float spawnDistance = 2;

        Vector3 spawnPos = playerPos + playerDirection * spawnDistance;

        teleportPillar.transform.position = spawnPos;
        teleportPillar.transform.rotation = playerRotation;
        teleportPillar.SetActive(true);
    }

    // This function moves the player around the 5 wave zones
    // Input: location is an optional parameter to specify an specific location to teleport to
    public void Teleport(bool toggleWaves, int location = -1)
    {
        Vector3 destinationPos;
        int temp = caseSwitch;
        caseSwitch += 1;

        // Get camera rig and head position
        Transform cameraRigT = cameraRig.transform;
        Vector3 headPosition = vrCamera.transform.position;

        temp = temp % 5;

        // optional parameter, input specific location to transport to
        if (location >= 0)
        {
            temp = location % 5;
            caseSwitch = temp;
        }

        switch (temp)
        {
            case 0:
                destinationPos = new Vector3(9, 0.25f, 33);
                break;
            case 1:
                destinationPos = new Vector3(22.6f, 0.5f, 23f);
                break;
            case 2:
                destinationPos = new Vector3(-3f, 0.75f, 3.1f);
                break;
            case 3:
                destinationPos = new Vector3(26, 1f, -22.8f);
                break;
            case 4:
                destinationPos = new Vector3(-1.5f, 0.75f, -31.5f);
                break;
            default:
                destinationPos = new Vector3(0, 0, 0);
                break;
        }

        // Calculate translation
        Vector3 groundPosition = new Vector3(headPosition.x, cameraRigT.position.y, headPosition.z);
        Vector3 translateVector = destinationPos - groundPosition;

        // move
        cameraRigT.position += translateVector;

        if (toggleWaves)
        {
            Invoke("TogglePauseWaveSystem", BETWEEN_LOCATIONS);
        }
        teleportPillar.SetActive(false);

        // Reposition the ability ring
        StartCoroutine(GameObject.FindWithTag("Right Hand").GetComponent<PlayerAbility>().RepositionAbilityRing());
        StartCoroutine(GameObject.FindWithTag("Left Hand").GetComponent<PlayerAbility>().RepositionAbilityRing());
    }

    public void TogglePauseWaveSystem()
    {
        pauseWaveSystem = !pauseWaveSystem;
    }

}