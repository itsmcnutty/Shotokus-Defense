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

    private float currentTime;
    private bool pauseWaveSystem = true;

//    // todo testing
//    private bool restart= false;


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
            // todo maybe restart enemy energy when restarting game
//            playerEnergy = player.GetComponent<PlayerEnergy> ();
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
//
//        // todo testing
//        if (restart)
//        {
//            //
//        }
        
        currentTime += Time.deltaTime;

        if (currentWave != null)
        {
            SpawnInfo spawnInfo = currentWave.GetSpawnAtTime(currentTime);
            if (spawnInfo != null && spawnInfo.Location != SpawnInfo.SpawnLocation.None)
            {
                enemyProducer.Spawn(spawnInfo);
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
            // not all enemies have been destroyed, so don't do anything
            return;
        }

        SpawnInfo spawnInfo = currentWave.GetNextSpawnTimeInfo(out float? newTime);
        if (spawnInfo != null)
        {
            // if all enemies have been killed before next wave time, then spawn early
            currentTime = newTime.Value; // this moves foward time to spawnInfo time
            enemyProducer.Spawn(spawnInfo);
            return;
        }

        currentTime = 0;
        currentWave = currentLocation.GetNextWave();

        // dont continue in the function if there are more waves in the current location
        if (currentWave != null)
        {
            TogglePauseWaveSystem();
            Invoke("TogglePauseWaveSystem", BETWEEN_WAVES);
            return;
        }

        // if there are no waves left, check if there are more locations
        if (allLocationWaves.Count != 0)
        {
//            currentLocation = resetLocation = allLocationWaves.Dequeue();
//            resetLocation = new Queue<LocationWaves>(allLocationWaves);

            currentTime = 0;
            currentLocationCounter++;
            currentLocation = allLocationWaves.Dequeue();
            currentWave = currentLocation.GetNextWave();
            TogglePauseWaveSystem();
            Invoke("TogglePauseWaveSystem", BETWEEN_LOCATIONS);
            Teleport();
            return;
        }
        // no more waves left so you win
        Debug.Log("YOU WIN");
    }

    public void StartGameWithTutorial()
    {
        Teleport();
        TutorialController.Instance.SelectTutorial(TutorialController.TutorialSections.Rock);
    }

    // this function restarts the allLocationWaves queue by enqueue all the location json files based on the current location postion counter
    public void restartQueue()
    {
        allLocationWaves = new Queue<LocationWaves>();
        for (int i = currentLocationCounter; i < locationWaveFiles.Length; i++)
        {
            allLocationWaves.Enqueue(JsonParser.parseJson(locationWaveFiles[i]));
        }
    }

    public void StartGameWithoutTutorial()
    {
        Teleport();
        PlayerAbility.ToggleRockAbility();
        PlayerAbility.ToggleSpikeAbility();
        PlayerAbility.ToggleWallAbility();
        PlayerAbility.ToggleQuicksandAbility();
        Invoke("TogglePauseWaveSystem", BEFORE_WAVE1);
    }

    // Future: delete all other instances of objects in the scene
    // delete walls, spikes, rocks
    public void RestartGame()
    {

//        restart = true;
        
        // reactivate pause functionality
        UIControllerObj.GetComponent<MenuUIController>().enabled = true;
        
        // destroy all objects in scene before restarting
        destroyAll();
        
        Debug.Log("Restarting game");

        // Reset values of wave (queue, timer, enemies counter)
        enemiesAlive = 0;
        restartQueue();
        currentLocation = allLocationWaves.Dequeue();
        currentWave = currentLocation.GetNextWave();
        playerHealth.RecoverAllHealth();
        // todo restore all  energy
    }

    // this function destroys all the following game objects instances:
    // rocks, walls, spikes, quicksand, menus, enemies, particles of the abilities
    public void destroyAll()
    {
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (var enemy in enemies) 
            Destroy(enemy);

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
        destroyAll();
        gameOverController.GameOverScreen();
    }

    // This function keeps track of destroyed enemies by updating enemiesDestroyed variable
    // To be called when an enemey is destroyed
    public void EnemyGotDestroyed()
    {
        enemiesAlive--;
    }

    // Called in EnemyProducer, updates number of enemies alive
    public void EnemyAddNumAlive(int amount)
    {
        enemiesAlive += amount;
    }

    // This function moves the player around the 5 wave zones
    // todo update player object position too
    public void Teleport()
    {
        Vector3 destinationPos;
        int temp = caseSwitch;
        caseSwitch += 1;

        // Get camera rig and head position
        Transform cameraRigT = cameraRig.transform;
        Vector3 headPosition = vrCamera.transform.position;

        Debug.Log("Teleport!");
        temp = temp % 5;
        switch (temp)
        {
            case 0:
                // playerObj.transform.position = new Vector3(9,0.25f,33);
                destinationPos = new Vector3(9, 0.25f, 33);
                break;
            case 1:
                // playerObj.transform.position = new Vector3(22.6f,0.25f,18.8f);
//                destinationPos = new Vector3(22.6f, 0.5f, 18.8f);
                destinationPos = new Vector3(26f, 0.5f, 18.8f);
                break;
            case 2:
                // playerObj.transform.position = new Vector3(-3f,0.25f,3.1f);
                destinationPos = new Vector3(-3f, 0.75f, 3.1f);
                break;
            case 3:
                // playerObj.transform.position = new Vector3(26,0.25f,-22.8f);
                destinationPos = new Vector3(26, 1f, -22.8f);
                break;
            case 4:
                // playerObj.transform.position = new Vector3(-1.5f,0.25f,-31.5f);
                destinationPos = new Vector3(-1.5f, 0.75f, -31.5f);
                break;
            default:
                // playerObj.transform.position = new Vector3(0,0,0);
                destinationPos = new Vector3(0, 0, 0);
                break;
        }

        // Calculate translation
        Vector3 groundPosition = new Vector3(headPosition.x, cameraRigT.position.y, headPosition.z);
        Vector3 translateVector = destinationPos - groundPosition;

        // move
        cameraRigT.position += translateVector;
        //        playerObj.transform.position = destinationPos;

        // Reposition the ability ring
        StartCoroutine(GameObject.FindWithTag("Right Hand").GetComponent<PlayerAbility>().RepositionAbilityRing());
        StartCoroutine(GameObject.FindWithTag("Left Hand").GetComponent<PlayerAbility>().RepositionAbilityRing());
    }

    public void TogglePauseWaveSystem()
    {
        pauseWaveSystem = !pauseWaveSystem;
    }

}