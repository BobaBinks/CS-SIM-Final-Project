using UnityEngine;
using System;
public class Vampire : EnemyAI
{
    public override void DealDamage()
    {
        base.DealDamage();
        animator.CrossFade(IdleAnimationName, 1f);
    }

    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);

        if (healthBar)
            healthBar.fillAmount = HealthPoints / MaxHealthPoints;

        if (animator)
        {
            // animator.Play(TakeDamageAnimationName);
        }
    }

    public override void Die()
    {
        base.Die();
        Boss.OnBossDie?.Invoke();
    }
}