using UnityEngine;
using System.Collections.Generic;
public class AStarPathfinding: MonoBehaviour
{
    int width;
    int height;

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

    public bool  InitializeGridDimensions(List<DungeonRoomInstance> rooms)
    {
        if (rooms == null || rooms.Count < 1)
            return false;


        return true;
    }

    public int ConvertCellToIndex(Vector3Int cell)
    {
        if (width <= 0 || height <= 0) return -1;

        // compute offset based on whether width/height are even or odd
        Vector2Int offset = CalculateOffset(width, height);

        if (offset == Vector2Int.one * -1)
            return -1;

        // map from (-width/2 - width/2) to (0 - width)
        int x = cell.x + offset.x;

        // map from (-height/2 - height/2) to (0 - height)
        int y = cell.y + offset.y;

        // out of bounds check
        if (x < 0 || x >= width || y < 0 || y >= height)
            return -1;

        return y * width + x;
    }

    public Vector3Int ConvertIndexToCell(int index, out bool success)
    {
        success = false;

        int x = index % width;
        int y = index / width;

        // compute offset based on whether width/height are even or odd
        Vector2Int offset = CalculateOffset(width, height);

        if (offset == Vector2Int.one * -1)
            return Vector3Int.zero;

        return new Vector3Int(x - offset.x, y - offset.y);
    }

    public Vector2Int CalculateOffset(int width, int height)
    {
        if (width <= 0 || height <= 0)
            return Vector2Int.one * -1;

        // compute offset based on whether width/height are even or odd
        int xOffset = width % 2 == 0 ? (width - 1) / 2 : width / 2;
        int yOffset = height % 2 == 0 ? (height - 1) / 2 : height / 2;

        return new Vector2Int(xOffset, yOffset);
    }

    // iterate through corridor floor tiles
    // do same as room
}
