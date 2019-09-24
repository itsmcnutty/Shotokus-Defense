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
    // The enemy's ragdoll controller
    private RagdollController ragdollController;

    // This enemy's GameObject
    private GameObject gameObj;
    // The enemy properties component
    private EnemyProperties enemyProps;

    private EnemyMediumProperties medEnemyProps;
    
    // States to transition to
    private IState aboveWallState;
    private RagdollState ragdollState;

    public ClimbingState(EnemyMediumProperties enemyProps)
    {
        animator = enemyProps.animator;
        agent = enemyProps.agent;
        obstacle = enemyProps.obstacle;
        animator = enemyProps.animator;
        gameObj = enemyProps.gameObject;
        ragdollController = enemyProps.ragdollController;
        this.enemyProps = enemyProps;
    }
    
    // Initializes the IState instance fields. This occurs after the enemy properties class has constructed all of the
    // necessary states for the machine
    public void InitializeStates(EnemyMediumProperties enemyProps)
    {
        aboveWallState = enemyProps.aboveWallState;
        ragdollState = enemyProps.ragdollState;
//        medEnemyProps = enemyProps;
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
//        medEnemyProps.climbCounter++;
    }

    // Called during Update while currently in this state
    public void Action()
    {
        Debug.Log("climbing");
    }
    
    // Called immediately after Action. Returns an IState if it can transition to that state, and null if no transition
    // is possible
    public IState Transition()
    {
        // when finish climbing transition to abovewall state
        if (!agent.isOnOffMeshLink) // todo this if statement should ask if canClimb = 1? and a differente for canClimb = 2?
        {
            animator.SetTrigger("ClimbEnd");
            return aboveWallState;
        }
        
        // Transition to ragdoll state if ragdolling
        if (ragdollController.IsRagdolling())
        {
            animator.SetTrigger("Ragdoll");
            return ragdollState;
        }
		
        // Continue climbing
        return null;
    }
    
    public override string ToString()
    {
        return "Climbing";
    }
    
    
}
