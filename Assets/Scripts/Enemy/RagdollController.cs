using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RagdollController : MonoBehaviour
{

    // Physics material for all of the enemy's colliders
    public PhysicMaterial PHYSIC_MATERIAL;
    
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
        Debug.Log("I am ragdoll");
        rigidbodies = GetComponentsInChildren<Rigidbody>();
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        AddCPCToChildren(rigidbodies[0].gameObject);

        for (int i = 0; i < rigidbodies.Length; i++)
        {
            rigidbodies[i].gameObject.GetComponent<Collider>().material = PHYSIC_MATERIAL;
        }
        
    }

    // Adds a CallParentCollider component to this gameobject and all of its children recursively
    private void AddCPCToChildren(GameObject obj)
    {
        obj.AddComponent<CallParentCollision>();
        
        // Break after looping through all children (or has no children)
        for (int i = 0; i < obj.transform.childCount; i++)
        {
            AddCPCToChildren(obj.transform.GetChild(i).gameObject);
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
        agent.velocity = Vector3.zero;

        // Zero velocity of all rigidbodies so they don't maintain this from the animation
        foreach (var rigidbody in rigidbodies)
        {
            rigidbody.velocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
        }

        StartCoroutine("WaitAndStop");
    }

    // Re-enables the Animator to regain control of Rigidbodies
    public void StopRagdoll()
    {
        Debug.Log("Stop ragdoll");
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
