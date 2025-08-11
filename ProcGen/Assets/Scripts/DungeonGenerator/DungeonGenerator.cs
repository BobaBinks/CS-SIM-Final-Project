using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using System.Collections;

public class DungeonGenerator : MonoBehaviour
{
    [SerializeField] private List<DungeonLayout> layouts;
    [SerializeField] private Grid grid;
    [SerializeField] private GameObject roomsGO;

    private DungeonLayout selectedLayout;

    #region Tiles/Tilemaps
    [Header("Tiles/Tilemaps")]
    [SerializeField] private Tilemap corridorFloorTilemap;
    [SerializeField] private Tilemap corridorWallTilemap;
    [SerializeField] private Tilemap backgroundTilemap;
    [SerializeField] private Tilemap aStarGridTilemap;
    [SerializeField] private Tilemap miniMapTilemap;


    [SerializeField] private Tile backgroundTile;

    [Header("Corridor Tiles")]
    public CorridorTiles corridorTiles;
    #endregion

    [Header("RandomWalk Parameters")]
    [SerializeField] int maxCorridorPathIterations = 50;
    [SerializeField] int maxPairFail = 5;
    [SerializeField] int maxConnectionFail = 5;

    private Dictionary<DungeonRoom, DungeonRoomInstance> roomsDict;
    private Dictionary<DungeonRoom, int> roomDepthDict;

    #region ASTAR


    bool startNodePicked = false;
    Vector3Int startPosition;
    Vector3Int endPosition;
    List<Vector3Int> path;
    #endregion

    #region GraphRewriting
    [SerializeField] GraphRewriteRuleList graphRewriteRuleList;
    GraphRewriteRuleList GraphRewriteRuleList => graphRewriteRuleList;
    #endregion

    void FillBackground(Tilemap backgroundTilemap, Tile backgroundTile)
    {
        if (backgroundTile == null || backgroundTilemap == null) return;

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

    private void Update()
    {
        // DebugAStarPath();
    }

    public void GenerateGameEnvironment()
    {
        selectedLayout = layouts[Random.Range(0, layouts.Count)];

        Debug.Log($"Selected Layout: {selectedLayout.name}");



        List<DungeonRoom> modifiedGraph;


        // APPLY GRAPH REWRITE BEFORE GENERATION
        if (GraphRewriteRuleList)
        {
            GraphRewriter graphRewriter = new GraphRewriter(GraphRewriteRuleList);
            bool rewriteSuccessful = graphRewriter.RewriteGraph(selectedLayout, out modifiedGraph);

            if (rewriteSuccessful)
            {
                GenerateDungeon(modifiedGraph);
                return;
            }
        }
        else
            GenerateDungeon(selectedLayout.dungeonRoomList);
    }

    public Dictionary<DungeonRoom, int> GetRoomDepthDict()
    {
        return roomDepthDict;
    }

    public AStarPathfinder InitializeAStarGrid()
    {
        if (roomsDict == null || grid == null)
        {
            Debug.LogError("A* initialization failed: missing roomsDict or grid");
            return null;
        }
        // initialize A* grid
        AStarPathfinder aStar = new AStarPathfinder();

        aStarGridTilemap.ClearAllTiles();
        aStar.SetAStarGridTilemap(aStarGridTilemap);

        miniMapTilemap.ClearAllTiles();
        aStar.SetMiniMapTilemap(miniMapTilemap);
        aStar.corridorTiles = corridorTiles;

        bool aStarGridInitialized = aStar.InitializeGridDimensions(roomsDict, grid);

        if (!aStarGridInitialized)
            return null;

        bool traversableCellsMarked = aStar.MarkTraversableCells(roomsDict, corridorFloorTilemap, grid);
        bool markMinimapCells = aStar.MarkMinimapCells(roomsDict, corridorFloorTilemap, grid);

        if (!traversableCellsMarked || !markMinimapCells)
        {
            Debug.LogError("A* failed to mark traversable cells or minimap cells.");
            return null;
        }
        return aStar;
    }

    public Dictionary<DungeonRoom, DungeonRoomInstance> GetDungeonRooms()
    {
        return roomsDict;
    }

    private void DebugAStarPath()
    {
        AStarPathfinder aStar = GameManager.Instance.aStarPathfinder;

        if (aStar == null)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPosition = aStarGridTilemap.WorldToCell(mouseWorldPos);

            if (!startNodePicked)
            {
                startPosition = cellPosition;
                startNodePicked = true;
                Debug.Log("Start Cell: " + startPosition);
            }
            else
            {
                endPosition = cellPosition;
                Debug.Log("End Cell: " + endPosition);

                // Now find the path
                path = aStar.GetShortestCellPath(startPosition, endPosition);

                if (path != null)
                {
                    foreach (var cell in path)
                    {
                        aStarGridTilemap.SetTile(cell, corridorTiles.TopHorizontalWall);
                    }
                }

                startNodePicked = false;
            }

        }
    }

    public bool CreateRoomGraph(List<DungeonRoom> layout)
    {
        roomDepthDict = new Dictionary<DungeonRoom, int>();

        if (roomsDict == null ||
            roomsDict.Count == 0 ||
            layout == null ||
            layout.Count == 0)
            return false;

        Queue<DungeonRoom> roomsToCheck = new Queue<DungeonRoom>();

        // find entrance room
        // get entrance room
        DungeonRoom entranceRoom = layout.Find((x) => { return x.roomType.name == "EntranceRoomType"; });

        // checks if entrance room is present in the layout
        if (entranceRoom == null)
        {
            Debug.Log("DungeonGenerator(CreateRoomGraph): Could not find entrance room in layout!");
            return false;
        }

        // start with entrance room, depth 0
        roomsToCheck.Enqueue(entranceRoom);
        roomDepthDict[entranceRoom] = 0;

        while(roomsToCheck.Count > 0)
        {
            DungeonRoom currRoom = roomsToCheck.Dequeue();
            int currRoomDepth = roomDepthDict[currRoom];

            if (currRoom.connectionList == null)
                continue;

            // add children into queue, setting their depth to parent depth + 1
            // repeat with other children
            foreach (var room in currRoom.connectionList)
            {
                if (room == null || roomDepthDict.ContainsKey(room))
                    continue;

                roomDepthDict[room] = currRoomDepth + 1;
                roomsToCheck.Enqueue(room);
            }
        }

        return true;
    }

    void GenerateDungeon(List<DungeonRoom> layout)
    {
        if (layout == null || layout.Count == 0 || corridorFloorTilemap == null || roomsGO == null) return;

        int attempts = 0;
        int maxAttempts = 10;
        while (attempts < maxAttempts)
        {
            if (GraphBasedGeneration.GenerateDungeon(layout, grid, roomsGO, corridorFloorTilemap, corridorWallTilemap, corridorTiles, out roomsDict, maxPlacementFailCount: 3))
            {
                FillBackground(backgroundTilemap, backgroundTile);
                return;
            }

            attempts++;
            Debug.Log($"Dungeon Generation Failed Attempt {attempts}/{maxAttempts}");
        }
    }
}
