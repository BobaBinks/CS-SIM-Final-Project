using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
public class EndGameTransitionMagicCircle : MonoBehaviour
{
    private void Start()
    {
        Boss.OnBossDie += OnDie;
        gameObject.SetActive(false);
    }

    private void OnDie()
    {
        gameObject.SetActive(true);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            SceneManager.LoadScene("Menu");
        }
    }
}
