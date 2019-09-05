using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// Struct to keep track of information for pointsAround() function
struct CircularCoord
{
	public Vector3 coord; // point calculated around center for pointsAround() function
	public int index; // keeping track of last position. Using the index replaces the need for keeping track of angles 
	// todo maybe keep track of angle? although index is sufficient for now
	public bool isReachable; // keeps track if the coordinate is reachable, false if path is invalid or partial
	// todo maybe store the NavMesh path variable??
}

public class StrafeState : IState
{
	// Radius for melee attacks
	private float meleeRadius;
	// Radius for strafe state
	private float rangedRadius;
    
	// This is the agent to move around by NavMesh
	private NavMeshAgent agent;
	// The enemy's Animator component
	private Animator animator;
	// The enemy's ragdoll controller
	private RagdollController ragdollController;
	// The NavMeshObstacle used to block enemies pathfinding when not moving
	private NavMeshObstacle obstacle;
	// Speed of navmesh agent in this state
	private float maxStrafeSpeed;
	// Doesn't walk if true (for debugging)
	private bool debugNoWalk;

	// Player GameObject
	private GameObject player;
	// Player's head's world position
	private Vector3 playerPos;
	// This enemy GameObject
	private GameObject gameObj;
	// The enemy properties component
	private EnemyProperties enemyProps;
	
	// States to transition to
	private RunState runState;
	private MeleeState meleeState;
	private RagdollState ragdollState;
	private ClimbingState climbingState;
	
	// shooting variables
	private Vector3 agentHead; // this is where the ray cast originates, determines if enemy can see player
	private float fireRate; // how many second to wait between shots
	private float initialVelocityX; // Initial velocity in X-axis for projectile
	
	// strafing variables
	private float strafeDistance; // distance that the enemy will start strafing around player
	private bool isStrafing; // bool indicating if agent is in strafing state
	private CircularCoord[] pointsAroundTarget; // points around target(player) with radius, and every 45 degrees
	private Vector3 circularPointDest; // point where the agent will move towards when strafying in circular motion
	private int lastPointIndex; // last point index value in the pointsAroundTarget array
	private bool isClockwise; // walk in a clockwise direction when strafying
	private float radiusReduction; // float that will reduce the radius of points around center every time, the agent reaches a point
	private float totalCurrentReduction; // float that will keep track of the increase in radiausReduction
	
	// get instance of right hand for shooting
	private ShootingAbility shootingAbility;

	public StrafeState(EnemyMediumProperties enemyProps)
	{
		meleeRadius = enemyProps.MELEE_RADIUS;
		rangedRadius = enemyProps.RANGED_RADIUS;
		agent = enemyProps.agent;
		animator = enemyProps.animator;
		ragdollController = enemyProps.ragdollController;
		obstacle = enemyProps.obstacle;
		maxStrafeSpeed = enemyProps.MAX_STRAFE_SPEED; // todo add this functionality
		debugNoWalk = enemyProps.debugNoWalk;
		player = enemyProps.player;
		playerPos = enemyProps.playerPos;
		gameObj = enemyProps.gameObject;
		this.enemyProps = enemyProps;
		
		// shooting variables
		agentHead = enemyProps.agentHead;
		fireRate = enemyProps.RANGED_DELAY; 
		initialVelocityX = enemyProps.PROJECTILE_VEL_X;

		// strafing variables
		strafeDistance = enemyProps.STRAFE_DIST; 
		isStrafing = enemyProps.isStrafing; 
//		pointsAroundTarget = enemyProps.pointsAroundTarget;
//		circularPointDest = enemyProps.circularPointDest; 
		lastPointIndex = enemyProps.lastPointIndex; 
		isClockwise = enemyProps.isClockwise;
		radiusReduction = enemyProps.RADIUS_REDUCTION;
		totalCurrentReduction = 0;
		
		// shooting ability
		shootingAbility = gameObj.GetComponentInChildren<ShootingAbility>();
	}
	
	// Initializes the IState instance fields. This occurs after the enemy properties class has constructed all of the
	// necessary states for the machine
	public void InitializeStates(EnemyMediumProperties enemyProps)
	{
		runState = enemyProps.runState;
		meleeState = enemyProps.meleeState;
		ragdollState = enemyProps.ragdollState;
	}

	// Called upon entering this state from anywhere
	public void Enter()
	{
		// No longer obstacle
		obstacle.enabled = false;
		enemyProps.EnablePathfind();
		
		// Settings for agent
		agent.stoppingDistance = 0;
		agent.speed = maxStrafeSpeed;
		agent.angularSpeed = 0;
		
		// Restart radius reduction, to prevent enemy approaching you right away after being launched from ragdoll
		totalCurrentReduction = 0;
	}

	// Called upon exiting this state
	public void Exit()
	{
		isStrafing = false;
		animator.SetTrigger("ResetShoot");
		shootingAbility.DropArrow();
	}

