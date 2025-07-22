using UnityEngine;
using System.Collections.Generic;
public class AttackState : BaseState<EnemyAI>
{

    public AttackState(string stateId) : base(stateId)
    {

    }

    public override void EnterState(EnemyAI owner)
    {
        Debug.Log("Entering Attack State");
        owner.AttackTimer = 0;
    }

    public override void ExitState(EnemyAI owner)
    {
        Debug.Log("Exiting Attack State");
    }

    public override void UpdateState(EnemyAI owner, double deltaTime)
    {
        if (owner.HealthPoints <= 0)
        {
            owner.Sm.SetNextState("death");
            return;
        }
        if (owner.player == null)
        {
            Debug.LogWarning("Attack: No player assigned!");
            owner.Sm.SetNextState("idle");
            return;
        }

        if (!owner.PlayerInAttackRange())
        {
            // change to chase state
            owner.Sm.SetNextState("chase");
        }
        else
        {
            owner.AttackTimer -= (float)deltaTime;

            if(owner.AttackTimer < 0)
            {
                owner.AttackTimer = owner.AttackCooldown;
                owner.animator.Play("SkeletonWarriorAttack");
            }
        }
    }
}
