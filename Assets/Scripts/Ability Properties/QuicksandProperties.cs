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

    private ParticleSystem destroyQuicksandParticles;

    private AudioSource audioSource;
    private AudioClip quicksandIdleSound;
    private AudioClip quicksandSlowSound;
    private AudioClip quicksandBreakSound;
    private static Dictionary<NavMeshAgent, float> slowedEnemies = new Dictionary<NavMeshAgent, float>();

    public static void CreateComponent(GameObject quicksand, float maxEarthquakeDistance, float earthquakeDuration, ParticleSystem destroyQuicksandParticles,
        AudioClip quicksandIdleSound, AudioClip quicksandSlowSound, AudioClip quicksandBreakSound)
    {
        QuicksandProperties properties = quicksand.AddComponent<QuicksandProperties>();
        properties.maxEarthquakeDistance = maxEarthquakeDistance;
        properties.earthquakeDuration = earthquakeDuration;
        properties.quicksandIdleSound = quicksandIdleSound;
        properties.quicksandSlowSound = quicksandSlowSound;
        properties.quicksandBreakSound = quicksandBreakSound;
        properties.destroyQuicksandParticles = destroyQuicksandParticles;
    }

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        
        // Invokes the method to destroy quicksand after the allotted time is up
        Invoke("DestroyQuicksand", quicksandLifetime);

        if (PlayerAbility.EarthquakeEnabled)
        {
            // Applies earthquake effect to all enemies
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (GameObject enemy in enemies)
            {
                // Gets all of the NavMeshAgents
                NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
                if (agent != null)
                {
                    float distanceToEarthquake = (enemy.transform.position - transform.position).magnitude;

                    // Slow those within earthquake radius, ragdoll those within quicksand bounds
                    if (distanceToEarthquake < maxEarthquakeDistance)
                    {
                        float slowRate = distanceToEarthquake / maxEarthquakeDistance;
                        StartCoroutine(SlowEnemyForTime(agent, slowRate * quicksandSpeedReduction, earthquakeDuration));
                        if (distanceToEarthquake < gameObject.GetComponentInChildren<MeshRenderer>().bounds.size.x)
                        {
                            RagdollController ragdollController = enemy.GetComponent<RagdollController>();
                            ragdollController.StartRagdoll();
                        }
                    }
                }
            }
        }
    }

    void OnDestroy()
    {
        // Prevents the destruction method from being called again if destroyed by another means
        CancelInvoke("DestroyQuicksand");

        // Resets the speed of any enemies within the quicksand
        Collider[] collidersInSand = Physics.OverlapSphere(transform.position, GetComponentInChildren<MeshRenderer>().bounds.size.x / 2);

        foreach (var collider in collidersInSand)
        {
            NavMeshAgent agent = collider.GetComponentInParent<NavMeshAgent>();
            if (agent)
            {
                UnslowAgent(agent);
            }
        }

        if (destroyQuicksandParticles != null)
        {
            // Plays the particles effect for destroying quicksand
            ParticleSystem particleSystem = Instantiate(destroyQuicksandParticles);
            particleSystem.transform.position = transform.position;
            particleSystem.transform.rotation = transform.rotation;

            // Changes the area size and number of particles based on size of the mesh
            UnityEngine.ParticleSystem.ShapeModule shape = particleSystem.shape;
            shape.scale = transform.localScale / 2;

            UnityEngine.ParticleSystem.EmissionModule emission = particleSystem.emission;
            emission.rateOverTimeMultiplier = transform.localScale.x * emission.rateOverTimeMultiplier;
        }
    }

    private void DestroyQuicksand()
    {
        audioSource.PlayOneShot(quicksandBreakSound);
        Destroy(gameObject);
    }

    private void OnTriggerStay(Collider other)
    {
        // Slows any enemies within the quicksand
        NavMeshAgent agent = other.gameObject.GetComponentInParent<NavMeshAgent>();
        if (agent != null)
        {
            SlowAgent(agent, quicksandSpeedReduction);
        }
        else if (other.attachedRigidbody != null)
        {
            // Slows any object within the quicksand
            other.attachedRigidbody.velocity /= quicksandSpeedReduction;
        }
        audioSource.PlayOneShot(quicksandSlowSound);
    }

    private void OnTriggerExit(Collider other)
    {
        // Enemies speed back up after leaving the quicksand
        NavMeshAgent agent = other.gameObject.GetComponentInParent<NavMeshAgent>();
        if (agent != null)
        {
            UnslowAgent(agent);
        }
        //quicksandSlowSound.Stop();
    }

    private IEnumerator SlowEnemyForTime(NavMeshAgent agent, float slowRate, float duration)
    {
        // Slows the given enemy for the given duration
        SlowAgent(agent, slowRate);
        yield return new WaitForSeconds(duration);
        if (agent)
        {
            UnslowAgent(agent);
        }
    }

    // Set the speed of the nav agent to be the current maximum speed divided by the specified slow divisor
    private void SlowAgent(NavMeshAgent agent, float slowDivisor)
    {
        EnemyProperties enemyProps = agent.gameObject.GetComponent<EnemyProperties>();
        if (enemyProps)
        {
            agent.speed = enemyProps.GetCurrentMaxSpeed() / slowDivisor;
        }
    }

    // Set the speed of the nav agent to be the current maximum speed for the state it is in (no slow multiplier)
    private void UnslowAgent(NavMeshAgent agent)
    {
        EnemyProperties enemyProps = agent.gameObject.GetComponent<EnemyProperties>();
        if (enemyProps)
        {
            agent.speed = enemyProps.GetCurrentMaxSpeed();
        }
    }
}