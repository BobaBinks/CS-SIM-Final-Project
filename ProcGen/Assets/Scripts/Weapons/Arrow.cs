using UnityEngine;
[RequireComponent(typeof(BoxCollider2D))]
public class Arrow : BaseProjectile
{
    private float damage = 10f;

    public void InitializeArrow(float damage = 10f, float lifetime = 5f)
    {
        this.damage = Mathf.Max(0.01f, damage);
        Destroy(gameObject, Mathf.Max(0.01f, lifetime));
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        CharacterBase character = collision.GetComponent<CharacterBase>();
        if (character && damage > 0)
        {
            character.TakeDamage(damage);
        }
        Destroy(gameObject);
    }
}
