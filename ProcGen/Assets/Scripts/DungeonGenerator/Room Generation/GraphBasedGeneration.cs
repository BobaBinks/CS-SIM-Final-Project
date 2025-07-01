using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

public class GraphBasedGeneration
{
    public static bool GenerateDungeon(DungeonLayout layout, Grid grid, GameObject roomsGO, Tilemap corridorTilemap, CorridorTiles corridorTiles,out Dictionary<DungeonRoom, DungeonRoomInstance> placedRooms, int maxPlacementFailCount = 5, int maxPrefabFailCount = 5)
    {
        placedRooms = new Dictionary<DungeonRoom, DungeonRoomInstance>();

        if (corridorTiles == null || corridorTilemap == null) return false;

        Queue<DungeonRoom> childPlacementQueue = new Queue<DungeonRoom>();

        // get entrance room
        DungeonRoom entranceRoom = layout.dungeonRoomList.Find((x) => { return x.roomType.name == "EntranceRoomType"; });

        // checks if entrance room is present in the layout
        if (entranceRoom == null)
        {
            Debug.Log("GraphBasedGeneration: Could not find entrance room in layout!");
            return false;
        }

        // to keep track of which edge of the room already has a corridor
        Dictionary<DungeonRoom, Dictionary<TilemapHelper.Edge, bool>> roomEdgeOccupiedTracker = CreateEdgeTracker(layout);

        if (roomEdgeOccupiedTracker == null) return false;

        // enqueue entrance room
        childPlacementQueue.Enqueue(entranceRoom);

        // place entrance room
        DungeonRoomInstance entranceRoomInstance = GetOrCreateDungeonRoomInstance(entranceRoom, placedRooms);

        int randomX = Random.Range(0, layout.width);
        int randomY = Random.Range(0, layout.height);

        // if could not instantiate the entrance room then stop generation
        if (!entranceRoomInstance.InstantiateInstance(new Vector3Int(randomX, randomY), grid, roomsGO))
            return false;

        // place and connect child rooms
        while(childPlacementQueue.Count > 0)
        {
            DungeonRoom currRoom = childPlacementQueue.Dequeue();

            // get available edges of current room
            Dictionary<TilemapHelper.Edge, bool> roomEdges = roomEdgeOccupiedTracker[currRoom];

            // https://learn.microsoft.com/en-us/dotnet/api/system.linq.enumerable.aggregate?view=net-9.0
            int availableEdgesCount = roomEdges.Aggregate(0, (count, edge) =>  edge.Value == false ? count + 1 : count );

            // list of available edges currently
            List<TilemapHelper.Edge> availableEdges = roomEdges.
                Where(kvp => kvp.Value == false).
                Select(kvp => kvp.Key).ToList();

            // check if there are enough available edges for the new connections
            if (currRoom.connectionList.Count > availableEdgesCount)
            {
                Debug.Log("GraphBasedGeneration: Not enough available edges");
                return false;
            }

            // get the current room instance
            DungeonRoomInstance currRoomInstance = GetOrCreateDungeonRoomInstance(currRoom, placedRooms);

            if (currRoomInstance == null || currRoomInstance.wallMap == null)
            {
                Debug.Log("Invalid current room instance");
                return false;
            }

            // place rooms and corridors
            foreach(DungeonRoom childRoom in currRoom.connectionList)
            {
                if(childRoom == null) continue;

                // pick a random available edge from curr room
                int edgeIndex = Random.Range(0, availableEdges.Count);
                TilemapHelper.Edge edge = availableEdges[edgeIndex];


                // pick a point in the edge for corridor
                Vector3Int cellPosition;
                bool currRoomEdgeCellPicked = TilemapHelper.PickEdgeCell(currRoomInstance.groundMap, edge, out cellPosition);


                if(currRoomEdgeCellPicked)
                    Debug.Log($"Edge cell {cellPosition + currRoomInstance.GetPositionInCell(grid)} picked for {currRoom.roomType.name}: {edge.ToString()}");
                Debug.Log($"Edge cell {cellPosition}");

                //corridorTilemap.SetTile(cellPosition + currRoomInstance.GetPositionInCell(grid), corridorTiles.BottomHorizontalWall);

                Debug.Log(currRoomInstance.GetPositionInCell(grid));

                bool childExisted = placedRooms.ContainsKey(childRoom);

                // try to get the child room instance if it exists
                DungeonRoomInstance childRoomInstance = GetOrCreateDungeonRoomInstance(childRoom, placedRooms);

                // if child room instance did not exist generate corridor using random walk
                if (!childExisted)
                {
                    // random walk
                    List<Vector3Int> corridorCells = RandomWalk.GetCorridorFloorCells(cellPosition, edge, childRoomInstance, maxIterations: 2);

                    bool generateCorridor = PlaceCorridorCells(corridorCells, corridorTiles, corridorTilemap, currRoomInstance, grid);
                    // place room at end of corridor on it's appropriate edge
                }
                else
                {
                    // if child room instance existed, use A star algorithm to generate a valid non overlapping path
                }


                // check for overlaps
                // placedRooms.Add(childRoom, childRoomInstance);

                // remove edge from available list
                availableEdges.Remove(edge);

                // update the edges occupation for both rooms

            }
            // place all the child rooms
        }

        return false;
    }

