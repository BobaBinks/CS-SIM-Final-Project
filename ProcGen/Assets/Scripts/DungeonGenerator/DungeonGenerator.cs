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

    [SerializeField]
    private Tilemap aStarGridTilemap;

    // [Header("Corridor Tiles")]
    public CorridorTiles corridorTiles;

    private Dictionary<DungeonRoom, DungeonRoomInstance> roomsDict;
    private Dictionary<DungeonRoom, int> roomDepthDict;

    // debug aStar
    bool startNodePicked = false;
    Vector3Int startPosition;
    Vector3Int endPosition;
    List<Vector3Int> path;

    // Start is called once before the first execution of Update after the MonoBehaviour is created

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

    private void Update()
    {
        // DebugAStarPath();
    }

    public bool GenerateGameEnvironment()
    {
        bool dungeonGenerated = GenerateDungeon();

        if (!dungeonGenerated)
        {
            Debug.Log($"Failed to generated dungeon.");
            return false;
        }

        return true;
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

        // this just for debugging
        aStar.SetAStarGridTilemap(aStarGridTilemap);
        aStar.corridorTiles = corridorTiles;

        bool aStarGridInitialized = aStar.InitializeGridDimensions(roomsDict, grid);

        if (!aStarGridInitialized)
            return null;

        bool traversableCellsMarked = aStar.MarkTraversableCells(roomsDict, corridorFloorTilemap, grid);

        if (!traversableCellsMarked)
        {
            Debug.LogError("A* failed to mark traversable cells.");
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

    public bool CreateRoomGraph()
    {
        roomDepthDict = new Dictionary<DungeonRoom, int>();

        if (roomsDict == null ||
            roomsDict.Count == 0 ||
            layout == null ||
            layout.dungeonRoomList == null ||
            layout.dungeonRoomList.Count == 0)
            return false;

        Queue<DungeonRoom> roomsToCheck = new Queue<DungeonRoom>();

        // find entrance room
        // get entrance room
        DungeonRoom entranceRoom = layout.dungeonRoomList.Find((x) => { return x.roomType.name == "EntranceRoomType"; });

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

    bool GenerateDungeon()
    {
        if (layout == null || corridorFloorTilemap == null || roomsGO == null) return false;

        int maxAttempts = 5;
        int attempts = 0;
        while (attempts < maxAttempts)
        {
            if (GraphBasedGeneration.GenerateDungeon(layout, grid, roomsGO, corridorFloorTilemap, corridorWallTilemap, corridorTiles, out roomsDict, maxPlacementFailCount: 3))
            {
                FillBackground(backgroundTilemap, backgroundTile, layout);
                return true;
            }


            attempts += 1;
            Debug.Log($"Failed to generate dungeon: attempt {attempts}/{maxAttempts}");
        }
        return false;
    }
}
