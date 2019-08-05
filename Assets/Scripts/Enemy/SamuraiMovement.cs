using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Timers;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Experimental.PlayerLoop;

public class SamuraiMovement : MonoBehaviour
{

    // Character speed
    public float SPEED = 0f;
    // How close to the player the enemy attempts to stay
    public double FOLLOW_RADIUS = 1f;
    // Time between attacks (seconds)
    public float ATTACK_DELAY = 2f;
    // How close the enemy must be to begin attacking
    private double attackRadius;
    // Timer for attack delay
    private float attackTimer = 0f;
    
    // THis is the agent to move around by NavMesh/**/
    public NavMeshAgent agent;


    private CharacterController characterController;
    private Animator animator;
    private GameObject player;
    
    private Vector3 playerPos;
    private Vector3 randomPos;

    private void Start()
    {
        characterController = gameObject.GetComponent<CharacterController>();
        animator = gameObject.GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("MainCamera");

        attackRadius = FOLLOW_RADIUS + 0.5;
     
        playerPos = player.transform.position;
        randomPos = GetRandomNearTarget(playerPos);
        Debug.Log("Player pos is: " + playerPos);
        Debug.Log("Enemy pos is: " + randomPos);

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
//        float moveSpeed = SPEED * (float)Math.Min(1f, dist - FOLLOW_RADIUS);
        // todo what do i need? current speed at a certain time?
        float moveSpeed = agent.velocity.magnitude;
//        Debug.Log("movement speed is " + moveSpeed);
//        
	    // Move
//        characterController.SimpleMove(moveSpeed * Time.deltaTime * moveDir);
        agent.SetDestination(playerPos);
        
        
        // Pass speed to animation controller
        animator.SetFloat("WalkSpeed", moveSpeed );

        // Decrement attack timer
        attackTimer -= Time.deltaTime;
        
        
        // when attackTimer is lower than 0, it allows the enemy to attack again 
        if (attackTimer <= 0f && dist <= attackRadius)
        {
//            Debug.Log("attackTime: " + attackTimer);
//            Debug.Log("Im going to SLASH");
            agent.isStopped = true;
            animator.SetTrigger("Slash");
            attackTimer = ATTACK_DELAY;
        }
        // outside attack radius, therefore animator should be walking
        else
        {
            if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Slashing")) // if not in slashing animation
            {
//                Debug.Log("Im not slashing, I WILL WALK");
                agent.isStopped = false;
            }
            else // if slashing then stop walking
            {
                agent.isStopped = true;
            }
        }
        
    }
    
    // Returns a position near the target (player) based on their transforms
    Vector3 GetRandomNearTarget(Vector3 playerPos)
    {
        int maxRadius = 5;
        int minRadius = 2;
        
        Vector2 rndPos = UnityEngine.Random.insideUnitCircle * (maxRadius - minRadius);
        rndPos += rndPos.normalized * minRadius;
        return new Vector3(playerPos.x + rndPos.x, playerPos.y, playerPos.z + rndPos.y);
    }
    
    
}
