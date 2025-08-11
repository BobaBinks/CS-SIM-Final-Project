using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using System;

public class AStarPathfinder
{
    int width;
    int height;
    BoundsInt combinedBounds;
    List<AStarNode> nodes;

    public CorridorTiles corridorTiles;
    public Tilemap AStarGridTilemap { get; private set; }
    public Tilemap MiniMapTilemap { get; private set; }

    /// <summary>
    /// Marks traversible cells on the A* grid by checking tiles in dungeon rooms and corridor floor tilemaps.
    /// </summary>
    /// <param name="roomsDict">A dictionary mapping each dungeon room to it's corresponding GameObject Instance</param>
    /// <param name="corridorFloorTilemap">The tilemap containing the corridor floors</param>
    /// <param name="grid">The Grid used to convert local tile positions to world cell positions</param>
    /// <returns>True if operation completes successfully, otherwise false.</returns>
    public bool MarkTraversableCells(Dictionary<DungeonRoom, DungeonRoomInstance> roomsDict, Tilemap corridorFloorTilemap, Grid grid)
    {
        if (roomsDict == null || roomsDict.Count < 1 || AStarGridTilemap == null)
            return false;

        // getting traversible tiles in rooms
        foreach(var kvp in roomsDict)
        {
            DungeonRoomInstance roomInstance = kvp.Value;

            if (roomInstance == null || roomInstance.instance == null)
                continue;

            Vector3Int roomCellPosition = roomInstance.GetPositionInCell(grid);

            // get props map
            Transform propsWithCollisionTransform = roomInstance.instance.transform.Find("PropsCollision");
            Tilemap propsWithCollisionTilemap = null;

            if (propsWithCollisionTransform != null)
            {
                propsWithCollisionTilemap = propsWithCollisionTransform.GetComponent<Tilemap>();
            }

            if (propsWithCollisionTilemap == null || roomInstance.groundMap == null)
                continue;

            Tilemap wallMap = roomInstance.wallMap;
            List<Vector3Int> interactableObjectCellPositions;
            bool interactableObjectsObtained = GetInteractbleObjectsPositions(roomInstance, out interactableObjectCellPositions);


            // mark traversible cells
            foreach (var cell in roomInstance.groundMap.cellBounds.allPositionsWithin)
            {
                if (roomInstance.groundMap.GetTile(cell) == null || propsWithCollisionTilemap.GetTile(cell) != null)
                    continue;

                // this to check if there are walls inside the rooms excluding the outer walls
                if (wallMap && wallMap.GetTile(cell) != null)
                    continue;

                if (interactableObjectCellPositions.Contains(cell))
                    continue;

                Vector3Int cellWorldPosition = cell + roomCellPosition;
                int index = ConvertCellToIndex(cellWorldPosition);

                if (index == -1 || index >= nodes.Count)
                    continue;

                if(nodes[index] != null)
                {
                    nodes[index].occupied = false;
                    AStarGridTilemap.SetTile(cellWorldPosition, corridorTiles.corridorFloor);
                }

            }
        }

        corridorFloorTilemap.CompressBounds();
        // getting traversible tiles in corridors
        foreach (var cell in corridorFloorTilemap.cellBounds.allPositionsWithin)
        {
            if (corridorFloorTilemap.GetTile(cell) == null)
                continue;

            int index = ConvertCellToIndex(cell);

            if (index == -1 || index >= nodes.Count)
                continue;

            nodes[index].occupied = false;

            AStarGridTilemap.SetTile(cell, corridorTiles.corridorFloor);
        }

        return true;
    }

