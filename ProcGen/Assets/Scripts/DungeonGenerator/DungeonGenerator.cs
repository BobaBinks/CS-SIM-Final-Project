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

    #region ASTAR
    [Header("A Star")]
    bool startNodePicked = false;
    Vector3Int startPosition;
    Vector3Int endPosition;
    List<Vector3Int> path;
    #endregion

    #region GraphRewriting
    [Header("Graph Rewriting")]
    [SerializeField] GraphRewriteRuleList graphRewriteRuleList;
    GraphRewriteRuleList GraphRewriteRuleList => graphRewriteRuleList;
    [SerializeField] bool applyRewrite = false;
    #endregion

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

    [Header("Dungeon Generation Parameters")]
    [SerializeField] int maxGenerationAttempts = 5;
    [SerializeField] int maxGraphGenerationAttempts = 5;


    private Dictionary<DungeonRoom, DungeonRoomInstance> roomsDict;
    private Dictionary<DungeonRoom, int> roomDepthDict;

    #region Props
    [Header("Props")]
    [SerializeField] PropsSet propSet;
    [SerializeField] PropsSet chestSet;
    [SerializeField] PropsSet trapSet;
    [SerializeField] int propsMaxAttempts = 5;
    #endregion

    #region InteriorWalls
    [Header("Interior Walls")]
    [SerializeField] InteriorWallSet interiorWallSet;
    [SerializeField] int maxTurnsInteriorWalls = 2;
    [SerializeField] int minStepsInteriorWalls = 2;
    [SerializeField] int maxStepsInteriorWalls = 5;
    [SerializeField] int maxAttemptsInteriorWalls = 5;
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

    public bool GenerateGameEnvironment()
    {
        int attempts = 0;
        int maxAttempts = Mathf.Max(maxGenerationAttempts, 1);

        while(attempts < maxAttempts)
        {
            bool generationSuccessful = false;

            // pick random layout
            selectedLayout = layouts[Random.Range(0, layouts.Count)];

            Debug.Log($"Selected Layout: {selectedLayout.name}");

            List<DungeonRoom> modifiedGraph;

            // APPLY GRAPH REWRITE BEFORE GENERATION
            if (GraphRewriteRuleList && applyRewrite)
            {
                GraphRewriter graphRewriter = new GraphRewriter(GraphRewriteRuleList);
                bool rewriteSuccessful = graphRewriter.RewriteGraph(selectedLayout, out modifiedGraph);

                // attempt generating rewritten graph
                if (rewriteSuccessful)
                {
                    generationSuccessful = GenerateDungeon(modifiedGraph);
                }
            }
            else
            {
                // generate original graph if rule list does not exist
                generationSuccessful = GenerateDungeon(selectedLayout.dungeonRoomList);
            }

            // if generation succeeded stop reattempts
            if (generationSuccessful)
            {
                break;
            }

            attempts++;
        }

        if (attempts >= maxAttempts)
            return false;

        // place interior walls
        if (roomsDict != null && roomsDict.Count > 0)
        {
            InteriorWallPlacement.PlaceInteriorWalls(roomsDict,
                                                    interiorWallSet,
                                                    maxTurnsInteriorWalls,
                                                    minStepsInteriorWalls,
                                                    maxStepsInteriorWalls,
                                                    maxAttemptsInteriorWalls);
        }

        // place props
        if ((propSet?.propsPrefab != null && propSet.propsPrefab.Count > 0) &&
            (chestSet?.propsPrefab != null && chestSet.propsPrefab.Count > 0) &&
            (trapSet?.propsPrefab != null && trapSet.propsPrefab.Count > 0) &&
            roomsDict != null && 
            roomsDict.Count > 0)
        {
            PropPlacement.PlaceProps(roomsDict,
                                    propSet.propsPrefab,
                                    chestSet.propsPrefab,
                                    trapSet.propsPrefab,
                                    propsMaxAttempts);
        }

        // place waypoints
        if(roomsDict != null &&
            roomsDict.Count > 0)
        {
            // iterate through each room
            foreach (var kvp in roomsDict)
            {
                DungeonRoomInstance dungeonRoomInstance = kvp.Value;

                if (dungeonRoomInstance == null || dungeonRoomInstance.instance == null)
                    continue;

                GameObject room = dungeonRoomInstance.instance;

                // get the propspawn tilemap
                Waypointplacement wayPointPlacement = room.GetComponentInChildren<Waypointplacement>();
                if (wayPointPlacement != null)
                {
                    wayPointPlacement.PlaceWaypoints();
                }
            }
        }

        return true;
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

    bool GenerateDungeon(List<DungeonRoom> layout)
    {
        if (layout == null || layout.Count == 0 || corridorFloorTilemap == null || roomsGO == null) return false;

        int attempts = 0;
        int maxAttempts = Mathf.Max(maxGraphGenerationAttempts, 1);
        while (attempts < maxAttempts)
        {
            if (GraphBasedGeneration.GenerateDungeon(layout, grid, roomsGO, corridorFloorTilemap, corridorWallTilemap, corridorTiles, out roomsDict, maxPlacementFailCount: 3))
            {
                FillBackground(backgroundTilemap, backgroundTile);
                return true;
            }

            attempts++;
            Debug.Log($"Graph Generation Failed Attempt {attempts}/{maxAttempts}");
        }

        return false;
    }
}
