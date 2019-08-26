using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class QuicksandProperties : MonoBehaviour
{
    private float quicksandLifetime = 30.0f;
    private float quicksandSpeedReduction = 3f;
    private static List<NavMeshAgent> slowedEnemies = new List<NavMeshAgent>();
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

    private void OnTriggerEnter(Collider other)
    {
        NavMeshAgent agent = other.gameObject.GetComponentInParent<NavMeshAgent>();
        if (agent != null && !slowedEnemies.Contains(agent))
        {
            agent.speed /= quicksandSpeedReduction;
            slowedEnemies.Add(agent);
        }
        else if (other.attachedRigidbody != null)
        {
            other.attachedRigidbody.velocity /= quicksandSpeedReduction;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        NavMeshAgent agent = other.gameObject.GetComponentInParent<NavMeshAgent>();
        if (agent != null && slowedEnemies.Contains(agent))
        {
            agent.speed *= quicksandSpeedReduction;
            slowedEnemies.Remove(agent);
        }
    }
}