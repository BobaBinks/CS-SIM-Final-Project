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
    private Tilemap corridorFloorTilemap;

    [SerializeField]
    private Tilemap corridorWallTilemap;

    [SerializeField]
    private Tilemap backgroundTilemap;

    [SerializeField]
    private Tile backgroundTile;

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
        FillBackground(backgroundTilemap, backgroundTile, layout);
        GenerateDungeon();
    }

    void FillBackground(Tilemap backgroundTilemap, Tile backgroundTile, DungeonLayout layout)
    {
        if (backgroundTile == null || backgroundTilemap == null || layout == null) return;

        int width = (int)((int)layout.width * 1.5f) * 10;
        int height = (int)((int)layout.height * 1.5f) * 10;

        int halfWidth = width / 2;
        int halfHeight = height / 2;

        Vector3Int[] grid = new Vector3Int[width * height];
        TileBase[] tiles = new TileBase[width * height];

        int index = 0;
        for(int x = -halfWidth; x < halfWidth; ++x)
        {
            for(int y = -halfHeight; y < halfHeight; ++y)
            {
                grid[index] = new Vector3Int(x, y);
                tiles[index] = backgroundTile;
                index++;
            }
        }

        //  backgroundTilemap.BoxFill(
        //  position: new Vector3Int(0, 0, 0),
        //  tile: backgroundTile,
        //  startX: -halfWidth,
        //  startY: -halfHeight,
        //  endX: halfWidth,
        //  endY: halfHeight
        //);

        backgroundTilemap.SetTiles(grid, tiles);
    }
    void GenerateDungeon()
    {
        if (layout == null || corridorFloorTilemap == null || roomsGO == null) return;

        int maxAttempts = 5;
        int attempts = 0;
        while (attempts < maxAttempts)
        {
            if(GraphBasedGeneration.GenerateDungeon(layout, grid, roomsGO, corridorFloorTilemap, corridorWallTilemap , corridorTiles, out roomsDict, maxPlacementFailCount: 3))
                break;

            attempts += 1;
            Debug.Log($"Failed to generate dungeon: attempt {attempts}/{maxAttempts}");
        }
   
    }

}
