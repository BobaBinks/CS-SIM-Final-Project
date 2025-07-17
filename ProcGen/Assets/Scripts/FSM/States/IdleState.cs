using UnityEngine;
public class IdleState : BaseState<EnemyAI>
{
    public IdleState(string stateId): base(stateId)
    {
       
    }

    public override void EnterState(EnemyAI owner)
    {
        owner.animator.Play("SkeletonWarriorIdle");
        //owner.animator.SetBool("isIdle", true);
        Debug.Log("Entering Idle State");
    }

    public override void ExitState(EnemyAI owner)
    {
        //owner.animator.SetBool("isIdle", false);
        Debug.Log("Exiting Idle State");
    }

    public override void UpdateState(EnemyAI owner, double deltaTime)
    {
        if (owner.HealthPoints < 0)
            owner.Sm.SetNextState("death");
    }
}
