using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class IState<T>
{
    public T owner;
    public virtual void Enter() { }
    public virtual void Execute() { }
    public virtual void Exit() { }
}



public class StateMachine<T>
{
    IState<T> currentState;

    public StateMachine(IState<T> defaultState)
    {
        SetState(defaultState);
    }

    public void StateUpdate()
    {
        if (currentState!=null)
            currentState.Execute();
    }

    public void SetState(IState<T> newState)
    {
        if (currentState != null)
            currentState.Exit();

        currentState = newState;

        if (currentState != null)
            currentState.Enter();
    }
}
