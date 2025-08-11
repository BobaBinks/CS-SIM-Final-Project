using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D), typeof(SpriteFlipper))]
public class Player : CharacterBase, IDamagable, IHealable
{
    [SerializeField] protected GameObject floatingTextLevelUpContainer;
    [SerializeField] protected GameObject floatingTextWeaponLevelUpContainer;
    [SerializeField] protected GameObject floatingTextGeneralContainer;

    public Rigidbody2D rigidBody { get; protected set; }
    public SpriteFlipper spriteFlipper { get; protected set; }

    public Animator animator { get; protected set; }

    public WeaponManager weaponManager;

    public bool attackOnCooldown = false;

    #region Additional Stats
    public float currentXp;

    [SerializeField]
    float attackCooldownTime = 1f;
    public float AttackCooldownTime
    {
        get { return attackCooldownTime; }
    }
    #endregion

    #region Stat Curves
    [SerializeField] AnimationCurve healthCurve;
    [SerializeField] AnimationCurve speedCurve;
    [SerializeField] AnimationCurve levelXPCurve;

    public AnimationCurve LevelXPCurve => levelXPCurve;
    #endregion

    #region Events
    #endregion

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        spriteFlipper = GetComponent<SpriteFlipper>();
        animator = GetComponent<Animator>();

        maxHealthPoints = healthCurve.Evaluate(Level);
        moveSpeed = speedCurve.Evaluate(Level);

        if (UIManager.Instance)
        {
            UIManager.Instance.SetHealth(HealthPoints, maxHealthPoints);
            UIManager.Instance.SetLevel(currentXp, levelXPCurve.Evaluate(Level), Level);
        }
    }
    
    public void GainXP(float xp)
    {
        float xpRequirements = levelXPCurve.Evaluate(level);

        currentXp += xp;
        string floatingText = $"{xp: 0} XP gained";
        UpdateXP();

        if (SoundManager.Instance && SoundLibrary.Instance)
            SoundManager.Instance.PlaySoundEffect(
                    SoundLibrary.Instance.GetAudioClip(SoundLibrary.Player.XP_GAIN)
                );

        if (floatingTextPrefab && floatingTextGeneralContainer)
            InstantiateFloatingText(floatingText, floatingTextGeneralContainer.transform, offset: false);

        if (currentXp >= xpRequirements)
        {
            level += 1;
            currentXp = currentXp - xpRequirements;
            Debug.Log($"currentXp: {currentXp}/{xpRequirements}");
            OnLevelUp();
        }
    }

    private void OnLevelUp()
    {
        moveSpeed = speedCurve.Evaluate(Level);
        maxHealthPoints = healthCurve.Evaluate(Level);
        HealthPoints = maxHealthPoints;
        UpdateHealthBar();
        UpdateXP();

        if (weaponManager)
            weaponManager.SetWeaponLevels(Level);

        if (floatingTextPrefab && floatingTextLevelUpContainer && floatingTextWeaponLevelUpContainer)
        {
            InstantiateFloatingText("Level +1", floatingTextLevelUpContainer.transform);
            InstantiateFloatingText("Weapon Levels +1", floatingTextWeaponLevelUpContainer.transform);
        }
    }

    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);

        UpdateHealthBar();

        Debug.Log($"Player health: {HealthPoints}");

        if (SoundManager.Instance && SoundLibrary.Instance)
            SoundManager.Instance.PlaySoundEffect(
                SoundLibrary.Instance.GetAudioClip(SoundLibrary.Player.PLAYER_HIT),
                volumeScale: 1);

        if (HealthPoints <= 0)
        {
            // game end
            SceneManager.LoadScene("Menu");
        }
    }

    public void Heal(float amount)
    {
        HealthPoints = Mathf.Min(HealthPoints + amount, maxHealthPoints);

        string floatingText = $"+ {amount:0} HP";

        if (floatingTextPrefab && floatingTextGeneralContainer)
            InstantiateFloatingText(floatingText, floatingTextGeneralContainer.transform, offset: false);

        if (SoundManager.Instance && SoundLibrary.Instance)
            SoundManager.Instance.PlaySoundEffect(
                    SoundLibrary.Instance.GetAudioClip(SoundLibrary.Player.HEAL)
                );

        UpdateHealthBar();
    }

    public void UpdateHealthBar()
    {
        if (UIManager.Instance)
            UIManager.Instance.SetHealth(HealthPoints, MaxHealthPoints);
    }

    public void UpdateXP()
    {
        if (UIManager.Instance)
        {
            UIManager.Instance.SetLevel(currentXp, levelXPCurve.Evaluate(Level), Level);
        }
    }

    public void PlayFootSteps()
    {
    }
}
