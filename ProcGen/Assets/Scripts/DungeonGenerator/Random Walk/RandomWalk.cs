using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;
using System.Collections.Generic;

public class RandomWalk
{
    private enum Axis 
    { 
        HORIZONTAL,
        VERTICAL,
        MAX_EXCLUSIVE,
    }

    //public static bool GenerateCorridors(DungeonLayout layout, Grid grid, Dictionary<DungeonRoom, Tilemap> placedRooms, Tilemap corridorTilemap, CorridorTiles corridorTiles, int maxCorridorPathIterations = 50, int maxPairFail = 5, int maxConnectionFail = 5)
    //{
    //    // list to contain unique connections
    //    List<RoomConnection> connections = new List<RoomConnection>();

    //    // create unique connections
    //    foreach (DungeonRoom room1 in layout.dungeonRoomList)
    //    {
    //        if(room1 == null || room1.connectionList == null) continue;

    //        foreach(DungeonRoom room2 in room1.connectionList)
    //        {
    //            RoomConnection con = new RoomConnection(room1, room2);
    //            if (RoomConnection.CheckConnectionUnique(connections, con))
    //                connections.Add(con);
    //        }
    //    }

    //    // iterate through each unique connection
    //    foreach(RoomConnection connection in connections)
    //    {
    //        DungeonRoom room1 = connection.GetRoom1();
    //        DungeonRoom room2 = connection.GetRoom2();

    //        if (connection == null || room1 == null || room2 == null) continue;

    //        // get the tilemap that dungeon room is associated with
    //        // used placedRooms
    //        Tilemap room1Map = placedRooms[room1];
    //        Tilemap room2Map = placedRooms[room2];

    //        if (room1Map == null || room2Map == null) continue;

    //        // identify direction from room1 to room2
    //        Vector3 dir = (room2Map.transform.position - room1Map.transform.position).normalized;
    //        Debug.Log("dir: " + dir);

    //        // find closest edges of room1 and room2
    //        TilemapHelper.Edge room1Edge, room2Edge;
    //        Vector3Int room1CorridorCell, room2CorridorCell;

    //        bool room1CorridorPicked = PickCorridorCell(room1Map, room2Map, out room1CorridorCell, out room1Edge);
    //        bool room2CorridorPicked = PickCorridorCell(room2Map, room1Map, out room2CorridorCell, out room2Edge);

    //        if(room1CorridorPicked && room2CorridorPicked)
    //        {
    //            ConnectRooms(room1Map, room2Map, corridorTilemap, room1CorridorCell, room2CorridorCell, room1Edge, room2Edge, corridorTiles, maxCorridorPathIterations, maxConnectionFail);
    //        }
    //    }


    //    return true;
    //}

    //private static bool ConnectRooms(Tilemap room1Map, Tilemap room2Map, Tilemap corridorTilemap, Vector3Int room1CorridorCell, Vector3Int room2CorridorCell, TilemapHelper.Edge room1Edge, TilemapHelper.Edge room2Edge, CorridorTiles corridorTiles, int maxIterations = 50, int maxConnectionFail = 5)
    //{
    //    // execute random walk from room 1 towards room 2


    //    // get the wall tilemap to replace the wall with the floor as the entrance

    //    // room1
    //    Transform room1WallsTransform = room1Map.transform.parent.Find("Walls");

    //    if (room1WallsTransform == null) return false;

    //    Tilemap room1WallMap = room1WallsTransform.GetComponent<Tilemap>();
    //    room1WallMap.SetTile(room1CorridorCell, corridorTiles.corridorFloor);

    //    // room2
    //    Transform room2WallsTransform = room2Map.transform.parent.Find("Walls");

    //    if (room2WallsTransform == null) return false;

    //    Tilemap room2WallMap = room2WallsTransform.GetComponent<Tilemap>();
    //    room2WallMap.SetTile(room2CorridorCell, corridorTiles.corridorFloor);

    //    // convert corridor cells to world position
    //    Vector3 convertedRoom1Corridor = room1Map.CellToWorld(room1CorridorCell);
    //    Vector3 convertedRoom2Corridor = room2Map.CellToWorld(room2CorridorCell);

    //    // then convert back to cell position for corridorTilemap
    //    room1CorridorCell = corridorTilemap.WorldToCell(convertedRoom1Corridor);
    //    room2CorridorCell = corridorTilemap.WorldToCell(convertedRoom2Corridor);

    //    Debug.Log($"room1CorridorCell Position: {room1CorridorCell}");
    //    Debug.Log($"room2CorridorCell Position: {room2CorridorCell}");

    //    // create path
    //    List<Vector3Int> corridorFloorCells;
    //    GetCorridorFloorCells(room1CorridorCell, room2CorridorCell, room1Edge, out corridorFloorCells);

