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
    
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("MainCamera");
        playerPos = player.transform.position;
    }

    
    // todo fix remaining distance not working properly
    
    
    // Update is called once per frame
    void Update()
    {
//        playerPos = player.transform.position;
        agent.SetDestination(playerPos);

        agentHead = transform.position;
        agentHead.y = 2.5f;

        Debug.Log(agent.remainingDistance);

        var temp = agent.remainingDistance;
        
        // if agent is close enough, do range attack
//        if (temp < 5f)
//        {
//            agent.isStopped = true;
//        }
        
        
        // check that target is inside range radius
        if (agent.remainingDistance < 15f)
        {
            // check for visibility to target through ray cast
            RaycastHit hit;

            Vector3 rayDirection = playerPos - agentHead; // todo this might shoot from the feet
            
            // set where the ray is coming from and its direction
            Ray visionRay = new Ray(agentHead, rayDirection);
            
            Debug.DrawRay(agentHead, rayDirection);

            // if player is visible, shoot!! (beware of shooting rate)
            if (Physics.Raycast(visionRay, out hit)) 
            {
                if (hit.collider.tag == "PlayerCollider")
                {
                    // we can hit the player, so shoot
                    Debug.Log("We can shoot the player");
                    projectile = Instantiate(projectilePrefab);
                    projectile.transform.position = agentHead;
                    projectile.transform.LookAt(playerPos);
                    projectile.GetComponent<Rigidbody>().AddForce(projectile.transform.forward * bulletSpeed);

                }
            }



        }
        
        
    }
    
    
    
}
