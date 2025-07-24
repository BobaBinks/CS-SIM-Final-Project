using UnityEngine;
using System.Collections.Generic;
public class ChaseState : BaseState<EnemyAI>
{
    AStarPathfinder aStarPathfinder;
    Vector3 savedPlayerPosition;
    public ChaseState(string stateId) : base(stateId)
    {

    }

    public override void EnterState(EnemyAI owner)
    {
        Debug.Log("Entering Chase State");

        if (GameManager.Instance.aStarPathfinder != null)
        {
            aStarPathfinder = GameManager.Instance.aStarPathfinder;

            // set intial path
            savedPlayerPosition = owner.transform.position;
            owner.pathMover.SetPathTo(savedPlayerPosition, aStarPathfinder);
            owner.animator.Play(owner.MoveAnimationName);
        }
    }

    public override void ExitState(EnemyAI owner)
    {
        Debug.Log("Exiting Chase State");
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
            Debug.LogWarning("ChaseState: No player assigned!");
            owner.Sm.SetNextState("idle");
            return;
        }
        if(aStarPathfinder == null)
        {
            Debug.LogWarning("ChaseState: No AStarPathFinder assigned!");
            owner.Sm.SetNextState("idle");
            return;
        }

        // check if path is outdated
        float differenceSquared = (savedPlayerPosition - owner.player.transform.position).sqrMagnitude;
        if (differenceSquared > owner.AttackRange * owner.AttackRange)
        {
            // update path
            savedPlayerPosition = owner.player.transform.position;
            owner.pathMover.UpdatePath(savedPlayerPosition, aStarPathfinder);
            return;
        }

        if (owner.pathMover.IsPathComplete())
        {
            owner.Sm.SetNextState("attack");
            return;
        }

        // move along path
        owner.pathMover.MoveAlongPath(deltaTime, owner.MoveSpeed);

        // ensure correct animation is playing after any interruption like taking damage
        AnimatorStateInfo stateInfo = owner.animator.GetCurrentAnimatorStateInfo(0);
        if (!stateInfo.IsName(owner.MoveAnimationName))
        {
            owner.animator.Play(owner.MoveAnimationName);
        }
    }
}
