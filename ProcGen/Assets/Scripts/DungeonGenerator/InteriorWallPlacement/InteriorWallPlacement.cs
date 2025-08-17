using UnityEngine;
using System.Collections.Generic;

public class InteriorWallPlacement : MonoBehaviour
{
    public static bool PlaceInteriorWalls(Dictionary<DungeonRoom, DungeonRoomInstance> roomsDict, InteriorWallSet interiorWallSet,
                int maxTurns = 2,
                int minSteps = 2,
                int maxSteps = 5,
                int maxAttempts = 5)
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
                interiorWallSpawnArea.PlaceInteriorWalls(interiorWallSet,
                                                        maxTurns,
                                                        minSteps,
                                                        maxSteps,
                                                        maxAttempts);
            }
        }

        return true;
    }
}
