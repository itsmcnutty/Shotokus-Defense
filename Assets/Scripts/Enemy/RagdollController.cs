using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RagdollController : MonoBehaviour
{

    // All Rigidbodies of the enemy's ragdoll
    private Rigidbody[] rigidbodies;
    // The enemy's Animator component
    private Animator animator;
    // The enemy's NavMeshAgent componenet
    private NavMeshAgent agent;
    // True when ragdolling
    private bool ragdolling = false;
    
    // Start is called before the first frame update
    void Start()
    {
        rigidbodies = GetComponentsInChildren<Rigidbody>();
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

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
        
        // Disable animation and pathfinding
        animator.enabled = false;
        agent.isStopped = true;

        // Zero velocity of all rigidbodies so they don't maintain this from the animation
        foreach (var rigidbody in rigidbodies)
        {
            rigidbody.velocity = Vector3.zero;
        }

        StartCoroutine("WaitAndStop");
    }

    // Re-enables the Animator to regain control of Rigidbodies
    public void StopRagdoll()
    {
        ragdolling = false;
        
        // Re-enable animation
        animator.enabled = true;
        
        // Move to position where ragdoll was laying and re-enable pathfinding
        transform.position = rigidbodies[0].transform.position;
        agent.isStopped = false;
        
        // Restart animation in Walking state
        animator.SetTrigger("Reset");
        animator.Update(0f);
    }

    IEnumerator WaitAndStop()
    {
        yield return new WaitForSeconds(4f);
        StopRagdoll();
    }
}
