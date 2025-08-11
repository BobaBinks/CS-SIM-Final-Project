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
        if (SoundManager.Instance && SoundLibrary.Instance)
            SoundManager.Instance.PlaySoundEffect(
                    SoundLibrary.Instance.GetAudioClip(SoundLibrary.Enemy.PHYSICAL_ATTACK)
                );
    }
}
