using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RetreatState : StateMachineBehaviour
{
	// Radius to retreat to
	public float retreatRadius;
    
	// This is the agent to move around by NavMesh
	public NavMeshAgent agent;
	// The player's head
	public GameObject player;
	// Doesn't walk if true (for debugging)
	public bool debugNoWalk = false;
	
	// How fast enemy turns to face player
	private float TURN_SPEED = 0.03f;
	// Squared retreat radius (for optimized calculations)
	private float sqrRetreatRadius;
    
	// The player's head's world location
	private Vector3 playerPos;
	// This enemy's transform component
	private Transform gameObjTransform;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);

		sqrRetreatRadius = retreatRadius * retreatRadius;
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateUpdate(animator, stateInfo, layerIndex);

		if (!agent.enabled)
		{
			agent.enabled = true;
			return;
		}
		
		// Store transform variables for player and this enemy
		playerPos = player.transform.position;
		gameObjTransform = animator.gameObject.transform;
		Vector3 gameObjPos = gameObjTransform.position;
		
		// Back up
		Vector3 backUpVector = gameObjPos - playerPos;
		backUpVector.Normalize();
                
		if (agent.enabled && !debugNoWalk)
		{
			agent.SetDestination(playerPos + 1.5f * retreatRadius * backUpVector);
			agent.angularSpeed = 0f;
			
			// Don't decelerate
			agent.stoppingDistance = 0f;
		}

		TurnToPlayer();
                
		// Pass reverse move speed to animator
		animator.SetFloat("WalkSpeed", -agent.velocity.magnitude); 
		
		///// Transition /////

		// Calculate enemy distance
		float sqrDist = (float)(Math.Pow(playerPos.x - gameObjPos.x, 2) +
		                        Math.Pow(playerPos.z - gameObjPos.z, 2));
		
		if (sqrDist - sqrRetreatRadius > 0f)
		{
			// Done retreating, attack
			animator.SetTrigger("Melee");
		}
	}

	// Smoothly turn enemy to face player
	private void TurnToPlayer()
	{
		Vector3 vectorToPlayer = playerPos - gameObjTransform.position;
		Quaternion lookAtPlayer = Quaternion.Euler(
			0f,
			(float) (180.0 / Math.PI * Math.Atan2(vectorToPlayer.x, vectorToPlayer.z)),
			0f);
        
		// Set Y-rotation to be the same as the Y-rotation of the vector to the player
		gameObjTransform.rotation = Quaternion.Lerp(gameObjTransform.rotation, lookAtPlayer, TURN_SPEED);
	}
}
