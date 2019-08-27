using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIStateMachine : MonoBehaviour
{
    private IState currentState;

    public void ChangeState(IState newState)
    {
        // Call Exit on last state
        if (currentState != null)
        {
            currentState.Exit();
        }

        // Transition to new state
        currentState = newState;
        currentState.Enter();
    }
    
    // Update is called once per frame
    void Update()
    {
        // Perform whatever actions the state has
        currentState.Action();
        
        // Check if the state should transition to another state
        IState nextState = currentState.Transition();
        if (nextState != null)
        {
            ChangeState(nextState);
        }
    }
}

public interface IState
{
    void Enter();
    void Exit();
    void Action();
    IState Transition();
}
