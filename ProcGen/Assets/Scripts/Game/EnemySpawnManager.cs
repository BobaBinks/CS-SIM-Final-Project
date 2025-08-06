using UnityEngine;
using System.Collections.Generic;

public class EnemySpawnManager : MonoBehaviour
{
    [Header("Enemy prefabs")]
    [SerializeField] List<GameObject> enemyPrefabs = new List<GameObject>();

    [Header("Boss prefabs")]
    [SerializeField] List<GameObject> bossPrefabs = new List<GameObject>();

    [SerializeField] Transform enemyContainerTransform;

    [SerializeField] private AnimationCurve difficultyCurve;

    #region Stats
    [SerializeField] int enemyLevelDifference = 3;
    [SerializeField] int bossLevelDifference = 5;
    #endregion

    private HashSet<DungeonRoom> roomsWithEnemy;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        roomsWithEnemy = new HashSet<DungeonRoom>();
        RemoveInvalidEnemyPrefabs(enemyPrefabs);
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void RegisterSpawnEvent()
    {
        SpawnEnemiesInNextDepth.OnPlayerEnter += OnPlayerEnter;
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

        // get all rooms in the next depth
        List<DungeonRoom> roomsToSpawnEnemies = new List<DungeonRoom>();

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
                if (dungeonRoom.roomType.name == "BossRoomType")
                {
                    SpawnBossInRoom(roomInstance);
                }
                else
                    SpawnEnemiesInRoom(roomInstance);

                roomsWithEnemy.Add(dungeonRoom);
            }
        }
    }



    public void SpawnEnemiesInRooms(Dictionary<DungeonRoom, DungeonRoomInstance> roomsDict)
    {
        if (roomsDict == null)
            return;

        foreach(var kvp in roomsDict)
        {
            // iterate through each room
            DungeonRoomInstance roomInstance = kvp.Value;

            if (roomInstance == null || roomInstance.instance == null)
                continue;

            SpawnEnemiesInRoom(roomInstance);

            //// get the enemy spawn points and waypoints
            //Transform enemySpawnPointContainer = Helper.FindChildWithTag(roomInstance.instance.transform, "EnemySpawnPoints");
            //Transform enemyPatrolWayPointContainer = Helper.FindChildWithTag(roomInstance.instance.transform, "EnemyPatrolWaypoints");

            //// if enemy spawn points does not exist, continue
            //if (!enemySpawnPointContainer)
            //    continue;

            //// else random select a prefab and spawn on spawnpoint
            //SpawnEnemies(enemySpawnPointContainer, enemyPatrolWayPointContainer);
        }
    }

    public void SpawnEnemiesInRoom(DungeonRoomInstance roomInstance)
    {
        if (roomInstance == null)
            return;

        // get the enemy spawn points and waypoints
        Transform enemySpawnPointContainer = GetEnemySpawnPointContainer(roomInstance);

        if (!enemySpawnPointContainer)
            return;

        Transform enemyPatrolWayPointContainer = GetEnemyWaypointContainer(roomInstance);

        // random select a prefab and spawn on spawnpoint
        SpawnEnemies(enemySpawnPointContainer, enemyPatrolWayPointContainer);

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

    private void SpawnEnemies(Transform enemySpawnPointContainer, Transform enemyPatrolWayPointContainer, int numOfEnemies = 2)
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

        for(int enemyCount = 0; enemyCount < numOfEnemies; ++enemyCount)
        {
            int prefabIndex = Random.Range(0, enemyPrefabs.Count);

            int spawnPointIndex = Random.Range(0, enemySpawnPointContainer.childCount);

            Vector3 spawnPosition = enemySpawnPointContainer.GetChild(spawnPointIndex).position;

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
}
