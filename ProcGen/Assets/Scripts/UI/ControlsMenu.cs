using UnityEngine;
using System;
public class ControlsMenu : MonoBehaviour
{
    public static Action OnBack; 

    public void OnExitClicked()
    {
        gameObject.SetActive(false);
        OnBack?.Invoke();
    }
}
