using UnityEngine;
using UnityEngine.Tilemaps;
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

    public bool InstantiateInstance(Vector3Int position, Grid grid, GameObject roomsGO)
    {
        if (prefab == null || grid == null || roomsGO == null)
        {
            Debug.LogError("Cannot instantiate room: Missing prefab, grid, or parent object.");
            return false;
        }

        Vector3 worldPosition = grid.CellToWorld(position);
        instance = GameObject.Instantiate(prefab, worldPosition, Quaternion.identity, roomsGO.transform);
        return true;
    }

    public Vector3Int GetPositionInCell(Grid grid) 
    {
        // return an invalid cell position
        if (instance == null) return Vector3Int.one * -1;

        return grid.WorldToCell(instance.transform.position);
    }
}
