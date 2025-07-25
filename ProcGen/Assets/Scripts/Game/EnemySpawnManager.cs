using UnityEngine;
using System.Collections.Generic;

public class EnemySpawnManager : MonoBehaviour
{
    [Header("Enemy prefabs")]
    [SerializeField]
    List<GameObject> enemyPrefabs = new List<GameObject>();

    [SerializeField]
    Transform enemyContainerTransform;

    [SerializeField]
    private AnimationCurve difficultyCurve;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        RemoveInvalidEnemyPrefabs(enemyPrefabs);
    }

    // Update is called once per frame
    void Update()
    {
        
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

            // get the enemy spawn points and waypoints
            Transform enemySpawnPointContainer = Helper.FindChildWithTag(roomInstance.instance.transform, "EnemySpawnPoints");
            Transform enemyPatrolWayPointContainer = Helper.FindChildWithTag(roomInstance.instance.transform, "EnemyPatrolWaypoints");

            // if enemy spawn points does not exist, continue
            if (!enemySpawnPointContainer)
                continue;

            // else random select a prefab and spawn on spawnpoint
            SpawnEnemies(enemySpawnPointContainer, enemyPatrolWayPointContainer);
        }
    }

    private void SpawnEnemies(Transform enemySpawnPointContainer, Transform enemyPatrolWayPointContainer, int numOfEnemies = 2)
    {
        if (!enemyContainerTransform)
            return;

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
                    int patrolChance = Random.Range(0, 2);
                    //ai.Initialize(patrolChance == 0 ? false : true, waypoints);
                    ai.Initialize(true, waypoints);
                }
                else
                {
                    ai.Initialize();
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
