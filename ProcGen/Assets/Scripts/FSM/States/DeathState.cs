using UnityEngine;

public class DeathState : BaseState<EnemyAI>
{
    public DeathState(string stateId) : base(stateId)
    {

    }

    public override void EnterState(EnemyAI owner)
    {
        owner.Die();
        owner.animator.Play(owner.DeathAnimationName);
        Debug.Log("Entering Death State");
    }

    public override void ExitState(EnemyAI owner)
    {
        Debug.Log("Exiting Death State");
    }

    public override void UpdateState(EnemyAI owner, double deltaTime)
    {
    }
}
