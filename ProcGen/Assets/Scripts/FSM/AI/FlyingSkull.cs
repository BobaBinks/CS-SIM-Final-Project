using UnityEngine;

public class FlyingSkull : EnemyAI
{
    [SerializeField] GameObject projectilePrefab;
    public Vector3 aimDirection;
    public float leadDistance = 0.3f;


    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);

        if (healthBar)
            healthBar.fillAmount = HealthPoints / MaxHealthPoints;

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
    }
}