    public bool MarkMinimapCells(Dictionary<DungeonRoom, DungeonRoomInstance> roomsDict, Tilemap corridorFloorTilemap, Grid grid)
    {
        if (roomsDict == null || roomsDict.Count < 1 || MiniMapTilemap == null)
            return false;

        // getting traversible tiles in rooms
        foreach (var kvp in roomsDict)
        {
            DungeonRoomInstance roomInstance = kvp.Value;

            if (roomInstance == null || roomInstance.instance == null)
                continue;

            Vector3Int roomCellPosition = roomInstance.GetPositionInCell(grid);


            Tilemap wallMap = roomInstance.wallMap;

            // mark traversible cells
            foreach (var cell in roomInstance.groundMap.cellBounds.allPositionsWithin)
            {
                if (roomInstance.groundMap.GetTile(cell) == null)
                    continue;

                // this to check if there are walls inside the rooms excluding the outer walls
                if (wallMap && wallMap.GetTile(cell) != null)
                    continue;

                Vector3Int cellWorldPosition = cell + roomCellPosition;
                MiniMapTilemap.SetTile(cellWorldPosition, corridorTiles.corridorFloor);
            }
        }

        corridorFloorTilemap.CompressBounds();
        // getting traversible tiles in corridors
        foreach (var cell in corridorFloorTilemap.cellBounds.allPositionsWithin)
        {
            if (corridorFloorTilemap.GetTile(cell) == null)
                continue;

            MiniMapTilemap.SetTile(cell, corridorTiles.corridorFloor);
        }

        return true;
    }
    /// <summary>
    /// Gets the cell positions of the interactable objects in the interactable object container in the room
    /// </summary>
    /// <param name="roomInstance"></param>
    /// <param name="grid"></param>
    /// <param name="interactableObjects"></param>
    /// <returns></returns>
    private bool GetInteractbleObjectsPositions(DungeonRoomInstance roomInstance,out List<Vector3Int> interactableObjects)
    {
        interactableObjects = new List<Vector3Int>();

        if (roomInstance == null || roomInstance.instance == null)
            return false;

        Transform roomTransform = roomInstance.instance.transform;
        Transform interactableObjectTransform = null;

        // Search for interactable object parent transform in this room
        for(int i = 0; i < roomTransform.childCount; ++i)
        {
            Transform child = roomTransform.GetChild(i);
            if (child.CompareTag("InteractableObjects"))
            {
                interactableObjectTransform = child;
                break;
            }
        }

        if (interactableObjectTransform != null)
        {
            // Search for interactable object transforms in this room
            for (int i = 0; i < interactableObjectTransform.childCount; ++i)
            {
                Transform child = interactableObjectTransform.GetChild(i);
                Vector3Int cellPos = roomInstance.groundMap.WorldToCell(child.position);
                interactableObjects.Add(cellPos);
            }
        }

        return interactableObjects.Count > 0;
    }

    public bool InitializeGridDimensions(Dictionary<DungeonRoom, DungeonRoomInstance> roomsDict, Grid grid)
    {
        if (roomsDict == null || roomsDict.Count < 1)
            return false;

        BoundsInt combinedBounds;
        bool combinedBoundsRetrieved = TilemapHelper.GetCombinedBounds(roomsDict, grid, out combinedBounds);

        if (!combinedBoundsRetrieved)
            return false;

        // number of cells in grid
        width = combinedBounds.size.x;
        height = combinedBounds.size.y;

        int arraySize = width * height;

        // init grid
        nodes = new List<AStarNode>(arraySize);

        for (int y = combinedBounds.yMin; y < combinedBounds.yMax; y++)
        {
            for (int x = combinedBounds.xMin; x < combinedBounds.xMax; x++)
            {
                Vector3Int position = new Vector3Int(x, y);
                nodes.Add(new AStarNode(position));
            }
        }


        this.combinedBounds = combinedBounds;
        return true;
    }

    public int ConvertCellToIndex(Vector3Int cell)
    {
        if (width <= 0 || height <= 0 || combinedBounds == null) return -1;

            // compute offset based on whether width/height are even or odd

            int x = cell.x - combinedBounds.min.x;
            int y = cell.y - combinedBounds.min.y;

            // out of bounds check
            if (x < 0 || x >= width || y < 0 || y >= height)
                return -1;

            return y * width + x;
    }

    public Vector3Int ConvertIndexToCell(int index)
    {
        if (index < 0 || index >= width * height)
            throw new ArgumentOutOfRangeException("Index is out of bounds");

        int x = index % width;
        int y = index / width;

        return new Vector3Int(x + combinedBounds.min.x, y + combinedBounds.min.y);
    }

    public void SetAStarGridTilemap(Tilemap tilemap)
    {
        AStarGridTilemap = tilemap;
    }

    public void SetMiniMapTilemap(Tilemap tilemap)
    {
        MiniMapTilemap = tilemap;
    }

    public List<Vector3> GetShortestWorldPath(Vector3 startPos, Vector3 endPos)
    {
        // get current position in cell
        Vector3Int startCellPosition = AStarGridTilemap.WorldToCell(startPos);
        Vector3Int destinationCellPosition = AStarGridTilemap.WorldToCell(endPos);

        // get initial path
        List<Vector3Int> pathCells = GetShortestCellPath(startCellPosition, destinationCellPosition);
        return pathCells == null ? null : ConvertPathToWorldPositions(pathCells);
    }

