using System;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class SpawnBoss : MonoBehaviour
{
    public static event Action<GameObject> OnPlayerEnterBossRoom;
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
            OnPlayerEnterBossRoom?.Invoke(this.gameObject);
            boxCollider2D.enabled = false;
            
            if(SoundManager.Instance && SoundLibrary.Instance)
            {
                SoundManager.Instance.PlayMusic(SoundLibrary.Instance.GetAudioClip(SoundLibrary.Music.BOSS));
            }
        }
    }
}
