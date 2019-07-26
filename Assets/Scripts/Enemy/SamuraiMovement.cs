using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SamuraiMovement : MonoBehaviour
{

    // Character speed
    public float SPEED = 0f;
    // How close the character must be before it stops coming closer
    public float FOLLOW_RADIUS = 1f;

    // Update is called once per frame
    void Update()
    {
        // Store transform variables for player and this enemy
        Vector3 playerPos = GameObject.FindGameObjectWithTag("Player").transform.position;
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
        gameObject.GetComponent<CharacterController>().SimpleMove(moveSpeed * Time.deltaTime * moveDir);
        
        // Pass speed to animation controller
        gameObject.GetComponent<Animator>().parameters.SetValue(moveSpeed, 0);
    }
}
