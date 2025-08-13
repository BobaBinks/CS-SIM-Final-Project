using UnityEngine;
using System;
public class Vampire : EnemyAI
{

    public override void DealDamage()
    {
        base.DealDamage();
        animator.CrossFade(IdleAnimationName, 1f);
        PlayBiteSound();
    }

    public override void Die()
    {
        base.Die();
        Boss.OnBossDie?.Invoke();
    }

    public void PlayBiteSound()
    {
        if (SoundManager.Instance && SoundLibrary.Instance)
            SoundManager.Instance.PlaySoundEffect(
                SoundLibrary.Instance.GetAudioClip(SoundLibrary.Enemy.BITE));
    }
}