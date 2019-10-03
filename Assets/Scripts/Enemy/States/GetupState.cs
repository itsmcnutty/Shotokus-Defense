using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GetUpState : IState
{
	
	// The enemy's animator component
	private Animator animator;
	// The NavMeshObstacle used to block enemies pathfinding when not moving
	private NavMeshObstacle obstacle;
	
	// This enemy's GameObject
	private GameObject gameObj;
	// The enemy's ragdoll controller
	private RagdollController ragdollController;


	// Timer variables to wait for getup animation
	private float waitTimeOut = 0.5f;
	private float waitTimer = 0;
	
	// States to transition to
	private IState resetState;
	private RagdollState ragdollState;

	public GetUpState(EnemyProperties enemyProps)
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
		ragdollState = enemyProps.ragdollState;
	}
	
	// Initializes the IState instance fields. This occurs after the enemy properties class has constructed all of the
	// necessary states for the machine
	public void InitializeStates(EnemyMediumProperties enemyProps)
	{
		resetState = enemyProps.runState;
		ragdollState = enemyProps.ragdollState;
	}
	
	// Initializes the IState instance fields. This occurs after the enemy properties class has constructed all of the
	// necessary states for the machine
	public void InitializeStates(EnemyLightProperties enemyProps)
	{
		resetState = enemyProps.runState;
		ragdollState = enemyProps.ragdollState;
	}
	
	// Called upon entering this state from anywhere
	public void Enter()
	{
		// Not an obstacle
		obstacle.enabled = false;
		
		// get up animation
		waitTimer = 0;
	}

	// Called upon exiting this state
	public void Exit()
	{
	}

	// Called during Update while currently in this state
	public void Action()
	{
		waitTimer += Time.deltaTime;
		
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

		// If the enemy can recover from ragdolling, transition to resetState
		if (waitTimer > waitTimeOut)
		{
			waitTimer = 0;
			animator.SetTrigger("Ragdoll");
			return resetState;
		}
		
		// Continue get up animation
		return null;
	}
	

	public override string ToString()
	{
		return "GetUp";
	}
}
