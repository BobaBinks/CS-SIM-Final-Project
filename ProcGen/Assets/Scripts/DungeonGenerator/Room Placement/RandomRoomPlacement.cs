using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;

public class RandomRoomPlacement
{
    public static bool GenerateRooms(DungeonLayout layout, Transform grid, int maxPlacementFailCount = 5, int maxPrefabFailCount = 5)
    {
        // to store placed rooms
        List<Tilemap> placedRooms = new List<Tilemap>();

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
                if (!PlaceRoom(prefab, grid, layout, placedRooms, maxPlacementFailCount))
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
                    int childCount = grid.childCount;
                    for (int i = 0; i < childCount; ++i)
                    {
                        Object.Destroy(grid.GetChild(i).gameObject);
                    }
                    Debug.Log("Room Generation Failed");
                    return false;
                }
            }
        }
        Debug.Log("Room Generation Succeeded");
        return true;
    }

    public static bool PlaceRoom(GameObject prefab, Transform grid,DungeonLayout layout, List<Tilemap> placedRooms, int maxFailCount)
    {
        // get the ground tilemap
        Transform groundT = prefab.transform.Find("Ground");
        Tilemap tMap = groundT.GetComponentInChildren<Tilemap>();

        // get width, height of prefab
        Vector2 dimensions = GetDimensions(tMap);

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

            // check for overlap with existing rooms or border of dungeon
            if (!CheckOverlap(new Vector2(randomX, randomY), dimensions, placedRooms, layout.minGapBetweenRooms))
            {
                // instantiate in that position
                Vector3 position = new Vector3(randomX, randomY, 0);
                GameObject roomObject = GameObject.Instantiate(prefab, position, Quaternion.identity, grid);

                // get the ground tilemap to add to existing room list
                Transform roomT = roomObject.transform.Find("Ground");
                Tilemap roomMap = roomT.GetComponentInChildren<Tilemap>();
                placedRooms.Add(roomMap);
                return true;
            }
            else
            {
                // if overlap, retry, after certain amount of fails, try different prefab
                failCounter++;
            }

            // stop the loop
            if (failCounter >= maxFailCount)
                return false;
        }
    }

    public static bool CheckOverlap(Vector2 roomPosition, Vector2 room1Dimensions, List<Tilemap> tileMaps, int minGapBetweenRooms)
    {
        if (tileMaps == null || room1Dimensions == null || room1Dimensions == Vector2.zero)
            return false;

        foreach(Tilemap map in tileMaps)
        {
            // get the bound width and height
            Vector2 room2Dimensions = GetDimensions(map);

            if(room2Dimensions == Vector2.zero)
                continue;

            // check if position overlaps a room
            if (InRange(roomPosition, map.transform, room1Dimensions , room2Dimensions, minGapBetweenRooms))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Checks if a position is overlapping with a room
    /// </summary>
    /// <param name="xPos"></param>
    /// <param name="yPos"></param>
    /// <param name="room"></param>
    /// <param name="dimension"></param>
    /// <returns></returns>
    public static bool InRange(Vector2 room1, Transform room2, Vector2 room1Dimension , Vector2 room2Dimension, int minGapBetweenRooms)
    {
        if (room1 == null || room2 == null ||room2Dimension == null || room2Dimension == Vector2.zero)
            return true;

        // left box
        Vector2 position1 = room1.x < room2.position.x ? room1 : room2.position;

        // right box
        Vector2 position2 = room1.x > room2.position.x ? room1 : room2.position;

        // divides the dimension into half to calculate the range that the room occupies based off it's center position
        Vector2 offset1 = position1 == room1 ? room1Dimension / 2 : room2Dimension / 2;

        Vector2 offset2 = position2 == room1 ? room1Dimension / 2 : room2Dimension / 2;

        offset1 += Vector2.one * minGapBetweenRooms;
        offset2 += Vector2.one * minGapBetweenRooms;


        return position1.x - offset1.x <= position2.x + offset2.x &&
           position1.x + offset1.x >= position2.x - offset2.x &&
           position1.y - offset1.y <= position2.y + offset2.y &&
           position1.y + offset1.y >= position2.y - offset2.y;
    }

    public static Vector2 GetDimensions(Tilemap map)
    {
        if (map == null) return Vector2.zero;
        map.CompressBounds();
        return new Vector2(map.cellBounds.size.x, map.cellBounds.size.y);
    }
}
