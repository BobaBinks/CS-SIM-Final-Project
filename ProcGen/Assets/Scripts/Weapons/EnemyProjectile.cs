using UnityEngine;

public class EnemyProjectile : BaseProjectile
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Player character = collision.GetComponent<Player>();
        if (character && damage > 0)
        {
            character.TakeDamage(damage);
        }

        if(!collision.CompareTag("Enemies"))
            Destroy(gameObject);
    }
}
