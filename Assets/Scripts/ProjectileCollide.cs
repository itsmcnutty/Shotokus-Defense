using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileCollide : MonoBehaviour
{
    private GameObject player;
    private PlayerHealth playerHealth;

    public float PROJECTILE_DAMAGE = 100;
    
    // Start is called before the first frame update
    void Start()
    {
        // todo should this check for player tag or maincamera tag?
        player = GameObject.FindGameObjectWithTag("MainCamera");
        playerHealth = player.GetComponent<PlayerHealth>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision other)
    {
        // todo in theory nothing else but the player should have the player collider tag, therefore i can be sure that inside this if statement i should damage the playtyer
        if (other.gameObject.CompareTag("PlayerCollider"))
        {
            Debug.Log("Damaging the player");
            // todo only needed if doing non-VR mode
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(PROJECTILE_DAMAGE);
            }
            
            // todo put this back into the code
//            other.gameObject.GetComponent<PlayerHealth>().TakeDamage(PROJECTILE_DAMAGE);
//            Destroy(this.gameObject);
        }
    }

}
