using UnityEngine;

public class RotateWeapon : MonoBehaviour
{
    [SerializeField] SpriteRenderer childWeaponSpriteRenderer;

    void Update()
    {
        RotateTowardsMouse();
    }

    private void RotateTowardsMouse()
    {
        Vector3 mouseScreenPos = Input.mousePosition;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = 0f;

        transform.right = (mouseWorldPos - this.transform.position).normalized;
    }
}
