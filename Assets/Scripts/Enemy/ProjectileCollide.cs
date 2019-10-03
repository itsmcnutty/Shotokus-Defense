using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class ProjectileCollide : MonoBehaviour
{
    private GameObject player;
    private PlayerHealth playerHealth;

    public float projectileDamage = 100;
    public TrailRenderer trail;

    [Header("Audio")]
    public AudioSource flyLoop;
    public AudioSource hitPlayer;
    public AudioMultiClipSource hitSolid;
    public AudioMultiClipSource hitFoliage;
    public PhysicMaterial foliageMaterial;

    private Rigidbody projectileRigidbody;
    private float WALL_LIFETIME = 3f;
    private Quaternion initialRotation;

    
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("MainCamera");
        playerHealth = player.GetComponent<PlayerHealth>();
        projectileRigidbody = GetComponent<Rigidbody>();
        initialRotation = Quaternion.Euler(90, 0, 0);
        trail.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (trail.enabled)
        {
            // Begin flying audio loop
            if (!flyLoop.isPlaying)
            {
                flyLoop.Play();
            }
            transform.rotation = Quaternion.LookRotation(projectileRigidbody.velocity) * initialRotation;
        }
    }

    public void StartRotation()
    {
        trail.enabled = true;
    }

    private void OnCollisionEnter(Collision other)
    {
        // No longer flying
        flyLoop.Stop();
        trail.enabled = false;
        
        // todo in theory nothing else but the player should have the player collider tag, therefore i can be sure that inside this if statement i should damage the player
        if (other.gameObject.CompareTag("PlayerCollider") && projectileDamage > 0)
        {
            // Needed if doing non-VR mode
            if (playerHealth != null)
            {
                // Play sound and deal damage
                hitPlayer.Play();
                playerHealth.TakeDamage(projectileDamage);
            }
        }
        else
        {
            // Collision sound
            if (other.collider.material.Equals(foliageMaterial))
            {
                hitFoliage.PlayRandom();
            }
            else
            {
                hitSolid.PlayRandom();
            }
        }
        // destroy projectile after colliding with any object and make its damage 0
        Destroy(gameObject, WALL_LIFETIME);
        projectileDamage = 0;
    }

}
