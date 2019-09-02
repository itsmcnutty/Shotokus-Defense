using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class StrafeState : IState
{
	// Radius for melee attacks
	private float meleeRadius;
    
	// This is the agent to move around by NavMesh
	private NavMeshAgent agent;
	// The enemy's Animator component
	private Animator animator;
	// The enemy's ragdoll controller
	private RagdollController ragdollController;
	// The NavMeshObstacle used to block enemies pathfinding when not moving
	private NavMeshObstacle obstacle;
	// Speed of navmesh agent in this state
	private float maxStrafeSpeed;
	// Doesn't walk if true (for debugging)
	private bool debugNoWalk;
    
	// Squared melee radius (for optimized calculations)
	private float sqrMeleeRadius;
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
	private RunState runState;
	private MeleeState meleeState;
	private RagdollState ragdollState;
	
	public StrafeState(EnemyMediumProperties enemyProps)
	{
		meleeRadius = enemyProps.MELEE_RADIUS;
		agent = enemyProps.agent;
		animator = enemyProps.animator;
		ragdollController = enemyProps.ragdollController;
		obstacle = enemyProps.obstacle;
		maxStrafeSpeed = enemyProps.MAX_STRAFE_SPEED;
		debugNoWalk = enemyProps.debugNoWalk;
		sqrMeleeRadius = enemyProps.sqrMeleeRadius;
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
		runState = enemyProps.runState;
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
		agent.stoppingDistance = meleeRadius;
		agent.speed = maxStrafeSpeed;
		agent.angularSpeed = 0;
	}

	// Called upon exiting this state
	public void Exit() {}

	// Called during Update while currently in this state
	public void Action()
	{
		// Store transform variables for player and this enemy
		playerPos = player.transform.position;
		Vector3 enemyVelocity = agent.velocity;
		
		// Dot product of world velocity and transform's forward/right vector gives local forward/right velocity
		float strafeSpeedForward = Vector3.Dot(enemyVelocity, gameObj.transform.forward);
		float strafeSpeedRight = Vector3.Dot(enemyVelocity, gameObj.transform.right);
		
		// Pass to animator
		animator.SetFloat("StrafeSpeedForward", strafeSpeedForward);
		animator.SetFloat("StrafeSpeedRight", strafeSpeedRight);
		
		// Move to player if outside attack range, otherwise transition
		if (agent.enabled && !debugNoWalk)
		{
			// Too far, walk closer
			agent.SetDestination(playerPos);

			// Stopping distance will cause enemy to decelerate into attack radius
			agent.stoppingDistance = meleeRadius + enemyVelocity.magnitude * enemyVelocity.magnitude / (2 * agent.acceleration);
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

		// If outside ranged radius, transition to run state
		if (sqrDist - sqrRangedRadius > 0)
		{
			animator.SetTrigger("Run");
			return runState;
		}
		
		// If within melee range, transition to melee state
		if (sqrDist - sqrMeleeRadius < 0)
		{
			animator.SetTrigger("Melee");
			return meleeState;
		}
		
		// Otherwise, don't transition
		return null;
	}

	public override string ToString()
	{
		return "Strafe";
	}
}
