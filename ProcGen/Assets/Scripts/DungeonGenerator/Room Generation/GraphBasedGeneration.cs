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

        // get a tracker for visited rooms to keep track of which room to enqueue
        List<DungeonRoom> roomVisited = new List<DungeonRoom>();

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
            roomVisited.Add(currRoom);

            // get available edges of current room
            Dictionary<TilemapHelper.Edge, bool> roomEdges = roomEdgeOccupiedTracker[currRoom];

            // https://learn.microsoft.com/en-us/dotnet/api/system.linq.enumerable.aggregate?view=net-9.0
            int availableEdgesCount = roomEdges.Aggregate(0, (count, edge) =>  edge.Value == false ? count + 1 : count );

            // check if there are enough available edges for the new connections
            if (currRoom.connectionList.Count > availableEdgesCount)
            {
                Debug.Log("GraphBasedGeneration: Not enough available edges");
                return false;
            }

            // list of available edges currently
            List<TilemapHelper.Edge> availableEdges = roomEdges.
                Where(kvp => kvp.Value == false).
                Select(kvp => kvp.Key).ToList();

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

                TilemapHelper.Edge room2Edge = TilemapHelper.Edge.INVALID;

                // if child room instance did not exist generate corridor using random walk
                if (!childExisted)
                {
                    // random walk
                    List<List<Vector3Int>> corridor = RandomWalk.GetCorridorFloorCells(cellPosition, edge, childRoomInstance, maxIterations: 4);

                    bool generateCorridor = PlaceCorridor(corridor, corridorTiles, corridorTilemap, currRoomInstance, grid);

                    // place room at end of corridor on it's appropriate edge

                    bool room2Placed = PlaceRoomAtCorridorEnd(corridor, currRoomInstance, childRoomInstance, grid, roomsGO, ref room2Edge);

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
                roomEdges[edge] = true;
                roomEdgeOccupiedTracker[childRoom][room2Edge] = true;


                // add child to queue child has not been visited before.
                if(!roomVisited.Contains(childRoom))
                    childPlacementQueue.Enqueue(childRoom);
            }
            // place all the child rooms
        }

        return false;
    }

    private static bool PlaceRoomAtCorridorEnd(List<List<Vector3Int>> corridor, DungeonRoomInstance currRoom, DungeonRoomInstance room2, Grid grid, GameObject roomsGO, ref TilemapHelper.Edge room2Edge)
    {
        if (corridor == null ||
            currRoom == null ||
            room2 == null ||
            roomsGO == null) return false;
        // identify direction of corridor end

        // gets the last 2 corridor strips
        List<List<Vector3Int>> last2CorridorStrips = corridor.GetRange(corridor.Count - 2, 2);

        // get center cell of each strip
        List<Vector3Int> last2CenterCells = new List<Vector3Int> 
        { 
            { last2CorridorStrips[0][1] },
            { last2CorridorStrips[1][1] } 
        };

        Vector3 dir = last2CenterCells[1] - last2CenterCells[0];
        dir = dir.normalized;

        Vector3Int corridorOffset;

        // identify edge of room2
        if (dir == Vector3.down)
        {
            // room2 should use top edge
            room2Edge = TilemapHelper.Edge.TOP;
            corridorOffset = Vector3Int.up;
        }
        else if (dir == Vector3.up)
        {
            // room2 should use bottom edge
            room2Edge = TilemapHelper.Edge.BOTTOM;
            corridorOffset = Vector3Int.down;
        }
        else if (dir == Vector3.left)
        {
            // room2 should use right edge
            room2Edge = TilemapHelper.Edge.RIGHT;
            corridorOffset = Vector3Int.right;
        }
        else if (dir == Vector3.right)
        {
            // room2 should use left edge
            room2Edge = TilemapHelper.Edge.LEFT;
            corridorOffset = Vector3Int.left;
        }
        else
        {
            Debug.Log("Could not obtain direction");
            room2Edge = TilemapHelper.Edge.INVALID;
            return false;
        }

        // pick a edge cell for room2
        Vector3Int cellPosition;
        bool edgeCellPicked = TilemapHelper.PickEdgeCell(room2.groundMap, room2Edge, out cellPosition);


        if (!edgeCellPicked) return false;

        // "place" room2 on the last corridorCell

        // calculate the corridor's final cell position in the global grid space,
        // by adding the room's grid offset to the last corridor cell (local to room)
        Vector3Int corridorEndPositionInGrid = last2CenterCells.Last() + currRoom.GetPositionInCell(grid);

        // calculate the difference between the edge cell and room2 center (prob just invert the edge cell)
        Vector3Int room2Position = corridorEndPositionInGrid;

        // offset the room2 position with the difference
        room2Position = room2Position - cellPosition - corridorOffset;

        room2.InstantiateInstance(room2Position, grid, roomsGO);

        return true;
    }

    private static bool InsertCorridorFloorTileEntranceToRoom(CorridorTiles corridorTiles, Vector3Int cell, DungeonRoomInstance dungeonRoomInstance)
    {
        return true;
    }

    private static bool PlaceCorridor(List<List<Vector3Int>> corridor, CorridorTiles corridorTiles, Tilemap corridorTilemap, DungeonRoomInstance currRoomInstance, Grid grid)
    {
        if (corridor == null ||
            currRoomInstance == null ||
            corridorTiles == null ||
            corridorTilemap == null ||
            grid == null) return false;

        foreach (var corridorStrip in corridor)
        {
            // cell location of the room in the grid
            Vector3Int roomCellPositionOffset = currRoomInstance.GetPositionInCell(grid);

            // invalid offset
            if (roomCellPositionOffset == Vector3Int.one * -1) return false;

            foreach(var corridorCell in corridorStrip)
            {
                Vector3Int position = corridorCell + roomCellPositionOffset;

                corridorTilemap.SetTile(position, corridorTiles.corridorFloor);

                Debug.Log($"Corridor Cell: {position}");
            }
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
