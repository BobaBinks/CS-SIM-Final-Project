using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;


public class FlyingSkull : EnemyAI
{
    [Header("Shooting Parameters")]
    [SerializeField] EnemyProjectile projectilePrefab;
    [SerializeField] float timeBetweenBursts = 2f;
    [SerializeField] int burstCount = 2;
    [SerializeField] int projectilePerBurst = 3;
    [SerializeField][Range(0,359)] int angleSpread = 3;
    [SerializeField] float coolDown = 3f;
    [SerializeField] bool stagger = false;
    [SerializeField] bool oscillate = false;
    [SerializeField] bool randomnizeShootingParams = false;

    bool collectionCheck = false;
    IObjectPool<EnemyProjectile> enemyProjectilePool;
    int defaultPoolCapacity  = 10;
    int maxPoolCapacity = 100;

    private bool isShooting = false;
    Vector3 aimDirection;

    private void RandomnizeShootingParameters()
    {
        timeBetweenBursts = Random.Range(.3f, .7f);
        burstCount = Random.Range(2, 5);
        projectilePerBurst = Random.Range(4, 10);

        angleSpread = Random.Range(0, 150);
        coolDown = Random.Range(2f, 3f);

        stagger = Random.Range(0, 2) == 1 ? true : false;
        oscillate = Random.Range(0, 2) == 1 ? true : false;
    }

    public override void Initialize(int level = 0)
    {
        enemyProjectilePool = new ObjectPool<EnemyProjectile>(CreateProjectile,
            OnGetFromPool,
            OnReleaseToPool,
            OnDestroyPooledObject,
            collectionCheck,
            defaultPoolCapacity,
            maxPoolCapacity);

        HealthPoints = maxHealthPoints;
        this.level = level;
        if (levelText)
            levelText.text = $"Lvl {level}";
        if (GameManager.Instance.player)
        {
            player = GameManager.Instance.player;
        }

        Sm = new StateMachine<EnemyAI>(this);
        Sm.AddState(new IdleState("idle"));
        Sm.AddState(new ChaseState("chase"));
        Sm.AddState(new RangedAttackState("attack"));
        Sm.AddState(new DeathState("death"));
        Sm.SetInitialState("idle");

        if (Level >= 0 && Level <= 100)
        {
            attackDamage = damageCurve.Evaluate(Level);
            HealthPoints = healthCurve.Evaluate(Level);
            maxHealthPoints = healthCurve.Evaluate(Level);
            moveSpeed = speedCurve.Evaluate(Level);
        }

        if (randomnizeShootingParams)
            RandomnizeShootingParameters();
    }

    private EnemyProjectile CreateProjectile()
    {
        EnemyProjectile projectileInstance = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        projectileInstance.EnemyProjectilePool = enemyProjectilePool;
        return projectileInstance;
    }

    private void OnGetFromPool(EnemyProjectile projectile)
    {
        projectile.gameObject.SetActive(true);
    }

    private void OnReleaseToPool(EnemyProjectile projectile)
    {
        projectile.gameObject.SetActive(false);
    }

    private void OnDestroyPooledObject(EnemyProjectile projectile)
    {
        Destroy(projectile.gameObject);
    }

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

    public void Shoot()
    {
        if (!isShooting)
        {
            StartCoroutine(ShootBurstRoutine());
        }
    }

    private IEnumerator ShootBurstRoutine()
    {
        if (!projectilePrefab)
            yield return null;

        isShooting = true;

        float timeBetweenProjectiles = 0f;
        if(stagger){ timeBetweenProjectiles = timeBetweenBursts / projectilePerBurst; }

        // initialize something
        float startAngle, currAngle, angleStep, endAngle;
        SetTargetCone(out startAngle, out currAngle, out angleStep, out endAngle);

        for (int burst = 0; burst < burstCount; ++burst)
        {
            if (!oscillate)
            {
                SetTargetCone(out startAngle, out currAngle, out angleStep, out endAngle);
            }
            else
            {
                currAngle = endAngle;
                endAngle = startAngle;
                startAngle = currAngle;
                angleStep *= -1;
            }

            if (!stagger)
            {
                PlayProjectileSound();
            }

            for (int projectile = 0; projectile < projectilePerBurst; projectile++)
            {
                EnemyProjectile enemyProjectile = enemyProjectilePool.Get();

                if (enemyProjectile)
                {
                    enemyProjectile.transform.SetPositionAndRotation(transform.position, Quaternion.identity);
                    enemyProjectile.InitializeProjectile(damage: damageCurve.Evaluate(Level));
                    enemyProjectile.Deactivate();

                    if (stagger)
                    {
                        PlayProjectileSound();
                    }
                }

                // convert angle to vector2 direction
                float rad = currAngle * Mathf.Deg2Rad;
                Vector2 currDir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

                if(enemyProjectile.TryGetComponent<Rigidbody2D>(out Rigidbody2D projectileRB))
                {
                    projectileRB.linearVelocity = currDir * 1f;
                }

                // update angle for next projectile
                currAngle += angleStep;


                if (stagger) yield return new WaitForSeconds(timeBetweenProjectiles);
            }

            // reset current angle to start angle in between bursts
            currAngle = startAngle;
            yield return new WaitForSeconds(timeBetweenBursts);
        }



        yield return new WaitForSeconds(coolDown);
        isShooting = false;
    }

    private void SetTargetCone(out float startAngle, out float currAngle, out float angleStep, out float endAngle)
    {
        Vector3 dir = player.transform.position - transform.position;
        dir.Normalize();

        float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        startAngle = endAngle = currAngle = targetAngle;
        angleStep = 0f;
        if (angleSpread != 0)
        {
            angleStep = angleSpread / (projectilePerBurst - 1f);

            float halfAngleSpread = angleSpread * 0.5f;
            startAngle = targetAngle - halfAngleSpread;
            endAngle = targetAngle + halfAngleSpread;
            currAngle = startAngle;
        }

        // for debug
        aimDirection = dir;
    }


    private void PlayProjectileSound()
    {

        if (SoundManager.Instance && SoundLibrary.Instance)
        {
            SoundManager.Instance.PlaySoundEffect(
                    SoundLibrary.Instance.GetAudioClip(SoundLibrary.Spells.FIREBALL_1)
                );
        }
    }

    private void OnDrawGizmos()
    {
        if (aimDirection != Vector3.zero)
        {
            Debug.DrawLine(transform.position, transform.position + aimDirection * 3f, Color.red);
        }
    }
}
