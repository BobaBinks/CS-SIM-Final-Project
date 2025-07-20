using UnityEngine;
using System.Collections.Generic;
public class SkeletonWarrior : EnemyAI
{
    public override void DealDamage()
    {
        base.DealDamage();
        animator.CrossFade("SkeletonWarriorIdle", 1f);
    }
}
