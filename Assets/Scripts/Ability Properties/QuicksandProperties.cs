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

    private AudioSource quicksandIdleSound;
    private AudioSource quicksandSlowSound;
    private AudioSource quicksandBreakSound;
    private static Dictionary<NavMeshAgent, float> slowedEnemies = new Dictionary<NavMeshAgent, float>();

    public static void CreateComponent(GameObject quicksand, float maxEarthquakeDistance, float earthquakeDuration,
    AudioSource quicksandIdleSound, AudioSource quicksandSlowSound, AudioSource quicksandBreakSound)
    {
        QuicksandProperties properties = quicksand.AddComponent<QuicksandProperties>();
        properties.maxEarthquakeDistance = maxEarthquakeDistance;
        properties.earthquakeDuration = earthquakeDuration;
        properties.quicksandIdleSound = quicksandIdleSound;
        properties.quicksandSlowSound = quicksandSlowSound;
        properties.quicksandBreakSound = quicksandBreakSound;
    }

    // Start is called before the first frame update
    void Start()
    {
        quicksandIdleSound.Play();
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
        quicksandBreakSound.Play();
        quicksandIdleSound.Stop();
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
        quicksandSlowSound.Play();
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
        quicksandSlowSound.Stop();
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