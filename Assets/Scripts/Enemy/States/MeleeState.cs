using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class MeleeState : StateMachineBehaviour
{
	// Radius for attacking
	public float attackRadius;
	// Time between attacks (seconds)
	public float attackDelay;

	// This is the agent to move around by NavMesh
	public NavMeshAgent agent;
	// The NavMeshObstacle used to block enemies pathfinding when not moving
	public NavMeshObstacle obstacle;
	// The player's head
	public GameObject player;
	
	// Allowed space around attack radius that enemy's can attack from
	private float ATTACK_MARGIN = 1f;
	// How fast enemy turns to face player
	private float TURN_SPEED = 0.03f;
	// Squared attack radius (for optimized calculations)
	private float sqrAttackRadius;
	
	// Timer for attack delay
	private float attackTimer = 0f;
	// The player's head's world location
	private Vector3 playerPos;
	// This enemy's transform component
	private Transform gameObjTransform;
	
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		
		sqrAttackRadius = attackRadius * attackRadius;
		playerPos = player.transform.position;
		
		// Can't walk, acts as an obstacle
		agent.enabled = false;
		obstacle.enabled = true;
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);

		obstacle.enabled = false;
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateUpdate(animator, stateInfo, layerIndex);
		
		// Store transform variables for player and this enemy
		playerPos = player.transform.position;
		gameObjTransform = animator.gameObject.transform;

		// Decrement attack timer
		attackTimer -= Time.deltaTime;

		TurnToPlayer();

		// When attackTimer is lower than 0, it allows the enemy to attack again
		if (attackTimer <= 0f)
		{
			animator.SetInteger("AttackNum", Random.Range(0, 2));
			animator.SetTrigger("Swing");
			attackTimer = attackDelay;
		}
		
		///// Transition /////
		
		// Calculate enemy distance
		Vector3 gameObjPos = gameObjTransform.position;
		float sqrDist = (float)(Math.Pow(playerPos.x - gameObjPos.x, 2) +
		                        Math.Pow(playerPos.z - gameObjPos.z, 2));
		
		// Continue to attack if within attack range, otherwise transition
		if (sqrDist - sqrAttackRadius > ATTACK_MARGIN)
		{
			// Too far, advance
			animator.SetTrigger("Advance");
		}
		else if (sqrDist - sqrAttackRadius < -ATTACK_MARGIN)
		{
			// Too close, retreat
			animator.SetTrigger("Retreat");
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