	// Called during Update while currently in this state
	public void Action()
	{
		if (agent.isOnOffMeshLink) 
			Debug.Log("im on nav mesh climbing");
		
		// Store transform variables for player and this enemy
		playerPos = player.transform.position;
		Vector3 enemyVelocity = agent.velocity;
		
		// Store position for agent
		Vector3 gameObjPos = gameObj.transform.position;
		
		// Store position for agent's head, where the raycast for shooting visibility will come from
		agentHead = gameObj.transform.position;
		// todo change this, this is the head height value
		agentHead.y = 2.5f;
		
		// Dot product of world velocity and transform's forward/right vector gives local forward/right velocity
		float strafeSpeedForward = Vector3.Dot(enemyVelocity, gameObj.transform.forward);
		float strafeSpeedRight = Vector3.Dot(enemyVelocity, gameObj.transform.right);
		
		// Pass to animator
		animator.SetFloat("StrafeSpeedForward", strafeSpeedForward);
		animator.SetFloat("StrafeSpeedRight", strafeSpeedRight);
		
		// Turn to player
		enemyProps.TurnToPlayer();
		
		// Move to player if outside attack range, otherwise transition
//		if (agent.enabled && !debugNoWalk)
//		{
//			// Too far, walk closer
//			agent.SetDestination(playerPos);
//
//			// Stopping distance will cause enemy to decelerate into attack radius
//			agent.stoppingDistance = meleeRadius + enemyVelocity.magnitude * enemyVelocity.magnitude / (2 * agent.acceleration);
//		}

		// remaining distance to target
		float distanceToPlayer = enemyProps.calculateDist(playerPos, gameObjPos);
//        Debug.Log("vector3 distance is " + distanceToPlayer);

		// only enters here, first time it enters te strafing state
		// if agent is close enough and not strafing yet, enter strafe/shooting state
		// calculate points and set new destination
		if (!isStrafing)
		{
			Debug.Log("Strafing mode / calculations");
			// do not enter here if already strafing
			isStrafing = true;
            
			// Calculate points around the target (player) given a set radius, and every 45 degrees (pi/4 radians)
			pointsAroundTarget = pointsAround(playerPos, strafeDistance);
            
			// pick the closest of these points that has a complete path to the enemy
			circularPointDest = closestPoint(gameObjPos, pointsAroundTarget);
            
			// change enemy agent target to the new point
			agent.SetDestination(circularPointDest);
			Debug.Log("my destination is " + circularPointDest);
//			Debug.DrawRay(circularPointDest, Vector3.up, Color.blue);
		}
		
		
		// if moving towards strafing point, check if it has being destination has been reached
		// if reached, calculate points around circle again on a reduced radius
		// and start moving to the next point (medium enemy)
		if (isStrafing)
		{
			Debug.Log("Strafing mode / moving");
			// do not change destination until current one is reached
			// when destination is reached, move to next point 
			float strafeRemainingDist = enemyProps.calculateDist(circularPointDest, gameObjPos);
            Debug.Log("remaning distance from strafe waypoint "+ strafeRemainingDist);
				
			// if point reached, recalculate points around center and move to the next one
			if (strafeRemainingDist < 1f)
			{
				// recalculate points around circle with smaller radius
				totalCurrentReduction += radiusReduction; // reduce by 2f for next point
				// this prevents over shooting from the agent
				if (totalCurrentReduction > strafeDistance)
				{
					totalCurrentReduction = strafeDistance;
				}
				pointsAroundTarget = pointsAround(playerPos, strafeDistance - totalCurrentReduction);
				
				// update lastPointIndex to next circular point coordinate
				lastPointIndex = GetNextCircularPointIndex(lastPointIndex);
				
				Debug.Log("last point index: " + lastPointIndex);
				circularPointDest = pointsAroundTarget[lastPointIndex].coord; // todo check where to change lastpointindex
//                Debug.Log("Changing target to index " + lastPointIndex);
                Debug.Log("moving towards " +circularPointDest);
				agent.SetDestination(circularPointDest);
			}
		}
		
		// todo SHOOTING STATE
//		Debug.Log("Shooting mode");
		// check for visibility to target through ray cast
		RaycastHit hit;

		Vector3 rayDirection = playerPos - agentHead; // todo this might shoot from the feet
            
		// set where the ray is coming from and its direction
		Ray visionRay = new Ray(agentHead, rayDirection);
            
		Debug.DrawRay(agentHead, rayDirection, Color.red);

		// if player is visible and fire Rate is good, shoot!
		if (Physics.Raycast(visionRay, out hit)) 
		{
			if (hit.collider.CompareTag("PlayerCollider"))
			{
				// we can hit the player, so shoot
//				shoot(); // todo uncoomment
				// todo look at player when shooting
				shootingAbility.Shoot(initialVelocityX, fireRate, animator);
			}
		}
	}
	

