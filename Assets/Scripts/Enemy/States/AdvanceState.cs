using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AdvanceState : IState
{
	// Radius for attacking
	private float attackRadius;
    
	// This is the agent to move around by NavMesh
	private NavMeshAgent agent;
	// The enemy's Animator component
	private Animator animator;
	// The enemy's ragdoll controller
	private RagdollController ragdollController;
	// Speed of navmesh agent in this state
	private float maxWalkSpeed;
	// The NavMeshObstacle used to block enemies pathfinding when not moving
	private NavMeshObstacle obstacle;
	// Doesn't walk if true (for debugging)
	private bool debugNoWalk;

	// Allowed space around attack radius that enemy's can attack from
	private float attackMargin;

	// Player GameObject
	private GameObject player;
	// Player's head's world position
	private Vector3 playerPos;
	// This enemy GameObject
	private GameObject gameObj;
	// The enemy properties component
	private EnemyProperties enemyProps;
	
	// States to transition to
	private MeleeState meleeState;
	private RagdollState ragdollState;

	public AdvanceState(EnemyHeavyProperties enemyProps)
	{
		attackRadius = enemyProps.ATTACK_RADIUS;
		agent = enemyProps.agent;
		animator = enemyProps.animator;
		ragdollController = enemyProps.ragdollController;
		obstacle = enemyProps.obstacle;
		maxWalkSpeed = enemyProps.MAX_RUN_SPEED;
		debugNoWalk = enemyProps.debugNoWalk;
		attackMargin = enemyProps.ATTACK_MARGIN;
		player = enemyProps.player;
		playerPos = enemyProps.playerPos;
		gameObj = enemyProps.gameObject;
		this.enemyProps = enemyProps;
	}
	
	public AdvanceState(EnemyMediumProperties enemyProps)
	{
		attackRadius = enemyProps.MELEE_RADIUS;
		agent = enemyProps.agent;
		animator = enemyProps.animator;
		ragdollController = enemyProps.ragdollController;
		maxWalkSpeed = enemyProps.MAX_STRAFE_SPEED;
		obstacle = enemyProps.obstacle;
		debugNoWalk = enemyProps.debugNoWalk;
		attackMargin = enemyProps.ATTACK_MARGIN;
		player = enemyProps.player;
		playerPos = enemyProps.playerPos;
		gameObj = enemyProps.gameObject;
		this.enemyProps = enemyProps;
	}
	
	// Initializes the IState instance fields. This occurs after the enemy properties class has constructed all of the
	// necessary states for the machine
	public void InitializeStates(EnemyHeavyProperties enemyProps)
	{
		meleeState = enemyProps.meleeState;
		ragdollState = enemyProps.ragdollState;
	}
	
	// Initializes the IState instance fields. This occurs after the enemy properties class has constructed all of the
	// necessary states for the machine
	public void InitializeStates(EnemyMediumProperties enemyProps)
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
		
		// Settings for agent
		agent.stoppingDistance = attackRadius;
		agent.speed = maxWalkSpeed;
		agent.angularSpeed = 8000f;
		
	}

	// Called upon exiting this state
	public void Exit() {}

	// Called during Update while currently in this state
	public void Action()
	{
		// Store transform variables for player and this enemy
		playerPos = player.transform.position;
        
		// Pass speed to animation controller
		float moveSpeed = agent.velocity.magnitude;
		animator.SetFloat("WalkSpeed", moveSpeed);
		
		// Move to player if outside attack range, otherwise transition
		if (agent.enabled && !debugNoWalk)
		{
			// Too far, walk closer
			agent.SetDestination(playerPos);

			// Stopping distance will cause enemy to decelerate into attack radius
			agent.stoppingDistance = attackRadius + moveSpeed * moveSpeed / (2 * agent.acceleration) - attackMargin;
		}
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
		
		// Get enemy position with y = 0 for distance calculations
		Vector3 gameObjPos = gameObj.transform.position;

		float distanceToPlayer = enemyProps.calculateSqrDist(playerPos, gameObjPos);
		
		// If within melee range, transition to melee state
		if (distanceToPlayer <= attackRadius * attackRadius)
		{
			animator.SetTrigger("Melee");
			return meleeState;
		}
		
		// Otherwise, don't transition
		return null;
	}

	public override string ToString()
	{
		return "Advance";
	}
}
