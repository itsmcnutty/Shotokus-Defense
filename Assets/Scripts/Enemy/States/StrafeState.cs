using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using UnityEngine;
using UnityEngine.AI;

// Struct to keep track of information for pointsAround() function
struct CircularCoord
{
	public Vector3 coord; // point calculated around center for pointsAround() function
	public bool isReachable; // keeps track if the coordinate is reachable, false if path is invalid or partial
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
	private EnemyProperties props;

	private EnemyMediumProperties medEnemyProps;
	
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
	private int AMOUNT_OF_CIRCLE_POINTS = 8; // this is the amount of points that will be calculated around a center
	
	// climbing variables
	private float climbingTimer = 0; // keeps track of how much time has occured after climbing
	private float climbingTimeout = 3; // once climbingTimer reaches this counter, agent can climb again
	private float canClimb = 0; // counter that keeps track of amount of times the agent has climbed

	// get instance of right hand for shooting
	private ShootingAbility shootingAbility;

	public StrafeState(EnemyMediumProperties props)
	{
		meleeRadius = props.MELEE_RADIUS;
		rangedRadius = props.RANGED_RADIUS;
		agent = props.agent;
		animator = props.animator;
		ragdollController = props.ragdollController;
		obstacle = props.obstacle;
		maxStrafeSpeed = props.MAX_STRAFE_SPEED;
		debugNoWalk = props.debugNoWalk;
		player = props.player;
		playerPos = props.playerPos;
		gameObj = props.gameObject;
		this.props = props;
		
		// shooting variables
		agentHead = props.agentHead;
		fireRate = props.RANGED_DELAY; 
		initialVelocityX = props.PROJECTILE_VEL_X;

		// strafing variables
		strafeDistance = props.STRAFE_DIST; 
		isStrafing = props.isStrafing;
		lastPointIndex = props.lastPointIndex; 
		isClockwise = props.isClockwise;
		radiusReduction = props.RADIUS_REDUCTION;
		totalCurrentReduction = 0;
		
		// climbing variables
		canClimb = props.climbCounter;
		medEnemyProps = props;
		
		// shooting ability
		shootingAbility = gameObj.GetComponentInChildren<ShootingAbility>();
	}

	public StrafeState(EnemyLightProperties props)
	{
		meleeRadius = -1; // The light enemy will never enter the melee state
		rangedRadius = props.RANGED_RADIUS;
		agent = props.agent;
		animator = props.animator;
		ragdollController = props.ragdollController;
		obstacle = props.obstacle;
		maxStrafeSpeed = props.MAX_STRAFE_SPEED;
		debugNoWalk = props.debugNoWalk;
		player = props.player;
		playerPos = props.playerPos;
		gameObj = props.gameObject;
		this.props = props;
		
		// shooting variables
		agentHead = props.agentHead;
		fireRate = props.RANGED_DELAY; 
		initialVelocityX = props.PROJECTILE_VEL_X;

		// strafing variables
		strafeDistance = props.STRAFE_DIST; 
		isStrafing = props.isStrafing;
		lastPointIndex = props.lastPointIndex; 
		isClockwise = props.isClockwise;
		radiusReduction = 0;
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
		climbingState = enemyProps.climbingState;
	}
	
	// Initializes the IState instance fields. This occurs after the enemy properties class has constructed all of the
	// necessary states for the machine
	public void InitializeStates(EnemyLightProperties enemyProps)
	{
		runState = enemyProps.runState;
		ragdollState = enemyProps.ragdollState;
	}

