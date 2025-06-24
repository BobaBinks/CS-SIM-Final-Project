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
    private Tilemap corridorTilemap;

    // [Header("Corridor Tiles")]
    public CorridorTiles corridorTiles;

    private Dictionary<DungeonRoom, Tilemap> rooms;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GenerateDungeon();
    }



    void GenerateDungeon()
    {
        if (layout == null) return;
        //RandomRoomPlacement.GenerateRooms(layout, grid, out rooms, maxPlacementFailCount: 3);
        while (true)
        {
            // room generation
            if (RandomRoomPlacement.GenerateRooms(layout, grid, out rooms, maxPlacementFailCount: 3))
            {
                // corridor generation
                RandomWalk.GenerateCorridors(layout, grid, rooms, corridorTilemap, corridorTiles);
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