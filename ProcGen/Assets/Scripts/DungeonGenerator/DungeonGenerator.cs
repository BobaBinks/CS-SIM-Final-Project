using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
public class DungeonGenerator : MonoBehaviour
{
    [SerializeField]
    private DungeonLayout layout;

    [SerializeField]
    private Grid grid;

    [SerializeField]
    private GameObject roomsGO;

    [SerializeField]
    private Tilemap corridorTilemap;

    [Header("RandomWalk Parameters")]
    [SerializeField] int maxCorridorPathIterations = 50;
    [SerializeField] int maxPairFail = 5;
    [SerializeField] int maxConnectionFail = 5;

    // [Header("Corridor Tiles")]
    public CorridorTiles corridorTiles;

    private Dictionary<DungeonRoom, Tilemap> roomsDict;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GenerateDungeon();
    }

    void GenerateDungeon()
    {
        if (layout == null || corridorTilemap == null || roomsGO == null) return;
        //RandomRoomPlacement.GenerateRooms(layout, grid, out rooms, maxPlacementFailCount: 3);
        while (true)
        {
            // room generation
            if (RandomRoomPlacement.GenerateRooms(layout, grid, roomsGO, out roomsDict, maxPlacementFailCount: 3))
            {
                // corridor generation
                RandomWalk.GenerateCorridors(layout, grid, roomsDict, corridorTilemap, corridorTiles, maxCorridorPathIterations, maxPairFail, maxConnectionFail);
                return;
            }
        }
    }
}

[System.Serializable]
public class CorridorTiles
{
    public Tile corridorFloor;
    public Tile TopHorizontalWall;
    public Tile BottomHorizontalWall;

    public Tile LeftVerticalWall;
    public Tile RightVerticalWall;

    public Tile TopLeftCornerWall;
    public Tile TopRightCornerWall;

}