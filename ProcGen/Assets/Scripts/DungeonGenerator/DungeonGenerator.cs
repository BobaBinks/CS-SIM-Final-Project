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

    private AStarPathfinding aStar;

    // [Header("Corridor Tiles")]
    public CorridorTiles corridorTiles;

    private Dictionary<DungeonRoom, DungeonRoomInstance> roomsDict;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        bool dungeonGenerated = GenerateDungeon();

        if (!dungeonGenerated)
            return;

        FillBackground(backgroundTilemap, backgroundTile, layout);

        // initialize A* grid
        aStar = new AStarPathfinding();
        bool aStarGridInitialized = aStar.InitializeGridDimensions(roomsDict, grid);

    }

    void FillBackground(Tilemap backgroundTilemap, Tile backgroundTile, DungeonLayout layout)
    {
        if (backgroundTile == null || backgroundTilemap == null || layout == null) return;

        BoundsInt combinedBounds;
        bool combinedBoundsRetrieved = TilemapHelper.GetCombinedBounds(roomsDict, grid, out combinedBounds);

        if (!combinedBoundsRetrieved)
            return;

        // expand the bounds
        combinedBounds = TilemapHelper.ExpandBounds(combinedBounds, 0.75f);

        // set tiles
        List<Vector3Int> positions = new List<Vector3Int>();
        List<TileBase> tileBases = new List<TileBase>();

        foreach (Vector3Int pos in combinedBounds.allPositionsWithin)
        {
            positions.Add(pos);
            tileBases.Add(backgroundTile);
        }

        backgroundTilemap.SetTiles(positions.ToArray(), tileBases.ToArray());
    }

    bool GenerateDungeon()
    {
        if (layout == null || corridorFloorTilemap == null || roomsGO == null) return false;

        int maxAttempts = 5;
        int attempts = 0;
        while (attempts < maxAttempts)
        {
            if (GraphBasedGeneration.GenerateDungeon(layout, grid, roomsGO, corridorFloorTilemap, corridorWallTilemap, corridorTiles, out roomsDict, maxPlacementFailCount: 3))
                return true;

            attempts += 1;
            Debug.Log($"Failed to generate dungeon: attempt {attempts}/{maxAttempts}");
        }
        return false;
    }
}
