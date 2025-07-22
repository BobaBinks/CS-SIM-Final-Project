using UnityEngine;
using System.Collections.Generic;
public class SkeletonWarrior : EnemyAI
{
    public override void DealDamage()
    {
        base.DealDamage();
        animator.CrossFade("SkeletonWarriorIdle", 1f);
    }

    public override void TakeDamage(float damage)
    {
        HealthPoints -= damage;
        


        if (HealthPoints < 0)
        {
            Debug.Log($"{name} Died");
            // call died function
            return;
        }

        if (animator)
        {
            animator.Play("SkeletonWarriorTakeDamage");
        }
    }
}
