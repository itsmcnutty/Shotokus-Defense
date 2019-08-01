using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollController : MonoBehaviour
{

    // All Rigidbodies of the enemy's ragdoll
    private Rigidbody[] rigidbodies;
    // The enemy's Animator component
    private Animator animator;
    // True when ragdolling
    private bool ragdolling = false;
    
    // Start is called before the first frame update
    void Start()
    {
        rigidbodies = GetComponentsInChildren<Rigidbody>();
        animator = GetComponent<Animator>();

        foreach (var rigidbody in rigidbodies)
        {
            rigidbody.gameObject.AddComponent<CallParentCollision>();
            rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }

        foreach (var collider in GetComponentsInChildren<Collider>())
        {
            collider.enabled = false;
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
        animator.enabled = false;

        // Zero velocity of all rigidbodies so they don't maintain this from the animation
        foreach (var rigidbody in rigidbodies)
        {
            rigidbody.velocity = 10f*Vector3.back;
        }

        StartCoroutine("WaitAndStop");
    }

    // Re-enables the Animator to regain control of Rigidbodies
    public void StopRagdoll()
    {
        ragdolling = false;
        animator.enabled = true;
        transform.position = rigidbodies[0].transform.position;
        animator.SetTrigger("Reset");
        animator.Update(0f);
    }

    IEnumerator WaitAndStop()
    {
        yield return new WaitForSeconds(4f);
        StopRagdoll();
    }
}
