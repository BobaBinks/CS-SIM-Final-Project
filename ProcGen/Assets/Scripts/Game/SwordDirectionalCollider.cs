using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(BoxCollider2D))]
public class SwordDirectionalCollider : MonoBehaviour
{
    BoxCollider2D boxCollider;

    List<CharacterBase> charactersInCollider;
    void Start()
    {
        charactersInCollider = new List<CharacterBase>();
        boxCollider = GetComponent<BoxCollider2D>();
        boxCollider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemies"))
        {
            CharacterBase character = collision.GetComponent<CharacterBase>();
            if (character != null && !charactersInCollider.Contains(character))
            {
                charactersInCollider.Add(character);
                // Debug.Log($"{character.name} entered sword collider.");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemies"))
        {
            CharacterBase character = collision.GetComponent<CharacterBase>();
            if (character != null && charactersInCollider.Contains(character))
            {
                charactersInCollider.Remove(character);
                Debug.Log($"{character.name} exited sword collider.");
            }
        }
    }

    public void DealDamage(float damage)
    {
        foreach (CharacterBase character in charactersInCollider)
        {
            character.TakeDamage(damage);
            Debug.Log($"{character.name} HP: {character.HealthPoints}");
        }
    }
}
