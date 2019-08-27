using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class QuicksandProperties : MonoBehaviour
{
    private float quicksandLifetime = 30.0f;
    private float quicksandSpeedReduction = 3f;
    private float maxEarthquakeDistance;
    private float earthquakeDuration;
    private static List<NavMeshAgent> slowedEnemies = new List<NavMeshAgent>();

    public static void CreateComponent(GameObject quicksand, float maxEarthquakeDistance, float earthquakeDuration)
    {
        QuicksandProperties properties = quicksand.AddComponent<QuicksandProperties>();
        properties.maxEarthquakeDistance = maxEarthquakeDistance;
        properties.earthquakeDuration = earthquakeDuration;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (PlayerAbility.EarthquakeEnabled())
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (GameObject enemy in enemies)
            {
                NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
                if (agent != null)
                {
                    Debug.Log(agent.name);
                    float distanceToEarthquake = (enemy.transform.position - gameObject.transform.position).magnitude;
                    if (distanceToEarthquake < maxEarthquakeDistance && !slowedEnemies.Contains(agent))
                    {
                        slowedEnemies.Add(agent);
                        float slowRate = distanceToEarthquake / maxEarthquakeDistance;
                        slowRate = (slowRate != 0) ? (float) Math.Pow(slowRate, 1) : float.MinValue;
                        StartCoroutine(SlowEnemyForTime(agent, slowRate, earthquakeDuration));
                        if(distanceToEarthquake < gameObject.GetComponentInChildren<MeshRenderer>().bounds.size.x)
                        {
                            RagdollController ragdollController = enemy.GetComponent<RagdollController>();
                            ragdollController.StartRagdoll();
                        }
                    }
                }
            }
        }
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

    private IEnumerator SlowEnemyForTime(NavMeshAgent agent, float slowRate, float duration)
    {

        Debug.Log("Before slow: " + agent.speed);
        agent.speed *= slowRate;
        Debug.Log("After slow: " + agent.speed);
        yield return new WaitForSeconds(duration);
        if (agent != null)
        {
            agent.speed /= slowRate;
            Debug.Log("Finished slow: " + agent.speed);
        }
        slowedEnemies.Remove(agent);
        Debug.Log("NumSlowed=" + slowedEnemies.Count);
    }
}