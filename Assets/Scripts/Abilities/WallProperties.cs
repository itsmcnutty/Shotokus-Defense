using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WallProperties : MonoBehaviour
{

    public NavMeshSurface surface;
    public float wallLifetime = 5.0f;
    private bool collisionDetected;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Destroy(gameObject, wallLifetime);
    }

    void OnDestroy()
    {
        surface.BuildNavMesh();
    }
}
