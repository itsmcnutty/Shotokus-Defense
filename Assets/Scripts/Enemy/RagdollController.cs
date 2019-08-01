using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollController : MonoBehaviour
{

    // All Rigidbodies of the enemy's ragdoll
    private Rigidbody[] rigidbodies;
    // True when ragdolling
    private bool ragdolling;
    
    // Start is called before the first frame update
    void Start()
    {
        rigidbodies = GetComponentsInChildren<Rigidbody>();

        foreach (var rigidbody in rigidbodies)
        {
            rigidbody.gameObject.AddComponent<CallParentCollision>();
            rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }
    }
    
    // Returns true when the enemy is ragdolling
    public bool IsRagdolling()
    {
        return ragdolling;
    }

    // Disables the Animator to allow Rigidbodies to obey physics
    public void StartRagdoll()
    {
        ragdolling = true;
        GetComponent<Animator>().enabled = false;

        // Zero velocity of all rigidbodies so they don't maintain this from the animation
        foreach (var rigidbody in rigidbodies)
        {
            rigidbody.velocity = Vector3.zero;
        }
    }

    // Re-enables the Animator to regain control of Rigidbodies
    public void StopRagdoll()
    {
        ragdolling = false;
        GetComponent<Animator>().enabled = true;
    }
}
