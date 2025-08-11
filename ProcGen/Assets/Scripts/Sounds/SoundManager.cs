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
        FOOTSTEPS,
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
            Destroy(gameObject); // Destroy duplicate
            return;
        }

        Instance = this;
    }

    public void PlaySoundEffect(SoundEffects sfx, float volumeScale = 1)
    {
        int sfxIndex = (int)sfx;
        if (sfxIndex < 0 || sfx >= SoundEffects.SOUND_EFFECT_SIZE || sfxSource == null)
            return;

        volumeScale = Mathf.Clamp(volumeScale, 0, 1);

        sfxSource.PlayOneShot(audioClips[sfxIndex], volumeScale);
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
