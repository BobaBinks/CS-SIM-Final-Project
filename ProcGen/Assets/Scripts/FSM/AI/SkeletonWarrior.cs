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
        base.TakeDamage(damage);

        if(healthBar)
            healthBar.fillAmount = HealthPoints / MaxHealthPoints;

        if (animator)
        {
            animator.Play("SkeletonWarriorTakeDamage");
        }
    }
}
