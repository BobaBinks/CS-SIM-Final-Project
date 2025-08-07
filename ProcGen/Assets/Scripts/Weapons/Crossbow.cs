using System.Collections;
using UnityEngine;

public class Crossbow : BaseWeapon
{
    [SerializeField] GameObject arrowPrefab;
    [SerializeField] Transform arrowSpawnPoint;
    [SerializeField] float arrowForce = 5f;
    [SerializeField] float arrowLifetime = 5f;
    [SerializeField] float attackCooldown = 1f;

    bool onCooldown = false;


    public override void Fire()
    {
        if (onCooldown) return;

        if(arrowPrefab && arrowSpawnPoint)
        {
            GameObject arrowGO = Instantiate(arrowPrefab, arrowSpawnPoint.position, transform.parent.rotation);

            Rigidbody2D rb = arrowGO.GetComponent<Rigidbody2D>();
            Arrow arrow = arrowGO.GetComponent<Arrow>();

            float damage = damageCurve.Evaluate(Level);
            arrow.InitializeArrow(damage, arrowLifetime);


            if (rb)
            {
                rb.AddForce(transform.parent.right * arrowForce, ForceMode2D.Impulse);
            }
            StartCoroutine(StartCooldown(attackCooldown));
            Debug.Log("Crossbow fired arrow");
        }
    }

    IEnumerator StartCooldown(float cooldown)
    {
        if (float.IsNaN(cooldown) || float.IsNegative(cooldown))
            cooldown = 1f;

        onCooldown = true;
        yield return new WaitForSeconds(cooldown);
        onCooldown = false;
    }
}
