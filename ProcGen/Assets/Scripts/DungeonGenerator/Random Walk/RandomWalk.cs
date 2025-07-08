using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;
using System.Collections.Generic;

public class RandomWalk
{
    /// <summary>
    /// This will generate a corridor before placing room2
    /// </summary>
    /// <param name="room1CorridorCell"></param>
    /// <param name="room1Edge"></param>
    /// <param name="corridorFloorCells"></param>
    /// <param name="maxIterations"></param>
    /// <param name="maxConnectionFail"></param>
    /// <returns></returns>
    public static List<List<Vector3Int>> GetCorridorFloorCells(Vector3Int room1CorridorCell, TilemapHelper.Edge room1Edge, DungeonRoomInstance room2Instance, int maxIterations = 3, int maxConnectionFail = 5, int corridorWidth = 5)
    {
        if (room2Instance == null || room2Instance.wallMap == null) return null;

        // list to contain the floor cells
        List<List<Vector3Int>>  corridorFloorCells = new List<List<Vector3Int>>();

        // keep track of the latest corridor cell
        Vector3Int currCell = room1CorridorCell;

        room2Instance.wallMap.CompressBounds();

        BoundsInt room2Bounds = room2Instance.wallMap.cellBounds;

        // keep track of the axis that room1Edge is on
        TilemapHelper.Axis room1EdgeAxis;

        // only 1 valid initial direction for the 1st iteration to prevent corridor from overlapping with room1
        Vector2Int initDir;
        int initStep;

        // initial iteration
        switch (room1Edge)
        {
            case TilemapHelper.Edge.LEFT:
                {
                    initDir = new Vector2Int(-1, 0);
                    room1EdgeAxis = TilemapHelper.Axis.HORIZONTAL;
                    break;
                }
            case TilemapHelper.Edge.RIGHT:
                {
                    initDir = new Vector2Int(1, 0);
                    room1EdgeAxis = TilemapHelper.Axis.HORIZONTAL;
                    break;
                }
            case TilemapHelper.Edge.TOP:
                {
                    initDir = new Vector2Int(0, 1);
                    room1EdgeAxis = TilemapHelper.Axis.VERTICAL;
                    break;
                }
            case TilemapHelper.Edge.BOTTOM:
                {
                    initDir = new Vector2Int(0, -1);
                    room1EdgeAxis = TilemapHelper.Axis.VERTICAL;
                    break;
                }
            default: return null;
        }

        // initial steps 
        initStep = GetRandomSteps(room2Bounds, room1EdgeAxis);

        AddNewFloorCells(ref currCell, initStep, initDir, corridorFloorCells, corridorWidth);

        // stop loop if reach destination
        int numOfIteration = Random.Range(1, maxIterations);
        int currIteration = 0;

        // if destination not reached and not max iterations yet
        while (currIteration < numOfIteration)
        {
            // pick horizontal or vertical direction
            TilemapHelper.Axis axis = (TilemapHelper.Axis)Random.Range(0, (int)TilemapHelper.Axis.MAX_EXCLUSIVE);

            // pick a random number of steps
            int steps = GetRandomSteps(room2Bounds, axis);

            Vector2Int dir = PickNextDirection(axis, room1EdgeAxis, room1Edge);

            // add new path and update currCell
            AddNewFloorCells(ref currCell, steps, dir, corridorFloorCells, corridorWidth);

            currIteration++;
        }

        //corridorFloorCells.Remove(corridorFloorCells.Last());
        //corridorFloorCells.Remove(corridorFloorCells.First());

        return corridorFloorCells;
    }

    /// <summary>
    /// Get random steps for corridor generation iteration
    /// </summary>
    /// <returns></returns>
    private static int GetRandomSteps(BoundsInt room2Bounds, TilemapHelper.Axis chosenAxis, int maxStepsOffset = 3)
    {
        if (maxStepsOffset < 0 || room2Bounds.size.x == 0 || room2Bounds.size.y == 0) return 0;

        // min step should based on the size of the room on chosen axis
        int minSteps = chosenAxis == TilemapHelper.Axis.HORIZONTAL ? room2Bounds.size.x : room2Bounds.size.y;

        return Random.Range(minSteps, minSteps + maxStepsOffset);
    }

    private static Vector2Int PickNextDirection(TilemapHelper.Axis chosenAxis, TilemapHelper.Axis room1EdgeAxis, TilemapHelper.Edge room1Edge)
    {
        Vector2Int dir;

        if (chosenAxis == TilemapHelper.Axis.HORIZONTAL)
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
    private static void AddNewFloorCells(ref Vector3Int currCell, int steps, Vector2Int dir, List<List<Vector3Int>> corridorFloorCells, int corridorWidth = 3)
    {
        if(corridorWidth == 0)
        {
            Debug.Log("AddNewFloorCells: Cannot divide by zero");
            return;
        }

        TilemapHelper.Axis axis = TilemapHelper.Axis.VERTICAL;

        if (dir == Vector2Int.up || dir == Vector2Int.down)
            axis = TilemapHelper.Axis.HORIZONTAL;

        Vector3Int step = new Vector3Int(dir.x, dir.y);


        // go in reverse direction to set walls for corner
        if (corridorFloorCells.Count > 0)
        {
            Vector3Int reverseStep = step * -1;

            for (int i = corridorWidth / 2; i >= 0; --i)
            {
                Vector3Int position = currCell + reverseStep * i;
                List<Vector3Int> corridorStrip = TilemapHelper.GetAdjacentCells(position, axis, corridorWidth);
                if (i == corridorWidth / 2)
                {
                    corridorStrip = new List<Vector3Int> { corridorStrip.First(), corridorStrip.Last() };
                }
                else
                {
                    corridorStrip.Insert(corridorWidth / 2, position);
                }

                corridorFloorCells.Add(corridorStrip);
            }

        }

        for (int i = 0; i < steps; ++i)
        {
            currCell += step;

            List<Vector3Int> corridorStrip = TilemapHelper.GetAdjacentCells(currCell, axis, corridorWidth);
            corridorStrip.Insert(corridorWidth / 2, currCell);

            corridorFloorCells.Add(corridorStrip);
        }



        List<Vector3Int> finalCorridorStrip = TilemapHelper.GetAdjacentCells(currCell + step, axis, corridorWidth);
        finalCorridorStrip.Insert(corridorWidth / 2, currCell + step);
        corridorFloorCells.Add(finalCorridorStrip);

        // fill missing pieces of the corridor at the corner 
        //for (int i = 1; i <= corridorWidth / 2; ++i)
        //{
        //    List<Vector3Int> finalCorridorStrip = TilemapHelper.GetAdjacentCells(currCell + step * i, axis, corridorWidth);
        //    finalCorridorStrip.Insert(corridorWidth / 2, currCell + step * i);
        //    corridorFloorCells.Add(finalCorridorStrip);
        //}
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


