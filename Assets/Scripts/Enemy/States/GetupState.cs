//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.AI;
//
//public class GetupState : IState
//{
//    // The enemy's animator component
//    private Animator animator;
//    
//    // This is the agent to move around by NavMesh
//    private NavMeshAgent agent;
//    // The NavMeshObstacle used to block enemies pathfinding when not moving
//    private NavMeshObstacle obstacle;
//    // The enemy's ragdoll controller
//    private RagdollController ragdollController;
//
//    // reference to player gameobject
//    private GameObject player;
//    // Player's head's world position
//    private Vector3 playerPos;
//    // This enemy's GameObject
//    private GameObject gameObj;
//    // The enemy properties component
//    private EnemyProperties enemyProps;
//
//    private EnemyMediumProperties medEnemyProps;
//    
//    // States to transition to
//    private ClimbingState climbingState;
//    private RagdollState ragdollState;
//    private StrafeState strafeState; 
//    
//    // climbing variables
//    private float waitTimer = 0; // keeps track of how much time has occured after climbing
//    private float waitTimeout = 1f; // once climbingTimer reaches this counter, agent can climb again
////    private float canClimb = 0; // counter that keeps track of amount of times the agent has climbed
//
//
//    public AboveWallState(EnemyMediumProperties enemyProps)
//    {
//        animator = enemyProps.animator;
//        agent = enemyProps.agent;
//        obstacle = enemyProps.obstacle;
//        animator = enemyProps.animator;
//        gameObj = enemyProps.gameObject;
//        player = enemyProps.player;
//        playerPos = enemyProps.playerPos;
//        ragdollController = enemyProps.ragdollController;
//        this.enemyProps = enemyProps;
//    }
//    
//    // Initializes the IState instance fields. This occurs after the enemy properties class has constructed all of the
//    // necessary states for the machine
//    public void InitializeStates(EnemyMediumProperties enemyProps)
//    {
//        strafeState = enemyProps.strafeState;
//        ragdollState = enemyProps.ragdollState;
//    }
//    
//    // Called upon entering this state from anywhere
//    public void Enter()
//    {
//        // Not an obstacle
//        obstacle.enabled = false;
//        waitTimer = 0;
//        agent.isStopped = true;
//    }
//    
//    // Called upon exiting this state
//    public void Exit()
//    { }
//
//    // Called during Update while currently in this state
//    public void Action()
//    {
//        waitTimer += Time.deltaTime;
////        Debug.Log("above wall : " + waitTimer);
//        // if enough time has passed, allow to climb again
//        if (waitTimer > waitTimeout)
//        {
//            waitTimer -= waitTimeout;
//            agent.isStopped = false;
//        }
//    }
//    
//    // Called immediately after Action. Returns an IState if it can transition to that state, and null if no transition
//    // is possible
//    public IState Transition()
//    {
//        // Transition to reset state if agent timer has stopped
//        if (ragdollController.IsRagdolling())
//        {
//            animator.SetTrigger("Ragdoll");
//            return ragdollState;
//        }
//		
//        // Continue not moving
//        return null;
//    }
//    
//    public override string ToString()
//    {
//        // todo ooooooooooooooooooooooo check this with will
//        return "GetUp";
//    }
//    
//}
