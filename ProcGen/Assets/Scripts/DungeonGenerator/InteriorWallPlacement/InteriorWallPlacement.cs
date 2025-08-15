using UnityEngine;
using System.Collections.Generic;

public class InteriorWallPlacement : MonoBehaviour
{
    public static bool PlaceInteriorWalls(Dictionary<DungeonRoom, DungeonRoomInstance> roomsDict, InteriorWallSet interiorWallSet)
    {
        if (roomsDict == null || roomsDict.Count == 0 || interiorWallSet == null)
        {
            Debug.Log("Failed to place interior walls");
            return false;
        }

        // iterate through each room
        foreach (var kvp in roomsDict)
        {
            DungeonRoomInstance dungeonRoomInstance = kvp.Value;

            if (dungeonRoomInstance == null || dungeonRoomInstance.instance == null)
                continue;

            GameObject room = dungeonRoomInstance.instance;

            // get the propspawn tilemap
            InteriorWallSpawnArea interiorWallSpawnArea = room.GetComponentInChildren<InteriorWallSpawnArea>();
            if (interiorWallSpawnArea != null)
            {
                interiorWallSpawnArea.PlaceInteriorWalls(interiorWallSet);
            }
        }

        return true;
    }
}
