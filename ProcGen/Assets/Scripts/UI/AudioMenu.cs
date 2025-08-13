using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;

public class AudioMenu : MonoBehaviour
{
    [Header("Music")]
    [SerializeField] TextMeshProUGUI musicVolumeValueText;
    [SerializeField] Slider musicSlider;

    [Header("Sound Effects")]
    [SerializeField] TextMeshProUGUI sfxVolumeValueText;
    [SerializeField] Slider sfxSlider;
    public static Action OnBack;
    public void OnExitClicked()
    {
        gameObject.SetActive(false);
        OnBack?.Invoke();
    }

    private void OnEnable()
    {
        // get music and sfx volume from sound manager
        if (musicVolumeValueText && musicSlider && SoundManager.Instance)
        {
            float value = SoundManager.Instance.GetMusicVolume();
            musicVolumeValueText.text = (value * 100).ToString("0");
            musicSlider.value = value;
        }

        if (sfxVolumeValueText && sfxSlider && SoundManager.Instance)
        {
            float value = SoundManager.Instance.GetSoundEffectsVolume();
            sfxVolumeValueText.text = (value * 100).ToString("0");
            sfxSlider.value = value;
        }
    }

    public void OnMusicVolumeChange(float vol)
    {
        if (SoundManager.Instance)
        {
            SoundManager.Instance.OnMusicVolumeChange(vol);

            if(musicVolumeValueText)
                musicVolumeValueText.text = (vol * 100).ToString("0");
        }
    }

    public void OnSoundEffectsVolumeChange(float vol)
    {
        if (SoundManager.Instance)
        {
            SoundManager.Instance.OnSoundEffectsVolumeChange(vol);

            if(sfxVolumeValueText)
                sfxVolumeValueText.text = (vol * 100).ToString("0");
        }
    }
}
