using System.Collections;
using UnityEngine;

public class Crossbow : BaseWeapon
{
    [SerializeField] GameObject arrowPrefab;
    [SerializeField] Transform arrowSpawnPoint;
    [SerializeField] float arrowForce = 5f;
    [SerializeField] float arrowLifetime = 5f;
    [SerializeField] float attackCooldown = 1f;

    private float nextFireTime = 0f;


    public override void Fire()
    {
        if (Time.time < nextFireTime) return;

        if(arrowPrefab && arrowSpawnPoint)
        {
            GameObject arrowGO = Instantiate(arrowPrefab, arrowSpawnPoint.position, transform.parent.rotation);

            Rigidbody2D rb = arrowGO.GetComponent<Rigidbody2D>();
            Arrow arrow = arrowGO.GetComponent<Arrow>();

            float damage = damageCurve.Evaluate(Level);
            arrow.InitializeProjectile(damage, arrowLifetime);

            if (SoundManager.Instance && SoundLibrary.Instance)
                SoundManager.Instance.PlaySoundEffect(
                    SoundLibrary.Instance.GetAudioClip(SoundLibrary.Player.CROSSBOW_SFX));

            if (rb)
            {
                rb.AddForce(transform.parent.right * arrowForce, ForceMode2D.Impulse);
            }

            nextFireTime = Time.time + attackCooldown;
            
            Debug.Log("Crossbow fired arrow");
        }
    }
}
