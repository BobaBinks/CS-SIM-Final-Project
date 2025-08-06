using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
public class EndGameTransitionMagicCircle : MonoBehaviour
{
    Collider2D collider;
    private void Start()
    {
        collider = GetComponent<Collider2D>();
        collider.isTrigger = true;
        Boss.OnBossDie += OnDie;
        gameObject.SetActive(false);
        collider.enabled = false;
    }

    private void OnDie()
    {
        gameObject.SetActive(true);
        collider.enabled = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            SceneManager.LoadScene("Menu");
        }
    }
}
