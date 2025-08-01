using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class XPDrops : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Player player = collision.GetComponent<Player>();

            if(player && player.LevelXPCurve != null)
            {
                float xpReward = player.LevelXPCurve.Evaluate(player.Level) * 0.15f;
                xpReward = Mathf.Max(xpReward, 0f);
                player.GainXP(xpReward);
            }
            Destroy(this.gameObject);
        }
    }
}
