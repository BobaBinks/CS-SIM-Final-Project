using UnityEngine;
using System.Collections.Generic;
using Unity.Cinemachine;
using System.Linq;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    DungeonGenerator dungeonGenerator;
    EnemySpawnManager enemySpawnManager;
    public AStarPathfinder aStarPathfinder { get; private set; }

    [SerializeField] private Transform enemiesTransform;

    [SerializeField] GameObject playerGO;

    [SerializeField] CinemachineCamera cinemachineCamera;

    public Player player { get; private set; }

    public static GameManager Instance { get; private set; }

    public DungeonGenerator DungeonGenerator => dungeonGenerator;
    public EnemySpawnManager EnemySpawnManager => enemySpawnManager;

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
        // DontDestroyOnLoad(gameObject);
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

        if(!gameEnvironmentGenerated)
        {
            SceneManager.LoadScene("GameScene");
        }
        // generate A star grid
        aStarPathfinder = dungeonGenerator.InitializeAStarGrid();

        if(aStarPathfinder == null)
        {
            Debug.Log($"Failed to initialize A star Grid.");
            return;
        }

        Dictionary<DungeonRoom, DungeonRoomInstance> roomsDict = dungeonGenerator.GetDungeonRooms();
        bool roomGraphCreated = dungeonGenerator.CreateRoomGraph(roomsDict.Keys.ToList());

        if (!roomGraphCreated)
        {
            Debug.Log($"Failed to create room graph.");
            return;
        }

        // map difficulty to depth?
        // get the max depth - that is 1 on the animation curve
        // get current depth / max depth and find the associated value in the animation curve. that is your multiplier

        // use this multiplier for other curves like health and damage

        // spawn player
        bool playerSpawned = SpawnPlayer(roomsDict);

        // spawn enemies
        //if(playerSpawned)
        //    enemySpawnManager.SpawnEnemiesInRooms(roomsDict);

        if (playerSpawned)
            enemySpawnManager.RegisterSpawnEvent();


    }

    private void Update()
    {
        
    }

    public void TogglePause()
    {
        Time.timeScale = Time.timeScale == 0f ? 1f : 0f;
        if (UIManager.Instance)
            UIManager.Instance.TogglePauseMenu();
    }

    private bool SpawnPlayer(Dictionary<DungeonRoom, DungeonRoomInstance> roomsDict)
    {
        if (roomsDict == null || roomsDict.Count == 0)
        {
            return false;
        }

        DungeonRoomInstance entranceRoom = null;

        foreach(var kvp in roomsDict)
        {
            DungeonRoom dungeonRoom = kvp.Key;

            if (dungeonRoom == null)
                continue;

            if(dungeonRoom.roomType.name == "EntranceRoomType")
            {
                entranceRoom = kvp.Value;
                break;
            }
        }

        if (entranceRoom == null || entranceRoom.instance == null)
        {
            Debug.Log("GameManager: Dungeon does not have an entrance room to spawn player");
            return false;
        }

        // get spawn point in entrance room instance
        Transform spawnPoint = Helper.FindChildWithTag(entranceRoom.instance.transform, "PlayerSpawnPoint");

        // Instantiate player on spawnpoint
        if (spawnPoint)
        {
            GameObject playerGO = GameObject.Instantiate(this.playerGO, spawnPoint.position, Quaternion.identity);

            player = playerGO.GetComponent<Player>();

            if (player == null)
            {
                Debug.LogError("Spawned player GameObject is missing the Player component.");
                return false;
            }

            if (cinemachineCamera)
                cinemachineCamera.Follow = playerGO.transform;
            return true;
        }
        return false;
    }
}
