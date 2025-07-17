using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    DungeonGenerator dungeonGenerator;
    EnemySpawnManager enemySpawnManager;
    public AStarPathfinder aStarPathfinder { get; private set; }
    [SerializeField]
    private Transform enemiesTransform;

    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        dungeonGenerator = GetComponent<DungeonGenerator>();
        enemySpawnManager = GetComponent<EnemySpawnManager>();
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // generate dungeon
        if (!dungeonGenerator)
        {
            Debug.Log("Dungeon generator does not exist");
            return;
        }

        if (!enemySpawnManager)
        {
            Debug.Log("Enemy Spawn Manager does not exist");
            return;
        }

        bool gameEnvironmentGenerated = dungeonGenerator.GenerateGameEnvironment();
        Debug.Log($"Game Environment generated {gameEnvironmentGenerated}");

        // generate A star grid
        aStarPathfinder = dungeonGenerator.InitializeAStarGrid();

        if(aStarPathfinder == null)
        {
            Debug.Log($"Failed to initialize A star Grid.");
            return;
        }

        // spawn enemies? only spawn enemies in rooms in the next depth from the player in the graph?
        Dictionary<DungeonRoom, DungeonRoomInstance> roomsDict = dungeonGenerator.GetDungeonRooms();
        enemySpawnManager.SpawnEnemiesInRooms(roomsDict);
    }


}
