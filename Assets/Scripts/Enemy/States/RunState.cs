using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RunState : IState
{
	// Radius for ranged attacks
	private float rangedRadius;
    
	// This is the agent to move around by NavMesh
	private NavMeshAgent agent;
	// The enemy's Animator component
	private Animator animator;
	// The enemy's ragdoll controller
	private RagdollController ragdollController;
	// The NavMeshObstacle used to block enemies pathfinding when not moving
	private NavMeshObstacle obstacle;
	// Speed of navmesh agent in this state
	private float maxRunSpeed;
	// Doesn't walk if true (for debugging)
	private bool debugNoWalk;
    
	// Squared ranged radius (for optimized calculations)
	private float sqrRangedRadius;
    
	// Player GameObject
	private GameObject player;
	// Player's head's world position
	private Vector3 playerPos;
	// This enemy GameObject
	private GameObject gameObj;
	// The enemy properties component
	private EnemyProperties enemyProps;
	
	// States to transition to
	private StrafeState strafeState;
	private RagdollState ragdollState;
	
	public RunState(EnemyMediumProperties enemyProps)
	{
		rangedRadius = enemyProps.RANGED_RADIUS;
		agent = enemyProps.agent;
		animator = enemyProps.animator;
		ragdollController = enemyProps.ragdollController;
		obstacle = enemyProps.obstacle;
		maxRunSpeed = enemyProps.MAX_RUN_SPEED;
		debugNoWalk = enemyProps.debugNoWalk;
		sqrRangedRadius = enemyProps.sqrRangedRadius;
		player = enemyProps.player;
		playerPos = enemyProps.playerPos;
		gameObj = enemyProps.gameObject;
		this.enemyProps = enemyProps;
	}
	
	// Initializes the IState instance fields. This occurs after the enemy properties class has constructed all of the
	// necessary states for the machine
	public void InitializeStates(EnemyMediumProperties enemyProps)
	{
		strafeState = enemyProps.strafeState;
		ragdollState = enemyProps.ragdollState;
	}

	// Called upon entering this state from anywhere
	public void Enter()
	{
		// No longer obstacle
		obstacle.enabled = false;
		enemyProps.EnablePathfind();
		
		// Settings for agent
		agent.stoppingDistance = rangedRadius;
		agent.speed = maxRunSpeed;
		agent.angularSpeed = 8000f;
	}

	// Called upon exiting this state
	public void Exit() {}

	// Called during Update while currently in this state
	public void Action()
	{
		// Store transform variables for player
		playerPos = player.transform.position;
        
		// Pass speed to animation controller
		float moveSpeed = agent.velocity.magnitude;
		animator.SetFloat("RunSpeed", moveSpeed);
		
		// Move to player if outside attack range, otherwise transition
		if (agent.enabled && !debugNoWalk)
		{
			// Too far, walk closer
			agent.SetDestination(playerPos);

			// Stopping distance will cause enemy to decelerate into attack radius
			agent.stoppingDistance = rangedRadius + moveSpeed * moveSpeed / (2 * agent.acceleration);
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
		
		// Get enemy position
		Vector3 gameObjPos = gameObj.transform.position;
		
		// Calculate enemy distance
		float sqrDist = (float)(Math.Pow(playerPos.x - gameObjPos.x, 2) +
		                        Math.Pow(playerPos.z - gameObjPos.z, 2));

		// If within ranged attack range, transition to strafe state
		if (sqrDist - sqrRangedRadius < 0)
		{
			animator.SetTrigger("Strafe");
			return strafeState;
		}
		
		// Otherwise, don't transition
		return null;
	}

	public override string ToString()
	{
		return "Run";
	}
}
