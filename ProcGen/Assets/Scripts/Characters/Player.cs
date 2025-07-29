using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D), typeof(SpriteFlipper))]
public class Player : CharacterBase, IDamagable
{
    public Rigidbody2D rigidBody { get; protected set; }
    public SpriteFlipper spriteFlipper { get; protected set; }

    public Animator animator { get; protected set; }

    public WeaponManager weaponManager;

    public bool attackOnCooldown = false;

    #region Additional Stats
    public float currentXp;
    public float BaseSwordDamage { get; private set; }

    [SerializeField]
    float attackCooldownTime = 1f;
    public float AttackCooldownTime
    {
        get { return attackCooldownTime; }
    }
    #endregion

    #region Stat Curves
    [SerializeField] AnimationCurve damageCurve;
    [SerializeField] AnimationCurve healthCurve;
    [SerializeField] AnimationCurve speedCurve;
    [SerializeField] AnimationCurve levelXPCurve;
    #endregion

    #region Events
    #endregion

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        spriteFlipper = GetComponent<SpriteFlipper>();
        animator = GetComponent<Animator>();

        BaseSwordDamage = damageCurve.Evaluate(Level);
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
        Debug.Log($"{xp} XP gained");
        if(currentXp >= xpRequirements)
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

        if (UIManager.Instance)
        {
            UIManager.Instance.SetHealth(HealthPoints, MaxHealthPoints);
            UIManager.Instance.SetLevel(currentXp, levelXPCurve.Evaluate(Level), Level);
        }
    }

    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);

        if (UIManager.Instance)
            UIManager.Instance.SetHealth(HealthPoints, MaxHealthPoints);

        Debug.Log($"Player health: {HealthPoints}");

        if(HealthPoints <= 0)
        {
            // game end
        }
    }
}
