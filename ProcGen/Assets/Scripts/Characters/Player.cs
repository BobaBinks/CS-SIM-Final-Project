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

    public bool attackOnCooldown = false;

    // [SerializeField] float baseSwordDamage = 10f;

    public float BaseSwordDamage { get; private set; }

    [SerializeField]
    float attackCooldownTime = 1f;

    #region Stat Curves
    [SerializeField] AnimationCurve damageCurve;
    [SerializeField] AnimationCurve healthCurve;
    [SerializeField] AnimationCurve speedCurve;
    #endregion

    public float AttackCooldownTime 
    {
        get { return attackCooldownTime; }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        spriteFlipper = GetComponent<SpriteFlipper>();
        animator = GetComponent<Animator>();

        BaseSwordDamage = damageCurve.Evaluate(Level);
        maxHealthPoints = healthCurve.Evaluate(Level);
        moveSpeed = speedCurve.Evaluate(Level);
    }
    


    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);

        if (UIManager.Instance)
            UIManager.Instance.SetHealthBar(HealthPoints, MaxHealthPoints);

        Debug.Log($"Player health: {HealthPoints}");

        if(HealthPoints <= 0)
        {
            // game end
        }
    }
}
