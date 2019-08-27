using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.AI;

public class EnemyHeavyProperties : MonoBehaviour
{
    
    // Time between attacks (seconds)
    public float ATTACK_DELAY = 2f;
    // Radius for attacking
    public float ATTACK_RADIUS;
    // Doesn't walk if true (for debugging)
    public bool debugNoWalk = false;
    
    // This is the agent to move around by NavMesh
    public NavMeshAgent agent;
    // The NavMeshObstacle used to block enemies pathfinding when not moving
    public NavMeshObstacle obstacle;
    // The RagdollConotroller on the enemy that operates all ragdoll functionality
    public RagdollController ragdollController;
    // The enemy's Animator for animations and finite state machine AI logic
    public Animator animator;
    
    // Start is called before the first frame update
    void Start()
    {
        // Find player to pass to other states
        GameObject player = GameObject.FindGameObjectWithTag("MainCamera");

        // When re-enabling after a ragdoll, animator will still be in ragdoll state
        animator.keepAnimatorControllerStateOnDisable = true;
        
        // Get list of states
        ChildAnimatorState[] states = ((AnimatorController) animator.runtimeAnimatorController).layers[0].stateMachine.states;
        
        // Pass necessary values from this component to each of the state behaviors in the machine
        foreach (var state in states)
        {
            foreach (var behavior in state.state.behaviours)
            {
                switch (behavior)
                {
                    case AdvanceState s1:
                        AdvanceState advanceState = (AdvanceState)behavior;
                        advanceState.attackRadius = ATTACK_RADIUS;
                        advanceState.agent = agent;
                        advanceState.player = player;
                        advanceState.debugNoWalk = debugNoWalk;
                        break;
                    
                    case MeleeState s2:
                        MeleeState meleeState = (MeleeState)behavior;
                        meleeState.attackRadius = ATTACK_RADIUS;
                        meleeState.attackDelay = ATTACK_DELAY;
                        meleeState.agent = agent;
                        meleeState.obstacle = obstacle;
                        meleeState.player = player;
                        break;
                    
                    case RetreatState s3:
                        RetreatState retreatState = (RetreatState) behavior;
                        retreatState.retreatRadius = ATTACK_RADIUS;
                        retreatState.agent = agent;
                        retreatState.player = player;
                        retreatState.debugNoWalk = debugNoWalk;
                        break;
                    
                    case SwingState s4:
                        break;
                    
                    case RagdollState s5:
                        RagdollState ragdollState = (RagdollState) behavior;
                        ragdollState.agent = agent;
                        ragdollState.obstacle = obstacle;
                        break;
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
