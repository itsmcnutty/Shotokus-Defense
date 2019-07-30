using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;

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

    private CharacterController characterController;
    private Animator animator;
    private GameObject player;

    private void Start()
    {
        characterController = gameObject.GetComponent<CharacterController>();
        animator = gameObject.GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("MainCamera");

        attackRadius = FOLLOW_RADIUS + 0.5;
    }

    // Update is called once per frame
    void Update()
    {
        // Store transform variables for player and this enemy
        Vector3 playerPos = player.transform.position;
        Vector3 gameObjPos = transform.position;
    
        // Calculate direction 
        Vector3 moveDir = playerPos - gameObjPos;
        moveDir.y = 0;
        moveDir.Normalize();
        transform.forward = moveDir;
    
        // Calculate enemy distance
        double dist = Math.Sqrt(Math.Pow(playerPos.x - gameObjPos.x, 2) +
                                      Math.Pow(playerPos.z - gameObjPos.z, 2));
        
        // Move speed is equal to speed if enemy is far away. Otherwise proportional to dist from follow radius.
        float moveSpeed = SPEED * (float)Math.Min(1f, dist - FOLLOW_RADIUS);
        
	    // Move
        characterController.SimpleMove(moveSpeed * Time.deltaTime * moveDir);
        
        // Pass speed to animation controller
        animator.SetFloat("WalkSpeed", moveSpeed / 80f);

        // Decrement attack timer
        attackTimer -= Time.deltaTime;
        
        if (attackTimer <= 0f && dist <= attackRadius)
        {
            animator.SetTrigger("Slash");
            attackTimer = ATTACK_DELAY;
        }
    }
}
