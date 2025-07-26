using System;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider2D))]
public class SpawnEnemiesInNextDepth : MonoBehaviour
{
    public static event Action<GameObject> OnPlayerEnter;
    private BoxCollider2D boxCollider2D;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        boxCollider2D = GetComponent<BoxCollider2D>();
        boxCollider2D.isTrigger = true;

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // notify enemy spawn manager which room player has entered
            OnPlayerEnter?.Invoke(this.gameObject);
            boxCollider2D.enabled = false;
            // make sure to account which rooms already has spawned enemies
            // disable the collider
        }
    }
}
