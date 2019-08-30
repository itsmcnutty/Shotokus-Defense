using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMediumProperties : EnemyProperties
{
	// Time between ranged attacks (seconds)
	public float RANGED_DELAY;
	// Minimum distance before using ranged attacks on player
	public float RANGED_RADIUS;
    // Time between melee attacks (seconds)
    public float MELEE_DELAY = 2f;
    // Radius for melee attacking
    public float MELEE_RADIUS;

    // Allowed space around attack radius that enemies can attack from
    [NonSerialized] public float ATTACK_MARGIN = 1f;
    
    // Squared attack radii (for optimized calculations)
    [NonSerialized] public float sqrRangedRadius;
    [NonSerialized] public float sqrMeleeRadius;
	
    // All states
    [NonSerialized] public RunState runState;
    [NonSerialized] public StrafeState strafeState;
    [NonSerialized] public AdvanceState advanceState;
    [NonSerialized] public MeleeState meleeState;
    [NonSerialized] public RetreatState retreatState;
    [NonSerialized] public SwingState swingState;
    [NonSerialized] public RagdollState ragdollState;

    // Start is called before the first frame update
    void Start()
    {
        base.Start();
		
        sqrMeleeRadius = MELEE_RADIUS * MELEE_RADIUS;
        sqrRangedRadius = RANGED_RADIUS * RANGED_RADIUS;
        
        // Instantiate states with the properties above
        runState = new RunState(this);
        strafeState = new StrafeState(this);
        advanceState = new AdvanceState(this);
        meleeState = new MeleeState(this);
        retreatState = new RetreatState(this);
        swingState = new SwingState(this);
        ragdollState = new RagdollState(this);
		
        // Initialize states within these state objects
        runState.InitializeStates(this);
        strafeState.InitializeStates(this);
        advanceState.InitializeStates(this);
        meleeState.InitializeStates(this);
        retreatState.InitializeStates(this);
        swingState.InitializeStates(this);
        ragdollState.InitializeStates(this);
		
        // Give FSM an initial state
        stateMachine.ChangeState(advanceState);
    }
}
