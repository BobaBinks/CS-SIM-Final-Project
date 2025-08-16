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

    public void PlaceInteriorWalls(InteriorWallSet wallSet,
        int maxNumOfInteriorWalls = 4,
        int maxTurns = 2,
        int minSteps = 2,
        int maxSteps = 5,
        int maxAttempts = 5)
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

        #region Input Sanitization
        // make sure minSteps is at least 1
        minSteps = Mathf.Max(minSteps, 1);

        // make sure maxSteps is not smaller than minSteps
        maxSteps = Mathf.Max(minSteps, maxSteps);

        // make sure maxAttempts is at least 1
        maxAttempts = Mathf.Max(maxAttempts, 1);

        // make sure maxTurns is at least 1
        maxTurns = Mathf.Max(maxTurns, 1);

        // make sure maxNumOfInteriorWalls is at least 1
        maxNumOfInteriorWalls = Mathf.Max(maxNumOfInteriorWalls, 1);
        #endregion

        int numOfInteriorWalls = Random.Range(1, maxNumOfInteriorWalls + 1);

        HashSet<Vector3Int> occupiedCells = new HashSet<Vector3Int>();

        for(int i = 0; i < numOfInteriorWalls; ++i)
        {
            int attempts = 0;

            while(attempts < maxAttempts)
            {
                if (PlaceInteriorWall(spawnCells, occupiedCells, maxTurns, minSteps, maxSteps))
                    break;
                attempts++;
            }

        }

        PaintWallTiles(wallMap, wallSet, occupiedCells);
    }

    private bool IsEmptySpace(Vector3Int position, HashSet<Vector3Int> occupiedCells)
    {
        if (occupiedCells == null || occupiedCells.Count == 0)
            return false;

        List<Vector3Int> neighbours = GetMooresNeighbour(position);

        if (neighbours == null || neighbours.Count < 8)
            return false;

        foreach(var neighbour in neighbours)
        {
            if (!occupiedCells.Contains(neighbour))
                return false;
        }
        return true;
    }

    private void PaintWallTiles(Tilemap wallMap, InteriorWallSet wallSet, HashSet<Vector3Int> occupiedCells)
    {
        if (wallMap == null || wallSet == null || occupiedCells == null || occupiedCells.Count == 0)
            return;

        // assign the correct tile for corridor connection

        #region comments
        //foreach (var wallCell in wallCellToReplace)
        //{
        //    bool hasWallLeft = wallMap.GetTile(wallCell + Vector3Int.left) != null;
        //    bool hasWallRight = wallMap.GetTile(wallCell + Vector3Int.right) != null;
        //    bool hasWallTop = wallMap.GetTile(wallCell + Vector3Int.up) != null;
        //    bool hasWallBottom = wallMap.GetTile(wallCell + Vector3Int.down) != null;



        //    TilemapHelper.Corner corner = TilemapHelper.IsCornerEdgeWall(wallCell, wallMap);

        //    // if corner edge then ignore.
        //    if (corner == TilemapHelper.Corner.INVALID)
        //    {
        //        if (edge == TilemapHelper.Edge.LEFT)
        //        {
        //            if (hasWallTop)
        //            {
        //                wallMap.SetTile(wallCell, corridorTiles.TopHorizontalWall);
        //            }
        //            else if (hasWallBottom)
        //            {
        //                wallMap.SetTile(wallCell, corridorTiles.TurningBottomLeftCornerWall);
        //            }
        //        }
        //        else if (edge == TilemapHelper.Edge.RIGHT)
        //        {
        //            if (hasWallTop)
        //            {
        //                wallMap.SetTile(wallCell, corridorTiles.TopHorizontalWall);
        //            }
        //            else if (hasWallBottom)
        //            {
        //                wallMap.SetTile(wallCell, corridorTiles.TurningBottomRightCornerWall);
        //            }
        //        }
        //        else if (edge == TilemapHelper.Edge.TOP)
        //        {
        //            wallMap.SetTile(wallCell, corridorTiles.TopHorizontalWall);
        //        }
        //        else if (edge == TilemapHelper.Edge.BOTTOM)
        //        {
        //            if (hasWallLeft)
        //            {
        //                wallMap.SetTile(wallCell, corridorTiles.TurningBottomLeftCornerWall);
        //            }
        //            else
        //            {
        //                wallMap.SetTile(wallCell, corridorTiles.TurningBottomRightCornerWall);
        //            }
        //        }
        //    }
        //    else
        //    {
        //        // check if there is another corridor sharing the corner
        //        if (groundTransform)
        //        {
        //            Tilemap groundMap = groundTransform.GetComponent<Tilemap>();
        //            bool hasGroundLeft = groundMap.GetTile(wallCell + Vector3Int.left) != null;
        //            bool hasGroundRight = groundMap.GetTile(wallCell + Vector3Int.right) != null;
        //            bool hasGroundTop = groundMap.GetTile(wallCell + Vector3Int.up) != null;
        //            bool hasGroundBottom = groundMap.GetTile(wallCell + Vector3Int.down) != null;

        //            if ((hasGroundRight && hasGroundBottom) || (hasGroundLeft && hasGroundBottom))
        //            {
        //                // top left or top right
        //                wallMap.SetTile(wallCell, corridorTiles.TopHorizontalWall);
        //            }
        //            else if (hasGroundRight && hasGroundTop)
        //            {
        //                // bottom left
        //                wallMap.SetTile(wallCell, corridorTiles.TurningBottomLeftCornerWall);
        //            }
        //            else if (hasGroundLeft && hasGroundTop)
        //            {
        //                // bottom right
        //                wallMap.SetTile(wallCell, corridorTiles.TurningBottomRightCornerWall);
        //            }
        //            else
        //            {
        //                // if there are no other corridors sharing this corner
        //                if (edge == TilemapHelper.Edge.BOTTOM)
        //                {
        //                    if (corner == TilemapHelper.Corner.BOTTOM_RIGHT)
        //                    {
        //                        // bottom right corner
        //                        wallMap.SetTile(wallCell, corridorTiles.RightVerticalWall);
        //                    }
        //                    else
        //                    {
        //                        // bottom left corner
        //                        wallMap.SetTile(wallCell, corridorTiles.LeftVerticalWall);
        //                    }
        //                }
        //                else if (edge == TilemapHelper.Edge.TOP)
        //                {
        //                    if (corner == TilemapHelper.Corner.TOP_LEFT)
        //                    {
        //                        // top left corner
        //                        wallMap.SetTile(wallCell, corridorTiles.LeftVerticalWall);
        //                    }
        //                    else if (hasWallLeft && hasWallBottom)
        //                    {
        //                        // top right corner
        //                        wallMap.SetTile(wallCell, corridorTiles.RightVerticalWall);
        //                    }
        //                }
        //                else if (edge == TilemapHelper.Edge.LEFT)
        //                {
        //                    if (corner == TilemapHelper.Corner.TOP_LEFT)
        //                    {
        //                        // top left corner
        //                        wallMap.SetTile(wallCell, corridorTiles.TopHorizontalWall);
        //                    }
        //                    else
        //                    {
        //                        // bottom left corner
        //                        wallMap.SetTile(wallCell, corridorTiles.BottomHorizontalWall);
        //                    }
        //                }
        //                else if (edge == TilemapHelper.Edge.RIGHT)
        //                {
        //                    if (corner == TilemapHelper.Corner.TOP_RIGHT)
        //                    {
        //                        // top right corner
        //                        wallMap.SetTile(wallCell, corridorTiles.TopHorizontalWall);
        //                    }
        //                    else
        //                    {
        //                        // bottom left corner
        //                        wallMap.SetTile(wallCell, corridorTiles.BottomHorizontalWall);
        //                    }
        //                }
        //            }
        //        }


        //    }
        //}
        #endregion

        HashSet<Vector3Int> emptySpaces = new HashSet<Vector3Int>();

        // paint empty spaces first
        foreach (var cell in occupiedCells)
        {
            if (IsEmptySpace(cell, occupiedCells))
            {
                wallMap.SetTile(cell, wallSet.emptySpace);
                emptySpaces.Add(cell);
            }
        }

        occupiedCells.ExceptWith(emptySpaces);

        // paint wall tiles
        foreach (var cell in occupiedCells)
        {
            //if (emptySpaces.Contains(cell))
            //    continue;

            bool hasWallLeft = occupiedCells.Contains(cell + Vector3Int.left);
            bool hasWallRight = occupiedCells.Contains(cell + Vector3Int.right);
            bool hasWallTop = occupiedCells.Contains(cell + Vector3Int.up);
            bool hasWallBottom = occupiedCells.Contains(cell + Vector3Int.down);

            bool hasEmptySpaceLeft = emptySpaces.Contains(cell + Vector3Int.left);
            bool hasEmptySpaceRight = emptySpaces.Contains(cell + Vector3Int.right);
            bool hasEmptySpaceTop = emptySpaces.Contains(cell + Vector3Int.up);
            bool hasEmptySpaceBottom = emptySpaces.Contains(cell + Vector3Int.down);

            bool hasEmptySpaceBottomLeft = emptySpaces.Contains(cell + Vector3Int.left + Vector3Int.down);
            bool hasEmptySpaceBottomRight = emptySpaces.Contains(cell + Vector3Int.right + Vector3Int.down);
            bool hasEmptySpaceTopLeft = emptySpaces.Contains(cell + Vector3Int.left + Vector3Int.up);
            bool hasEmptySpaceTopRight = emptySpaces.Contains(cell + Vector3Int.right + Vector3Int.up);

            Tile tileToPlace = wallSet.emptySpace;

            // handle standard top,left,right,down wall
            if (hasWallLeft && hasWallRight && hasEmptySpaceBottom)
            {
                // top wall
                tileToPlace = wallSet.topWall;
            }
            else if(hasWallLeft && hasWallRight && hasEmptySpaceTop)
            {
                // bottom wall
                tileToPlace = wallSet.bottomWall;
            }
            else if(hasWallTop && hasWallBottom && hasEmptySpaceRight)
            {
                // left wall
                tileToPlace = wallSet.leftWall;
            }
            else if(hasWallTop && hasWallBottom && hasEmptySpaceLeft)
            {
                // right wall
                tileToPlace = wallSet.rightWall;
            }

            // handle corner wall
            if (hasWallBottom && hasWallRight && hasEmptySpaceBottomRight)
            {
                // top left corner
                tileToPlace = wallSet.topLeftCornerWall;
            }
            else if (hasWallBottom && hasWallLeft && hasEmptySpaceBottomLeft)
            {
                // top right corner
                tileToPlace = wallSet.topRightCornerWall;
            }
            else if (hasWallTop && hasWallRight && hasEmptySpaceTopRight)
            {
                // bottom left corner
                tileToPlace = wallSet.bottomLeftCornerWall;
            }
            else if (hasWallTop && hasWallLeft && hasEmptySpaceTopLeft)
            {
                // bottom right corner
                tileToPlace = wallSet.bottomRightCornerWall;
            }

            // handle L shape corners
            if (hasWallLeft && hasWallBottom && hasEmptySpaceTop && hasEmptySpaceRight)
            {
                // top left L corner
                tileToPlace = wallSet.topLeftInvertedCornerWall;
            }
            else if (hasWallBottom && hasWallRight && hasEmptySpaceTop && hasEmptySpaceLeft)
            {
                // top right L corner
                tileToPlace = wallSet.topRightInvertedCornerWall;
            }
            else if (hasWallTop && hasWallRight && hasEmptySpaceBottom && hasEmptySpaceLeft)
            {
                // bottom left L corner
                tileToPlace = wallSet.bottomLeftInvertedCornerWall;
            }
            else if (hasWallTop && hasWallLeft && hasEmptySpaceBottom && hasEmptySpaceRight)
            {
                // bottom right L corner
                tileToPlace = wallSet.bottomRightInvertedCornerWall;
            }

            wallMap.SetTile(cell, tileToPlace);
        }

    }





    private bool PlaceInteriorWall(List<Vector3Int> spawnCells, HashSet<Vector3Int> occupiedCells, int maxTurns = 2, int minSteps = 2, int maxSteps = 5)
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
            return false;

        // add the currCell and its neighbours to wallCell positions
        wallCellPositions.Add(currCell);
        wallCellPositions.UnionWith(currCellMooresNeighbour);

        // do random walk
        int turns = Random.Range(1, maxTurns + 1);

        for (int turn = 0; turn < turns; ++turn)
        {
            int steps = Random.Range(minSteps, maxSteps + 1);
            Vector3Int dir = GetRandomDirection();
            for (int step = 0; step < steps; ++step)
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
        foreach (var cell in wallCellPositions)
        {
            occupiedCells.Add(cell);
            spawnCells.Remove(cell);
        }

        return true;
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
