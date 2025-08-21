using System.Collections;
using UnityEngine;
using UnityEngine.Pool;
public class EnemyProjectile : BaseProjectile
{
    [SerializeField] float delay = 5f;
    IObjectPool<EnemyProjectile> enemyProjectilePool;
    public IObjectPool<EnemyProjectile> EnemyProjectilePool { set => enemyProjectilePool = value; }

    private Coroutine delayCoroutine;

    public override void InitializeProjectile(float damage = 10, float lifetime = 5)
    {
        this.damage = Mathf.Max(0.01f, damage);
    }

    public void Deactivate()
    {
        delayCoroutine = StartCoroutine(DeactivateCoroutine(delay));
    }

    private IEnumerator DeactivateCoroutine(float delay)
    {
        delay = Mathf.Max(0.1f, delay);
        yield return new WaitForSeconds(delay);

        ResetAndReleaseProjectile();
    }

    private void ResetAndReleaseProjectile()
    {
        // stop coroutine
        if (delayCoroutine != null)
        {
            StopCoroutine(delayCoroutine);
            delayCoroutine = null;
        }


        // reset projectile
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.angularVelocity = 0;
        rb.linearVelocity = Vector2.zero;

        // release back to object pool
        enemyProjectilePool.Release(this);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Player player = collision.GetComponent<Player>();
        if (player && damage > 0)
        {
            player.TakeDamage(damage);
        }

        if (!collision.CompareTag("Enemies"))
        {
            ResetAndReleaseProjectile();
        }
    }
}
