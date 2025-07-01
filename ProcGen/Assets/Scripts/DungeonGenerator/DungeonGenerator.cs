using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using System.Collections;

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

    private Dictionary<DungeonRoom, DungeonRoomInstance> roomsDict;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GenerateDungeon();
    }

    void GenerateDungeon()
    {
        if (layout == null || corridorTilemap == null || roomsGO == null) return;
        //while (true)
        //{
        //    // room generation
        //    if (RandomRoomPlacement.GenerateRooms(layout, grid, roomsGO, out roomsDict, maxPlacementFailCount: 3))
        //    {
        //        // corridor generation
        //        RandomWalk.GenerateCorridors(layout, grid, roomsDict, corridorTilemap, corridorTiles, maxCorridorPathIterations, maxPairFail, maxConnectionFail);
        //        return;
        //    }
        //}

        GraphBasedGeneration.GenerateDungeon(layout, grid, roomsGO, corridorTilemap ,corridorTiles, out roomsDict, maxPlacementFailCount: 3);
    }

}

//[System.Serializable]
//public class CorridorTiles
//{
//    public Tile corridorFloor;
//    public Tile TopHorizontalWall;
//    public Tile BottomHorizontalWall;

//    public Tile LeftVerticalWall;
//    public Tile RightVerticalWall;

//    public Tile TopLeftCornerWall;
//    public Tile TopRightCornerWall;

//}