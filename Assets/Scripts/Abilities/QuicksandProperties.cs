using UnityEngine;
using UnityEngine.AI;

public class QuicksandProperties : MonoBehaviour
{
    private float quicksandLifetime = 30.0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Destroy(gameObject, quicksandLifetime);
    }

    void OnDestroy()
    {
//        GameObject.FindWithTag("NavMesh").GetComponent<NavMeshSurface>().BuildNavMesh();
    }
}
