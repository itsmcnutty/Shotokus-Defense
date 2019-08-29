using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RetreatState : IState
{
	// Radius to retreat to
	public float retreatRadius;
    
	// This is the agent to move around by NavMesh
	public NavMeshAgent agent;
	// The enemy's Animator component
	private Animator animator;
	// The enemy's ragdoll controller
	private RagdollController ragdollController;
	// The NavMeshObstacle used to block enemies pathfinding when not moving
	private NavMeshObstacle obstacle;
	// Doesn't walk if true (for debugging)
	public bool debugNoWalk = false;
	
	// Squared retreat radius (for optimized calculations)
	private float sqrRetreatRadius;
    
	// The player's head
	public GameObject player;
	// The player's head's world location
	private Vector3 playerPos;
	// This enemy's GameObject
	private GameObject gameObj;
	//This enemy's world location
	private Vector3 gameObjPos;
	// The enemy properties component
	private EnemyHeavyProperties enemyProps;

	// States to transition to
	private MeleeState meleeState;
	private RagdollState ragdollState;

	public RetreatState(EnemyHeavyProperties enemyProps)
	{
		retreatRadius = enemyProps.ATTACK_RADIUS;
		agent = enemyProps.agent;
		animator = enemyProps.animator;
		ragdollController = enemyProps.ragdollController;
		obstacle = enemyProps.obstacle;
		player = enemyProps.player;
		debugNoWalk = enemyProps.debugNoWalk;
		sqrRetreatRadius = enemyProps.sqrAttackRadius;
		playerPos = enemyProps.playerPos;
		gameObj = enemyProps.gameObject;
		meleeState = enemyProps.meleeState;
		ragdollState = enemyProps.ragdollState;
		this.enemyProps = enemyProps;
	}
	
	// Initializes the IState instance fields. This occurs after the enemy properties class has constructed all of the
	// necessary states for the machine
	public void InitializeStates(EnemyHeavyProperties enemyProps)
	{
		meleeState = enemyProps.meleeState;
		ragdollState = enemyProps.ragdollState;
	}

	// Called upon entering this state from anywhere
	public void Enter()
	{
		// No longer obstacle
		obstacle.enabled = false;
		enemyProps.EnablePathfind();
		
		// Don't automatically decelerate or rotate
		agent.stoppingDistance = 0f;
		agent.angularSpeed = 0f;
	}

	// Called upon exiting this state
	public void Exit() {}

	// Called during Update while currently in this state
	public void Action()
	{
		if (!agent.enabled)
		{
			agent.enabled = true;
			return;
		}
		
		// Store transform variables for player and this enemy
		playerPos = player.transform.position;
		gameObjPos = gameObj.transform.position;
		
		// Back up
		Vector3 backUpVector = gameObjPos - playerPos;
		backUpVector.Normalize();
                
		if (agent.enabled && !debugNoWalk)
		{
			agent.SetDestination(playerPos + 1.5f * retreatRadius * backUpVector);
		}

		enemyProps.TurnToPlayer();
                
		// Pass reverse move speed to animator
		animator.SetFloat("WalkSpeed", -agent.velocity.magnitude); 
	}

	// Called immediately after Action. Returns an IState if it can transition to that state, and null if no transition
	// is possible
	public IState Transition()
	{
		// Transition to ragdoll state if ragdolling
		if (ragdollController.IsRagdolling())
		{
			animator.SetTrigger("Ragdoll");
			return ragdollState;
		}
		
		// Store positions
		playerPos = player.transform.position;
		gameObjPos = gameObj.transform.position;
		
		// Calculate enemy distance
		float sqrDist = (float)(Math.Pow(playerPos.x - gameObjPos.x, 2) +
		                        Math.Pow(playerPos.z - gameObjPos.z, 2));
		
		if (sqrDist - sqrRetreatRadius > 0f)
		{
			// Done retreating, attack
			animator.SetTrigger("Melee");
			return meleeState;
		}
		
		// Continue retreating
		return null;
	}

	public override string ToString()
	{
		return "Retreat";
	}
}
