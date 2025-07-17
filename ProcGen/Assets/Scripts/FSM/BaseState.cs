using UnityEngine;

public abstract class BaseState<T>
{
    protected string stateId;

    public BaseState(string stateId)
    {
        this.stateId = stateId;
    }

    public string GetStateId() { return stateId; }
    public abstract void EnterState(T owner);
    public abstract void UpdateState(T owner, double deltaTime);
    public abstract void ExitState(T owner);
}
