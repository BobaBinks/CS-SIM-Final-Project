using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;
using System.Collections.Generic;

public class RandomWalk
{
    public static bool GenerateCorridors(DungeonLayout layout, Transform grid, Dictionary<DungeonRoom, Tilemap> placedRooms,Tilemap corridorTilemap, int maxPairFail = 5, int maxConnectionFail = 5)
    {
        // option 1: place the walls on one tilemaps(room prefabs)
        // option 2: place the walls on both tilemaps(room prefabs)

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
            Vector2Int room1CorridorCell, room2CorridorCell;

            bool room1CorridorPicked = PickCorridorCell(room1Map, room2Map, out room1CorridorCell);
            bool room2CorridorPicked = PickCorridorCell(room2Map, room1Map, out room2CorridorCell);

            if(room1CorridorPicked && room2CorridorPicked)
            {
                // execute random walk from room 1 towards room 2
                // place the tiles on the corridor tilemap
                // corridor tilemap should be above the wall layer so it covers the walls in the room prefabs
            }
        }


        return true;
    }

    private static bool PickCorridorCell(Tilemap room1Map, Tilemap room2Map, out Vector2Int room1CorridorCell)
    {
        // get the position of room1 edges
        Dictionary<string, Vector3> edgePositions = TilemapHelper.GetEdgePositions(room1Map);

        // calculate distances between edges of room1 and center of room2
        Dictionary<string, float> edgeDistances = TilemapHelper.GetEdgeDistances(room2Map.transform.position, edgePositions);

        // get the closest edge of room1 by comparing distances
        string closestEdge = edgeDistances.Aggregate((x, y) => x.Value < y.Value ? x : y).Key;

        return PickEdgeCell(room1Map, closestEdge, out room1CorridorCell);
    }

    private static bool PickEdgeCell(Tilemap map, string closestEdge, out Vector2Int cellPosition)
    {
        string[] validEdges = { "LEFT", "RIGHT", "TOP", "BOTTOM" };
        cellPosition = Vector2Int.zero;

        if (map == null || !validEdges.Contains(closestEdge))
            return false;

        // pick a point at the edge of room
        switch (closestEdge)
        {
            case "LEFT":
                {
                    // tilemap bounds will be 2 less cuz we using the ground tilemap so walls not included
                    int y = Random.Range(map.cellBounds.yMin - 1, map.cellBounds.yMax + 1);
                    cellPosition = new Vector2Int(map.cellBounds.xMin - 1, y);
                    return true;
                }
            case "RIGHT":
                {
                    // tilemap bounds will be 2 less cuz we using the ground tilemap so walls not included
                    int y = Random.Range(map.cellBounds.yMin - 1, map.cellBounds.yMax + 1);
                    cellPosition = new Vector2Int(map.cellBounds.xMax + 1, y);
                    return true;
                }
            case "TOP":
                {
                    // tilemap bounds will be 2 less cuz we using the ground tilemap so walls not included
                    int x = Random.Range(map.cellBounds.xMin - 1, map.cellBounds.xMax + 1);
                    cellPosition = new Vector2Int(x, map.cellBounds.yMax + 1);
                    return true;
                }
            case "BOTTOM":
                {
                    // tilemap bounds will be 2 less cuz we using the ground tilemap so walls not included
                    int x = Random.Range(map.cellBounds.xMin - 1, map.cellBounds.xMax + 1);
                    cellPosition = new Vector2Int(x, map.cellBounds.yMin - 1);
                    return true;
                }

            default: return false;
        }
    }
}


