using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public class EnemySpawnManager : MonoBehaviour
{
    [Header("Enemy prefabs")]
    [SerializeField] List<GameObject> enemyPrefabs = new List<GameObject>();

    [Header("Boss prefabs")]
    [SerializeField] List<GameObject> bossPrefabs = new List<GameObject>();

    [SerializeField] Transform enemyContainerTransform;

    // based off room area size
    [SerializeField] private AnimationCurve enemiesPerRoomCurve;

    #region Stats
    [SerializeField] int enemyLevelDifference = 3;
    [SerializeField] int bossLevelDifference = 5;
    [SerializeField] int maxNumEnemies = 5;
    [SerializeField] int minNumEnemies = 1;
    #endregion

    private HashSet<DungeonRoom> roomsWithEnemy;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        roomsWithEnemy = new HashSet<DungeonRoom>();
        RemoveInvalidEnemyPrefabs(enemyPrefabs);
        RemoveInvalidEnemyPrefabs(bossPrefabs);
    }

    private void OnEnable()
    {
        SpawnEnemiesInNextDepth.OnPlayerEnter += OnPlayerEnter;
        SpawnBoss.OnPlayerEnterBossRoom += OnPlayerEnterBossRoom;
    }

    private void OnDisable()
    {
        SpawnEnemiesInNextDepth.OnPlayerEnter -= OnPlayerEnter;
        SpawnBoss.OnPlayerEnterBossRoom -= OnPlayerEnterBossRoom;
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void OnPlayerEnterBossRoom(GameObject roomGO)
    {
        if (!GameManager.Instance || !GameManager.Instance.DungeonGenerator)
            return;
        Dictionary<DungeonRoom, DungeonRoomInstance> roomsDict = GameManager.Instance.DungeonGenerator.GetDungeonRooms();
        if (roomsDict == null || roomsDict.Count == 0)
            return;

        foreach (var kvp in roomsDict)
        {
            DungeonRoom dungeonRoom = kvp.Key;
            DungeonRoomInstance roomInstance = kvp.Value;

            // find the room that triggered event
            if(roomInstance != null && roomInstance.instance == roomGO)
            {
                // ensure rooms does not already have spawned enemies
                if (!roomsWithEnemy.Contains(dungeonRoom))
                {
                    SpawnBossInRoom(roomInstance);
                    roomsWithEnemy.Add(dungeonRoom);
                }
            }
        }
    }

    public void OnPlayerEnter(GameObject roomGO)
    {
        if (!GameManager.Instance || !GameManager.Instance.DungeonGenerator)
            return;
        Dictionary<DungeonRoom, DungeonRoomInstance> roomsDict = GameManager.Instance.DungeonGenerator.GetDungeonRooms();
        Dictionary<DungeonRoom, int> roomDepthDict = GameManager.Instance.DungeonGenerator.GetRoomDepthDict();
        if (roomsDict == null || roomsDict.Count == 0 || roomDepthDict == null || roomDepthDict.Count != roomsDict.Count)
            return;

        int roomDepth = 0;

        // check if roomGO in roomsDict and grab its depth
        foreach(var kvp in roomsDict)
        {
            DungeonRoom dungeonRoom = kvp.Key;
            DungeonRoomInstance roomInstance = kvp.Value;

            if (roomInstance.instance == roomGO)
            {
                if (roomDepthDict.ContainsKey(dungeonRoom))
                    roomDepth = roomDepthDict[dungeonRoom];
                else
                    return;
            }
        }

        foreach (var kvp in roomsDict)
        {
            DungeonRoom dungeonRoom = kvp.Key;
            DungeonRoomInstance roomInstance = kvp.Value;

            // ensure rooms does not already have spawned enemies
            // and check if its depth is +1 of room entered
            if(!roomsWithEnemy.Contains(dungeonRoom) && 
                roomDepthDict.ContainsKey(dungeonRoom) && 
                roomDepthDict[dungeonRoom] == roomDepth + 1)
            {
                if (dungeonRoom.roomType.name != "BossRoomType")
                {
                    SpawnEnemiesInRoom(roomInstance);

                    roomsWithEnemy.Add(dungeonRoom);
                }
            }
        }
    }

    public void SpawnEnemiesInRoom(DungeonRoomInstance roomInstance)
    {
        if (roomInstance == null)
            return;
        // get enemy waypoints if any
        Transform enemyPatrolWayPointContainer = GetEnemyWaypointContainer(roomInstance);

        // get the ground tilemap and compress it
        Tilemap groundMap = roomInstance.groundMap;
        groundMap.CompressBounds();

        // get the number of enemies to spawn in this room
        int enemySpawnCount = GetRoomEnemyCount(groundMap.cellBounds.size.x * groundMap.cellBounds.size.y);
        Debug.Log($"{roomInstance.instance.name}'s will spawn {enemySpawnCount}.");


        for(int enemyIndex = 0; enemyIndex < enemySpawnCount; ++enemyIndex)
        {
            // pick a cell in the room, convert it to world
            // convert world to astargrid local position and verify if cell is occupied in its tilemap
            int failCounter = 0;
            int maxFailCounter = 5;
            while (failCounter < maxFailCounter)
            {

                int xPos = Random.Range(groundMap.cellBounds.xMin, groundMap.cellBounds.xMax);
                int yPos = Random.Range(groundMap.cellBounds.yMin, groundMap.cellBounds.yMax);

                if (GameManager.Instance.aStarPathfinder.AStarGridTilemap)
                {
                    Vector3 positionWorld = groundMap.CellToWorld(new Vector3Int(xPos, yPos));
                    Vector3Int localPosition = GameManager.Instance.aStarPathfinder.AStarGridTilemap.WorldToCell(positionWorld);

                    if (GameManager.Instance.aStarPathfinder.AStarGridTilemap.GetTile(localPosition) == null)
                    {
                        // not a valid position to spawn
                        failCounter++;
                        continue;
                    }
                    Vector3 cellCenterPosition = GameManager.Instance.aStarPathfinder.AStarGridTilemap.GetCellCenterLocal(localPosition);
                    cellCenterPosition = GameManager.Instance.aStarPathfinder.AStarGridTilemap.LocalToWorld(cellCenterPosition);
                    InstantiateEnemy(enemyPatrolWayPointContainer, cellCenterPosition);
                    break;
                }
            }
        }
    }

    private int GetRoomEnemyCount(int roomSize)
    {
        if(enemiesPerRoomCurve == null)
            return 0;


        Keyframe[] keys = enemiesPerRoomCurve.keys;

        if (keys.Length == 0)
            return 0;

        float time = Mathf.Clamp(roomSize, keys[0].time, keys[keys.Length - 1].time);

        return Mathf.RoundToInt(enemiesPerRoomCurve.Evaluate(time));
    }

    private void SpawnBossInRoom(DungeonRoomInstance roomInstance)
    {
        if (roomInstance == null)
            return;

        // get the enemy spawn points and waypoints
        Transform enemySpawnPointContainer = GetEnemySpawnPointContainer(roomInstance);

        if (!enemySpawnPointContainer)
            return;

        enemyLevelDifference = Mathf.Max(bossLevelDifference, 0);
        int bossLevel = CalculateEnemyLevel(levelDifference: bossLevelDifference);

        int prefabIndex = Random.Range(0, bossPrefabs.Count);

        Vector3 spawnPosition = enemySpawnPointContainer.GetChild(0).position;

        GameObject enemyGO = GameObject.Instantiate(bossPrefabs[prefabIndex], spawnPosition, Quaternion.identity, enemyContainerTransform);

        EnemyAI ai = enemyGO.GetComponent<EnemyAI>();

        if (ai)
        {
            ai.Initialize(bossLevel);
        }
    }

    private Transform GetEnemySpawnPointContainer(DungeonRoomInstance roomInstance)
    {
        if (roomInstance == null)
            return null;

        // get the enemy spawn points and waypoints
        return Helper.FindChildWithTag(roomInstance.instance.transform, "EnemySpawnPoints");
    }

    private Transform GetEnemyWaypointContainer(DungeonRoomInstance roomInstance)
    {
        if (roomInstance == null)
            return null;

        // get the enemy spawn points and waypoints
        return Helper.FindChildWithTag(roomInstance.instance.transform, "EnemyPatrolWaypoints");
    }

    private int CalculateEnemyLevel(int minLevel = 0, int maxLevel = 100, int levelDifference = 2)
    {
        minLevel = Mathf.Max(minLevel, 0);
        maxLevel = Mathf.Max(maxLevel, 0);
        levelDifference = Mathf.Max(levelDifference, 0);

        Player player = GameManager.Instance.player;

        if (!player)
            return minLevel;

        int enemyLevel = player.Level + levelDifference;
        return Mathf.Clamp(enemyLevel, minLevel, maxLevel);
    }
    
    private void InstantiateEnemy(Transform enemyPatrolWayPointContainer, Vector3 spawnPosition)
    {
        if (!enemyContainerTransform)
            return;

        enemyLevelDifference = Mathf.Max(enemyLevelDifference, 0);
        int enemyLevel = CalculateEnemyLevel(levelDifference: enemyLevelDifference);

        List<Transform> waypoints = new List<Transform>();
        if (enemyPatrolWayPointContainer)
        {
            for (int waypointIndex = 0; waypointIndex < enemyPatrolWayPointContainer.childCount; ++waypointIndex)
            {
                waypoints.Add(enemyPatrolWayPointContainer.GetChild(waypointIndex));
            }
        }

        int prefabIndex = Random.Range(0, enemyPrefabs.Count);

        GameObject enemyGO = GameObject.Instantiate(enemyPrefabs[prefabIndex], spawnPosition, Quaternion.identity, enemyContainerTransform);

        EnemyAI ai = enemyGO.GetComponent<EnemyAI>();

        if (ai)
        {
            if (enemyPatrolWayPointContainer)
            {
                ai.Initialize(true, waypoints, enemyLevel);
            }
            else
            {
                ai.Initialize(enemyLevel);
            }
        }
    }
    /// <summary>
    /// Removes any GameObjects from the list that do not have an EnemyAI component.
    /// </summary>
    /// <param name="enemyPrefabs">List of potential enemy prefabs.</param>
    private void RemoveInvalidEnemyPrefabs(List<GameObject> enemyPrefabs)
    {
        if (enemyPrefabs == null || enemyPrefabs.Count == 0)
            return;

        // iterate backwards to safely remove items from the list.
        for (int i = enemyPrefabs.Count - 1; i >= 0; --i)
        {
            GameObject prefab = enemyPrefabs[i];

            if (prefab == null || prefab.GetComponent<EnemyAI>() == null)
            {
                enemyPrefabs.RemoveAt(i);
            }
        }
    }

    private void ClearEnemies(GameObject enemiesContainerGO)
    {
        if (enemiesContainerGO == null) return;

        for (int childIndex = enemiesContainerGO.transform.childCount - 1; childIndex >= 0; --childIndex)
        {
            GameObject.Destroy(enemiesContainerGO.transform.GetChild(childIndex).gameObject);
        }
    }

    public void Reset()
    {
        ClearEnemies(enemyContainerTransform.gameObject);

        if (roomsWithEnemy != null)
            roomsWithEnemy.Clear();
        else
            roomsWithEnemy = new HashSet<DungeonRoom>();
    }
}
