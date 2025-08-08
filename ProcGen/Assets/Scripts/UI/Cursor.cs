using UnityEngine;
using UnityEngine.UI;
public class Cursor : MonoBehaviour
{
    [SerializeField] Image cursorImage;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UnityEngine.Cursor.visible = false;
#if UNITY_EDITOR
        UnityEngine.Cursor.visible = true;
#endif
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Input.mousePosition;
    }
}
