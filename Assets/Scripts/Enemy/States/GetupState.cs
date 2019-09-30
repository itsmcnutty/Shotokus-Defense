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

	// Timer variables to wait for getup animation
	private float waitTimeOut = 2f;
	private float waitTimer = 0;
	
	// States to transition to
	private IState resetState;

	public GetUpState(EnemyProperties enemyProps)
	{
		animator = enemyProps.animator;
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
	
	// Initializes the IState instance fields. This occurs after the enemy properties class has constructed all of the
	// necessary states for the machine
	public void InitializeStates(EnemyLightProperties enemyProps)
	{
		resetState = enemyProps.runState;
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
