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

        if (!owner.PlayerInMaxAttackRange())
        {
            // change to chase state
            owner.Sm.SetNextState("chase");
        }
        else
        {
            if (owner.requireLOS)
            {
                Vector2 ownerPosition = new Vector2(owner.transform.position.x, owner.transform.position.y);
                Vector2 playerPosition = new Vector2(owner.player.transform.position.x, owner.player.transform.position.y);
                Vector2 dir = (playerPosition - ownerPosition).normalized;

                FlyingSkull flyingSkull = owner as FlyingSkull;

                float radius = flyingSkull.GetProjectileRadius();

                // cast 2d ray
                RaycastHit2D hit = Physics2D.CircleCast(ownerPosition, radius, dir, distance: 10f, layerMask: owner.losLayerMask);
                flyingSkull.DebugDrawCircle(owner.transform.position, radius, Color.green);
                if (hit.collider != null && !hit.collider.CompareTag("Player"))
                {
                    owner.Sm.SetNextState("chase");
                    return;
                }
            }


            owner.AttackTimer -= (float)deltaTime;

            if(owner.AttackTimer < 0)
            {
                owner.AttackTimer = owner.AttackCooldown;
                owner.animator.Play(owner.AttackAnimationName);
            }
        }
    }
}