    //    Debug.Log($"corridorFloorCells: {corridorFloorCells.Count}");
    //    // replace the tiles with corridor tiles
    //    foreach (var cell in corridorFloorCells)
    //    {
    //        // Debug.Log($"Cell: {cell}");
    //        corridorTilemap.SetTile(cell, corridorTiles.corridorFloor);
    //    }
    //    // prob need a prev tile tracker and next tile to figure out the orientation

    //    return true;
    //}

    //private static bool GetCorridorFloorCells(Vector3Int room1CorridorCell, Vector3Int room2CorridorCell, TilemapHelper.Edge room1Edge, out List<List<Vector3Int>> corridorFloorCells, int maxIterations = 50, int maxConnectionFail = 5)
    //{
    //    corridorFloorCells = new List<List<Vector3Int>>();

    //    Vector3Int currCell = room1CorridorCell;
    //    int maxStepsExclusive = 4;
    //    int minimumSteps = 2;

    //    int initStep = Random.Range(minimumSteps, maxStepsExclusive);

    //    Vector2Int initDir;
    //    // do initial step
    //    switch (room1Edge) 
    //    {
    //        case TilemapHelper.Edge.LEFT:
    //            {
    //                initDir = new Vector2Int(-1, 0);
    //                break;
    //            }
    //        case TilemapHelper.Edge.RIGHT:
    //            {
    //                initDir = new Vector2Int(1, 0);
    //                break;
    //            }
    //        case TilemapHelper.Edge.TOP:
    //            {
    //                initDir = new Vector2Int(0, 1);
    //                break;
    //            }
    //        case TilemapHelper.Edge.BOTTOM:
    //            {
    //                initDir = new Vector2Int(0, -1);
    //                break;
    //            }
    //        default: return false;
    //    }

    //    AddNewFloorCells(ref currCell, initStep, initDir, corridorFloorCells);

    //    // stop loop if reach destination
    //    int currIteration = 0;

    //    // if destination not reached and not max iterations yet
    //    while (true)
    //    {
    //        // pick a random number of steps
    //        int steps = Random.Range(minimumSteps, maxStepsExclusive);

    //        // pick horizontal or vertical direction
    //        Axis axis = (Axis)Random.Range(0, (int)Axis.MAX_EXCLUSIVE);

    //        Vector2Int dir;

    //        if (axis == Axis.HORIZONTAL)
    //        {
    //            int xDir = room2CorridorCell.x > currCell.x ? 1 : -1;

    //            // get direction towards room2
    //            dir = new Vector2Int(xDir, 0);
    //        }
    //        else
    //        {
    //            int yDir = room2CorridorCell.y > currCell.y ? 1 : -1;

    //            // get direction towards room2
    //            dir = new Vector2Int(0, yDir);
    //        }

    //        // add new path and update currCell
    //        AddNewFloorCells(ref currCell, steps, dir, corridorFloorCells);
    //        currIteration++;

    //        if (corridorFloorCells.Contains(room2CorridorCell))
    //        {
    //            int destinationIndex = corridorFloorCells.FindLastIndex(0, (x) => { return x == room2CorridorCell; });
    //            corridorFloorCells.RemoveRange(destinationIndex + 1, corridorFloorCells.Count - (destinationIndex + 1));

    //            Debug.Log("Hataraku Saibo");
    //            return true;
    //        }
            
    //        if(currIteration >= maxIterations)
    //        {
    //            Debug.Log($"Max Iteration reached: {currIteration} / {maxIterations}");
    //            return false;
    //        }
    //    }
    //}

