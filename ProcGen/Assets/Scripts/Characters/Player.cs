using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D), typeof(SpriteFlipper))]
public class Player : CharacterBase
{
    public Rigidbody2D rigidBody { get; protected set; }
    public SpriteFlipper spriteFlipper { get; protected set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        spriteFlipper = GetComponent<SpriteFlipper>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