    public List<Vector3Int> GetShortestCellPath(Vector3Int startCell, Vector3Int destinationCell)
    {
        if (nodes == null || nodes.Count < 1)
            return null;

        int startIndex = ConvertCellToIndex(startCell);
        int endIndex = ConvertCellToIndex(destinationCell);

        // check start and end index is valid
        if (startIndex < 0 || startIndex >= nodes.Count || endIndex < 0 || endIndex >= nodes.Count)
            return null;

        AStarMinHeap openList = new AStarMinHeap();
        HashSet<Vector3Int> closedList = new HashSet<Vector3Int>();

        AStarNode startNode = nodes[startIndex];
        AStarNode endNode = nodes[endIndex];
        startNode.g = 0;
        startNode.h = GetManhattenbDistance(startCell, destinationCell);
        startNode.f = startNode.h + startNode.g;
        startNode.prevNode = null;

        openList.Insert(startNode);

        while (!openList.IsEmpty())
        {
            AStarNode currNode = openList.ExtractMin();
            closedList.Add(currNode.position);

            if (currNode == endNode)
                break;

            // get neighbours
            List<AStarNode> neighbours = GetNeighbours(currNode.position);

            if (neighbours == null)
                continue;

            foreach(var neighbour in neighbours)
            {
                if (neighbour.occupied || closedList.Contains(neighbour.position))
                    continue;

                int newG = currNode.g + 1;

                // if new path to neighbour is shorter or neighbour not in open list
                bool notInOpen = !openList.Contains(neighbour.position);
                if (notInOpen || newG < neighbour.g)
                {
                    neighbour.h = GetManhattenbDistance(neighbour.position, destinationCell);
                    neighbour.g = newG;
                    neighbour.f = neighbour.g + neighbour.h;
                    neighbour.prevNode = currNode;

                    if (notInOpen)
                        openList.Insert(neighbour);
                    else
                        openList.Update(neighbour);
                }
            }
        }
        return ReconstructPath(endNode);
    }
    // iterate through corridor floor tiles
    // do same as room

    private int GetManhattenbDistance(Vector3Int startCell, Vector3Int destinationCell) 
    {
        return Mathf.Abs(destinationCell.x - startCell.x) + Mathf.Abs(destinationCell.y - startCell.y);
    }

    public List<Vector3Int> ReconstructPath(AStarNode endNode)
    {
        List<Vector3Int> path = new List<Vector3Int>();
        AStarNode current = endNode;
        while (current != null)
        {
            path.Add(current.position);
            current = current.prevNode;
        }
        path.Reverse();
        return path;
    }

    public List<Vector3> ConvertPathToWorldPositions(List<Vector3Int> path)
    {
        if (path == null || AStarGridTilemap == null)
            return null;

        List<Vector3> convertedPath = new List<Vector3>();
        foreach(var cell in path)
        {
            Vector3 worldPosition = AStarGridTilemap.GetCellCenterWorld(cell);
            convertedPath.Add(worldPosition);
        }

        return convertedPath;
    }

    private List<AStarNode> GetNeighbours(Vector3Int centerCell)
    {
        if (nodes == null || nodes.Count < 1)
            return null;

        List<AStarNode> neighbours = new List<AStarNode>();

        // iterate over 3x3 grid around the centerCell
        for (int xOffset = -1; xOffset <= 1; xOffset++)
        {
            for (int yOffset = -1; yOffset <= 1; yOffset++)
            {
                // skip the center cell itself
                if (xOffset == 0 && yOffset == 0)
                    continue;

                if (IsDiagonalMoveBlocked(centerCell, xOffset, yOffset))
                    continue;

                Vector3Int neighbourPos = new Vector3Int(centerCell.x + xOffset, centerCell.y + yOffset);
                int index = ConvertCellToIndex(neighbourPos);

                if (index >= 0 && index < nodes.Count)
                {
                    neighbours.Add(nodes[index]);
                }
            }
        }

        // get the top down left right neighbours
        //Vector3Int[] directions = new Vector3Int[]
        //{
        //    new Vector3Int(-1, 0, 0), // Left
        //    new Vector3Int(1, 0, 0),  // Right
        //    new Vector3Int(0, 1, 0),  // Up
        //    new Vector3Int(0, -1, 0)  // Down
        //};

        //foreach (var dir in directions)
        //{
        //    Vector3Int neighbourPos = centerCell + dir;
        //    int index = ConvertCellToIndex(neighbourPos);

        //    if (index >= 0 && index < nodes.Count)
        //    {
        //        neighbours.Add(nodes[index]);
        //    }
        //}

        return neighbours;
    }

    bool IsDiagonalMoveBlocked(Vector3Int center, int xOffset, int yOffset)
    {
        // not diagonal
        if (Mathf.Abs(xOffset) != 1 || Mathf.Abs(yOffset) != 1)
            return false;

        Vector3Int sideA = new Vector3Int(center.x + xOffset, center.y, 0);
        Vector3Int sideB = new Vector3Int(center.x, center.y + yOffset, 0);

        return !IsValidNeighbour(sideA) || !IsValidNeighbour(sideB);
    }

    bool IsValidNeighbour(Vector3Int position)
    {
        if (nodes == null || nodes.Count < 1)
            return false;
        // check bottom cell is walkable
        int index = ConvertCellToIndex(position);

        return index >= 0 && index < nodes.Count && !nodes[index].occupied;
    }
}
