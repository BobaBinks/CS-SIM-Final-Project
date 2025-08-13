using UnityEngine;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this);

        if(musicSource)
            musicSource.volume = 0.5f;

        if (sfxSource)
            sfxSource.volume = 0.5f;
    }

    public float GetMusicVolume()
    {
        if (!musicSource)
            return 0f;

        return musicSource.volume;
    }

    public float GetSoundEffectsVolume()
    {
        if (!sfxSource)
            return 0f;

        return sfxSource.volume;
    }

    public void OnMusicVolumeChange(float vol)
    {
        musicSource.volume = vol;
    }

    public void OnSoundEffectsVolumeChange(float vol)
    {
        sfxSource.volume = vol;
    }

    private void Update()
    {
        //if(musicSource1 && musicSource1.isPlaying)
        //{
        //    // check if current music about to end
        //}
    }

    public void PlaySoundEffect(AudioClip sfx)
    {
        if (sfx == null)
            return;

        sfxSource.PlayOneShot(sfx);
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