    /// <summary>
    /// This will generate a corridor before placing room2
    /// </summary>
    /// <param name="room1CorridorCell"></param>
    /// <param name="room1Edge"></param>
    /// <param name="corridorFloorCells"></param>
    /// <param name="maxIterations"></param>
    /// <param name="maxConnectionFail"></param>
    /// <returns></returns>
    public static List<List<Vector3Int>> GetCorridorFloorCells(Vector3Int room1CorridorCell, TilemapHelper.Edge room1Edge, DungeonRoomInstance room2Instance ,int maxIterations = 3, int maxConnectionFail = 5)
    {
        if (room2Instance == null || room2Instance.wallMap == null) return null;

        // list to contain the floor cells
        List<List<Vector3Int>>  corridorFloorCells = new List<List<Vector3Int>>();

        // keep track of the latest corridor cell
        Vector3Int currCell = room1CorridorCell;

        room2Instance.wallMap.CompressBounds();

        BoundsInt room2Bounds = room2Instance.wallMap.cellBounds;

        // keep track of the axis that room1Edge is on
        Axis room1EdgeAxis;

        // only 1 valid initial direction for the 1st iteration to prevent corridor from overlapping with room1
        Vector2Int initDir;
        int initStep;

        // initial iteration
        switch (room1Edge)
        {
            case TilemapHelper.Edge.LEFT:
                {
                    initDir = new Vector2Int(-1, 0);
                    room1EdgeAxis = Axis.HORIZONTAL;
                    break;
                }
            case TilemapHelper.Edge.RIGHT:
                {
                    initDir = new Vector2Int(1, 0);
                    room1EdgeAxis = Axis.HORIZONTAL;
                    break;
                }
            case TilemapHelper.Edge.TOP:
                {
                    initDir = new Vector2Int(0, 1);
                    room1EdgeAxis = Axis.VERTICAL;
                    break;
                }
            case TilemapHelper.Edge.BOTTOM:
                {
                    initDir = new Vector2Int(0, -1);
                    room1EdgeAxis = Axis.VERTICAL;
                    break;
                }
            default: return null;
        }

        // initial steps 
        initStep = GetRandomSteps(room2Bounds, room1EdgeAxis);

        AddNewFloorCells(ref currCell, initStep, initDir, corridorFloorCells);

        // stop loop if reach destination
        int numOfIteration = Random.Range(1, maxIterations);
        int currIteration = 0;

        // if destination not reached and not max iterations yet
        while (currIteration < numOfIteration)
        {
            // pick horizontal or vertical direction
            Axis axis = (Axis)Random.Range(0, (int)Axis.MAX_EXCLUSIVE);

            // pick a random number of steps
            int steps = GetRandomSteps(room2Bounds, axis);

            Vector2Int dir = PickNextDirection(axis, room1EdgeAxis, room1Edge);

            // add new path and update currCell
            AddNewFloorCells(ref currCell, steps, dir, corridorFloorCells);

            currIteration++;
        }

        return corridorFloorCells;
    }

    /// <summary>
    /// Get random steps for corridor generation iteration
    /// </summary>
    /// <returns></returns>
    private static int GetRandomSteps(BoundsInt room2Bounds, Axis chosenAxis, int maxStepsOffset = 3)
    {
        if (maxStepsOffset < 0 || room2Bounds.size.x == 0 || room2Bounds.size.y == 0) return 0;

        // min step should based on the size of the room on chosen axis
        int minSteps = chosenAxis == Axis.HORIZONTAL ? room2Bounds.size.x : room2Bounds.size.y;

        return Random.Range(minSteps, minSteps + maxStepsOffset);
    }

    private static Vector2Int PickNextDirection(Axis chosenAxis, Axis room1EdgeAxis, TilemapHelper.Edge room1Edge)
    {
        Vector2Int dir;

        if (chosenAxis == Axis.HORIZONTAL)
        {
            int xDir;
            // room1Edge is on horizontal axis
            if (room1EdgeAxis == chosenAxis)
            {
                // prevent backtracking
                xDir = room1Edge == TilemapHelper.Edge.LEFT ? -1 : 1;
            }
            else
            {
                xDir = Random.Range(0, 1) == 0 ? -1 : 1;
            }

            // get direction towards room2
            dir = new Vector2Int(xDir, 0);
        }
        else
        {
            int yDir;
            if (room1EdgeAxis == chosenAxis)
            {
                // prevent backtracking
                yDir = room1Edge == TilemapHelper.Edge.BOTTOM ? -1 : 1;
            }
            else
            {
                yDir = Random.Range(0, 1) == 0 ? -1 : 1;
            }

            // get direction towards room2
            dir = new Vector2Int(0, yDir);
        }

        return dir;
    }

    /// <summary>
    /// Adds a straight segment of 3 tile wide corridor floor cells in the given direction
    /// Each step adds a strip of 3 tiles: center + 2 adjacent cells based on the axis
    /// </summary>
    /// <param name="currCell"></param>
    /// <param name="steps"></param>
    /// <param name="dir"></param>
    /// <param name="corridorFloorCells"></param>
    private static void AddNewFloorCells(ref Vector3Int currCell, int steps, Vector2Int dir, List<List<Vector3Int>> corridorFloorCells)
    {
        Axis axis = Axis.HORIZONTAL;

        if (dir == Vector2Int.up || dir == Vector2Int.down)
            axis = Axis.VERTICAL;

        Vector3Int step = new Vector3Int(dir.x, dir.y);
        for (int i = 0; i < steps; ++i)
        {
            currCell += step;

            List<Vector3Int> corridorStrip = GetAdjacentCells(currCell, axis);
            corridorStrip.Insert(1, currCell);

            corridorFloorCells.Add(corridorStrip);
        }

        List<Vector3Int> finalCorridorStrip = GetAdjacentCells(currCell + step, axis);
        finalCorridorStrip.Insert(1, currCell + step);

        corridorFloorCells.Add(finalCorridorStrip);
    }

