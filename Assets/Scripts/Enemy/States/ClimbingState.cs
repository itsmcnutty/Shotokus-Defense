using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ClimbingState : IState
{
    // The enemy's animator component
    private Animator animator;
    
    // This is the agent to move around by NavMesh
    private NavMeshAgent agent;
    // The NavMeshObstacle used to block enemies pathfinding when not moving
    private NavMeshObstacle obstacle;
    
    // This enemy's GameObject
    private GameObject gameObj;
    // The enemy properties component
    private EnemyProperties enemyProps;

    
    // States to transition to
    private IState resetState;
    
    public ClimbingState(EnemyProperties enemyProps)
    {
        animator = enemyProps.animator;
        agent = enemyProps.agent;
        obstacle = enemyProps.obstacle;
        animator = enemyProps.animator;
        gameObj = enemyProps.gameObject;
        this.enemyProps = enemyProps;
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
    }
    
    // Called upon exiting this state
    public void Exit()
    {
        // todo fill out
        // Restart animation in Walking state
//        animator.SetTrigger("Ragdoll"); // todo change to climbing animation
    }

    // Called during Update while currently in this state
    public void Action()
    {
        // todo fill out
        // wait for climbing animation to be done??
        // set time for how long should it take??
        Debug.Log("climbing");
    }
    
    // Called immediately after Action. Returns an IState if it can transition to that state, and null if no transition
    // is possible
    public IState Transition()
    {
        // If the enemy can recover from ragdolling, transition to resetState
        // todo fill out
        if (!agent.isOnOffMeshLink)
        {
            Debug.Log("leaving climb state");
            return resetState;
        }
		
        // Continue climbing
        return null;
    }
    
    public override string ToString()
    {
        return "Climbing"; // todo is this correct??
    }
    
    
}
