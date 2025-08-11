using UnityEngine;

public class FloatingText : MonoBehaviour
{
    [SerializeField] float lifeTime = 10f;

    private void Start()
    {
        Destroy(gameObject, Mathf.Max(1f, lifeTime));
    }
}
