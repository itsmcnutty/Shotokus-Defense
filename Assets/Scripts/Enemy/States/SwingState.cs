using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingState : IState
{
	// The enemy's Animator component
	private Animator animator;
	// The enemy's ragdoll controller
	private RagdollController ragdollController;

	// States to transition to
	private MeleeState meleeState;
	private RagdollState ragdollState;

	public SwingState(EnemyHeavyProperties enemyProps)
	{
		animator = enemyProps.animator;
		ragdollController = enemyProps.ragdollController;
		meleeState = enemyProps.meleeState;
		ragdollState = enemyProps.ragdollState;
	}
	
	// Called upon entering this state from anywhere
	public void Enter()
	{
		throw new System.NotImplementedException();
	}

	// Called upon exiting this state
	public void Exit()
	{
		throw new System.NotImplementedException();
	}

	// Called during Update while currently in this state
	public void Action()
	{
		throw new System.NotImplementedException();
	}

	// Called immediately after Action. Returns an IState if it can transition to that state, and null if no transition
	// is possible
	public IState Transition()
	{
		// Transition to ragdoll state if ragdolling
		if (ragdollController.IsRagdolling())
		{
			return ragdollState;
		}
		
		// Transition back to melee state if done animating swing
		if (!animator.GetCurrentAnimatorStateInfo(0).IsTag("Swinging"))
		{
			return meleeState;
		}
		
		// Continue swinging
		return null;
	}
}
