using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SamuraiMovement : MonoBehaviour
{

    public float SPEED = 0.01f;

    // Update is called once per frame
    void Update()
    {
        // Store transform variables for player and this enemy
        Transform playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        Transform gameObjTransform = transform;

        // Calculate direction 
        Vector2 moveDir = gameObjTransform.position - playerTransform.position;
        moveDir.Normalize();
        gameObjTransform.forward = moveDir;

        gameObject.GetComponent<CharacterController>().SimpleMove(SPEED * Time.deltaTime * moveDir);
    }
}
