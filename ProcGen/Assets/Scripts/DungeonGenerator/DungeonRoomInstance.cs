using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;

public class DungeonRoomInstance
{
    public GameObject prefab;
    public GameObject instance;
    public DungeonRoom room;
    public Tilemap groundMap;
    public Tilemap wallMap;

    public DungeonRoomInstance(GameObject prefab, DungeonRoom room)
    {
        this.prefab = prefab;
        this.room = room;

        Transform ground = prefab.transform.Find("Ground");

        if(ground != null)
        {
            groundMap = ground.GetComponent<Tilemap>();
        }

        Transform walls = prefab.transform.Find("Walls");

        if (walls != null)
        {
            wallMap = walls.GetComponent<Tilemap>();
        }
    }

    public bool InstantiateInstance(Vector3Int position, Grid grid, GameObject roomsGO, HashSet<Vector3Int> occupiedCells)
    {
        if (prefab == null || grid == null || roomsGO == null || occupiedCells == null || wallMap == null)
        {
            Debug.LogError("Cannot instantiate room: Missing prefab, grid, occupiedCells set, or parent object.");
            return false;
        }

        Vector3 worldPosition = grid.CellToWorld(position);
        // check for overlap

        // get the wallmap as the bounds of the room
        wallMap.CompressBounds();

        // retrieve all the cells within the bounds.
        HashSet<Vector3Int> roomCells = new HashSet<Vector3Int>();
        foreach (var pos in wallMap.cellBounds.allPositionsWithin)
        {
            roomCells.Add(pos);
        }

        // convert the local cell position to world cell position
        HashSet<Vector3Int> cellsToCheck = TilemapHelper.PopulateCellsToCheck(position, roomCells);

        // check overlap
        if (TilemapHelper.CheckOverlap(cellsToCheck, occupiedCells))
        {
            Debug.Log("DungeonRoomInstance: Room to be placed is overlapping with something!");
            return false;
        }

        // add cells to the occupied cells
        occupiedCells.UnionWith(cellsToCheck);
        instance = GameObject.Instantiate(prefab, worldPosition, Quaternion.identity, roomsGO.transform);

        if(instance != null)
        {
            // update the wall and groundmap to the instantiated one instead of the prefab
            Transform walls = instance.transform.Find("Walls");

            if (walls != null)
            {
                wallMap = walls.GetComponent<Tilemap>();
            }

            Transform ground = instance.transform.Find("Ground");

            if (ground != null)
            {
                groundMap = ground.GetComponent<Tilemap>();
            }
        }

        return instance != null;
    }

    public Vector3Int GetPositionInCell(Grid grid) 
    {
        // return an invalid cell position
        if (instance == null) return Vector3Int.one * -1;

        return grid.WorldToCell(instance.transform.position);
    }
}
