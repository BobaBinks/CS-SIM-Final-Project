using UnityEngine;
using UnityEngine.UI;
public class UIManager: MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] Image healthBar;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetHealthBar(float hp, float maxHP)
    {
        if (!healthBar || float.IsNaN(hp) || float.IsNegative(hp) || float.IsNaN(maxHP) || float.IsNegative(maxHP))
            return;
        hp = Mathf.Clamp(hp, 0f, maxHP);
        healthBar.fillAmount = hp / maxHP;
    }
}
