using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingState : IState
{
	// The enemy's Animator component
	private Animator animator;
	// The enemy's ragdoll controller
	private RagdollController ragdollController;
	
	// The enemy properties component
	private EnemyHeavyProperties enemyProps;
	
	// Flags for tracking progression through swinging animations
	private bool startedSwinging;
	private bool finishedSwinging;

	// States to transition to
	private MeleeState meleeState;
	private RagdollState ragdollState;

	public SwingState(EnemyHeavyProperties enemyProps)
	{
		animator = enemyProps.animator;
		ragdollController = enemyProps.ragdollController;
		meleeState = enemyProps.meleeState;
		ragdollState = enemyProps.ragdollState;
		this.enemyProps = enemyProps;
	}
	
	// Initializes the IState instance fields. This occurs after the enemy properties class has constructed all of the
	// necessary states for the machine
	public void InitializeStates(EnemyHeavyProperties enemyProps)
	{
		meleeState = enemyProps.meleeState;
		ragdollState = enemyProps.ragdollState;
	}
	
	// Called upon entering this state from anywhere
	public void Enter()
	{
		startedSwinging = false;
		finishedSwinging = false;
	}

	// Called upon exiting this state
	public void Exit() {}

	// Called during Update while currently in this state
	public void Action()
	{
		enemyProps.TurnToPlayer();
		
		// When reaching the swing animation
		if (animator.GetCurrentAnimatorStateInfo(0).IsTag("Swinging"))
		{
			startedSwinging = true;
		}

		// When reaching the melee animation after already going through the swing animation
		if (animator.GetCurrentAnimatorStateInfo(0).IsName("Meleeing") && startedSwinging)
		{
			finishedSwinging = true;
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

		// Transition back to melee state if done animating swing
		if (finishedSwinging)
		{
			return meleeState;
		}
		
		// Continue swinging
		return null;
	}

	public override string ToString()
	{
		return "Swing";
	}
}
