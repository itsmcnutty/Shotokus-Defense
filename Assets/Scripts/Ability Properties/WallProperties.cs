using System;
using UnityEngine;
using UnityEngine.AI;

public class WallProperties : MonoBehaviour
{
    private float wallHeightPercent;
    private float wallMoveSpeed = 0f;
    private Vector3 direction = new Vector3();
    private ParticleSystem destroyWallParticles;

    private float wallLifetime = 1;//30.0f;
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
        shape.scale = transform.localScale;

        UnityEngine.ParticleSystem.EmissionModule emissionModule = particleSystem.emission;
        emissionModule.rateOverTimeMultiplier = (float) Math.Pow(700, gameObject.GetComponentInChildren<MeshRenderer>().bounds.size.x);
        Debug.Log(gameObject.GetComponentInChildren<MeshRenderer>().bounds.size.x);
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
        if(wallMoveSpeed != 0)
        {
            gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, gameObject.transform.position + (direction * wallMoveSpeed), 1f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag != "Ground" && other.gameObject.layer != 9 && other.gameObject.layer != 11 && other.gameObject.layer != 17)
        {
            CancelInvoke("MoveWall");
        }

        if(other.gameObject.name == "Player Ability Area")
        {
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
