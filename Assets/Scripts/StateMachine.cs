using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class State
{
    public virtual void Enter() { }
    public virtual void Execute() { }
    public virtual void Exit() { }
}


public class StateMachine<T> where T : Enum
{
    Dictionary<T, State> _states = new();
    State currentState;



    public void RegisterState(T type, State state)
    {
        if (_states.ContainsKey(type))
            throw new Exception("Stato già presente: " + type);

        _states.Add(type, state);
    }

    public void StateUpdate()
    {
            currentState?.Execute();
    }

    public void SetState(T type)
    {
        currentState?.Exit();

        currentState = _states[type];

        currentState.Enter();
    }
}
