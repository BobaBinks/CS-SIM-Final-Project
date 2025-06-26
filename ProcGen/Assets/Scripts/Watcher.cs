using UnityEngine;

public class Watcher : MonoBehaviour
{
    void OnDestroy()
    {
        Debug.Log($"{gameObject.name} was destroyed!");
    }
}
