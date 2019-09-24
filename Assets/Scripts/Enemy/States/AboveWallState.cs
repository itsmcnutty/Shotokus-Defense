using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AboveWallState : IState
{
    // The enemy's animator component
    private Animator animator;
    
    // This is the agent to move around by NavMesh
    private NavMeshAgent agent;
    // The NavMeshObstacle used to block enemies pathfinding when not moving
    private NavMeshObstacle obstacle;
    // The enemy's ragdoll controller
    private RagdollController ragdollController;

    // reference to player gameobject
    private GameObject player;
    // Player's head's world position
    private Vector3 playerPos;
    // This enemy's GameObject
    private GameObject gameObj;
    // The enemy properties component
    private EnemyProperties enemyProps;

    private EnemyMediumProperties medEnemyProps;
    
    // States to transition to
    private IState climbingState;
    private RagdollState ragdollState;


    public AboveWallState(EnemyMediumProperties enemyProps)
    {
        animator = enemyProps.animator;
        agent = enemyProps.agent;
        obstacle = enemyProps.obstacle;
        animator = enemyProps.animator;
        gameObj = enemyProps.gameObject;
        player = enemyProps.player;
        playerPos = enemyProps.playerPos;
        ragdollController = enemyProps.ragdollController;
        this.enemyProps = enemyProps;
    }
    
    // Initializes the IState instance fields. This occurs after the enemy properties class has constructed all of the
    // necessary states for the machine
    public void InitializeStates(EnemyMediumProperties enemyProps)
    {
        climbingState = enemyProps.runState;
        ragdollState = enemyProps.ragdollState;
    }
    
    // Called upon entering this state from anywhere
    public void Enter()
    {
        // Not an obstacle
        obstacle.enabled = false;

        agent.enabled = false; // todo testing
        
//        animator.SetTrigger();
    }
    
    // Called upon exiting this state
    public void Exit()
    { }

    // Called during Update while currently in this state
    public void Action()
    {
        Debug.Log("above wall");
        // todo delete -  agent is probably still moving towards player, no need to agent.dest()
    }
    
    // Called immediately after Action. Returns an IState if it can transition to that state, and null if no transition
    // is possible
    public IState Transition()
    {
        // If agent gets on top off an navmesh link from this state, agent should jump down
        if (agent.isOnOffMeshLink)
        {
            animator.SetTrigger("Jump");
            return climbingState;
        }
        
        // Transition to ragdoll state if ragdolling
        if (ragdollController.IsRagdolling())
        {
            animator.SetTrigger("Ragdoll");
            return ragdollState;
        }
		
        // Continue on top of wall
        return null;
    }
    
    public override string ToString()
    {
        // todoooooooooooooooooooooooo check this with will
        return "ClimbingUp";
    }
    
    
}
