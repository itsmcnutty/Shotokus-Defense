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
    public float ATTACK_RADIUS;
    
    // Squared attack radius (for optimized calculations)
    private float sqrAttackRadius;
    // Timer for attack delay
    private float attackTimer = 0f;
    
    // This is the agent to move around by NavMesh
    public NavMeshAgent agent;
    // The NavMeshObstacle used to block enemies pathfinding when not moving
    public NavMeshObstacle obstacle;
    
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
        agent.stoppingDistance = (float)ATTACK_RADIUS;
        sqrAttackRadius = ATTACK_RADIUS * ATTACK_RADIUS;
        
        playerPos = player.transform.position;
        randomPos = GetRandomNearTarget(playerPos);
    }

    // Update is called once per frame
    void Update()
    {
        // Store transform variables for player and this enemy
        playerPos = player.transform.position;
        Vector3 gameObjPos = transform.position;
        
        // Pass speed to animation controller
        float moveSpeed = agent.velocity.magnitude;
        animator.SetFloat("WalkSpeed", moveSpeed);

        // Decrement attack timer
        attackTimer -= Time.deltaTime;
    
        // Calculate direction 
//        Vector3 moveDir = playerPos - gameObjPos;
//        moveDir.y = 0;
//        moveDir.Normalize();
//        transform.forward = moveDir;
        
        /*
        // Calculate enemy distance
        double dist = Math.Sqrt(Math.Pow(playerPos.x - gameObjPos.x, 2) +
                                      Math.Pow(playerPos.z - gameObjPos.z, 2));
	    // Move
        agent.SetDestination(playerPos);

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
        */
        
        // TODO: Re-do movement and attack control with stopping and navmesh obstacle
        
        // Calculate enemy distance
        float sqrDist = (float)(Math.Pow(playerPos.x - gameObjPos.x, 2) +
                                Math.Pow(playerPos.z - gameObjPos.z, 2));

        // Ragdolling takes precedence over other behaviors
        if (ragdollController.IsRagdolling())
        {
            // No walking, no obstacle
            agent.enabled = false;
            obstacle.enabled = false;
        }
        // If not ragdolling, check if enemy is in attack range and done walking
        else if (sqrDist <= sqrAttackRadius)
        {
            // Can't walk, acts as an obstacle
            agent.enabled = false;
            obstacle.enabled = true;
            
            // Turn to face player
            Vector3 vectorToPlayer = playerPos - gameObjPos;
            transform.rotation = Quaternion.Euler(
                0f,
                (float)(180.0 / Math.PI * Math.Atan2(vectorToPlayer.x, vectorToPlayer.z)), 
                0f);

            // When attackTimer is lower than 0, it allows the enemy to attack again 
            if (attackTimer <= 0f)
            {
                animator.SetTrigger("Slash");
                attackTimer = ATTACK_DELAY;
            }
        }
        // Not attacking or ragdolling
        else if (!animator.GetCurrentAnimatorStateInfo(0).IsTag("Melee") &&
                 !animator.GetCurrentAnimatorStateInfo(0).IsName("BeginAttack"))
        {
            // Walks and is not an obstacle
            obstacle.enabled = false;
            agent.enabled = true;
            
            // Move
            agent.SetDestination(playerPos);

            // Stopping distance will cause enemy to decelerate into attack radius
            agent.stoppingDistance = ATTACK_RADIUS + moveSpeed * moveSpeed / (2 * agent.acceleration);
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
