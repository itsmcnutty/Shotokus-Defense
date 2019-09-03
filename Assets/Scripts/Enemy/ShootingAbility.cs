using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootingAbility : MonoBehaviour
{

    public GameObject projectilePrefab;
    private GameObject projectile;

    private bool allowShoot;
    
    
    // Start is called before the first frame update
    void Start()
    {
        allowShoot = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    // Instantiates the projectile prefab, sets a velocity and the origin transform (where the projectile comes from)
    // and shoots towards the target. 
    // This function also sets the fire rate of the gun
    public void shoot(Vector3 agentHead, Vector3 playerPos, float initialVelocityX, float fireRate)
    {
        
        if (allowShoot)
        {
            allowShoot = false;
            // Instantiate and set position where projectile spawns
            projectile = Instantiate(projectilePrefab);
            projectile.transform.position = agentHead;   // todo change this, so it comes out from right hand

            // start calculating direction and velocity in X and Y axis for projectile
            Vector3 dirToEnemy = playerPos - agentHead;
            dirToEnemy.y = 0;
            float velInitialX = initialVelocityX; // input initial velocity in X axis   // todo this is up to us, change as needed
            float distanceX = dirToEnemy.magnitude; // difference in the X axis from enemy to player
            float distanceY = playerPos.y - projectile.transform.position.y; // difference in Y axis from enemy to player
            double tempInitialY = (velInitialX / distanceX) *
                                  (distanceY + (- 0.5 * Physics.gravity.y * Mathf.Pow(distanceX, 2) / Mathf.Pow(velInitialX,2)));
            float velInitialY = (float) tempInitialY;
            dirToEnemy = dirToEnemy.normalized;
            Vector3 velocity = dirToEnemy * velInitialX + Vector3.up * velInitialY;
            
            // set rotation and add velocity vector to projectile
            projectile.transform.LookAt(playerPos);
            projectile.GetComponent<Rigidbody>().velocity = velocity;
            // wait for fire rate timer
            StartCoroutine(Wait(fireRate));
        }
    }

    // this function waits for the input number of seconds (waiTime), and then allows the enemy to shoot
    IEnumerator Wait(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        allowShoot = true;
    }

}
