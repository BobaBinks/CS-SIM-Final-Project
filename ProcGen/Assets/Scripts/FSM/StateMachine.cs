using UnityEngine;
using System.Collections.Generic;

public class StateMachine<T>
{
    public BaseState<T> CurrentState { get; set; }
    public BaseState<T> NextState { get; set; }

    public T owner;

    public Dictionary<string, BaseState<T>> states;

    public StateMachine(T owner)
    {
        this.owner = owner;
        CurrentState = null;
        states = new Dictionary<string, BaseState<T>>();
    }

    /// <summary>
    /// Add a new state to the states dictionary.
    /// </summary>
    /// <param name="state">The new state to add.</param>
    public void AddState(BaseState<T> state)
    {
        if (state == null)
            return;

        if (states.ContainsKey(state.GetStateId()))
            return;

        states.Add(state.GetStateId(), state);
    }

    /// <summary>
    /// Set the next state to transition to in the next update call.
    /// </summary>
    /// <param name="stateId">The ID of the next state</param>
    public void SetNextState(string stateId)
    {
        if (string.IsNullOrEmpty(stateId) || !states.ContainsKey(stateId)) return;

        NextState = states[stateId];
    }

    /// <summary>
    /// Immediately sets the current state without calling Exit/Enter methods.
    /// </summary>
    /// <param name="stateId">The ID of the state</param>
    public void SetCurrentState(string stateId)
    {
        if (string.IsNullOrEmpty(stateId) || !states.ContainsKey(stateId)) return;

        CurrentState = states[stateId];
    }

    /// <summary>
    /// Set the initial state of the state machine.
    /// </summary>
    /// <param name="stateId">The ID of the state</param>
    public void SetInitialState(string stateId)
    {
        if (string.IsNullOrEmpty(stateId) || !states.ContainsKey(stateId)) return;

        CurrentState = states[stateId];
        NextState = states[stateId];
        CurrentState.EnterState(owner);
    }

    /// <summary>
    /// Transition to the next state and update current state
    /// </summary>
    /// <param name="deltaTime"></param>
    public void Update(double deltaTime)
    {
        // transition to next state
        if(NextState != CurrentState)
        {
            CurrentState.ExitState(owner);
            CurrentState = NextState;
            CurrentState.EnterState(owner);
        }

        // update current state
        if (CurrentState != null)
            CurrentState.UpdateState(owner, deltaTime);
    }
}
