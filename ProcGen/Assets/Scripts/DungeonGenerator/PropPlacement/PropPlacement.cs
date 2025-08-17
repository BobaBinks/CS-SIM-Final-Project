using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
public class PropPlacement
{
    public static bool PlaceProps(Dictionary<DungeonRoom, DungeonRoomInstance> roomsDict, List<GameObject> propPrefabs, int maxAttempts = 5)
    {
        if (roomsDict == null || roomsDict.Count == 0 || propPrefabs == null || propPrefabs.Count == 0)
        {
            Debug.Log("Failed to place props");
            return false;
        }

        // iterate through each room
        foreach(var kvp in roomsDict)
        {
            DungeonRoomInstance dungeonRoomInstance = kvp.Value;

            if (dungeonRoomInstance == null || dungeonRoomInstance.instance == null)
                continue;

            GameObject room = dungeonRoomInstance.instance;

            // get the propspawn tilemap
            PropSpawnArea propSpawnArea = room.GetComponentInChildren<PropSpawnArea>();
            if (propSpawnArea != null)
            {
                propSpawnArea.SpawnProps(propPrefabs, maxAttempts: maxAttempts);
            }
        }

        return true;
    }

}
