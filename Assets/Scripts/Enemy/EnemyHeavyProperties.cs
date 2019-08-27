using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyHeavyProperties : MonoBehaviour
{
	// Time between attacks (seconds)
	public float ATTACK_DELAY = 2f;
	// Radius for attacking
	public float ATTACK_RADIUS;
    
	// This is the agent to move around by NavMesh
	public NavMeshAgent agent;
	// The NavMeshObstacle used to block enemies pathfinding when not moving
	public NavMeshObstacle obstacle;
	// The enemy's RagdollController component
	public RagdollController ragdollController;
	// The enemy's Animator component
	public Animator animator;
	// Doesn't walk if true (for debugging)
	public bool debugNoWalk = false;

	// Allowed space around attack radius that enemy's can attack from
	[NonSerialized] public float ATTACK_MARGIN = 1f;
	// How fast enemy turns to face player
	private float TURN_SPEED = 0.03f;
    
	// Squared attack radius (for optimized calculations)
	[NonSerialized] public float sqrAttackRadius;
	// Timer for attack delay
	private float attackTimer = 0f;
    
	// Player GameObject
	[NonSerialized] public GameObject player;
	// Player's head's world position
	[NonSerialized] public Vector3 playerPos;
    
	// This enemy's finite state machine
	private AIStateMachine stateMachine;
	
	// All states
	[NonSerialized] public AdvanceState advanceState;
	[NonSerialized] public MeleeState meleeState;
	[NonSerialized] public RetreatState retreatState;
	[NonSerialized] public SwingState swingState;
	[NonSerialized] public RagdollState ragdollState;

	// Start is called before the first frame update
	void Start()
	{
		// Find player for states to use in calculations
		player = GameObject.FindGameObjectWithTag("MainCamera");
		playerPos = player.transform.position;
        
		agent.stoppingDistance = ATTACK_RADIUS;
		sqrAttackRadius = ATTACK_RADIUS * ATTACK_RADIUS;
        
		// Instantiate states with the properties above
		advanceState = new AdvanceState(this);
		meleeState = new MeleeState(this);
		advanceState = new AdvanceState(this);
		advanceState = new AdvanceState(this);
		advanceState = new AdvanceState(this);
		
		// Create finite state machine
		stateMachine = gameObject.AddComponent<AIStateMachine>();
		stateMachine.ChangeState(advanceState);
	}

	// Smoothly turn enemy to face player
	public void TurnToPlayer()
	{
		// Get vector to look straight at player
		Vector3 vectorToPlayer = playerPos - transform.position;
		Quaternion lookAtPlayer = Quaternion.Euler(
			0f,
			(float) (180.0 / Math.PI * Math.Atan2(vectorToPlayer.x, vectorToPlayer.z)),
			0f);
        
		// Lerp Y-rotation until the same as the Y-rotation of the vector to the player
		transform.rotation = Quaternion.Lerp(transform.rotation, lookAtPlayer, TURN_SPEED);
	}

	// Calls a coroutine to enable the navmesh agent two frames later. Used to offset the re-enabling of the agent when
	// Disabling the enemy's obstacle
	public void EnablePathfind()
	{
		StartCoroutine(EnablePathfindAfterFrame());
	}
	
	// Enables the navmesh agent two frames after being called
	private IEnumerator EnablePathfindAfterFrame()
	{
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();

		agent.enabled = true;
	}

}