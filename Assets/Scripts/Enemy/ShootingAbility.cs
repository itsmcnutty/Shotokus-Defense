using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootingAbility : MonoBehaviour
{

    public GameObject projectilePrefab;
    private GameObject projectile;
    private Rigidbody rigidbody;
    private GameObject player;
    private float initialVelocityX;
    private float fireRate;

    private bool allowShoot;
    
    
    // Start is called before the first frame update
    void Start()
    {
        allowShoot = true;
        player = GameObject.FindWithTag("MainCamera");
    }

    // Instantiates the projectile prefab, sets a velocity and the origin transform (where the projectile comes from)
    // so it is ready to shoot the target.
    // This function also sets the fire rate of the gun
    public void Shoot(float initialVelocityX, float fireRate, Animator animator)
    {
        if (allowShoot)
        {
            // Block additional shots
            allowShoot = false;
            
            // Store info about arrow for shooting
            this.initialVelocityX = initialVelocityX;
            this.fireRate = fireRate;
            
            // Instantiate and set position where projectile spawns
            projectile = Instantiate(projectilePrefab, transform);
            rigidbody = projectile.GetComponent<Rigidbody>();
            
            // Begin animation
            animator.SetTrigger("Shoot");
        }
    }

    // Called by animator when entering the "ReleasingArrow" state
    public void ReleaseArrow()
    {
        // Store location of player and projectile
        Vector3 playerPos = player.transform.position;
        Vector3 projPos = transform.position;
        
        // Release the arrow from the hand
        DropArrow();

        // start calculating direction and velocity in X and Y axis for projectile
        Vector3 dirToEnemy = playerPos - projPos;
        dirToEnemy.y = 0;
        float distanceX = dirToEnemy.magnitude; // difference in the X axis from enemy to player
        float distanceY = playerPos.y - projectile.transform.position.y; // difference in Y axis from enemy to player
        double tempInitialY = (initialVelocityX / distanceX) *
                              (distanceY + (- 0.5 * Physics.gravity.y * Mathf.Pow(distanceX, 2) / Mathf.Pow(initialVelocityX,2)));
        float velInitialY = (float) tempInitialY;
        dirToEnemy = dirToEnemy.normalized;
        Vector3 velocity = dirToEnemy * initialVelocityX + Vector3.up * velInitialY;
            
        // set rotation and add velocity vector to projectile
        projectile.transform.LookAt(playerPos);
        rigidbody.velocity = velocity;
        
        // wait for fire rate timer
        StartCoroutine(Wait(fireRate));
    }

    // Waits for the input number of seconds (waiTime), and then allows the enemy to shoot
    private IEnumerator Wait(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        allowShoot = true;
    }

    // Removes the arrow in hand so it may be shot or dropped
    public void DropArrow()
    {
        projectile.transform.parent = null;
        rigidbody.isKinematic = false;
        rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }
    
}
