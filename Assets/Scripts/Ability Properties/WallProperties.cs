using System;
using UnityEngine;
using UnityEngine.AI;

public class WallProperties : MonoBehaviour
{
    private float wallHeightPercent;
    private float wallMoveSpeed = 0f;
    private Vector3 direction = new Vector3();
    private ParticleSystem destroyWallParticles;

    private float wallLifetime = 30.0f;
    private NavMeshSurface surfaceWalls;

    // Start is called before the first frame update
    void Start()
    {
        surfaceWalls = GameObject.FindGameObjectWithTag("NavMesh Walls").GetComponent<NavMeshSurface>();
        InvokeRepeating ("MoveWall", 0, 0.01f);
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
        particleSystem.transform.position = transform.position;
        particleSystem.transform.rotation = transform.rotation;

        UnityEngine.ParticleSystem.ShapeModule shape = particleSystem.shape;
        shape.scale = new Vector3(transform.localScale.x, transform.localScale.y * wallHeightPercent, transform.localScale.z);

        UnityEngine.ParticleSystem.EmissionModule emission = particleSystem.emission;
        emission.rateOverTimeMultiplier = gameObject.transform.localScale.x * emission.rateOverTimeMultiplier;
    }

    public static void CreateComponent (GameObject wall, float wallHeightPercent, Vector3 direction, float wallMoveSpeed, ParticleSystem destroyWallParticles)
    {
        WallProperties wallProperties = wall.AddComponent<WallProperties> ();
        wallProperties.wallHeightPercent = wallHeightPercent;
        wallProperties.direction = direction;
        wallProperties.wallMoveSpeed = wallMoveSpeed;
        wallProperties.destroyWallParticles = destroyWallParticles;
    }

    public static void CreateComponent (GameObject wall, float wallHeightPercent, ParticleSystem destroyWallParticles)
    {
        WallProperties wallProperties = wall.AddComponent<WallProperties> ();
        wallProperties.wallHeightPercent = wallHeightPercent;
        wallProperties.direction = Vector3.zero;
        wallProperties.wallMoveSpeed = 0;
        wallProperties.destroyWallParticles = destroyWallParticles;
    }

    private void MoveWall()
    {
        // Moves the wall if given a velocity from the move wall powerup
        if(wallMoveSpeed != 0)
        {
            gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, gameObject.transform.position + (direction * wallMoveSpeed), 1f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag != "Ground" && other.gameObject.layer != 9 && other.gameObject.layer != 11 && other.gameObject.layer != 17)
        {
            // Stops the wall from moving when it collides with something it can't move through
            CancelInvoke("MoveWall");
        }

        if(other.gameObject.name == "Player Ability Area")
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
