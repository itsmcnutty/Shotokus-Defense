using UnityEngine;
using UnityEngine.AI;

public class WallProperties : MonoBehaviour
{
    private float wallLifetime = 30.0f;
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
//        GameObject.FindWithTag("NavMesh").GetComponent<NavMeshSurface>().BuildNavMesh();
    }
}