	// Called upon entering this state from anywhere
	public void Enter()
	{
		// activate agent?
		agent.enabled = true; //todo i added this not sure

		// No longer obstacle
		obstacle.enabled = false;
		props.EnablePathfind();
		
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
		// Store transform variables for player and this enemy
		playerPos = player.transform.position;
		Vector3 enemyVelocity = agent.velocity;
		
		// Store position for agent
		Vector3 gameObjPos = gameObj.transform.position;
		
		// Store position for agent's head, where the raycast for shooting visibility will come from
		agentHead = gameObj.transform.position;
		// todo change this, this is the head height value
		agentHead.y = 2f;

		// Dot product of world velocity and transform's forward/right vector gives local forward/right velocity
		float strafeSpeedForward = Vector3.Dot(enemyVelocity, gameObj.transform.forward);
		float strafeSpeedRight = Vector3.Dot(enemyVelocity, gameObj.transform.right);
		
		// Pass to animator
		animator.SetFloat("StrafeSpeedForward", strafeSpeedForward);
		animator.SetFloat("StrafeSpeedRight", strafeSpeedRight);
		
		// Turn to player
		props.TurnToPlayer();

		// ignore light enemy
		// todo come back to this
		if (medEnemyProps != null)
		{
			if (medEnemyProps.climbCounter >= 2)
			{
				// Disallow climbing
				agent.autoTraverseOffMeshLink = false;
				// Update climbing counter
				climbingTimer += Time.deltaTime;
			}
		
			// if enough time has passed, allow to climb again
			if (climbingTimer > climbingTimeout) {
				climbingTimer -= climbingTimeout;
				agent.autoTraverseOffMeshLink = true;

				medEnemyProps.climbCounter = 0;
			}
		}
		
		// Move to player if outside attack range, otherwise transition
//		if (agent.enabled && !debugNoWalk)
//		{
//			// Too far, walk closer
//			agent.SetDestination(playerPos);
//
//			// Stopping distance will cause enemy to decelerate into attack radius
//			agent.stoppingDistance = meleeRadius + enemyVelocity.magnitude * enemyVelocity.magnitude / (2 * agent.acceleration);
//		}

		// Squared variables
		float sqrStrafeDistance = strafeDistance * strafeDistance;
		
		// calculate points around center and set new destination to closest point to agent, only enters here first time it enters the strafing state
		if (!isStrafing && agent.enabled)
		{
//			Debug.Log("first time in strafe state");
			float distanceToPlayer = props.calculateSqrDist(playerPos, gameObjPos);

			// do not enter here if already strafing
			isStrafing = true;
			
			// recalculate the totalCurrentradiusReduction if the enemy is already inside the strafe Distance radius when entering strafe state
			if (radiusReduction == 0)
			{
				// ignore light enemy
				totalCurrentReduction = 0;
			}
			else if (distanceToPlayer < sqrStrafeDistance)
			{
				float sqrTotalCurrentReduction = sqrStrafeDistance - distanceToPlayer;
				totalCurrentReduction = (float) Math.Sqrt(sqrTotalCurrentReduction);
			}
            
			// Calculate points around the target (player) given a set radius, and every 45 degrees (pi/4 radians)
			pointsAroundTarget = pointsAround(playerPos, strafeDistance - totalCurrentReduction);
            
			// pick the closest of these points that has a complete path to the enemy
			circularPointDest = closestPoint(gameObjPos, pointsAroundTarget);
            
			// change enemy agent target to the new point
			agent.SetDestination(circularPointDest);
//			Debug.Log("my destination is " + circularPointDest);
		}
		
		// if moving towards strafing point, check if destination has been reached
		// if reached, calculate points around circle again with a reduced radius and start moving to the next point (medium enemy)
		if (isStrafing && agent.enabled)
		{
//			Debug.Log("strafe state: moving in circles");
			// do not change destination until current one is reached
			// when destination is reached, move to next point 
			
			float strafeRemainingDist = props.calculateSqrDist(circularPointDest, gameObjPos);
//            Debug.Log("remaning distance from strafe waypoint "+ strafeRemainingDist);
            
            
			// if point reached, recalculate points around center and move to the next one
			if (strafeRemainingDist < 1.5f)
			{
				// only recalculate points if you are medium enemy for smaller radius points
				if (radiusReduction != 0)
				{
					// recalculate points around circle with smaller radius
					totalCurrentReduction += radiusReduction;
					// this prevents the agent's strafing over shooting the player's melee range
					if (totalCurrentReduction > strafeDistance)
					{
						totalCurrentReduction = strafeDistance;
					}
					pointsAroundTarget = pointsAround(playerPos, strafeDistance - totalCurrentReduction);
				}
				lastPointIndex = GetNextCircularPointIndex(lastPointIndex);
				circularPointDest = pointsAroundTarget[lastPointIndex].coord;
//				Debug.Log("last point index: " + lastPointIndex);
//                Debug.Log("moving towards " +circularPointDest);
				agent.SetDestination(circularPointDest);
			}
		}
		
		// SHOOTING STATE
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
				shootingAbility.Shoot(initialVelocityX, fireRate, animator);
			}
		}
		
	}
	
	//		// todo debug getup animation only - delete later 
