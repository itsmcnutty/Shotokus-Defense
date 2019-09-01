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

    private Vector3 agentHead; // this is where the ray cast originates, determines if enemy can see player

    private float bulletSpeed = 500f;
    private float fireRate = 3f; // how many second to wait between shots
    private bool allowShoot; // keep track if enemy can shoot based on fire rate timer

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("MainCamera");
        playerPos = player.transform.position;
        allowShoot = true;
    }


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
        if (remainingDist < 10f)
        {
            // todo start moving in circles
            // ...
            agent.isStopped = true;
        }

        if (agent.isStopped)
        {
            pointsAround(new Vector3(0f,0f,0f));
        }


        /* uncomment **********************************************************************8
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
                if (hit.collider.CompareTag("PlayerCollider"))
                {
                    // we can hit the player, so shoot
                    shoot();
                }
            }
        }
        */
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

    // given a point, return points around its circunference of radius r and every 45 degrees (pi/4 radians)
    Vector3[] pointsAround(Vector3 center)
    {
        float radius = 5;
        float angle = 0;
        Vector3[] points = new Vector3[8];
        Vector3 coord;

        for (int i = 0; i < 8; i++)
        {
            // x is x and y is z, in 3D coordinates unity
            float x = Mathf.Cos(angle) * radius;
            float y = Mathf.Sin(angle) * radius;
            Vector3 offset = new Vector3(center.x,0,center.z);
            coord = new Vector3(x, 0, y) + offset;
            Debug.Log("Iteration: " + i);
            Debug.Log("Angle: " + angle);
            Debug.Log(coord);
            Debug.Log("");
            angle += Mathf.PI / 4;
            points[i] = coord;
        }
        
        Debug.Log(points);
        return points;
    }
    
    // given a enemy position and an array of possible future positions, return the next closest point
    // todo do function for this



}
