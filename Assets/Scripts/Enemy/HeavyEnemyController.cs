using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Timers;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Experimental.PlayerLoop;

public class HeavyEnemyController : MonoBehaviour
{
    // Time between attacks (seconds)
    public float ATTACK_DELAY = 2f;
    // Radius for attacking
    public double ATTACK_RADIUS;
    // Timer for attack delay
    private float attackTimer = 0f;
    
    // This is the agent to move around by NavMesh/**/
    public NavMeshAgent agent;
    
    private RagdollController ragdollController;
    private Animator animator;
    private GameObject player;
    
    private Vector3 playerPos;
    private Vector3 randomPos;

    private void Start()
    {
        animator = gameObject.GetComponent<Animator>();
        ragdollController = gameObject.GetComponent<RagdollController>();
        player = GameObject.FindGameObjectWithTag("MainCamera");
        
        playerPos = player.transform.position;
        randomPos = GetRandomNearTarget(playerPos);
//        Debug.Log("Player pos is: " + playerPos);
//        Debug.Log("Enemy pos is: " + randomPos);

    }

    // Update is called once per frame
    void Update()
    {
        // Store transform variables for player and this enemy
        playerPos = player.transform.position;
        Vector3 gameObjPos = transform.position;
    
        // Calculate direction 
//        Vector3 moveDir = playerPos - gameObjPos;
//        moveDir.y = 0;
//        moveDir.Normalize();
//        transform.forward = moveDir;
    
        // Calculate enemy distance
        double dist = Math.Sqrt(Math.Pow(playerPos.x - gameObjPos.x, 2) +
                                      Math.Pow(playerPos.z - gameObjPos.z, 2));
        
        // Move speed is equal to speed if enemy is far away. Otherwise proportional to dist from follow radius.
        float moveSpeed = agent.velocity.magnitude;
	    // Move
        agent.SetDestination(playerPos);
        
        
        // Pass speed to animation controller
        animator.SetFloat("WalkSpeed", moveSpeed);

        // Decrement attack timer
        attackTimer -= Time.deltaTime;
        
        // When attackTimer is lower than 0, it allows the enemy to attack again 
        if (attackTimer <= 0f && dist <= ATTACK_RADIUS)
        {
            agent.isStopped = true;
            animator.SetTrigger("Slash");
            attackTimer = ATTACK_DELAY;
        }
        
        // Animator should be walking if outside attack radius
        if (agent.isStopped)
        {
            if (!ragdollController.IsRagdolling() &&
                !animator.GetCurrentAnimatorStateInfo(0).IsTag("Melee") &&
                !animator.GetCurrentAnimatorStateInfo(0).IsName("BeginAttack"))
            {
                agent.isStopped = false;
            }
        }
        
    }
    
    // todo WIP Returns a position near the target (player) based on their transforms
    Vector3 GetRandomNearTarget(Vector3 playerPos)
    {
        int maxRadius = 5;
        int minRadius = 2;
        
        Vector2 rndPos = UnityEngine.Random.insideUnitCircle * (maxRadius - minRadius);
        rndPos += rndPos.normalized * minRadius;
        return new Vector3(playerPos.x + rndPos.x, playerPos.y, playerPos.z + rndPos.y);
    }
    
}
