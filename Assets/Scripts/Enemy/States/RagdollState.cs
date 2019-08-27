using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RagdollState : StateMachineBehaviour
{
	// This is the agent to move around by NavMesh
	public NavMeshAgent agent;
	// The NavMeshObstacle used to block enemies pathfinding when not moving
	public NavMeshObstacle obstacle;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		
		// Can't walk, not an obstacle
		agent.enabled = false;
		obstacle.enabled = false;
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateUpdate(animator, stateInfo, layerIndex);
		
		///// Transition /////
		
		// Leave state as soon as animator is re-enabled
		animator.SetTrigger("Continue");
	}
}