//	private float timer = 0;
//	private float timeout = 2;
	
	// Called immediately after Action. Returns an IState if it can transition to that state, and null if no transition
	// is possible
	public IState Transition()
	{
//////		// todo debug getup animation only - delete later 
//		timer += Time.deltaTime;
//		if (timer > timeout)
//		{
//			timer = 0;
//			ragdollController.StartRagdoll();
//		}

		// Transition to ragdoll state if ragdolling
		if (ragdollController.IsRagdolling())
		{
			animator.SetTrigger("Ragdoll");
			return ragdollState;
		}
		
		// Transition into climbing up state
		if (agent.isOnOffMeshLink && agent.autoTraverseOffMeshLink && medEnemyProps.climbCounter == 0)
		{
			animator.SetTrigger("Climb");
			return climbingState;
		}
		
		// Transition to jumping down state
		if (agent.isOnOffMeshLink && agent.autoTraverseOffMeshLink && medEnemyProps.climbCounter == 1)
		{
			animator.SetTrigger("Jump");
			return climbingState;
		}
		
		// Get enemy position
		Vector3 gameObjPos = gameObj.transform.position;
		
		// Calculate enemy distance
		float distanceToPlayer = props.calculateSqrDist(playerPos, gameObjPos);

		// If outside ranged radius, transition to run state
		if (distanceToPlayer >  rangedRadius * rangedRadius)
		{
			animator.SetTrigger("Run");
			return runState;
		}
		
		// If within melee range, transition to melee state
		if (distanceToPlayer < meleeRadius * meleeRadius && meleeState != null)
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
		int amountOfPoints = AMOUNT_OF_CIRCLE_POINTS; // amount of points around center (more points, more accurate)
		float angle = 0;
		CircularCoord[] points = new CircularCoord[amountOfPoints];
		Vector3 offset = new Vector3(center.x,0,center.z);
		
		// checking for navmesh status on points (if available or not)
		NavMeshPath path = new NavMeshPath();
		
		for (int i = 0; i < amountOfPoints; i++)
		{
			// x is x and y is z, in 3D coordinates unity. For example, the 3D vector is represented as (x, 0, y).
			float x = Mathf.Cos(angle) * radius;
			float y = Mathf.Sin(angle) * radius;
			Vector3 coord = new Vector3(x, 0, y) + offset;
			// check that path to circular coordinate can be completed. Invalid or partial points are not accepted.
			agent.CalculatePath(coord, path);
			if (path.status != NavMeshPathStatus.PathComplete)
			{
				points[i].isReachable = false;
				// if point is not valid, attemp to find a random point nearby in the navmesh
//				Vector3 temp;
//				if (RandomPoint(coord, 1f, out temp))
//				{
//					points[i].isReachable = true;
//					coord = temp;
//				}
			}
			else
			{
				points[i].isReachable = true;
			}
			angle += (2* Mathf.PI) / AMOUNT_OF_CIRCLE_POINTS;
			points[i].coord = coord;
		}
		return points;
	}
    
	// TODO WHAT IF NONE OF THEM ARE REACHABLE?
	// ADDED: THIS FUNCTION DOESNT TAKE ON ACCOUNT ANY POINTS THAT ARE NOT VALID OR PARTIAL
	// given a enemy position and an array of possible future positions, return the next closest point
	private Vector3 closestPoint(Vector3 enemyPos, CircularCoord[] points)
	{
		// initialize temp variables to first value in array of points
		float closestDist =  props.calculateSqrDist(enemyPos, points[0].coord);	
		Vector3 closestPoint = points[0].coord;
        
		for(int i = 0; i < points.Length; i++)
		{
			// skip point that is not reachable
			if (!points[i].isReachable)
			{
				continue;
			}
			Vector3 point = points[i].coord;
			float tempDist = props.calculateSqrDist(enemyPos, point);
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
	// todo this will return the last number in the loop if none of the points are available probably causing the agent to stay still
	private int GetNextCircularPointIndex(int lastPointIndex)
	{
		int newIndex = lastPointIndex;

		for (int i = 0; i < pointsAroundTarget.Length; i++)
		{
			// todo debug delete later
//			isClockwise = false;
			if (isClockwise)
			{
				newIndex--;
				if (newIndex < 0) 
					newIndex = newIndex + pointsAroundTarget.Length;
			}
			else
			{
				newIndex++;
				newIndex %= pointsAroundTarget.Length;
			}
			if (pointsAroundTarget[newIndex].isReachable)
			{
				break;
			}
		}
		return newIndex;
	}
	
	
	// input: Vector3 coordinate that is not valid on navmesh
	// this function evaluates a invalid coordinate and tries to find a point around it that is active on the navmesh
	// returns: true if point found. Input Vector3 coordinate is updated if point found
	bool RandomPoint(Vector3 center, float range, out Vector3 result)
	{ 
		// checking for navmesh status on points (if available or not)
		NavMeshPath path = new NavMeshPath();
		
		for (int i = 0; i < 30; i++)
		{
			Vector3 randomPoint = center + UnityEngine.Random.insideUnitSphere * range;
			randomPoint.y = center.y;
			
			agent.CalculatePath(randomPoint, path);
			if (path.status == NavMeshPathStatus.PathComplete)
			{
				result = randomPoint;
				return true;
			}
		}
		result = Vector3.zero;
		return false;
	}


}
