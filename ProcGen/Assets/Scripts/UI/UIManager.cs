using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class UIManager: MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] Image healthBar;
    [SerializeField] Image levelBar;
    [SerializeField] TextMeshProUGUI healthText;
    [SerializeField] TextMeshProUGUI xpText;
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] GameObject pauseMenu;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // DontDestroyOnLoad(gameObject);
    }

    public void TogglePauseMenu()
    {
        if (pauseMenu)
            pauseMenu.SetActive(!pauseMenu.activeSelf);
    }

    public void SetHealth(float hp, float maxHP)
    {
        if (!healthBar || !healthText || float.IsNaN(hp) || float.IsNegative(hp) || float.IsNaN(maxHP) || float.IsNegative(maxHP))
            return;
        hp = Mathf.Clamp(hp, 0f, maxHP);
        healthBar.fillAmount = hp / maxHP;
        //healthText.text = $"{(int)hp} / {(int)maxHP}";
        healthText.text = $"{(int)hp} HP";
    }

    public void SetLevel(float currentXP, float maxXP, int level)
    {
        if (!levelBar ||
            !levelText ||
            !xpText ||
            float.IsNaN(currentXP) ||
            float.IsNegative(currentXP) ||
            float.IsNaN(maxXP) ||
            float.IsNegative(maxXP) ||
            float.IsNaN(level) ||
            float.IsNegative(level))
            return;
        currentXP = Mathf.Clamp(currentXP, 0f, maxXP);
        levelBar.fillAmount = currentXP / maxXP;
        //xpText.text = $"{(int)currentXP} / {(int)maxXP}";
        xpText.text = $"{(int)currentXP} XP";
        levelText.text = $"Lvl: {level}";
    }


}
