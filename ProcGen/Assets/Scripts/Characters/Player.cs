using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D), typeof(SpriteFlipper))]
public class Player : CharacterBase, IDamagable
{
    public Rigidbody2D rigidBody { get; protected set; }
    public SpriteFlipper spriteFlipper { get; protected set; }

    public Animator animator { get; protected set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        spriteFlipper = GetComponent<SpriteFlipper>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TakeDamage(float damage)
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
