using UnityEngine;
using System.Collections.Generic;
public class SkeletonWarrior : EnemyAI
{
    public override void DealDamage()
    {
        base.DealDamage();
        PlayAttackSound();
    }

    public void PlayAttackSound()
    {
        if (SoundManager.Instance)
            SoundManager.Instance.PlaySoundEffect(SoundManager.SoundEffects.ENEMY_PHYSICAL_ATTACK);
    }
}
