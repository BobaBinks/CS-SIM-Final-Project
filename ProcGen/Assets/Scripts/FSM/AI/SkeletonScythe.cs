using UnityEngine;
using System.Collections.Generic;
public class SkeletonScythe : EnemyAI
{

    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);

        if(healthBar)
            healthBar.fillAmount = HealthPoints / MaxHealthPoints;
    }
}
