﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Timers;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Experimental.PlayerLoop;

public class HeavyEnemyControllerOld : MonoBehaviour
{
    // Time between attacks (seconds)
    public float ATTACK_DELAY = 2f;
    // Radius for attacking
    public float ATTACK_RADIUS;

    // Allowed space around attack radius that enemy's can attack from
    private float ATTACK_MARGIN = 1f;
    // How fast enemy turns to face player
    private float TURN_SPEED = 0.03f;
    
    // Squared attack radius (for optimized calculations)
    private float sqrAttackRadius;
    // Timer for attack delay
    private float attackTimer = 0f;
    
    // This is the agent to move around by NavMesh
    public NavMeshAgent agent;
    // The NavMeshObstacle used to block enemies pathfinding when not moving
    public NavMeshObstacle obstacle;
    public bool debugNoWalk = false;
    
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
        agent.stoppingDistance = ATTACK_RADIUS;
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
        // If not ragdolling, check if enemy is in attack range or performing attack
        else if (Math.Abs(sqrDist - sqrAttackRadius) <= ATTACK_MARGIN ||
                 !animator.GetCurrentAnimatorStateInfo(0).IsTag("Movement") ||
                 animator.GetCurrentAnimatorStateInfo(0).IsName("BeginAttack"))
        {
            // Can't walk, acts as an obstacle
            agent.enabled = false;
            obstacle.enabled = true;

            TurnToPlayer();

            // When attackTimer is lower than 0, it allows the enemy to attack again
            if (attackTimer <= 0f)
            {
                animator.SetTrigger("Slash");
                attackTimer = ATTACK_DELAY;
            }
        }
        // Not attacking or ragdolling
        else if (animator.GetCurrentAnimatorStateInfo(0).IsTag("Movement"))
        {
            // Walks and is not an obstacle
            obstacle.enabled = false;
            if (!agent.enabled)
            {
                StartCoroutine("EnablePathfindAfterFrame");
            }

            // If too close
            if (sqrDist - sqrAttackRadius < 0)
            {
                // Back up
                Vector3 backUpVector = gameObjPos - playerPos;
                backUpVector.Normalize();
                
                if (agent.enabled && !debugNoWalk)
                {
                    agent.SetDestination(playerPos + 1.5f * ATTACK_RADIUS * backUpVector);
                    agent.angularSpeed = 0f;
                
                    // Don't decelerate
                    agent.stoppingDistance = 0f;
                }

                TurnToPlayer();
                
                // Pass reverse move speed to animator
                animator.SetFloat("WalkSpeed", -moveSpeed);
            }
            else
            {
                // Move to player
                if (agent.enabled && !debugNoWalk)
                {
                    agent.SetDestination(playerPos);
                    agent.angularSpeed = 8000f;

                    // Stopping distance will cause enemy to decelerate into attack radius
                    agent.stoppingDistance = ATTACK_RADIUS + moveSpeed * moveSpeed / (2 * agent.acceleration);
                }
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

    private void TurnToPlayer()
    {
        Vector3 vectorToPlayer = playerPos - transform.position;
        Quaternion lookAtPlayer = Quaternion.Euler(
            0f,
            (float) (180.0 / Math.PI * Math.Atan2(vectorToPlayer.x, vectorToPlayer.z)),
            0f);
        
        // Set Y-rotation to be the same as the Y-rotation of the vector to the player
        transform.rotation = Quaternion.Lerp(transform.rotation, lookAtPlayer, TURN_SPEED);
    }

    private IEnumerator EnablePathfindAfterFrame()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        agent.enabled = true;
    }
    
}