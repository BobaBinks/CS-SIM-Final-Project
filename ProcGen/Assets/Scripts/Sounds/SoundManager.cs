using UnityEngine;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;

    int currMusicSource = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this);
    }

    private void Update()
    {
        //if(musicSource1 && musicSource1.isPlaying)
        //{
        //    // check if current music about to end
        //}
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
