using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    [SerializeField] GameObject settingsMenu;
    [SerializeField] GameObject audioMenu;
    [SerializeField] GameObject controlMenu;
    [SerializeField] GameObject mainMenu;

    private void Start()
    {
        AudioMenu.OnBack += ActivateSettingsMenu;
        ControlsMenu.OnBack += ActivateSettingsMenu;
    }

    private void OnDestroy()
    {
        AudioMenu.OnBack -= ActivateSettingsMenu;
        ControlsMenu.OnBack -= ActivateSettingsMenu;
    }

    public void OnSettingsButtonClicked()
    {
        ActivateSettingsMenu();

        if (mainMenu)
        {
            mainMenu.SetActive(false);
        }
    }

    public void OnControlsButtonClick()
    {
        if (controlMenu && settingsMenu)
        {
            controlMenu.SetActive(true);

            settingsMenu.SetActive(false);
        }

    }

    public void ActivateSettingsMenu()
    {
        if (settingsMenu)
        {
            settingsMenu.SetActive(true);
        }
    }

    public void OnBackButtonClick()
    {
        if (settingsMenu)
            settingsMenu.SetActive(false);

        if (controlMenu)
            controlMenu.SetActive(false);

        if (audioMenu)
            audioMenu.SetActive(false);

        if(mainMenu)
            mainMenu.SetActive(true);
    }

    public void OnAudioButtonClick()
    {
        if (audioMenu && settingsMenu)
        {
            audioMenu.SetActive(true);

            settingsMenu.SetActive(false);
        }
    }

    public void OnStartButtonClick()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void OnExitButtonClick()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }
}
