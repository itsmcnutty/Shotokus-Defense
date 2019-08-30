using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Experimental.Rendering;

public class LightEnemyController : MonoBehaviour
{
    
    public NavMeshAgent agent;
    private GameObject player;
    private Vector3 playerPos;

    public GameObject projectilePrefab;
    private GameObject projectile;

    private Vector3 agentHead;

    private float bulletSpeed = 2500f;
    private float fireRate = 1f; // how many second to wait between shots
    private bool allowShoot;
    
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("MainCamera");
        playerPos = player.transform.position;
        allowShoot = true;
    }

    
    // todo fix remaining distance not working properly
    
    
    // Update is called once per frame
    void Update()
    {
        playerPos = player.transform.position;
        agent.SetDestination(playerPos);

        agentHead = transform.position;
        agentHead.y = 2.5f;

        // remaining distance to target
        float remainingDist = Vector3.Distance(playerPos, agentHead);
//        Debug.Log("vector3 distance is " + remainingDist);
        
        
        // if agent is close enough, do range attack
        if (remainingDist < 5f)
        {
            agent.isStopped = true;
        }
        
        // check that target is inside range radius
        if (remainingDist < 15f)
        {
            // check for visibility to target through ray cast
            RaycastHit hit;

            Vector3 rayDirection = playerPos - agentHead; // todo this might shoot from the feet
            
            // set where the ray is coming from and its direction
            Ray visionRay = new Ray(agentHead, rayDirection);
            
            Debug.DrawRay(agentHead, rayDirection);

            // if player is visible and fire Rate is good, shoot!
            if (Physics.Raycast(visionRay, out hit)) 
            {
                if (hit.collider.tag == "PlayerCollider")
                {
                    // we can hit the player, so shoot
                    shoot();
                }
            }
        }
    }

    // Instantiates the projectile prefab, sets a velocity and the origin transform (where the projectile comes from)
    // and shoots towards the target. 
    // This function also sets the fire rate of the gun
    private void shoot()
    {
        if (allowShoot)
        {
            Debug.Log("Shooting");
            allowShoot = false;
            projectile = Instantiate(projectilePrefab);
            projectile.transform.position = agentHead;
            projectile.transform.LookAt(playerPos);
            projectile.GetComponent<Rigidbody>().AddForce(projectile.transform.forward * bulletSpeed);
            StartCoroutine(Wait(fireRate));
        }
    }

    IEnumerator Wait(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        allowShoot = true;
    }
    
    
    
}
