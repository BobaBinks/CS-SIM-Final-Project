using UnityEngine;

[RequireComponent(typeof(Collider2D),typeof(Rigidbody2D))]
public class BaseProjectile : MonoBehaviour
{
    protected float damage = 10f;

    public virtual void InitializeProjectile(float damage = 10f, float lifetime = 5f)
    {
        this.damage = Mathf.Max(0.01f, damage);
        Destroy(gameObject, Mathf.Max(0.01f, lifetime));
    }
}
