using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyHeavyStateMachine : AIStateMachine
{
    // Time between attacks (seconds)
    [SerializeField] private float ATTACK_DELAY = 2f;
    // Radius for attacking
    [SerializeField] private float ATTACK_RADIUS;
    
    // This is the agent to move around by NavMesh
    [SerializeField] private NavMeshAgent agent;
    // The NavMeshObstacle used to block enemies pathfinding when not moving
    [SerializeField] public NavMeshObstacle obstacle;
    // The enemy's RagdollController component
    [SerializeField] private RagdollController ragdollController;
    // The enemy's Animator component
    [SerializeField] private Animator animator;
    // Doesn't walk if true (for debugging)
    [SerializeField] private bool debugNoWalk = false;

    // Allowed space around attack radius that enemy's can attack from
    private float ATTACK_MARGIN = 1f;
    // How fast enemy turns to face player
    private float TURN_SPEED = 0.03f;
    
    // Squared attack radius (for optimized calculations)
    private float sqrAttackRadius;
    // Timer for attack delay
    private float attackTimer = 0f;
    
    // Player GameObject
    private GameObject player;
    // Player's head position
    private Vector3 playerPos;
    
    //

    // Start is called before the first frame update
    void Start()
    {
        // Find player for states to use in calculations
        player = GameObject.FindGameObjectWithTag("MainCamera");
        playerPos = player.transform.position;
        
        agent.stoppingDistance = ATTACK_RADIUS;
        sqrAttackRadius = ATTACK_RADIUS * ATTACK_RADIUS;
        
        ChangeState();
    }

}
