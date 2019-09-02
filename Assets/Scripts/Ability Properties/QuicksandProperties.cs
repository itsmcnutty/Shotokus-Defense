using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class QuicksandProperties : MonoBehaviour
{
    private float quicksandLifetime = 3;//30.0f;
    private float quicksandSpeedReduction = 3f;
    private float maxEarthquakeDistance;
    private float earthquakeDuration;
    private ParticleSystem destroyQuicksandParticles;
    private static Dictionary<NavMeshAgent, float> slowedEnemies = new Dictionary<NavMeshAgent, float>();

    public static void CreateComponent(GameObject quicksand, float maxEarthquakeDistance, float earthquakeDuration, ParticleSystem destroyQuicksandParticles)
    {
        QuicksandProperties properties = quicksand.AddComponent<QuicksandProperties>();
        properties.maxEarthquakeDistance = maxEarthquakeDistance;
        properties.earthquakeDuration = earthquakeDuration;
        properties.destroyQuicksandParticles = destroyQuicksandParticles;
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
                    float distanceToEarthquake = (enemy.transform.position - gameObject.transform.position).magnitude;
                    if (distanceToEarthquake < maxEarthquakeDistance && !slowedEnemies.TryGetValue(agent, out float speed))
                    {
                        slowedEnemies.Add(agent, agent.speed);
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
        if(destroyQuicksandParticles != null)
        {
            ParticleSystem particleSystem = Instantiate(destroyQuicksandParticles);
            particleSystem.transform.position = transform.position;
            particleSystem.transform.rotation = transform.rotation;

            UnityEngine.ParticleSystem.ShapeModule shape = particleSystem.shape;
            shape.scale = transform.localScale;

            UnityEngine.ParticleSystem.EmissionModule emissionModule = particleSystem.emission;
            emissionModule.rateOverTimeMultiplier = (float) Math.Pow(700, gameObject.GetComponentInChildren<MeshRenderer>().bounds.size.x);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        NavMeshAgent agent = other.gameObject.GetComponentInParent<NavMeshAgent>();
        if (agent != null && !slowedEnemies.TryGetValue(agent, out float speed))
        {
            agent.speed /= quicksandSpeedReduction;
            slowedEnemies.Add(agent, agent.speed);
        }
        else if (other.attachedRigidbody != null)
        {
            other.attachedRigidbody.velocity /= quicksandSpeedReduction;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        NavMeshAgent agent = other.gameObject.GetComponentInParent<NavMeshAgent>();
        if(slowedEnemies.TryGetValue(agent, out float speed))
        {
            if(agent != null)
            {
                agent.speed = speed;
            }
            slowedEnemies.Remove(agent);
        }
    }

    private IEnumerator SlowEnemyForTime(NavMeshAgent agent, float slowRate, float duration)
    {
        agent.speed *= slowRate;
        yield return new WaitForSeconds(duration);
        if (agent != null && slowedEnemies.TryGetValue(agent, out float speed))
        {
            agent.speed = speed;
        }
        slowedEnemies.Remove(agent);
    }
}