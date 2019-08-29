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
    
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("MainCamera");
        playerPos = player.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
//        playerPos = player.transform.position;
        agent.SetDestination(playerPos);

        // check that target is inside range radius
        if (agent.remainingDistance < 5f)
        {
            
        }
        
        // check for visibility to target through ray cast
        
        // if visible shoot!! (beware of shooting rate)
        
    }
}
