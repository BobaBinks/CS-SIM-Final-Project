using UnityEngine;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public enum SoundEffects
    { 
        BITE,
        HIT,
        PLAYER_HIT,
        PLAYER_SWORD_SFX,
        PLAYER_CROSSBOX_SFX,
        HEAL,
        XP_GAIN,
        FIREBALL_1,
        TELEPORT,
        ENEMY_PHYSICAL_ATTACK,
        PLAYER_FOOTSTEP_1,
        PLAYER_FOOTSTEP_2,
        PLAYER_FOOTSTEP_3,
        PLAYER_FOOTSTEP_4,
        PLAYER_FOOTSTEP_5,
        SOUND_EFFECT_SIZE,
    };

    public static SoundManager Instance { get; private set; }
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private List<AudioClip> audioClips;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void PlaySoundEffect(AudioClip sfx, float volumeScale = 1)
    {
        if (sfx == null)
            return;

        volumeScale = Mathf.Clamp(volumeScale, 0, 1);

        sfxSource.PlayOneShot(sfx, volumeScale);
    }

    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (clip == null || musicSource == null)
            return;

        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.Play();
    }
}
