using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SamuraiMovement : MonoBehaviour
{

    public const float SPEED = 0.001f;

    // Update is called once per frame
    void Update()
    {
        Transform playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        Vector3 move = SPEED * (transform.position - playerTransform.position).normalized;
        
        transform.Translate(move);
    }
}
