using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;

public class RandomRoomPlacement
{
    public static bool GenerateRooms(DungeonLayout layout, Grid grid, GameObject roomsGO, out Dictionary<DungeonRoom, Tilemap> placedRooms, int maxPlacementFailCount = 5, int maxPrefabFailCount = 5)
    {
        // to store placed rooms
        // List<Tilemap> placedRooms = new List<Tilemap>();
        placedRooms = new Dictionary<DungeonRoom, Tilemap>();

        foreach(DungeonRoom room in layout.dungeonRoomList)
        {
            if (room.roomType.prefabs == null || room.roomType.prefabs.Count == 0) 
                continue;

            // create a temp prefab list so can remove and retry if failed to place the prefab room
            List<int> prefabIndexList = Enumerable.Range(0, room.roomType.prefabs.Count).ToList();

            int prefabFailCounter = 0;

            while(true)
            {
                int prefabIndex = Random.Range(0, prefabIndexList.Count);
                
                GameObject prefab = room.roomType.prefabs[prefabIndexList[prefabIndex]];

                // if room placed, stop loop, else increment fail counter
                if (!PlaceRoom(prefab, grid, roomsGO, layout, room, placedRooms, maxPlacementFailCount))
                {
                    // remove prefab from list
                    prefabIndexList.Remove(prefabIndex);
                    prefabFailCounter++;
                }
                else break;

                // stop generation if failed too many times or no more prefabs
                if (prefabFailCounter > maxPrefabFailCount || prefabIndexList.Count == 0)
                {
                    // remove all rooms
                    int childCount = roomsGO.transform.childCount;
                    for (int i = 0; i < childCount; ++i)
                    {
                        Object.Destroy(roomsGO.transform.GetChild(i).gameObject);
                    }
                    Debug.Log("Room Generation Failed");
                    return false;
                }
            }
        }
        Debug.Log("Room Generation Succeeded");
        return true;
    }

    public static bool PlaceRoom(GameObject prefab, Grid grid, GameObject roomsGO, DungeonLayout layout, DungeonRoom room, Dictionary<DungeonRoom, Tilemap> placedRooms, int maxFailCount)
    {
        // get the ground tilemap
        Transform groundT = prefab.transform.Find("Ground");
        Tilemap tMap = groundT.GetComponentInChildren<Tilemap>();

        // get width, height of prefab
        Vector2 dimensions = TilemapHelper.GetDimensions(tMap);

        // move to next room if this prefab dimensions are zero
        if (dimensions == Vector2.zero)
            return false;

        // get the half scale of the prefab
        int xOffset = (int)dimensions.x / 2;
        int yOffset = (int)dimensions.y / 2;


        int failCounter = 0;
        while (true)
        {
            int randomX = Random.Range(xOffset, layout.width - xOffset - 1);
            int randomY = Random.Range(yOffset, layout.height - yOffset - 1);

            Vector3 position = grid.CellToWorld(new Vector3Int(randomX, randomY, 0));
            GameObject roomObject = GameObject.Instantiate(prefab, position, Quaternion.identity, roomsGO.transform);

            // get the ground tilemap
            Transform roomT = roomObject.transform.Find("Ground");
            Tilemap roomMap = roomT.GetComponentInChildren<Tilemap>();


            // check for overlap with existing rooms or border of dungeon
            if (!TilemapHelper.CheckOverlap(roomMap, placedRooms, layout.minGapBetweenRooms))
            {
                // instantiate in that position
                //Vector3 position = new Vector3(randomX, randomY, 0);
                // Vector3Int cellPosition = grid.WorldToCell(position);
                placedRooms.Add(room, roomMap);
                return true;
            }
            else
            {
                GameObject.Destroy(roomObject);
                // if overlap, retry, after certain amount of fails, try different prefab
                failCounter++;
            }

            // stop the loop
            if (failCounter >= maxFailCount)
                return false;
        }
    }


}
