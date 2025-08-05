using UnityEngine;
using UnityEngine.SceneManagement;
public class PauseMenu : MonoBehaviour
{
    private void Start()
    {
        gameObject.SetActive(false);
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
        SceneManager.LoadScene("GameScene");
    }
}
