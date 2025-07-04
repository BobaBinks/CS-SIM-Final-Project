using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

public class GraphBasedGeneration
{
    public static bool GenerateDungeon(DungeonLayout layout, Grid grid, GameObject roomsGO, Tilemap corridorFloorTilemap, Tilemap corridorWallmap, CorridorTiles corridorTiles,out Dictionary<DungeonRoom, DungeonRoomInstance> placedRooms, int maxPlacementFailCount = 5, int maxPrefabFailCount = 5, int corridorWidth = 5)
    {
        // reset the dungeon
        // clear all children in roomGO
        // clear all tiles in corridorTilemap
        ResetDungeon(roomsGO, corridorFloorTilemap);

        // to check for overlaps
        HashSet<Vector3Int> occupiedCells = new HashSet<Vector3Int>();

        // monitor unique connection placement
        List<RoomConnection> connectionsPlaced = new List<RoomConnection>();

        placedRooms = new Dictionary<DungeonRoom, DungeonRoomInstance>();

        if (corridorTiles == null || corridorFloorTilemap == null) return false;

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
        placedRooms.Add(entranceRoom, entranceRoomInstance);

        int randomX = Random.Range(0, layout.width);
        int randomY = Random.Range(0, layout.height);

        // if could not instantiate the entrance room then stop generation
        if (!entranceRoomInstance.InstantiateInstance(new Vector3Int(randomX, randomY), grid, roomsGO, occupiedCells))
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
            int connectionAlreadyPlacedCount = connectionsPlaced.Aggregate(0, (count, roomConnection) => roomConnection.CheckRoomInConnection(currRoom) == true ? count + 1 : count);
            // check if there are enough available edges for the new connections
            if (currRoom.connectionList.Count - connectionAlreadyPlacedCount > availableEdgesCount)
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
                Debug.Log("GraphBasedGeneration: Invalid current room instance");
                return false;
            }

            // place all the child rooms
            bool childRoomsPlacedAndConnected = ConnectAndPlaceChildRooms(currRoom,
                                                                        currRoomInstance,
                                                                        grid,
                                                                        roomsGO,
                                                                        corridorTiles,
                                                                        corridorFloorTilemap,
                                                                        corridorWallmap,
                                                                        placedRooms,
                                                                        roomEdgeOccupiedTracker,
                                                                        occupiedCells,
                                                                        roomVisited,
                                                                        childPlacementQueue,
                                                                        availableEdges,
                                                                        roomEdges,
                                                                        connectionsPlaced, corridorWidth);

