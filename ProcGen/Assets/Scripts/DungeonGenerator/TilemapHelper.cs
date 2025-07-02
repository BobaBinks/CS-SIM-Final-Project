using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

public class TilemapHelper
{
    public enum Edge
    {
        LEFT,
        RIGHT,
        TOP,
        BOTTOM,
        INVALID
    }

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

    public static bool PickEdgeCell(Tilemap groundMap, Edge edge, out Vector3Int cellPosition)
    {
        Edge[] validEdges = { Edge.LEFT, Edge.RIGHT, Edge.BOTTOM, Edge.TOP };
        cellPosition = Vector3Int.zero;

        if (groundMap == null || !validEdges.Contains(edge))
            return false;

        groundMap.CompressBounds();

        // pick a point at the edge of room
        switch (edge)
        {
            case Edge.LEFT:
                {
                    // tilemap bounds will be 2 less cuz we using the ground tilemap so walls not included
                    int y = Random.Range(groundMap.cellBounds.yMin, groundMap.cellBounds.yMax);
                    cellPosition = new Vector3Int(groundMap.cellBounds.xMin, y);
                    return true;
                }
            case Edge.RIGHT:
                {
                    // tilemap bounds will be 2 less cuz we using the ground tilemap so walls not included
                    int y = Random.Range(groundMap.cellBounds.yMin, groundMap.cellBounds.yMax);
                    cellPosition = new Vector3Int(groundMap.cellBounds.xMax - 1, y);
                    return true;
                }
            case Edge.TOP:
                {
                    // tilemap bounds will be 2 less cuz we using the ground tilemap so walls not included
                    int x = Random.Range(groundMap.cellBounds.xMin, groundMap.cellBounds.xMax);
                    cellPosition = new Vector3Int(x, groundMap.cellBounds.yMax - 1);
                    return true;
                }
            case Edge.BOTTOM:
                {
                    // tilemap bounds will be 2 less cuz we using the ground tilemap so walls not included
                    int x = Random.Range(groundMap.cellBounds.xMin, groundMap.cellBounds.xMax);
                    cellPosition = new Vector3Int(x, groundMap.cellBounds.yMin);
                    return true;
                }

            default: return false;
        }
    }

    public static Vector3 RealignPrefab(Vector3 position,Grid grid) 
    {
        Vector3Int cellPosition = grid.WorldToCell(position);
        return grid.CellToWorld(cellPosition);
    }
}