    private static bool InsertCorridorFloorTileEntranceToRoom(CorridorTiles corridorTiles, Vector3Int cell, DungeonRoomInstance dungeonRoomInstance)
    {
        return true;
    }

    private static bool PlaceCorridorCells(List<Vector3Int> corridorCells, CorridorTiles corridorTiles, Tilemap corridorTilemap, DungeonRoomInstance currRoomInstance, Grid grid)
    {
        if (corridorCells == null ||
            currRoomInstance == null ||
            corridorTiles == null ||
            corridorTilemap == null ||
            grid == null) return false;

        foreach (var corridorCell in corridorCells)
        {
            // cell location of the room in the grid
            Vector3Int roomCellPositionOffset = currRoomInstance.GetPositionInCell(grid);

            // invalid offset
            if (roomCellPositionOffset == Vector3Int.one * -1) return false;

            Vector3Int position = corridorCell + roomCellPositionOffset;

            corridorTilemap.SetTile(position, corridorTiles.corridorFloor);

            Debug.Log($"Corridor Cell: {position}");
        }
        return true;
    }

    private static DungeonRoomInstance GetOrCreateDungeonRoomInstance(DungeonRoom room, Dictionary<DungeonRoom, DungeonRoomInstance> placedRooms)
    {
        if(room == null || placedRooms == null) return null;

        // create a DungeonRoomInstance for this room if it does not exist
        if (!placedRooms.ContainsKey(room))
        {
            // pick a prefab from this room type
            GameObject roomPrefab = room.roomType.GetRoomRandomPrefab();

            if (roomPrefab == null)
            {
                Debug.Log($"{room.roomType.name} has no prefabs to choose from.");
                return null;
            }

            // create DungeonRoomInstance
            DungeonRoomInstance instance = new DungeonRoomInstance(roomPrefab, room);
            placedRooms.Add(room, instance);

            return instance;
        }

        return placedRooms[room];
    }
    private static Dictionary<DungeonRoom, Dictionary<TilemapHelper.Edge, bool>> CreateEdgeTracker(DungeonLayout layout)
    {
        if(layout == null) return null;

        Dictionary<DungeonRoom, Dictionary<TilemapHelper.Edge, bool>> roomEdgeOccupiedTracker = new Dictionary<DungeonRoom, Dictionary<TilemapHelper.Edge, bool>>();
        // populate the edge tracker
        foreach (DungeonRoom room in layout.dungeonRoomList)
        {
            Dictionary<TilemapHelper.Edge, bool> roomEdges = new Dictionary<TilemapHelper.Edge, bool> {{ TilemapHelper.Edge.LEFT, false },
                                                                            { TilemapHelper.Edge.RIGHT, false },
                                                                            { TilemapHelper.Edge.TOP, false },
                                                                            { TilemapHelper.Edge.BOTTOM, false },};

            roomEdgeOccupiedTracker[room] = roomEdges;
        }

        return roomEdgeOccupiedTracker;
    }
}
