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
	// The enemy's Animator for animations and finite state machine AI logic
	public Animator animator;
    
	// Start is called before the first frame update
	void Start()
	{
		// Find player to pass to other states
		GameObject player = GameObject.FindGameObjectWithTag("MainCamera");

		// When re-enabling after a ragdoll, animator will still be in ragdoll state
		animator.keepAnimatorControllerStateOnDisable = true;
		
		Debug.Log(animator.name);

		// Add all behaviours in state machine to a list
		List<StateMachineBehaviour> behaviours = new List<StateMachineBehaviour>();
		AnimatorControllerLayer baseLayer = ((AnimatorController) animator.runtimeAnimatorController).layers[0];
		AddAllBehavioursFromMachine(behaviours, baseLayer.stateMachine);
		
		// Pass necessary values from this component to each of the state behaviors in the machine
		foreach (StateMachineBehaviour behaviour in behaviours)
		{
			switch (behaviour)
			{
				case AdvanceState s1:
					AdvanceState advanceState = (AdvanceState) behaviour;
					advanceState.attackRadius = ATTACK_RADIUS;
					advanceState.agent = agent;
					advanceState.player = player;
					advanceState.debugNoWalk = debugNoWalk;
					break;

				case MeleeState s2:
					MeleeState meleeState = (MeleeState) behaviour;
					meleeState.attackRadius = ATTACK_RADIUS;
					meleeState.attackDelay = ATTACK_DELAY;
					meleeState.agent = agent;
					meleeState.obstacle = obstacle;
					meleeState.player = player;
					break;

				case RetreatState s3:
					RetreatState retreatState = (RetreatState) behaviour;
					Debug.Log(gameObject.name);
					retreatState.retreatRadius = gameObject.GetHashCode();
					retreatState.agent = agent;
					retreatState.player = player;
					retreatState.debugNoWalk = debugNoWalk;
					break;

				case SwingState s4:
					break;

				case RagdollState s5:
					RagdollState ragdollState = (RagdollState) behaviour;
					ragdollState.agent = agent;
					ragdollState.obstacle = obstacle;
					break;
			}
		}
	}


	// Recursively add all behaviours in the given state machine and its sub-machines to list
	void AddAllBehavioursFromMachine(List<StateMachineBehaviour> list, AnimatorStateMachine stateMachine)
	{
		// Add all behaviours from the given state machine itself
		foreach (StateMachineBehaviour behaviour in stateMachine.behaviours)
		{
			list.Add(behaviour);
		}
		
		// Add all behaviours from states within the given state machine
		foreach (ChildAnimatorState state in stateMachine.states)
		{
			foreach (StateMachineBehaviour behaviour in state.state.behaviours)
			{
				list.Add(behaviour);
			}
		}
		
		// Recursive call on all sub-state machines in the given state machine
		foreach (ChildAnimatorStateMachine subMachine in stateMachine.stateMachines)
		{
			AddAllBehavioursFromMachine(list, subMachine.stateMachine);
		}
	}
}