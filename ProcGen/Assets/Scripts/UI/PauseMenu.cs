using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] GameObject controlMenu;
    private void Start()
    {
        if (controlMenu)
        {
            // controlMenu.SetActive(false);
            ControlsMenu.OnBack += ActivatePauseMenu;
        }

        if (GameManager.Instance)
            GameManager.Instance.TogglePause();
    }

    public void OnResumePress()
    {
        if (GameManager.Instance)
            GameManager.Instance.TogglePause();
        // unpause game
        Time.timeScale = 1f;
        gameObject.SetActive(false);
    }

    public void OnExitPress()
    {
        SceneManager.LoadScene("Menu");
    }

    public void OnRestartPress()
    {
        if(controlMenu)
            SceneManager.LoadScene("GameScene");
    }

    public void OnControlMenuPress()
    {
        if(controlMenu)
        {
            controlMenu.gameObject.SetActive(true);
            gameObject.SetActive(false);
        }    
    }

    public void ActivatePauseMenu()
    {
        gameObject.SetActive(true);
    }

    private void OnDestroy()
    {
        if(controlMenu)
            ControlsMenu.OnBack -= ActivatePauseMenu;
    }
}
