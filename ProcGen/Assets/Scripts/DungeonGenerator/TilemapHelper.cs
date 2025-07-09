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

    public enum Axis
    {
        HORIZONTAL,
        VERTICAL,
        MAX_EXCLUSIVE,
    }

    public enum Corner
    {
        TOP_LEFT,
        TOP_RIGHT,
        BOTTOM_LEFT,
        BOTTOM_RIGHT,
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
        
        if (groundMap.cellBounds.size.x < 5 || groundMap.cellBounds.size.y < 5)
            return false;

        // pick a point at the edge of room
        switch (edge)
        {
            case Edge.LEFT:
                {
                    // tilemap bounds will be 2 less cuz we using the ground tilemap so walls not included
                    int y = Random.Range(groundMap.cellBounds.yMin + 2, groundMap.cellBounds.yMax - 2);
                    cellPosition = new Vector3Int(groundMap.cellBounds.xMin, y);
                    return true;
                }
            case Edge.RIGHT:
                {
                    // tilemap bounds will be 2 less cuz we using the ground tilemap so walls not included
                    int y = Random.Range(groundMap.cellBounds.yMin + 2, groundMap.cellBounds.yMax - 2);
                    cellPosition = new Vector3Int(groundMap.cellBounds.xMax - 1, y);
                    return true;
                }
            case Edge.TOP:
                {
                    // tilemap bounds will be 2 less cuz we using the ground tilemap so walls not included
                    int x = Random.Range(groundMap.cellBounds.xMin + 2, groundMap.cellBounds.xMax - 2);
                    cellPosition = new Vector3Int(x, groundMap.cellBounds.yMax - 1);
                    return true;
                }
            case Edge.BOTTOM:
                {
                    // tilemap bounds will be 2 less cuz we using the ground tilemap so walls not included
                    int x = Random.Range(groundMap.cellBounds.xMin + 2, groundMap.cellBounds.xMax - 2);
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

    /// <summary>
    /// To check for overlaps
    /// </summary>
    /// <param name="cellsToCheck"></param>
    /// <param name="occupiedCells"></param>
    /// <returns></returns>
    public static bool CheckOverlap(HashSet<Vector3Int> cellsToCheck, HashSet<Vector3Int> occupiedCells)
    {
        if (cellsToCheck == null || occupiedCells == null) return true;

        foreach(Vector3Int cell in cellsToCheck)
        {
            if (occupiedCells.Contains(cell))
                return true;
        }

        return false;
    }

    public static Axis GetEdgeAxis(Edge edge)
    {
        return (edge == Edge.LEFT || edge == Edge.RIGHT) ? Axis.VERTICAL : Axis.HORIZONTAL;
    }

    /// <summary>
    /// Return adjacent cells for the provided cell position based on the axis
    /// </summary>
    /// <param name="position"></param>
    /// <param name="axis"></param>
    /// <returns></returns>
    public static List<Vector3Int> GetAdjacentCells(Vector3Int position, Axis axis, int width = 3)
    {
        int halfWidth = width / 2;
        List<Vector3Int> cells = new List<Vector3Int>();

        for(int i = -halfWidth; i <= halfWidth; ++i)
        {
            if (i == 0)
                continue;

            Vector3Int offset = axis == Axis.VERTICAL ?
                new Vector3Int(0, i) :
                new Vector3Int(i, 0);

            cells.Add(position + offset);
        }

        return cells;
    }

    public static List<Vector3Int> GetThreeByThreeBlockTilePositions(Vector3Int positionInGrid)
    {
        // insert row by row starting from top left to bottom right
        List<Vector3Int> threeByThreeBlock = new List<Vector3Int>();

        for(int row = 1; row >= -1; --row)
        {
            threeByThreeBlock.Add(positionInGrid + new Vector3Int(-1, row));
            threeByThreeBlock.Add(positionInGrid + new Vector3Int(0, row));
            threeByThreeBlock.Add(positionInGrid + new Vector3Int(1, row));
        }

        return threeByThreeBlock;
    }

    public static bool PlaceWall(Vector3Int positionInGrid, Tilemap corridorWallTilemap, Tilemap corridorFloorTilemap, CorridorTiles corridorTiles)
    {
        if (corridorTiles == null || corridorFloorTilemap == null || corridorWallTilemap == null)
            return false;

        List<Vector3Int> neighbours = GetThreeByThreeBlockTilePositions(positionInGrid);

        // place wall based on up down left right
        bool hasTop = corridorFloorTilemap.GetTile(neighbours[1]) != null;
        bool hasBottom = corridorFloorTilemap.GetTile(neighbours[7]) != null;
        bool hasLeft = corridorFloorTilemap.GetTile(neighbours[3]) != null;
        bool hasRight = corridorFloorTilemap.GetTile(neighbours[5]) != null;

        // if no up down left right, check diagonal instead
        if (hasTop)
        {
            corridorWallTilemap.SetTile(positionInGrid, corridorTiles.BottomHorizontalWall);
        }
        else if (hasBottom)
        {
            corridorWallTilemap.SetTile(positionInGrid, corridorTiles.TopHorizontalWall);
        }
        else if (hasLeft)
        {
            corridorWallTilemap.SetTile(positionInGrid, corridorTiles.RightVerticalWall);
        }
        else if (hasRight)
        {
            corridorWallTilemap.SetTile(positionInGrid, corridorTiles.LeftVerticalWall);
        }
        return hasRight || hasLeft || hasBottom || hasTop;
    }

    public static bool PlaceCornerWall(Vector3Int positionInGrid, Tilemap corridorWallTilemap, Tilemap corridorFloorTilemap, CorridorTiles corridorTiles)
    {
        if (corridorTiles == null || corridorFloorTilemap == null || corridorWallTilemap == null)
            return false;

        List<Vector3Int> neighbours = GetThreeByThreeBlockTilePositions(positionInGrid);

        // add it to the corridorWallTilemap
        bool hasGroundTopLeft = corridorFloorTilemap.GetTile(neighbours[0]) != null;
        bool hasGroundTopRight = corridorFloorTilemap.GetTile(neighbours[2]) != null;
        bool hasGroundBottomLeft = corridorFloorTilemap.GetTile(neighbours[6]) != null;
        bool hasGroundBottomRight = corridorFloorTilemap.GetTile(neighbours[8]) != null;

        if (hasGroundTopLeft)
        {
            corridorWallTilemap.SetTile(positionInGrid, corridorTiles.BottomRightCornerWall);
        }
        else if (hasGroundTopRight)
        {
            corridorWallTilemap.SetTile(positionInGrid, corridorTiles.BottomLeftCornerWall);
        }
        else if (hasGroundBottomLeft)
        {
            corridorWallTilemap.SetTile(positionInGrid, corridorTiles.TopRightCornerWall);
        }
        else if (hasGroundBottomRight)
        {
            corridorWallTilemap.SetTile(positionInGrid, corridorTiles.TopLeftCornerWall);
        }
        return hasGroundTopLeft || hasGroundTopRight || hasGroundBottomLeft || hasGroundBottomRight;
    }

    public static bool PlaceTurningBottomCornerWall(Vector3Int positionInGrid, Tilemap corridorWallTilemap, Tilemap corridorFloorTilemap, CorridorTiles corridorTiles)
    {
        if (corridorTiles == null || corridorFloorTilemap == null || corridorWallTilemap == null)
            return false;

        List<Vector3Int> neighbours = GetThreeByThreeBlockTilePositions(positionInGrid);

        // place wall based on up down left right
        bool hasGroundTopRight = corridorFloorTilemap.GetTile(neighbours[2]) != null;
        bool hasGroundTopLeft = corridorFloorTilemap.GetTile(neighbours[0]) != null;
        bool hasWallBottom = corridorWallTilemap.GetTile(neighbours[7]) != null;
        bool hasWallLeft = corridorWallTilemap.GetTile(neighbours[3]) != null;
        bool hasWallRight = corridorWallTilemap.GetTile(neighbours[5]) != null;

        // if no up down left right, check diagonal instead
        if (hasWallBottom && (hasGroundTopLeft || hasGroundTopRight))
        {
            if (hasWallLeft)
            {
                corridorWallTilemap.SetTile(positionInGrid, corridorTiles.TurningBottomLeftCornerWall);
            }
            else if (hasWallRight)
            {
                corridorWallTilemap.SetTile(positionInGrid, corridorTiles.TurningBottomRightCornerWall);
            }
            else
            {
                return false;
            }
        }

        return true;
    }

    public static bool GetCombinedBounds(Dictionary<DungeonRoom, DungeonRoomInstance> roomsDict, Grid grid, out BoundsInt combinedBounds)
    {
        combinedBounds = new BoundsInt();

        if (roomsDict == null)
            return false;

        bool first = true;
        foreach (var kvp in roomsDict)
        {
            DungeonRoomInstance dungeonRoomInstance = kvp.Value;
            if (dungeonRoomInstance.wallMap == null)
                continue;

            // get wall tilemap
            Tilemap wallMap = dungeonRoomInstance.wallMap;
            wallMap.CompressBounds();

            BoundsInt roomBounds = wallMap.cellBounds;
            Vector3Int roomOffset = dungeonRoomInstance.GetPositionInCell(grid);
            roomBounds.position += roomOffset;

            if (first)
            {
                first = false;
                combinedBounds = roomBounds;
            }
            else
            {
                combinedBounds.xMin = Mathf.Min(combinedBounds.xMin, roomBounds.xMin);
                combinedBounds.yMin = Mathf.Min(combinedBounds.yMin, roomBounds.yMin);
                combinedBounds.xMax = Mathf.Max(combinedBounds.xMax, roomBounds.xMax);
                combinedBounds.yMax = Mathf.Max(combinedBounds.yMax, roomBounds.yMax);
            }
        }

        return true;
    }

    public static BoundsInt ExpandBounds(BoundsInt bounds, float offset = 0.5f)
    {
        offset = Mathf.Clamp(offset, 0, 1);

        // expand the bounds
        int xOffset = Mathf.CeilToInt(bounds.size.x * offset);
        int yOffset = Mathf.CeilToInt(bounds.size.y * offset);

        Vector3Int newMin = bounds.min - new Vector3Int(xOffset, yOffset);
        Vector3Int newMax = bounds.max + new Vector3Int(xOffset, yOffset);

        bounds.SetMinMax(newMin, newMax);

        return bounds;
    }

    public static Vector3Int GetEdgeDirection(Edge edge)
    {
        switch (edge)
        {
            case TilemapHelper.Edge.LEFT:
                {
                    return Vector3Int.left;
                }
            case TilemapHelper.Edge.RIGHT:
                {
                    return Vector3Int.right;
                }
            case TilemapHelper.Edge.TOP:
                {
                    return Vector3Int.up;
                }
            case TilemapHelper.Edge.BOTTOM:
                {
                    return Vector3Int.down;
                }
            default: return Vector3Int.zero;
        }
    }

    public static Corner IsCornerEdgeWall(Vector3Int wallCell, Tilemap wallMap)
    {
        if (wallMap == null) return Corner.INVALID;

        wallMap.CompressBounds();

        // get corner edges
        int xMin = wallMap.cellBounds.xMin;
        int xMax = wallMap.cellBounds.xMax - 1;
        int yMin = wallMap.cellBounds.yMin;
        int yMax = wallMap.cellBounds.yMax - 1;

        Vector3Int bottomLeftCorner = new Vector3Int(xMin, yMin);
        Vector3Int bottomRightCorner = new Vector3Int(xMax, yMin);
        Vector3Int TopLeftCorner = new Vector3Int(xMin, yMax);
        Vector3Int TopRightCorner = new Vector3Int(xMax, yMax);

        bool isBottomLeftCorner = wallCell == bottomLeftCorner;
        bool isBottomRightCorner = wallCell == bottomRightCorner;
        bool isTopLeftCorner = wallCell == TopLeftCorner;
        bool isTopRightCorner = wallCell == TopRightCorner;

        if (isBottomLeftCorner)
            return Corner.BOTTOM_LEFT;
        if (isBottomRightCorner)
            return Corner.BOTTOM_RIGHT;
        if (isTopLeftCorner)
            return Corner.TOP_LEFT;
        if (isTopRightCorner)
            return Corner.TOP_RIGHT;

        return Corner.INVALID;
    }

    public static HashSet<Vector3Int> PopulateCellsToCheck(Vector3Int cellPositionOffset, HashSet<Vector3Int> cells)
    {
        HashSet<Vector3Int> cellsToCheck = new HashSet<Vector3Int>();

        foreach(Vector3Int cell in cells)
        {
            cellsToCheck.Add(cell + cellPositionOffset);
        }

        return cellsToCheck;
    }
}
