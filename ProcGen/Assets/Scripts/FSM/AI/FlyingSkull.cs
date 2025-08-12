using UnityEngine;

public class FlyingSkull : EnemyAI
{
    [SerializeField] GameObject projectilePrefab;

    public Vector3 aimDirection;
    public float leadDistance = 0.3f;

    public float GetProjectileRadius()
    {
        if (projectilePrefab == null)
        {
            Debug.Log("Projectile prefab not assigned.");
            return 0f;
        }

        // try to get a circle collider first
        CircleCollider2D circle = projectilePrefab.GetComponent<CircleCollider2D>();
        if (circle != null)
        {
            // account for prefab scale
            float maxScale = Mathf.Max(projectilePrefab.transform.localScale.x, projectilePrefab.transform.localScale.y);
            return circle.radius * maxScale + 0.02f;
        }

        return 0f;
    }

    public void DebugDrawCircle(Vector2 center, float radius, Color color, int segments = 30)
    {
        float angleStep = 360f / segments;
        Vector3 prevPoint = center + new Vector2(Mathf.Cos(0), Mathf.Sin(0)) * radius;

        for (int i = 1; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            Debug.DrawLine(prevPoint, newPoint, color);
            prevPoint = newPoint;
        }
    }

    public void FireProjectile()
    {
        if (!projectilePrefab)
            return;


        Vector3 spawnPos = transform.position;

        Rigidbody2D playerRb = null;

        if (player)
            playerRb = GetComponent<Rigidbody2D>();

        if (playerRb == null)
            return;

        Vector3 playerVelocity = new Vector3(playerRb.linearVelocityX, playerRb.linearVelocityY);
        Vector3 dir = (player.transform.position + playerVelocity * leadDistance) - spawnPos;
        dir.Normalize();

        GameObject projectileGO = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

        EnemyProjectile enemyProjectile = projectileGO.GetComponent<EnemyProjectile>();

        if(enemyProjectile)
        {
            enemyProjectile.InitializeProjectile(damage: damageCurve.Evaluate(Level));
        }
        projectileGO.GetComponent<Rigidbody2D>().linearVelocity = dir * 0.5f; // Example speed

        if (SoundManager.Instance && SoundLibrary.Instance)
        {
                SoundManager.Instance.PlaySoundEffect(
                        SoundLibrary.Instance.GetAudioClip(SoundLibrary.Spells.FIREBALL_1)
                    );
        }
    }
}
