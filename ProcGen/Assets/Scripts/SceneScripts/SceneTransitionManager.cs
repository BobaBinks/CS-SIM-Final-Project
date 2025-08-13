using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this);
    }

    void OnEnable()
    {
        // Subscribe to scene loaded event
        SceneManager.sceneLoaded += OnSceneChange;
    }

    void OnDisable()
    {
        // Unsubscribe to avoid leaks
        SceneManager.sceneLoaded -= OnSceneChange;
    }

    private void OnSceneChange(Scene scene, LoadSceneMode mode)
    {
        if(scene.name == "GameScene")
        {
            // play game music
            if (SoundManager.Instance && SoundLibrary.Instance)
                SoundManager.Instance.PlayMusic(
                        SoundLibrary.Instance.GetAudioClip(
                            SoundLibrary.Music.DUNGEON_1
                        ),
                        loop: true
                    );
        }
        else if(scene.name == "Menu")
        {
            // play menu music
            if (SoundManager.Instance && SoundLibrary.Instance)
                SoundManager.Instance.PlayMusic(
                        SoundLibrary.Instance.GetAudioClip(
                            SoundLibrary.Music.MENU
                        ),
                        loop: true
                    );
        }
    }
}
