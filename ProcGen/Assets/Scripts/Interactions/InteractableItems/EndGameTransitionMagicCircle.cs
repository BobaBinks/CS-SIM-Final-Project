using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
public class EndGameTransitionMagicCircle : MonoBehaviour
{
    Collider2D collider;
    private void Awake()
    {
        collider = GetComponent<Collider2D>();
        collider.isTrigger = true;
        Boss.OnBossDie += OnDie;
        collider.enabled = false;
    }

    private void OnDie()
    {
        collider.enabled = true;
    }

    private void OnDestroy()
    {
        Boss.OnBossDie -= OnDie;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            //SceneManager.LoadScene("Menu");
            GameManager.Instance.CreateGameLevel();
            if (SoundManager.Instance)
            {
                SoundManager.Instance.PlaySoundEffect(SoundManager.SoundEffects.TELEPORT);
            }
        }
    }
}
