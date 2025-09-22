using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(fileName = "RoomTypes", menuName = "Scriptable Objects/RoomTypes")]
public class RoomTypes : ScriptableObject
{
    public List<GameObject> prefabs; 

    public GameObject GetRoomRandomPrefab()
    {
        if (prefabs == null || prefabs.Count == 0)
        {
            Debug.Log("Could not get random prefab");
            return null;
        }

        int prefabIndex = Random.Range(0, prefabs.Count);

        return prefabs[prefabIndex];
    }
}
