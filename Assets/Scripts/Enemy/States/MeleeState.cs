using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class MeleeState : IState
{
	// Radius for attacking
	private float attackRadius;
	// Time between attacks (seconds)
	private float attackDelay;

	// This is the agent to move around by NavMesh
	private NavMeshAgent agent;
	// The enemy's Animator component
	private Animator animator;
	// The enemy's ragdoll controller
	private RagdollController ragdollController;
	// The NavMeshObstacle used to block enemies pathfinding when not moving
	private NavMeshObstacle obstacle;
	
	// Allowed space around attack radius that enemy's can attack from
	private float attackMargin = 1f;
	// Number of potential attacks in animator controller
	private int numAttacks;
	
	// Timer for attack delay
	private float attackTimer = 0f;
	// Player GameObject
	private GameObject player;
	// The player's head's world location
	private Vector3 playerPos;
	// This enemy's GameObject
	private GameObject gameObj;
	// The enemy properties component
	private EnemyProperties enemyProps;

	// True when enemy has begun swinging and should transition to swing state
	private bool swinging;

	// States to transition to
	private AdvanceState advanceState;
	private RetreatState retreatState;
	private SwingState swingState;
	private RagdollState ragdollState;

	public MeleeState(EnemyHeavyProperties enemyProps)
	{
		attackMargin = enemyProps.ATTACK_MARGIN;
		attackRadius = enemyProps.ATTACK_RADIUS;
		attackDelay = enemyProps.ATTACK_DELAY;
		agent = enemyProps.agent;
		animator = enemyProps.animator;
		ragdollController = enemyProps.ragdollController;
		obstacle = enemyProps.obstacle;
		numAttacks = animator.GetInteger("NumAttacks");
		player = enemyProps.player;
		gameObj = enemyProps.gameObject;
		this.enemyProps = enemyProps;
	}

	public MeleeState(EnemyMediumProperties enemyProps)
	{
		attackMargin = enemyProps.ATTACK_MARGIN;
		attackRadius = enemyProps.MELEE_RADIUS;
		attackDelay = enemyProps.MELEE_DELAY;
		agent = enemyProps.agent;
		animator = enemyProps.animator;
		ragdollController = enemyProps.ragdollController;
		obstacle = enemyProps.obstacle;
		numAttacks = animator.GetInteger("NumAttacks");
		player = enemyProps.player;
		gameObj = enemyProps.gameObject;
		this.enemyProps = enemyProps;
	}
	
	// Initializes the IState instance fields. This occurs after the enemy properties class has constructed all of the
	// necessary states for the machine
	public void InitializeStates(EnemyHeavyProperties enemyProps)
	{
		advanceState = enemyProps.advanceState;
		retreatState = enemyProps.retreatState;
		swingState = enemyProps.swingState;
		ragdollState = enemyProps.ragdollState;
	}
	
	// Initializes the IState instance fields. This occurs after the enemy properties class has constructed all of the
	// necessary states for the machine
	public void InitializeStates(EnemyMediumProperties enemyProps)
	{
		advanceState = enemyProps.advanceState;
		retreatState = enemyProps.retreatState;
		swingState = enemyProps.swingState;
		ragdollState = enemyProps.ragdollState;
	}

	// Called upon entering this state from anywhere
	public void Enter()
	{
		// Not swinging yet
		swinging = false;
		
		// Can't walk, acts as an obstacle
		agent.enabled = false;
		obstacle.enabled = true;
	}

	// Called upon exiting this state
	public void Exit() {}

	// Called during Update while currently in this state
	public void Action()
	{
		// Store transform variables for player and this enemy
		playerPos = player.transform.position;

		// Decrement attack timer
		attackTimer -= Time.deltaTime;

		enemyProps.TurnToPlayer();

		// When attackTimer is lower than 0, it allows the enemy to attack again
		if (attackTimer <= 0f)
		{
			animator.SetInteger("AttackNum", Random.Range(0, numAttacks));
			swinging = true;
			attackTimer = attackDelay;
		}
	}

	// Called immediately after Action. Returns an IState if it can transition to that state, and null if no transition
	// is possible
	public IState Transition()
	{
		Debug.Log("melee state");
		
		// Transition to ragdoll state if ragdolling
		if (ragdollController.IsRagdolling())
		{
			animator.SetTrigger("Ragdoll");
			return ragdollState;
		}

		// When enemy attack cooldown has ended and enemy is in Meleeing animation, enemy can attack
		if (swinging)
		{
			animator.SetTrigger("Swing");
			return swingState;
		}
		
		// Calculate enemy distance
		Vector3 gameObjPos = gameObj.transform.position;
		float distanceToPlayer = enemyProps.calculateSqrDist(playerPos, gameObjPos);
		float sqrAttackRadius = attackRadius * attackRadius;
		float sqrAttackMargin = attackMargin * attackMargin;
		

		// Continue to attack if within attack range, otherwise transition
		if (distanceToPlayer > sqrAttackRadius + sqrAttackMargin)
		{
			// Too far, advance
			animator.SetTrigger("Advance");
			return advanceState;
		}
		if (distanceToPlayer < sqrAttackRadius - sqrAttackMargin)
		{
			// Too close, retreat
			animator.SetTrigger("Retreat");
			return retreatState;
		}
		
		// Continue attacking
		return null;
	}

	public override string ToString()
	{
		return "Melee";
	}
}
