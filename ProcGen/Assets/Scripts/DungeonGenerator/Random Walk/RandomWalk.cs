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
    public static bool GenerateCorridors(DungeonLayout layout, Grid grid, Dictionary<DungeonRoom, Tilemap> placedRooms, Tilemap corridorTilemap, CorridorTiles corridorTiles, int maxCorridorPathIterations = 50, int maxPairFail = 5, int maxConnectionFail = 5)
    {
        // list to contain unique connections
        List<RoomConnection> connections = new List<RoomConnection>();

        // create unique connections
        foreach (DungeonRoom room1 in layout.dungeonRoomList)
        {
            if(room1 == null || room1.connectionList == null) continue;

            foreach(DungeonRoom room2 in room1.connectionList)
            {
                RoomConnection con = new RoomConnection(room1, room2);
                if (RoomConnection.CheckConnectionUnique(connections, con))
                    connections.Add(con);
            }
        }

        // iterate through each unique connection
        foreach(RoomConnection connection in connections)
        {
            DungeonRoom room1 = connection.GetRoom1();
            DungeonRoom room2 = connection.GetRoom2();

            if (connection == null || room1 == null || room2 == null) continue;

            // get the tilemap that dungeon room is associated with
            // used placedRooms
            Tilemap room1Map = placedRooms[room1];
            Tilemap room2Map = placedRooms[room2];

            if (room1Map == null || room2Map == null) continue;

            // identify direction from room1 to room2
            Vector3 dir = (room2Map.transform.position - room1Map.transform.position).normalized;
            Debug.Log("dir: " + dir);

            // find closest edges of room1 and room2
            Vector3Int room1CorridorCell, room2CorridorCell;

            bool room1CorridorPicked = PickCorridorCell(room1Map, room2Map, out room1CorridorCell);
            bool room2CorridorPicked = PickCorridorCell(room2Map, room1Map, out room2CorridorCell);

            if(room1CorridorPicked && room2CorridorPicked)
            {
                ConnectRooms(room1Map, room2Map, corridorTilemap, room1CorridorCell, room2CorridorCell, corridorTiles, maxCorridorPathIterations, maxConnectionFail);
            }
        }


        return true;
    }

    private static bool ConnectRooms(Tilemap room1Map, Tilemap room2Map, Tilemap corridorTilemap, Vector3Int room1CorridorCell, Vector3Int room2CorridorCell, CorridorTiles corridorTiles, int maxIterations = 50, int maxConnectionFail = 5)
    {
        // execute random walk from room 1 towards room 2


        // get the wall tilemap to replace the wall with the floor as the entrance

        // room1
        Transform room1WallsTransform = room1Map.transform.parent.Find("Walls");

        if (room1WallsTransform == null) return false;

        Tilemap room1WallMap = room1WallsTransform.GetComponent<Tilemap>();
        room1WallMap.SetTile(room1CorridorCell, corridorTiles.corridorFloor);

        // room2
        Transform room2WallsTransform = room2Map.transform.parent.Find("Walls");

        if (room2WallsTransform == null) return false;

        Tilemap room2WallMap = room2WallsTransform.GetComponent<Tilemap>();
        room2WallMap.SetTile(room2CorridorCell, corridorTiles.corridorFloor);

        // convert corridor cells to world position
        Vector3 convertedRoom1Corridor = room1Map.CellToWorld(room1CorridorCell);
        Vector3 convertedRoom2Corridor = room2Map.CellToWorld(room2CorridorCell);

        // then convert back to cell position for corridorTilemap
        room1CorridorCell = corridorTilemap.WorldToCell(convertedRoom1Corridor);
        room2CorridorCell = corridorTilemap.WorldToCell(convertedRoom2Corridor);

        Debug.Log($"room1CorridorCell Position: {room1CorridorCell}");
        Debug.Log($"room2CorridorCell Position: {room2CorridorCell}");

        // create path
        List<Vector3Int> corridorFloorCells;
        GetCorridorFloorCells(room1CorridorCell, room2CorridorCell, out corridorFloorCells);

        Debug.Log($"corridorFloorCells: {corridorFloorCells.Count}");
        // replace the tiles with corridor tiles
        foreach (var cell in corridorFloorCells)
        {
            // Debug.Log($"Cell: {cell}");
            corridorTilemap.SetTile(cell, corridorTiles.corridorFloor);
        }
        // prob need a prev tile tracker and next tile to figure out the orientation

        return true;
    }

    private static bool GetCorridorFloorCells(Vector3Int room1CorridorCell, Vector3Int room2CorridorCell, out List<Vector3Int> corridorFloorCells, int maxIterations = 50, int maxConnectionFail = 5)
    {
        corridorFloorCells = new List<Vector3Int>();

        Vector3Int currCell = room1CorridorCell;
        int maxStepsExclusive = 6;
        int minimumSteps = 2;

        // stop loop if reach destination
        int currIteration = 0;

        // if destination not reached and not max iterations yet
        while (true)
        {
            // pick a random number of steps
            int steps = Random.Range(minimumSteps, maxStepsExclusive);

            // pick horizontal or vertical direction
            Axis axis = (Axis)Random.Range(0, (int)Axis.MAX_EXCLUSIVE);

            Vector2Int dir;

            if (axis == Axis.HORIZONTAL)
            {
                int xDir = room2CorridorCell.x > currCell.x ? 1 : -1;

                // get direction towards room2
                dir = new Vector2Int(xDir, 0);
            }
            else
            {
                int yDir = room2CorridorCell.y > currCell.y ? 1 : -1;

                // get direction towards room2
                dir = new Vector2Int(0, yDir);
            }

            // add new path and update currCell
            for (int i = 0; i < steps; ++i)
            {
                currCell += new Vector3Int(dir.x, dir.y);
                corridorFloorCells.Add(currCell);
            }
            currIteration++;

            if (corridorFloorCells.Contains(room2CorridorCell))
            {
                Debug.Log("Hataraku Saibo");
                return true;
            }
            
            if(currIteration >= maxIterations)
            {
                Debug.Log($"Max Iteration reached: {currIteration} / {maxIterations}");
                return false;
            }
        }
    }

    private static bool PickCorridorCell(Tilemap room1Map, Tilemap room2Map, out Vector3Int room1CorridorCell)
    {
        // get the position of room1 edges
        Dictionary<string, Vector3> edgePositions = TilemapHelper.GetEdgePositions(room1Map);

        // calculate distances between edges of room1 and center of room2
        Dictionary<string, float> edgeDistances = TilemapHelper.GetEdgeDistances(room2Map.transform.position, edgePositions);

        // get the closest edge of room1 by comparing distances
        string closestEdge = edgeDistances.Aggregate((x, y) => x.Value < y.Value ? x : y).Key;

        Debug.Log($"{room1Map.transform.parent.name} edge distances:");
        foreach(var edgeDistance in edgeDistances)
        {
            Debug.Log($"{edgeDistance.Key}, {edgeDistance.Value}");
        }

        Debug.Log($"closestEdge: {closestEdge}");

        return PickEdgeCell(room1Map, closestEdge, out room1CorridorCell);
    }

    private static bool PickEdgeCell(Tilemap map, string closestEdge, out Vector3Int cellPosition)
    {
        string[] validEdges = { "LEFT", "RIGHT", "TOP", "BOTTOM" };
        cellPosition = Vector3Int.zero;

        if (map == null || !validEdges.Contains(closestEdge))
            return false;

        // pick a point at the edge of room
        switch (closestEdge)
        {
            case "LEFT":
                {
                    // tilemap bounds will be 2 less cuz we using the ground tilemap so walls not included
                    int y = Random.Range(map.cellBounds.yMin, map.cellBounds.yMax);
                    cellPosition = new Vector3Int(map.cellBounds.xMin - 1, y);
                    return true;
                }
            case "RIGHT":
                {
                    // tilemap bounds will be 2 less cuz we using the ground tilemap so walls not included
                    int y = Random.Range(map.cellBounds.yMin, map.cellBounds.yMax);
                    cellPosition = new Vector3Int(map.cellBounds.xMax, y);
                    return true;
                }
            case "TOP":
                {
                    // tilemap bounds will be 2 less cuz we using the ground tilemap so walls not included
                    int x = Random.Range(map.cellBounds.xMin, map.cellBounds.xMax);
                    cellPosition = new Vector3Int(x, map.cellBounds.yMax);
                    return true;
                }
            case "BOTTOM":
                {
                    // tilemap bounds will be 2 less cuz we using the ground tilemap so walls not included
                    int x = Random.Range(map.cellBounds.xMin, map.cellBounds.xMax);
                    cellPosition = new Vector3Int(x, map.cellBounds.yMin - 1);
                    return true;
                }

            default: return false;
        }
    }
}