	// Called immediately after Action. Returns an IState if it can transition to that state, and null if no transition
	// is possible
	public IState Transition()
	{
		// Transition to ragdoll state if ragdolling
		if (ragdollController.IsRagdolling())
		{
			animator.SetTrigger("Ragdoll");
			return ragdollState;
		}
		
		// todo CONTINUE TESTING THIS
		// Transition to climbing state if climbing
		if (agent.isOnOffMeshLink)
		{
			// todo do something with animator
			return climbingState;
		}
		
		// Get enemy position
		Vector3 gameObjPos = gameObj.transform.position;
		
		// Calculate enemy distance
		float distanceToPlayer = enemyProps.calculateDist(playerPos, gameObjPos);
//		Debug.Log(distanceToPlayer);

		// If outside ranged radius, transition to run state
		if (distanceToPlayer >  rangedRadius)
		{
			animator.SetTrigger("Run");
			return runState;
		}
		
		// If within melee range, transition to melee state
		if (distanceToPlayer < meleeRadius)
		{
			animator.SetTrigger("Melee");
			return meleeState;
		}
		
		// Otherwise, don't transition
		return null;
	}

	public override string ToString()
	{
		return "Strafe";
	}
	
	// given a point and a radius, return points around the center with the input radius and every 45 degrees (pi/4 radians)
	private CircularCoord[] pointsAround(Vector3 center, float radius)
	{
		float angle = 0;
		CircularCoord[] points = new CircularCoord[8];
		Vector3 offset = new Vector3(center.x,0,center.z);
        
		// todo keep track of every angle, might not be necessary anymore, index might be enough
        
		// checking for navmesh status on points (if available or not)
		NavMeshPath path = new NavMeshPath();
		
		for (int i = 0; i < 8; i++)
		{
			// x is x and y is z, in 3D coordinates unity. For example, the 3D vector is represented as (x, 0, y).
			float x = Mathf.Cos(angle) * radius;
			float y = Mathf.Sin(angle) * radius;
			Vector3 coord = new Vector3(x, 0, y) + offset;
            Debug.Log("Iteration: " + i);
            Debug.Log("Angle: " + angle);
            Debug.Log(coord);
			// check that path to circular coordinate can be completed. Invalid or partial points are not accepted.
			agent.CalculatePath(coord, path);
			if (path.status != NavMeshPathStatus.PathComplete)
			{
				points[i].isReachable = false;
			}
			else
			{
				points[i].isReachable = true;
			}
			Debug.Log("My path status is: " + path.status);
			angle += Mathf.PI / 4;
			points[i].coord = coord;
			points[i].index = i;
		}
		return points;
	}
    
	// TODO WHAT IF NONE OF THEM ARE REACHABLE?
	// ADDED: THIS FUNCTION DOESNT TAKE ON ACCOUNT ANY POINTS THAT ARE NOT VALID OR PARTIAL
	// given a enemy position and an array of possible future positions, return the next closest point
	private Vector3 closestPoint(Vector3 enemyPos, CircularCoord[] points)
	{
		// initialize temp variables to first value in array of points
		float closestDist =  enemyProps.calculateDist(enemyPos, points[0].coord);	
		Vector3 closestPoint = points[0].coord;
        
		for(int i = 0; i < points.Length; i++)
		{
			// skip point that is not reachable
			if (!points[i].isReachable)
			{
				continue;
			}
			Vector3 point = points[i].coord;
			float tempDist = enemyProps.calculateDist(enemyPos, point);
			if (tempDist < closestDist)
			{
				closestPoint = point;
				closestDist = tempDist;
				lastPointIndex = i; // keeps track of at which point in the rotation of points we were left at, this helps with moving to the next closest point
			}
		}
		return closestPoint;
	}

	// this function updates the lastPointIndex to indicate the agent which circular point to move towards 
	// this function skips over circular points that are not valid or partial
	// todo if no points found, then recalculate more points with smaller radius!!
	private int GetNextCircularPointIndex(int lastPointIndex)
	{
		// todo remove this DEBUGGING ONLY
		// clockwise and RD = 2 is broken??
		isClockwise = true;
		int newIndex = lastPointIndex;
		
		if (isClockwise)
		{
			for (int i = newIndex; i > -1; i--)
			{
				newIndex--;
				if (newIndex < 0) 
					newIndex = newIndex + pointsAroundTarget.Length;
				newIndex %= 8;
				if (pointsAroundTarget[newIndex].isReachable)
				{
					break;
				}
			}
		}
		else
		{
			for (int i = newIndex; i < pointsAroundTarget.Length; i++)
			{
				newIndex++;
				newIndex %= 8;
				if (pointsAroundTarget[newIndex].isReachable)
				{
					break;
				}
			}
		}
		// todo this will return the last number in the loop if none of the points are available probably causing the agent to stay still
		return newIndex;
	}
	
}
