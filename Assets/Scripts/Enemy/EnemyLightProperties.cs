using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyLightProperties : EnemyProperties
{
    [Header("Shooting Variables")]
    // Time between ranged attacks (seconds)
    public float RANGED_DELAY = 3f;
    public float PROJECTILE_VEL_X = 15f; // the projectile Initial velocity in X-axis for projectile
    // todo get the agentHead
    [NonSerialized] public Vector3 agentHead; // this is where the ray cast originates, determines if enemy can see player

    [Header("Strafing Variables")]
    // Speed of navmesh agent when strafing 
    public float MAX_STRAFE_SPEED;
    // Minimum distance before using ranged attacks on player
    public float RANGED_RADIUS = 23f;
    // distance that the enemy will start strafing around player
    public float STRAFE_DIST = 15f;
    [NonSerialized] public bool isStrafing = false; // bool indicating if agent is in strafing state
    [NonSerialized] public int lastPointIndex; // last point index value in the pointsAroundTarget array
    [NonSerialized] public bool isClockwise = false; // walk in a clockwise direction when strafying

    // All states
    [NonSerialized] public RunState runState;
    [NonSerialized] public StrafeState strafeState;
    [NonSerialized] public RagdollState ragdollState;
    
    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        
        isStrafing = false;
        lastPointIndex = 0; // just initialization
        // assign randomly if enemy will strafe clockwise or counter clockwise
        if (UnityEngine.Random.Range(0, 2) == 0)
        {
            isClockwise = true;
        }

        // todo delete - just debugging
        isClockwise = false;
        
        // Instantiate states with the properties above
        runState = new RunState(this);
        strafeState = new StrafeState(this);
        ragdollState = new RagdollState(this);
		
        // Initialize states within these state objects
        runState.InitializeStates(this);
        strafeState.InitializeStates(this);
        ragdollState.InitializeStates(this);
		
        // Give FSM an initial state
        stateMachine.ChangeState(runState);
    }
    
    public override float GetCurrentMaxSpeed()
    {
        switch (stateMachine.GetCurrentState())
        {
            case "Run":
                return MAX_RUN_SPEED;
            case "Strafe":
                return MAX_STRAFE_SPEED;
            default:
                return 0;
        }
    }
    
}
