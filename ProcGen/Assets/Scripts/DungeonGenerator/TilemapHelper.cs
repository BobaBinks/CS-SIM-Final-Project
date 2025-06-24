using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

public class TilemapHelper
{
    public static bool CheckOverlap(Tilemap room1Tilemap, Dictionary<DungeonRoom, Tilemap> otherTileMaps, int minGapBetweenRooms)
    {
        if (otherTileMaps == null || room1Tilemap == null)
            return false;

        foreach (KeyValuePair<DungeonRoom, Tilemap> kvp in otherTileMaps)
        {
            // check if position overlaps a room
            if (InRange(room1Tilemap, kvp.Value, minGapBetweenRooms))
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
    public static bool InRange(Tilemap room1Map, Tilemap room2Map, int minGapBetweenRooms)
    {
        if (room1Map == null || room2Map == null)
            return true;

        // compress bounds
        room1Map.CompressBounds();
        room2Map.CompressBounds();
            
        // Get padded bounds
        Bounds room1Bounds = GetRoomWorldBounds(room1Map, minGapBetweenRooms);
        Bounds room2Bounds = GetRoomWorldBounds(room2Map, minGapBetweenRooms);

        return room1Bounds.Intersects(room2Bounds);
    }

    private static Bounds GetRoomWorldBounds(Tilemap map, int padding)
    {
        BoundsInt bounds = map.cellBounds;

        // Pad the bounds
        bounds.xMin -= padding;
        bounds.xMax += padding;
        bounds.yMin -= padding;
        bounds.yMax += padding;

        // Convert to world positions
        Vector3 worldBoundsMin = map.CellToWorld(bounds.min);
        Vector3 worldBoundsMax = map.CellToWorld(bounds.max);

        // Create a Unity Bounds object
        Bounds worldBounds = new Bounds();
        worldBounds.SetMinMax(worldBoundsMin, worldBoundsMax);

        return worldBounds;
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
    public static Dictionary<string, Vector3> GetEdgePositions(Tilemap tileMap)
    {
        tileMap.CompressBounds();

        BoundsInt bounds = tileMap.cellBounds;

        return new Dictionary<string, Vector3>
        {
            { "LEFT", tileMap.CellToWorld(new Vector3Int(bounds.xMin,0)) },
            { "RIGHT", tileMap.CellToWorld(new Vector3Int(bounds.xMax,0)) },
            { "TOP", tileMap.CellToWorld(new Vector3Int(0,bounds.yMax)) },
            { "BOTTOM", tileMap.CellToWorld(new Vector3Int(0,bounds.yMin)) },
        };

        //return new Dictionary<string, Vector3>
        //{
        //    { "LEFT", new Vector3(bounds.xMin,bounds.center.y) },
        //    { "RIGHT", new Vector3(bounds.xMax,bounds.center.y) },
        //    { "TOP", new Vector3(bounds.center.x,bounds.yMax) },
        //    { "BOTTOM", new Vector3(bounds.center.x,bounds.yMin) },
        //};
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

    public static Vector3 RealignPrefab(Vector3 position,Grid grid) 
    {
        Vector3Int cellPosition = grid.WorldToCell(position);
        return grid.CellToWorld(cellPosition);
    }
}
