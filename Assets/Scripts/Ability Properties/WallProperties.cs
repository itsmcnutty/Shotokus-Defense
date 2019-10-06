using System;
using UnityEngine;
using UnityEngine.AI;

public class WallProperties : MonoBehaviour
{
    private float wallLifetime = 30.0f;
    private float wallHeightPercent;
    private float wallMoveSpeed = 0f;
    private Vector3 direction = new Vector3();
    private ParticleSystem destroyWallParticles;

    private NavMeshSurface surfaceWalls;
    private GameObject parentObject;

    // Start is called before the first frame update
    void Start()
    {
        surfaceWalls = GameObject.FindGameObjectWithTag("NavMesh Walls").GetComponent<NavMeshSurface>();
        InvokeRepeating("MoveWall", 0, 0.01f);
    }

    // Update is called once per frame
    void Update()
    {
        Destroy(gameObject, wallLifetime);
    }

    void OnDestroy()
    {
        surfaceWalls.BuildNavMesh();
        ParticleSystem particleSystem = Instantiate(destroyWallParticles);
        particleSystem.transform.position = parentObject.transform.position;
        particleSystem.transform.rotation = parentObject.transform.rotation;

        UnityEngine.ParticleSystem.ShapeModule shape = particleSystem.shape;
        shape.scale = new Vector3(parentObject.transform.localScale.x, parentObject.transform.localScale.y * wallHeightPercent, parentObject.transform.localScale.z);

        UnityEngine.ParticleSystem.EmissionModule emission = particleSystem.emission;
        emission.rateOverTimeMultiplier = parentObject.transform.localScale.x * emission.rateOverTimeMultiplier;
    }

    public static void CreateComponent(GameObject wall, float wallHeightPercent, Vector3 direction, float wallMoveSpeed,
    ParticleSystem destroyWallParticles)
    {
        WallProperties wallProperties = wall.transform.GetChild(0).gameObject.AddComponent<WallProperties>();
        wallProperties.parentObject = wall;
        wallProperties.wallHeightPercent = wallHeightPercent;
        wallProperties.direction = direction;
        wallProperties.wallMoveSpeed = wallMoveSpeed;
        wallProperties.destroyWallParticles = destroyWallParticles;
    }

    public static void CreateComponent(GameObject wall, float wallHeightPercent, ParticleSystem destroyWallParticles)
    {
        WallProperties wallProperties = wall.transform.GetChild(0).gameObject.AddComponent<WallProperties>();
        wallProperties.parentObject = wall;
        wallProperties.wallHeightPercent = wallHeightPercent;
        wallProperties.direction = Vector3.zero;
        wallProperties.wallMoveSpeed = 0;
        wallProperties.destroyWallParticles = destroyWallParticles;
    }

    public static void UpdateComponent(GameObject wall, float wallHeightPercent, Vector3 direction, float wallMoveSpeed)
    {
        WallProperties wallProperties = wall.GetComponentInChildren<WallProperties>();
        if (wallProperties)
        {
            wallProperties.wallHeightPercent = wallHeightPercent;
            wallProperties.direction = direction;
            wallProperties.wallMoveSpeed = wallMoveSpeed;
            if(wallMoveSpeed == 0)
            {
                Rigidbody[] rigidbodyWalls = wall.GetComponentsInChildren<Rigidbody>();
                foreach(Rigidbody rigidbodyWall in rigidbodyWalls)
                {
                    rigidbodyWall.isKinematic = true;
                }
            }
        }
    }

    private void MoveWall()
    {
        // Moves the wall if given a velocity from the move wall powerup
        if (wallMoveSpeed != 0)
        {
            parentObject.transform.position = Vector3.MoveTowards(parentObject.transform.position, parentObject.transform.position + (direction * wallMoveSpeed), 1f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 9 && wallHeightPercent == 0)
        {
            other.gameObject.GetComponentInParent<RagdollController>().StartRagdoll();
        }
        if (!other.CompareTag("Ground") && other.gameObject.layer != 11 && other.gameObject.layer != 17)
        {
            // Stops the wall from moving when it collides with something it can't move through
            Rigidbody rigidbodyWall = gameObject.GetComponent<Rigidbody>();
            rigidbodyWall.isKinematic = true;
            CancelInvoke("MoveWall");
        }

        if (other.gameObject.name == "Player Ability Area" && wallHeightPercent != 0)
        {
            // Destroys the wall if it enters the player's play area
            Destroy(gameObject);
        }
    }

    public float WallHeightPercent
    {
        get
        {
            return wallHeightPercent;
        }
    }
}