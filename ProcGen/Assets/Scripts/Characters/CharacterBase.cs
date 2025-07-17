using UnityEngine;

public abstract class CharacterBase : MonoBehaviour
{
    [SerializeField] protected float maxHealthPoints = 100f;
    [SerializeField] protected float moveSpeed = 3.5f;

    public float MaxHealthPoints => maxHealthPoints;
    public float MoveSpeed => moveSpeed;
    public float HealthPoints { get; protected set; }

    protected virtual void Awake()
    {
        HealthPoints = maxHealthPoints;
    }

    public virtual void TakeDamage(float amount)
    {
        HealthPoints -= amount;
        if (HealthPoints <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        Debug.Log($"{gameObject.name} died.");
    }

    public virtual void Heal(float amount)
    {
        HealthPoints = Mathf.Min(HealthPoints + amount, maxHealthPoints);
    }
}

