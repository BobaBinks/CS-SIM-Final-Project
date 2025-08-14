using UnityEngine;
using System.Collections.Generic;

public class InteriorWallPlacement : MonoBehaviour
{
    public static bool PlaceInteriorWalls(Dictionary<DungeonRoom, DungeonRoomInstance> roomsDict, List<GameObject> propPrefabs)
    {
        if (roomsDict == null || roomsDict.Count == 0)
        {
            Debug.Log("Failed to place interior walls");
            return false;
        }

        return true;
    }
}