    /// <summary>
    /// Return adjacent cells for the provided cell position based on the axis
    /// </summary>
    /// <param name="position"></param>
    /// <param name="axis"></param>
    /// <returns></returns>
    private static List<Vector3Int> GetAdjacentCells(Vector3Int position, Axis axis)
    {
        List<Vector3Int> cells = new List<Vector3Int>();
        if(axis == Axis.VERTICAL)
        {
            cells.Add(position + Vector3Int.left);
            cells.Add(position + Vector3Int.right);
        }
        else
        {
            cells.Add(position + Vector3Int.down);
            cells.Add(position + Vector3Int.up);
        }

        return cells;
    }

    /// <summary>
    /// Calculates the position of the edges of a room.
    /// </summary>
    /// <param name="roomPosition"></param>
    /// <param name="dimesions"></param>
    /// <returns></returns>
    private static Dictionary<TilemapHelper.Edge, Vector3> GetEdgePositions(Tilemap tileMap)
    {
        tileMap.CompressBounds();

        BoundsInt bounds = tileMap.cellBounds;

        return new Dictionary<TilemapHelper.Edge, Vector3>
        {
            { TilemapHelper.Edge.LEFT, tileMap.CellToWorld(new Vector3Int(bounds.xMin,0)) },
            { TilemapHelper.Edge.RIGHT, tileMap.CellToWorld(new Vector3Int(bounds.xMax,0)) },
            { TilemapHelper.Edge.TOP, tileMap.CellToWorld(new Vector3Int(0,bounds.yMax)) },
            { TilemapHelper.Edge.BOTTOM, tileMap.CellToWorld(new Vector3Int(0,bounds.yMin)) },
        };
    }

    /// <summary>
    /// Calculates distances between edges of room1 and center of room2.
    /// </summary>
    /// <param name="room2Position"></param>
    /// <param name="edgePositions"></param>
    /// <returns></returns>
    private static Dictionary<TilemapHelper.Edge, float> GetEdgeDistances(Vector3 room2Position, Dictionary<TilemapHelper.Edge, Vector3> room1EdgePositions)
    {
        TilemapHelper.Edge[] requiredKeys = { TilemapHelper.Edge.LEFT, TilemapHelper.Edge.RIGHT, TilemapHelper.Edge.BOTTOM, TilemapHelper.Edge.TOP };

        // https://learn.microsoft.com/en-us/dotnet/api/system.linq.enumerable.all?view=net-9.0
        // iterates through each element in requiredKeys and tests if edgePosition contains it using the predicate provided.
        // returns true if all elements passes the test
        if (!requiredKeys.All(room1EdgePositions.ContainsKey))
        {
            return new Dictionary<TilemapHelper.Edge, float>();
        }

        // a dictionary containing the edge and the distance between it and room2
        return new Dictionary<TilemapHelper.Edge, float>
        {
            { TilemapHelper.Edge.LEFT, (room2Position - room1EdgePositions[TilemapHelper.Edge.LEFT]).magnitude },
            { TilemapHelper.Edge.RIGHT, (room2Position - room1EdgePositions[TilemapHelper.Edge.RIGHT]).magnitude },
            { TilemapHelper.Edge.TOP, (room2Position - room1EdgePositions[TilemapHelper.Edge.TOP]).magnitude },
            { TilemapHelper.Edge.BOTTOM, (room2Position - room1EdgePositions[TilemapHelper.Edge.BOTTOM]).magnitude },
        };
    }

    private static bool PickCorridorCell(Tilemap room1Map, Tilemap room2Map, out Vector3Int room1CorridorCell, out TilemapHelper.Edge closestEdge)
    {
        // get the position of room1 edges
        Dictionary<TilemapHelper.Edge, Vector3> edgePositions = GetEdgePositions(room1Map);

        // calculate distances between edges of room1 and center of room2
        Dictionary<TilemapHelper.Edge, float> edgeDistances = GetEdgeDistances(room2Map.transform.position, edgePositions);

        // get the closest edge of room1 by comparing distances
        closestEdge = edgeDistances.Aggregate((x, y) => x.Value < y.Value ? x : y).Key;

        Debug.Log($"{room1Map.transform.parent.name} edge distances:");
        foreach(var edgeDistance in edgeDistances)
        {
            Debug.Log($"{edgeDistance.Key}, {edgeDistance.Value}");
        }

        Debug.Log($"closestEdge: {closestEdge}");

        return TilemapHelper.PickEdgeCell(room1Map, closestEdge, out room1CorridorCell);
    }


}


