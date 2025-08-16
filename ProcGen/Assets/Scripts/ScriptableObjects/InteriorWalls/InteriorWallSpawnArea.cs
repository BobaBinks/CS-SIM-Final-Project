using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

[RequireComponent(typeof(Tilemap))]
public class InteriorWallSpawnArea : MonoBehaviour
{
    public Tilemap InteriorWallSpawnAreaTilemap { get; private set; }
    [SerializeField] Tilemap wallMap;

    private void Awake()
    {
        InteriorWallSpawnAreaTilemap = GetComponent<Tilemap>();
    }

    public void PlaceInteriorWalls(InteriorWallSet wallSet)
    {
        if (wallSet == null ||
            InteriorWallSpawnAreaTilemap == null
            || wallMap == null)
            return;

        InteriorWallSpawnAreaTilemap.CompressBounds();

        // Get all cells in spawn area
        List<Vector3Int> spawnCells = new List<Vector3Int>();
        foreach (var pos in InteriorWallSpawnAreaTilemap.cellBounds.allPositionsWithin)
        {
            if (InteriorWallSpawnAreaTilemap.GetTile(pos) != null)
                spawnCells.Add(pos);
        }

        if (spawnCells.Count == 0)
            return;

        int maxAttempts = 5;
        int maxNumOfInteriorWalls = 4;
        int maxTurns = 2;
        int minSteps = 2;
        int maxSteps = 5;

        // make sure minSteps is at least 1
        minSteps = Mathf.Max(minSteps, 1);

        // make sure maxSteps is not smaller than minSteps
        maxSteps = Mathf.Max(minSteps, maxSteps);


        int numOfInteriorWalls = Random.Range(1, maxNumOfInteriorWalls + 1);

        HashSet<Vector3Int> occupiedCells = new HashSet<Vector3Int>();

        for(int i = 0; i < numOfInteriorWalls; ++i)
        {
            // keep track of all the cells belonging to this wall
            // will use it to update the occupiedWalls List and spawnArea list
            HashSet<Vector3Int> wallCellPositions = new HashSet<Vector3Int>();

            // pick a random cell
            Vector3Int currCell = spawnCells[Random.Range(0, spawnCells.Count)];

            List<Vector3Int> currCellMooresNeighbour = GetMooresNeighbour(currCell);

            // verify cell is valid
            if (occupiedCells.Contains(currCell) ||
                !IsValidWallCell(currCell, currCellMooresNeighbour, spawnCells, occupiedCells))
                continue;

            // add the currCell and its neighbours to wallCell positions
            wallCellPositions.Add(currCell);
            wallCellPositions.UnionWith(currCellMooresNeighbour);

            // do random walk
            int turns = Random.Range(1, maxTurns + 1);

            for (int turn = 0; turn < turns; ++turn)
            {
                int steps = Random.Range(minSteps, maxSteps + 1);
                Vector3Int dir = GetRandomDirection();
                for(int step = 0; step < steps; ++step)
                {
                    Vector3Int nextCell = currCell + dir;
                    List<Vector3Int> nextCellMooresNeighbour = GetMooresNeighbour(nextCell);

                    // out of bounds check and 
                    if (!IsValidWallCell(nextCell, nextCellMooresNeighbour, spawnCells, occupiedCells))
                    {
                        break;
                    }

                    currCell += dir;
                    wallCellPositions.Add(nextCell);
                    wallCellPositions.UnionWith(nextCellMooresNeighbour);
                }
            }

            // update the occupiedWalls List and spawnArea list
            foreach(var cell in wallCellPositions)
            {
                occupiedCells.Add(cell);
                spawnCells.Remove(cell);
            }
        }

        // place wall tiles
        foreach(var cell in occupiedCells)
        {
            wallMap.SetTile(cell, wallSet.emptySpace);
        }
    }

    private bool IsValidWallCell(Vector3Int cell, List<Vector3Int> mooresNeighbours, List<Vector3Int> spawnCells, HashSet<Vector3Int> occupiedCells)
    {
        if (mooresNeighbours == null || mooresNeighbours.Count != 8)
            return false;

        if (!spawnCells.Contains(cell) || occupiedCells.Contains(cell))
            return false;

        foreach(var neighbour in mooresNeighbours)
        {
            if (!spawnCells.Contains(neighbour) || occupiedCells.Contains(neighbour))
                return false;

            // check the neighbour's neighbour
            foreach (var second in GetMooresNeighbour(neighbour))
            {
                if (occupiedCells.Contains(second))
                    return false;
            }
        }

        return true;
    }

    private Vector3Int GetRandomDirection()
    {
        Vector3Int[] dirs = {
            new Vector3Int(1, 0, 0),
            new Vector3Int(-1, 0, 0),
            new Vector3Int(0, 1, 0),
            new Vector3Int(0, -1, 0)
        };

        return dirs[Random.Range(0, dirs.Length)];
    }

    private int GetCellIndex(Vector3Int position, List<Vector3Int> cells)
    {
        if (cells == null || cells.Count == 0 || !cells.Contains(position))
            return -1;

        return cells.FindIndex((x) => { return x == position; });
    }

    public List<Vector3Int> GetMooresNeighbour(Vector3Int centerCell)
    {
        List<Vector3Int> neighbours = new List<Vector3Int>();

        // iterate over 3x3 grid around the centerCell
        for (int xOffset = -1; xOffset <= 1; xOffset++)
        {
            for (int yOffset = -1; yOffset <= 1; yOffset++)
            {
                // skip the center cell itself
                if (xOffset == 0 && yOffset == 0)
                    continue;

                Vector3Int neighbourPos = new Vector3Int(centerCell.x + xOffset, centerCell.y + yOffset);
                neighbours.Add(neighbourPos);
            }
        }
        return neighbours;
    }
}
