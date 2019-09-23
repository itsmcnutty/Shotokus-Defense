using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileCollide : MonoBehaviour
{
    private GameObject player;
    private PlayerHealth playerHealth;

    public float projectileDamage = 100;
    public TrailRenderer trail;
    private float WALL_LIFETIME = 3f;

    
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("MainCamera");
        playerHealth = player.GetComponent<PlayerHealth>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision other)
    {
        trail.enabled = false;
        // todo in theory nothing else but the player should have the player collider tag, therefore i can be sure that inside this if statement i should damage the player
        if (other.gameObject.CompareTag("PlayerCollider"))
        {
            // Needed if doing non-VR mode
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(projectileDamage);
            }
        }
        // destroy projectile after colliding with any object and make its damage 0
        Destroy(gameObject, WALL_LIFETIME);
        projectileDamage = 0;
    }

}
