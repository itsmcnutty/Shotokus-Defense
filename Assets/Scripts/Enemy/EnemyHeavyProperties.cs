using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyHeavyProperties : EnemyProperties
{
	// Time between attacks (seconds)
	public float ATTACK_DELAY = 2f;
	// Radius for attacking
	public float ATTACK_RADIUS;

	// Allowed space around attack radius that enemies can attack from
	[NonSerialized] public float ATTACK_MARGIN = 1f;
	
	// All states
	[NonSerialized] public AdvanceState advanceState;
	[NonSerialized] public MeleeState meleeState;
	[NonSerialized] public RetreatState retreatState;
	[NonSerialized] public SwingState swingState;
	[NonSerialized] public RagdollState ragdollState;

	// Start is called before the first frame update
	new void Start()
	{
		base.Start();
		
		agent.stoppingDistance = ATTACK_RADIUS;
        
		// Instantiate states with the properties above
		advanceState = new AdvanceState(this);
		meleeState = new MeleeState(this);
		retreatState = new RetreatState(this);
		swingState = new SwingState(this);
		ragdollState = new RagdollState(this);
		
		// Initialize states within these state objects
		advanceState.InitializeStates(this);
		meleeState.InitializeStates(this);
		retreatState.InitializeStates(this);
		swingState.InitializeStates(this);
		ragdollState.InitializeStates(this);
		
		// Give FSM an initial state
		stateMachine.ChangeState(advanceState);
	}

	public override float GetCurrentMaxSpeed()
	{
		switch (stateMachine.GetCurrentState())
		{
			case "Advance":
				return MAX_RUN_SPEED;
			case "Retreat":
				return MAX_RUN_SPEED;
			default:
				return 0;
		}
	}
}