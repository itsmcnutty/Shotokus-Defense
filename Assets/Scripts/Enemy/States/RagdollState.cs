using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RagdollState : IState
{
	// Minimum time to stay in ragdoll (seconds)
	private float MINIMUM_DURATION = 1.5f;
	
	// The enemy's animator component
	private Animator animator;
	// The enemy's ragdoll controller
	private RagdollController ragdollController;
	// The NavMeshObstacle used to block enemies pathfinding when not moving
	private NavMeshObstacle obstacle;
	
	// This enemy's GameObject
	private GameObject gameObj;
	
	// Total time spent in animation (seconds)
	private float timeRagdolling = 0f;
	
	// States to transition to
	private IState resetState;

	public RagdollState(EnemyProperties enemyProps)
	{
		animator = enemyProps.animator;
		ragdollController = enemyProps.ragdollController;
		obstacle = enemyProps.obstacle;
		gameObj = enemyProps.gameObject;
	}
	
	// Initializes the IState instance fields. This occurs after the enemy properties class has constructed all of the
	// necessary states for the machine
	public void InitializeStates(EnemyHeavyProperties enemyProps)
	{
		resetState = enemyProps.advanceState;
	}
	
	// Initializes the IState instance fields. This occurs after the enemy properties class has constructed all of the
	// necessary states for the machine
	public void InitializeStates(EnemyMediumProperties enemyProps)
	{
		resetState = enemyProps.runState;
	}
	
	// Called upon entering this state from anywhere
	public void Enter()
	{
		// Not an obstacle
		obstacle.enabled = false;
		
		// Begin ragdolling
		timeRagdolling = 0;
	}

	// Called upon exiting this state
	public void Exit()
	{
		// Stop ragdolling
		ragdollController.StopRagdoll();
        
		// Restart animation in Walking state
		animator.SetTrigger("Ragdoll");
	}

	// Called during Update while currently in this state
	public void Action()
	{
		timeRagdolling += Time.deltaTime;
	}

	// Called immediately after Action. Returns an IState if it can transition to that state, and null if no transition
	// is possible
	public IState Transition()
	{
		// If the enemy can recover from ragdolling, transition to resetState
		if (CanRecover() && timeRagdolling > MINIMUM_DURATION)
		{
			return resetState;
		}
		
		// Continue ragdolling
		return null;
	}
	
	// Returns true if the enemy is in a suitable situation to recover from ragdolling and re-attach to the navmesh
	private bool CanRecover()
	{
		Rigidbody spine = gameObj.GetComponentInChildren<Rigidbody>();

		// If spine rigidbody is moving very slowly, enemy can recover
		return spine.velocity.magnitude < 0.13f;
	}

	public override string ToString()
	{
		return "Ragdoll";
	}
}
