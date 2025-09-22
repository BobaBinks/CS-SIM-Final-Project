using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "PropsSet", menuName = "Scriptable Objects/PropsSet")]
public class PropsSet : ScriptableObject
{
    public List<GameObject> propsPrefab;
}
