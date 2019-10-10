using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(NavMeshObstacle))]
[RequireComponent(typeof(RagdollController))]
[RequireComponent(typeof(Animator))]

public abstract  class EnemyProperties : MonoBehaviour
{
	// This is the agent to move around by NavMesh
	public NavMeshAgent agent;
	// The NavMeshObstacle used to block enemies pathfinding when not moving
	public NavMeshObstacle obstacle;
	// The enemy's RagdollController component
	public RagdollController ragdollController;
	// The enemy's Animator component
	public Animator animator;
	// Speed of navmesh agent when running
	public float MAX_RUN_SPEED;
	// Doesn't walk if true (for debugging)
	public bool debugNoWalk = false;
	// Rotation offset when turning to player
	public float ROTATION_OFFSET;

	[Header("Audio")]
	public AudioMultiClipSource groundFootstep;
	public AudioMultiClipSource quicksandFootstep;

	// How fast enemy turns to face player
	private float TURN_SPEED = 0.05f;
    
	// Player's head's world location
	[NonSerialized] public Vector3 playerPos;
	// Player GameObject
	[NonSerialized] public GameObject player;

	// This enemy's finite state machine
	protected AIStateMachine stateMachine;

	// Start is called before the first frame update
	protected void Start()
	{
		// Find player for states to use in calculations
		player = GameObject.FindGameObjectWithTag("MainCamera");

		animator.keepAnimatorControllerStateOnDisable = false;
		
		// Create finite state machine
		stateMachine = gameObject.AddComponent<AIStateMachine>();
	}

	// Smoothly turn enemy to face player
	public void TurnToPlayer()
	{
		playerPos = player.transform.position;
		
		// Get vector to look straight at player
		Vector3 vectorToPlayer = playerPos - transform.position;
		Quaternion lookAtPlayer = Quaternion.Euler(
			0f,
			(float) (180.0 / Math.PI * Math.Atan2(vectorToPlayer.x, vectorToPlayer.z)),
			0f);
        
		// Lerp Y-rotation until the same as the Y-rotation of the vector to the player
		transform.rotation = Quaternion.Lerp(transform.rotation, lookAtPlayer * Quaternion.Euler(0, ROTATION_OFFSET, 0), TURN_SPEED);
	}

	// Calls a coroutine to enable the navmesh agent two frames later. Used to offset the re-enabling of the agent when
	// Disabling the enemy's obstacle
	public void EnablePathfind()
	{
		StartCoroutine(EnablePathfindAfterFrame());
	}
	
	// Enables the navmesh agent two frames after being called
	private IEnumerator EnablePathfindAfterFrame()
	{
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();

		if (!obstacle.enabled)
		{
			agent.enabled = true;
		}
		else
		{
			EnablePathfind();
		}
	}

	// Returns the potential maxspeed of this enemy in their current state, unhindered by slowing effects like quicksand
	public abstract float GetCurrentMaxSpeed();

	// Plays a footstep sound effect
	public abstract void PlayFootstepSound();
	
	// Given two vectors, it sets the y axis to 0, and returns distance
	// Distance just in the x,z axis. Y = 0.
	public float calculateDist(Vector3 vector1, Vector3 vector2)
	{
		Vector3 v1 = new Vector3(vector1.x, 0,vector1.z);
		Vector3 v2 = new Vector3(vector2.x, 0,vector2.z);

		float remainingDist = Vector3.Distance(v1, v2);
		return remainingDist;
	}
	
	public float calculateSqrDist(Vector3 vector1, Vector3 vector2)
	{
		Vector3 v1 = new Vector3(vector1.x, 0,vector1.z);
		Vector3 v2 = new Vector3(vector2.x, 0,vector2.z);

		float remainingDist = Vector3.SqrMagnitude(v1 - v2);
		return remainingDist;
	}
	
}
