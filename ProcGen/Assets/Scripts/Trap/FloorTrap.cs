using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class FloorTrap : MonoBehaviour
{
    [SerializeField] float damage = 50;

    Collider2D collider;
    List<IDamagable> damagables;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        damagables = new List<IDamagable>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DealDamage()
    {
        damage = Mathf.Max(damage, 1);
        foreach(var damagable in damagables)
        {
            damagable.TakeDamage(damage);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.TryGetComponent<IDamagable>(out IDamagable damagable))
        {
            damagables.Add(damagable);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent<IDamagable>(out IDamagable damagable))
        {
            damagables.Remove(damagable);
        }
    }
}
