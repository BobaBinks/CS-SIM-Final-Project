using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;

[RequireComponent(typeof(SpriteRenderer), typeof(Collider))]
public abstract class CharacterBase : MonoBehaviour, IDamagable
{
    [SerializeField] protected float maxHealthPoints = 100f;
    [SerializeField] protected float moveSpeed = 3.5f;
    [SerializeField] protected int level = 0;
    [SerializeField] protected GameObject floatingTextPrefab;


    public int Level => level;
    public float MaxHealthPoints => maxHealthPoints;
    public float MoveSpeed => moveSpeed;
    public float HealthPoints { get; protected set; }

    private SpriteRenderer spriteRenderer;
    public Collider2D collider;

    protected virtual void Awake()
    {
        HealthPoints = maxHealthPoints;
        spriteRenderer = GetComponent<SpriteRenderer>();
        collider = GetComponent<Collider2D>();
    }

    public virtual void Die()
    {
        Debug.Log($"{gameObject.name} died.");
    }

    public virtual void TakeDamage(float damage)
    {
        if (HealthPoints <= 0)
        {
            Debug.Log($"{name} Died");
        }
        else if (spriteRenderer != null)
        {
            StartCoroutine(SpriteFlashRedThenFadeToWhite(0.1f, 0.1f));
        }
        HealthPoints -= damage;
    }

    public void InstantiateFloatingText(string text, Transform parentTransform, bool offset = false)
    {
        Vector3 position = parentTransform.position;

        if (offset)
        {
            float xOffset = Random.Range(-0.05f, 0.06f);
            float yOffset = Random.Range(-0.02f, 0.03f);
            position.x += xOffset;
            position.y += yOffset;
        }

        GameObject textGO = Instantiate(floatingTextPrefab, position, Quaternion.identity, parentTransform);
        if (textGO)
        {
            TextMeshProUGUI tmPro = textGO.GetComponent<TextMeshProUGUI>();
            if (tmPro)
                tmPro.text = text;
        }
    }

    public IEnumerator SpriteFlashRedThenFadeToWhite(float flashTime, float fadeRate)
    {
        spriteRenderer.material.color = Color.red;
        if (flashTime <= 0)
            flashTime = 1;
        yield return new WaitForSeconds(flashTime);

        spriteRenderer.material.color = Color.white;
    }

    public IEnumerator FadeSpriteToWhite(float rate)
    {
        if(spriteRenderer == null)
            yield break;

        Color currColor = spriteRenderer.color;

        while(currColor != Color.white)
        {
            float r = Mathf.MoveTowards(currColor.r, Color.white.r, rate * Time.deltaTime);
            float g = Mathf.MoveTowards(currColor.g, Color.white.g, rate * Time.deltaTime);
            float b = Mathf.MoveTowards(currColor.b, Color.white.b, rate * Time.deltaTime);

            spriteRenderer.color = new Color(r, g, b);

            yield return null;
        }
    }
}

