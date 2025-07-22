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

    [SerializeField] float baseSwordDamage = 10f;

    public float BaseSwordDamage { get; private set; }

    [SerializeField]
    float attackCooldownTime = 1f;

    public float AttackCooldownTime 
    {
        get { return attackCooldownTime; }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        BaseSwordDamage = baseSwordDamage;
        rigidBody = GetComponent<Rigidbody2D>();
        spriteFlipper = GetComponent<SpriteFlipper>();
        animator = GetComponent<Animator>();
    }

    public override void TakeDamage(float damage)
    {
        HealthPoints -= damage;

        Debug.Log($"Player health: {HealthPoints}");

        if (animator)
        {
            animator.Play("Player Take Damage");
        }

        if(HealthPoints < 0)
            Debug.Log("Player died");
    }
}
