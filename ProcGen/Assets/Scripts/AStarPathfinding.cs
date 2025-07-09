using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using System;

public class AStarPathfinding
{
    int width;
    int height;
    BoundsInt combinedBounds;
    List<bool> gridCells;
    // generate the 1D array representing the grid
    // initialize all cells in this array as occupied
    // the size will be dynamic, width = allRoom width * 2, same for height
    // because rooms can be in negative position, need to offset, when converting to index, divide width by 2 and add to x as offset
    // when converting 

    // iterate through each room,
        // get the ground tilemap and propwithcollision tilemap
        // iterate through tilesWithin of ground
            // check propwithcollision tilemap if the cell is occupied
                // if it is than continue to next cell
                // else
                    // check if the tile is null or not on groundtilemap
                    // if not null,
                    // convert that grid position to the index in 1D grid array (y * width + x)
                    // set that index's value to floor/traversable

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
        gridCells = new List<bool>(arraySize);
        for (int i = 0; i < arraySize; i++)
        {
            gridCells.Add(true);
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
            throw new ArgumentOutOfRangeException(nameof(index), "Index is out of bounds");

        int x = index % width;
        int y = index / width;

        return new Vector3Int(x + combinedBounds.min.x, y + combinedBounds.min.y);
    }

    // iterate through corridor floor tiles
    // do same as room
}
