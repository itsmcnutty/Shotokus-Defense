using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RagdollState : IState
{

	// The enemy's animator component
	private Animator animator;
	// The enemy's ragdoll controller
	private RagdollController ragdollController;
	// The NavMeshObstacle used to block enemies pathfinding when not moving
	private NavMeshObstacle obstacle;
	
	// This enemy's GameObject
	private GameObject gameObj;
	
	// States to transition to
	private AdvanceState advanceState;

	public RagdollState(EnemyHeavyProperties enemyProps)
	{
		animator = enemyProps.animator;
		ragdollController = enemyProps.ragdollController;
		obstacle = enemyProps.obstacle;
		gameObj = enemyProps.gameObject;
		advanceState = enemyProps.advanceState;
	}
	
	// Initializes the IState instance fields. This occurs after the enemy properties class has constructed all of the
	// necessary states for the machine
	public void InitializeStates(EnemyHeavyProperties enemyProps)
	{
		advanceState = enemyProps.advanceState;
	}
	
	// Called upon entering this state from anywhere
	public void Enter()
	{
		// Not an obstacle
		obstacle.enabled = false;
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
	public void Action() {}

	// Called immediately after Action. Returns an IState if it can transition to that state, and null if no transition
	// is possible
	public IState Transition()
	{
		// TODO: determine if enemy should recover from ragdoll
		
		// If the enemy can recover from ragdolling, transition to advanceState
		if (CanRecover())
		{
			return advanceState;
		}
		
		// Continue ragdolling
		return null;
	}
	
	// Returns true if the enemy is in a suitable situation to recover from ragdolling and re-attach to the navmesh
	private bool CanRecover()
	{
		Rigidbody spine = gameObj.GetComponentInChildren<Rigidbody>();

		// If spine rigidbody is moving very slowly, enemy can recover
		return spine.velocity.magnitude < 0.015f;
	}

	public override string ToString()
	{
		return "Ragdoll";
	}
}
