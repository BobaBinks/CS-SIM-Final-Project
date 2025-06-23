using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

public class TilemapHelper
{
    public static bool CheckOverlap(Vector2 roomPosition, Vector2 room1Dimensions, Dictionary<DungeonRoom, Tilemap> tileMaps, int minGapBetweenRooms)
    {
        if (tileMaps == null || room1Dimensions == null || room1Dimensions == Vector2.zero)
            return false;

        foreach (KeyValuePair<DungeonRoom, Tilemap> kvp in tileMaps)
        {
            // get the bound width and height
            Vector2 room2Dimensions = GetDimensions(kvp.Value);

            if (room2Dimensions == Vector2.zero)
                continue;

            // check if position overlaps a room
            if (InRange(roomPosition, kvp.Value.transform, room1Dimensions, room2Dimensions, minGapBetweenRooms))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Checks if a room1 and room2 overlap
    /// </summary>
    /// <param name="xPos"></param>
    /// <param name="yPos"></param>
    /// <param name="room"></param>
    /// <param name="dimension"></param>
    /// <returns></returns>
    public static bool InRange(Vector2 room1, Transform room2, Vector2 room1Dimension, Vector2 room2Dimension, int minGapBetweenRooms)
    {
        if (room1 == null || room2 == null || room2Dimension == null || room2Dimension == Vector2.zero)
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

        for (int x = map.cellBounds.xMin; x < map.cellBounds.xMax; x++)
        {
            for (int y = map.cellBounds.yMin; y < map.cellBounds.yMax; y++)
            {
                if (!map.HasTile(new Vector3Int(x, y, 0)))
                {
                    Debug.Log($"Tile not found at: ({x}, {y})");
                }
            }
        }

        return new Vector2(map.cellBounds.size.x, map.cellBounds.size.y);
    }

    /// <summary>
    /// Calculates the position of the edges of a room.
    /// </summary>
    /// <param name="roomPosition"></param>
    /// <param name="dimesions"></param>
    /// <returns></returns>
    public static Dictionary<string, Vector3> GetEdgePositions(Tilemap tileMap/*Vector3 roomPosition, Vector3 dimesions*/)
    {
        //Vector3 leftEdgePosition = roomPosition - new Vector3(dimesions.x, 0, 0);
        //Vector3 rightEdgePosition = roomPosition + new Vector3(dimesions.x, 0, 0);
        //Vector3 topEdgePosition = roomPosition + new Vector3(0, dimesions.y, 0);
        //Vector3 bottomEdgePosition = roomPosition - new Vector3(0, dimesions.y, 0);

        tileMap.CompressBounds();

        BoundsInt bounds = tileMap.cellBounds;

        return new Dictionary<string, Vector3>
        {
            { "LEFT", new Vector3(bounds.xMin,bounds.center.y) },
            { "RIGHT", new Vector3(bounds.xMax,bounds.center.y) },
            { "TOP", new Vector3(bounds.center.x,bounds.yMax) },
            { "BOTTOM", new Vector3(bounds.center.x,bounds.yMin) },
        };
    }

    /// <summary>
    /// Calculates distances between edges of room1 and center of room2.
    /// </summary>
    /// <param name="room2Position"></param>
    /// <param name="edgePositions"></param>
    /// <returns></returns>
    public static Dictionary<string,float> GetEdgeDistances(Vector3 room2Position, Dictionary<string, Vector3> room1EdgePositions)
    {
        string[] requiredKeys = { "LEFT", "RIGHT", "TOP", "BOTTOM" };

        // https://learn.microsoft.com/en-us/dotnet/api/system.linq.enumerable.all?view=net-9.0
        // iterates through each element in requiredKeys and tests if edgePosition contains it using the predicate provided.
        // returns true if all elements passes the test
        if (!requiredKeys.All(room1EdgePositions.ContainsKey))
        {
            return new Dictionary<string, float>();
        }

        // a dictionary containing the edge and the distance between it and room2
        return new Dictionary<string, float>
        {
            { "LEFT", (room2Position - room1EdgePositions["LEFT"]).magnitude },
            { "RIGHT", (room2Position - room1EdgePositions["RIGHT"]).magnitude },
            { "TOP", (room2Position - room1EdgePositions["TOP"]).magnitude },
            { "BOTTOM", (room2Position - room1EdgePositions["BOTTOM"]).magnitude },
        };
    }
}
