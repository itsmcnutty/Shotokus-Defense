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

    // shooting variables
    private float bulletSpeed = 500f;
    private float fireRate = 3f; // how many second to wait between shots
    private bool allowShoot; // keep track if enemy can shoot based on fire rate timer

    // variables for strafying
    private bool isStrafing; // bool indicating if agent is in strafing state
    private Vector3[] pointsAroundTarget; // points around target(player) with radius, and every 45 degrees
    private Vector3 circularPointDest; // point where the agent will move towards when strafying in circular motion
    private int lastPointIndex; // last point index value in the pointsAroundTarget array
    private bool isClockwise = false; // walk in a clockwise direction when strafying

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("MainCamera");
        playerPos = player.transform.position;
        agent.SetDestination(playerPos);
        allowShoot = true;
        isStrafing = false;
        lastPointIndex = 0;
        
        if (Random.Range(0, 2) == 0)
        {
            // clockwise
            isClockwise = true;
        }
    }


    // Update is called once per frame
    void Update()
    {
        
        agentHead = transform.position;
        // todo change this, this is the head height value
        agentHead.y = 0f;

        // remaining distance to target
        // todo instead of using agentHead, use new variable with positions in the floor (y = 0)
        float remainingDist = Vector3.Distance(playerPos, agentHead);
        Debug.Log("vector3 distance is " + remainingDist);

        // not in strafying mode
        if (remainingDist > 10f)
        {
            playerPos = player.transform.position;
            agent.SetDestination(playerPos);
        }


        // if agent is close enough and not strafing yet, enter strafe/shooting state
        // calculate points and set new destination
        if (remainingDist < 10f && !isStrafing)
        {
            // do not enter here if already strafing
            isStrafing = true;
            
            // Calculate points around the target (player) given a set radius, and every 45 degrees (pi/4 radians)
            pointsAroundTarget = pointsAround(playerPos);
            
            // pick the closest of these points to the enemy
            circularPointDest = closestPoint(agentHead, pointsAroundTarget);
            
            // change enemy agent target to the new point
            agent.SetDestination(circularPointDest);
        }

        // if moving towards strafing point, check if it has being destination has been reached
        // if reached, calculate next moving point
        if (isStrafing)
        {
            // do not change destination until current one is reached
            // when destination is reached, move to next point 
            float strafeRemainingDist = Vector3.Distance(agentHead, circularPointDest);
            Debug.Log(strafeRemainingDist);
            
            if (strafeRemainingDist < 1f)
            {
                // get next point
                if (isClockwise)
                {
                    // clockwise, do absolute value
                    Debug.Log("My agent is Clockwise");
                    lastPointIndex--;
                    if (lastPointIndex < 0)
                    {
                        lastPointIndex = pointsAroundTarget.Length;
                    }
                }
                else
                {
                    // counter clockwise
                    Debug.Log("My agent is counter clockwise");
                    lastPointIndex++;
                }
                circularPointDest = pointsAroundTarget[Mathf.Abs(lastPointIndex % 8)];
//                Debug.Log("Changing target to index " + lastPointIndex%8);
//                Debug.Log("moving towards " +circularPointDest);
                agent.SetDestination(circularPointDest);
            }
        }
        


        /* uncomment **********************************************************************
         // todo SHOOTING STATE
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
        float radius = 5; // todo range away from player that the enemy should start strafying
        float angle = 0;
        Vector3 coord;
        Vector3[] points = new Vector3[8];
        Vector3 offset = new Vector3(center.x,0,center.z);
        
        // todo keep track of every angle
        
        for (int i = 0; i < 8; i++)
        {
            // x is x and y is z, in 3D coordinates unity
            // vector is represented as (x, 0, y)
            float x = Mathf.Cos(angle) * radius;
            float y = Mathf.Sin(angle) * radius;
            coord = new Vector3(x, 0, y) + offset;
//            Debug.Log("Iteration: " + i);
//            Debug.Log("Angle: " + angle);
//            Debug.Log(coord);
//            Debug.Log("");
            angle += Mathf.PI / 4;
            points[i] = coord;
        }
        return points;
    }
    
    // given a enemy position and an array of possible future positions, return the next closest point
    Vector3 closestPoint(Vector3 enemyPos, Vector3[] points)
    {
        // initialize temp variables to first value in array of points
        float closestDist = Vector3.Distance(enemyPos, points[0]);;
        Vector3 closestPoint = points[0];
        
        for(int i = 0; i < points.Length; i++)
        {
            Vector3 point = points[i];
            float tempDist = Vector3.Distance(enemyPos, point);
            if (tempDist < closestDist)
            {
                closestPoint = point;
                closestDist = tempDist;
                lastPointIndex = i;
            }
        }
        return closestPoint;
    }



}
