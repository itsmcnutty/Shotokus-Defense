using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations;

// State machine behavior used by heavy enemy's Advancing state
public class AdvanceState : StateMachineBehaviour
{
	// Radius for attacking
	public float attackRadius;
    
	// This is the agent to move around by NavMesh
	public NavMeshAgent agent;
	// The player's head
	public GameObject player;
	// Doesn't walk if true (for debugging)
	public bool debugNoWalk = false;

	// Allowed space around attack radius that enemy's can attack from
	private float ATTACK_MARGIN = 1f;
	// Squared attack radius (for optimized calculations)
	private float sqrAttackRadius;
    
    // The player's head's world location
	private Vector3 playerPos;
	
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		agent.stoppingDistance = attackRadius;
		sqrAttackRadius = attackRadius * attackRadius;

		playerPos = player.transform.position;

		agent.enabled = true;
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateUpdate(animator, stateInfo, layerIndex);
		
		// Store transform variables for player and this enemy
		playerPos = player.transform.position;
		Vector3 gameObjPos = animator.gameObject.transform.position;
        
		// Pass speed to animation controller
		float moveSpeed = agent.velocity.magnitude;
		animator.SetFloat("WalkSpeed", moveSpeed);

		// Calculate enemy distance
		float sqrDist = (float)(Math.Pow(playerPos.x - gameObjPos.x, 2) +
		                        Math.Pow(playerPos.z - gameObjPos.z, 2));
		
		// Move to player if outside attack range, otherwise transition
		if (!debugNoWalk)
		{
			// Too far, walk closer
			agent.SetDestination(playerPos);
			agent.angularSpeed = 8000f;

			// Stopping distance will cause enemy to decelerate into attack radius
			agent.stoppingDistance = attackRadius + moveSpeed * moveSpeed / (2 * agent.acceleration);
		}
		
		///// Transition /////
		
		if (sqrDist - sqrAttackRadius < ATTACK_MARGIN)
		{
			// In range, attack
			animator.SetTrigger("Melee");
		}
	}
}