            if (!childRoomsPlacedAndConnected)
            {
                Debug.Log("GraphBasedGeneration: Failed to place or connect child rooms.");
                return false;
            }
        }

        return true;
    }


    private static bool ConnectAndPlaceChildRooms(
                            DungeonRoom currRoom,
                            DungeonRoomInstance currRoomInstance,
                            Grid grid,
                            GameObject roomsGO,
                            CorridorTiles corridorTiles,
                            Tilemap corridorFloorTilemap,
                            Tilemap corridorWallTilemap,
                            Dictionary<DungeonRoom, DungeonRoomInstance> placedRooms,
                            Dictionary<DungeonRoom, Dictionary<TilemapHelper.Edge, bool>> roomEdgeOccupiedTracker,
                            HashSet<Vector3Int> occupiedCells,
                            List<DungeonRoom> roomVisited,
                            Queue<DungeonRoom> childPlacementQueue,
                            List<TilemapHelper.Edge> availableEdges,
                            Dictionary<TilemapHelper.Edge, bool> roomEdges,
                            List<RoomConnection> connectionsPlaced,
                            int maxPlacementFailCount = 5,
                            int corridorWidth = 5)
    {
        if (currRoom == null ||
            currRoomInstance == null ||
            grid == null ||
            roomsGO == null ||
            corridorTiles == null ||
            corridorFloorTilemap == null ||
            corridorWallTilemap == null ||
            placedRooms == null ||
            roomEdgeOccupiedTracker == null ||
            occupiedCells == null ||
            roomVisited == null ||
            childPlacementQueue == null ||
            availableEdges == null ||
            roomEdges == null ||
            connectionsPlaced == null)
        {
            Debug.LogError("ConnectAndPlaceChildRooms: One or more required arguments are null.");
            return false;
        }

        // place rooms and corridors
        foreach (DungeonRoom childRoom in currRoom.connectionList)
        {
            if (childRoom == null) continue;

            RoomConnection connection = new RoomConnection(currRoom, childRoom);

            // check the connection has not been placed already
            if (!RoomConnection.CheckConnectionUnique(connectionsPlaced, connection))
                continue;

            int placementAttempts = 0;

            while (placementAttempts < maxPlacementFailCount)
            {
                Debug.Log($"Attempt {placementAttempts} / {maxPlacementFailCount}: Connecting {currRoom.roomType.name} - {childRoom.roomType.name}");

                // pick a random available edge from curr room
                int edgeIndex = Random.Range(0, availableEdges.Count);
                TilemapHelper.Edge edge = availableEdges[edgeIndex];

                // pick a point in the edge for corridor
                Vector3Int cellPosition;
                bool currRoomEdgeCellPicked = TilemapHelper.PickEdgeCell(currRoomInstance.wallMap, edge, out cellPosition);

                if (!currRoomEdgeCellPicked)
                {
                    Debug.Log($"ConnectAndPlaceChildRooms attempt {placementAttempts}/{maxPlacementFailCount}: Could not pick edge cell for curr room");
                    placementAttempts++;
                    continue;
                }

                // replace the wall tiles at the corridor position with floor tiles instead
                InsertCorridorFloorTileEntranceToRoom(corridorTiles, cellPosition, edge, currRoomInstance);


                bool childExisted = placedRooms.ContainsKey(childRoom);

                // try to get the child room instance if it exists
                DungeonRoomInstance childRoomInstance = GetOrCreateDungeonRoomInstance(childRoom, placedRooms);

                TilemapHelper.Edge room2Edge = TilemapHelper.Edge.INVALID;

                // if child room instance did not exist generate corridor using random walk
                if (!childExisted)
                {
                    // random walk
                    List<List<Vector3Int>> corridor = RandomWalk.GetCorridorFloorCells(cellPosition, edge, childRoomInstance, maxIterations: 4, corridorWidth);

                    // check if corridor overlaps with other occupied tiles
                    bool corridorOverlap = CheckCorridorOverlap(currRoomInstance, grid, corridor, occupiedCells);

                    if (corridorOverlap)
                    {
                        Debug.Log($"Corridor from {currRoom.roomType.name}'s {edge} edge to {childRoom.roomType.name} could not be placed");
                        placementAttempts++;
                        continue;
                    }

                    // place room at end of corridor on it's appropriate edge
                    bool room2Placed = PlaceRoomAtCorridorEnd(corridor, currRoomInstance, childRoomInstance, grid, roomsGO, occupiedCells, corridorTiles, ref room2Edge, corridorWidth);

                    if (!room2Placed)
                    {
                        placementAttempts++;
                        Debug.Log($"Failed room2 placement attempt: childRoomInstance Clone:{childRoomInstance.instance}");
                        continue;
                    }

                    placedRooms.Add(childRoom, childRoomInstance);

                    bool generateCorridor = PlaceCorridor(corridor, corridorTiles, corridorFloorTilemap, corridorWallTilemap, currRoomInstance, grid, occupiedCells, corridorWidth);

                    if (!generateCorridor)
                    {
                        placementAttempts++;
                        continue;
                    }
                }
                else
                {
                    // if child room instance existed, use A star algorithm to generate a valid non overlapping path
                }

                // remove edge from available list after room and corridor placement succeeds
                availableEdges.Remove(edge);

                // update the edges occupation for both rooms
                roomEdges[edge] = true;
                roomEdgeOccupiedTracker[childRoom][room2Edge] = true;

                // add this connection to the connectionPlaced Tracker
                connectionsPlaced.Add(connection);


                // add child to queue child has not been visited before.
                if (!roomVisited.Contains(childRoom))
                    childPlacementQueue.Enqueue(childRoom);
                break;
            }

            if (placementAttempts == maxPlacementFailCount)
            {
                Debug.LogWarning($"ConnectAndPlaceChildRooms: Failed to place {childRoom.roomType.name} after {maxPlacementFailCount} attempts.");
                return false;
            }

        }

        return true;
    }

    private static bool PlaceRoomAtCorridorEnd(List<List<Vector3Int>> corridor,
        DungeonRoomInstance currRoom,
        DungeonRoomInstance room2,
        Grid grid,
        GameObject roomsGO,
        HashSet<Vector3Int> occupiedCells,
        CorridorTiles corridorTiles,
        ref TilemapHelper.Edge room2Edge, int corridorWidth = 5)
    {
        if (corridor == null ||
            currRoom == null ||
            room2 == null ||
            occupiedCells == null ||
            roomsGO == null || corridorWidth == 0) return false;
        // identify direction of corridor end

        // gets the last 2 corridor strips
        List<List<Vector3Int>> last2CorridorStrips = corridor.GetRange(corridor.Count - 2, 2);

        int centerIndex = corridorWidth / 2;

        // get center cell of each strip
        List<Vector3Int> last2CenterCells = new List<Vector3Int> 
        { 
            { last2CorridorStrips[0][centerIndex] },
            { last2CorridorStrips[1][centerIndex] } 
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
        Vector3Int room2EdgeCellPosition;
        bool edgeCellPicked = TilemapHelper.PickEdgeCell(room2.wallMap, room2Edge, out room2EdgeCellPosition);


        if (!edgeCellPicked) return false;

        // "place" room2 on the last corridorCell

        // calculate the corridor's final cell position in the global grid space,
        // by adding the room's grid offset to the last corridor cell (local to room)
        Vector3Int corridorEndPositionInGrid = last2CenterCells.Last() + currRoom.GetPositionInCell(grid);

        // calculate the difference between the edge cell and room2 center (prob just invert the edge cell)
        Vector3Int room2Position = corridorEndPositionInGrid;

        // offset the room2 position with the difference
        room2Position = room2Position - corridorOffset - room2EdgeCellPosition;

        if (room2.InstantiateInstance(room2Position, grid, roomsGO, occupiedCells))
        {
            InsertCorridorFloorTileEntranceToRoom(corridorTiles, room2EdgeCellPosition, room2Edge, room2);
            return true;
        }


        return false;
    }

    private static bool InsertCorridorFloorTileEntranceToRoom(CorridorTiles corridorTiles, Vector3Int cell, TilemapHelper.Edge edge, DungeonRoomInstance dungeonRoomInstance)
    {
        if (dungeonRoomInstance == null
            || corridorTiles == null
            || corridorTiles.corridorFloor == null)
            return false;

        // get edge axis
        TilemapHelper.Axis axis = TilemapHelper.GetEdgeAxis(edge);

        // get adjacent cells
        List<Vector3Int> adjacentCells = TilemapHelper.GetAdjacentCells(cell, axis);
        adjacentCells.Insert(1, cell);

        // replace the tiles in the wall map and ground map
        TileBase[] nullTiles = Enumerable.Repeat<TileBase>(null, adjacentCells.Count).ToArray();
        TileBase[] groundTiles = Enumerable.Repeat<TileBase>(corridorTiles.corridorFloor, adjacentCells.Count).ToArray();

        Vector3Int[] adjacentCellsArray = adjacentCells.ToArray();

        Transform wallsTransform = dungeonRoomInstance.instance.transform.Find("Walls");
        if(wallsTransform != null)
        {
            Tilemap wallMap = wallsTransform.GetComponent<Tilemap>();

            if(wallMap != null)
                wallMap.SetTiles(adjacentCellsArray, nullTiles);
        }

        Transform propsNoCollisionTransform = dungeonRoomInstance.instance.transform.Find("PropsNoCollision");
        if (propsNoCollisionTransform != null)
        {
            Tilemap propsNoCollisionMap = propsNoCollisionTransform.GetComponent<Tilemap>();

            if (propsNoCollisionMap != null)
            {
                Vector3Int offsetDir = Vector3Int.zero;
                // get edge
                switch (edge) 
                {
                    case TilemapHelper.Edge.LEFT:
                        {
                            offsetDir = Vector3Int.right;
                            break;
                        }
                    case TilemapHelper.Edge.RIGHT:
                        {
                            offsetDir = Vector3Int.left;
                            break;
                        }
                    case TilemapHelper.Edge.TOP:
                        {
                            offsetDir = Vector3Int.zero;
                            break;
                        }
                    case TilemapHelper.Edge.BOTTOM:
                        {
                            offsetDir = Vector3Int.up;
                            break;
                        }
                }

                Vector3Int[] propsCellArray = adjacentCellsArray.Select(cell => cell + offsetDir)
                                                                .ToArray();

                propsNoCollisionMap.SetTiles(propsCellArray, nullTiles);
            }

        }

        Transform groundTransform = dungeonRoomInstance.instance.transform.Find("Ground");
        if (groundTransform != null)
        {
            Tilemap groundMap = groundTransform.GetComponent<Tilemap>();

            if (groundMap != null)
                groundMap.SetTiles(adjacentCellsArray, groundTiles);
        }

        return true;
    }

    /// <summary>
    /// Resets the dungeon by clearing all corridor tiles and removing all instantiated rooms from roomsGO
    /// </summary>
    /// <param name="roomsGO"></param>
    /// <param name="corridorTilemap"></param>
    private static void ResetDungeon(GameObject roomsGO, Tilemap corridorTilemap)
    {
        if (roomsGO == null || corridorTilemap == null) return;

        for (int childIndex = roomsGO.transform.childCount - 1; childIndex >= 0; --childIndex)
        {
            GameObject.Destroy(roomsGO.transform.GetChild(childIndex).gameObject);
        }

        corridorTilemap.ClearAllTiles();
    }

    private static bool PlaceCorridor(List<List<Vector3Int>> corridor, CorridorTiles corridorTiles, Tilemap corridorFloorTilemap, Tilemap corridorWallTilemap, DungeonRoomInstance currRoomInstance, Grid grid, HashSet<Vector3Int> occupiedCells, int corridorWidth = 5)
    {
        if (corridor == null ||
            currRoomInstance == null ||
            corridorTiles == null ||
            corridorFloorTilemap == null ||
            corridorWallTilemap == null ||
            occupiedCells == null ||
            grid == null || 
            corridorWidth == 0) return false;

        foreach (var corridorStrip in corridor)
        {
            // cell location of the room in the grid
            Vector3Int roomCellPositionOffset = currRoomInstance.GetPositionInCell(grid);

            // invalid offset
            if (roomCellPositionOffset == Vector3Int.one * -1) return false;

            List<Vector3Int> corridorFloorCells = new List<Vector3Int>(corridorStrip);
            corridorFloorCells.Remove(corridorStrip.First());
            corridorFloorCells.Remove(corridorStrip.Last());

            // place floor cells
            foreach (var floorCell in corridorFloorCells)
            {
                Vector3Int position = floorCell + roomCellPositionOffset;

                corridorFloorTilemap.SetTile(position, corridorTiles.corridorFloor);
            }

            // place wall cells
            // iterate through the edge cells
            // get the 3x3 block of tile positions
            // the tiles in the 3x3 that are occupied/have ground tiles should be ignored
            // every thing else should be placed with wall tiles
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

    private static bool CheckCorridorOverlap(DungeonRoomInstance currRoomInstance, Grid grid, List<List<Vector3Int>> corridor, HashSet<Vector3Int> occupiedCells)
    {
        if (currRoomInstance == null ||
            grid == null ||
            corridor == null ||
            occupiedCells == null)
            return true;

        HashSet<Vector3Int> cellsToCheck = new HashSet<Vector3Int>();
        // check for overlap
        foreach (var corridorStrip in corridor)
        {
            // cell location of the room in the grid
            Vector3Int roomCellPositionOffset = currRoomInstance.GetPositionInCell(grid);

            // invalid offset
            if (roomCellPositionOffset == Vector3Int.one * -1) return false;

            foreach (var corridorCell in corridorStrip)
            {
                Vector3Int position = corridorCell + roomCellPositionOffset;
                cellsToCheck.Add(position);
            }
        }

        if (TilemapHelper.CheckOverlap(cellsToCheck, occupiedCells))
        {
            Debug.Log("Corridor overlap");
            return true;
        }

        // add corridor cells to occupiedCells
        occupiedCells.UnionWith(cellsToCheck);

        return false;
    }
}
