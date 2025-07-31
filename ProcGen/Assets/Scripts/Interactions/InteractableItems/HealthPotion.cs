using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HealthPotion : MonoBehaviour
{
    [SerializeField] float heal = 50f;


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            IHealable player = collision.GetComponent<IHealable>();
            float healAmount = Mathf.Max(heal, 0f);
            player.Heal(healAmount);
            Destroy(this.gameObject);
        }
    }
}
