using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIStateMachine : MonoBehaviour
{
    private IState currentState = new FallBackState();

    public void ChangeState(IState newState)
    {
        // Call Exit on last state
        currentState?.Exit();

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
    
    // Returns the name of the current state
    public string GetCurrentState()
    {
        return currentState.ToString();
    }
}

public interface IState
{
    // Called upon entering this state from anywhere
    void Enter();
    
    // Called upon exiting this state
    void Exit();

    // Called during Update while currently in this state
    void Action();
    
    // Called immediately after Action. Returns an IState if it can transition to that state, and null if no transition
    // is possible
    IState Transition();

    // Returns the name of the state
    string ToString();
}

public class FallBackState : IState
{
    public void Enter() {}
    
    public void Exit() {}
    
    public void Action() {}

    public IState Transition()
    {
        return null;
    }

    override public string ToString()
    {
        return "FallBack";
    }
}
