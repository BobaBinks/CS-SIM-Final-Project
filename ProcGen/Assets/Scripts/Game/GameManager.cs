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
        CreateGameLevel();
    }

    public void CreateGameLevel()
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

        if (!gameEnvironmentGenerated)
        {
            // return back to menu if generation fails
            SceneManager.LoadScene("Menu");
        }

        // generate A star grid
        aStarPathfinder = dungeonGenerator.InitializeAStarGrid();

        if (aStarPathfinder == null)
        {
            Debug.Log($"Failed to initialize A star Grid.");
            SceneManager.LoadScene("Menu");
        }

        Dictionary<DungeonRoom, DungeonRoomInstance> roomsDict = dungeonGenerator.GetDungeonRooms();
        bool roomGraphCreated = dungeonGenerator.CreateRoomGraph(roomsDict.Keys.ToList());

        if (!roomGraphCreated)
        {
            Debug.Log($"Failed to create room graph.");
            SceneManager.LoadScene("Menu");
        }

        enemySpawnManager.Reset();

        // spawn player
        bool playerSpawned = SpawnPlayer(roomsDict);

        if(!playerSpawned)
        {
            Debug.Log($"Failed to create Player.");
            SceneManager.LoadScene("Menu");
        }
    }

    private void Update()
    {
        
    }

    public void TogglePause()
    {
        if (UIManager.Instance)
        {
            UIManager.Instance.TogglePauseMenu();

            Time.timeScale = UIManager.Instance.IsPause() == false ? 1f : 0f;
        }

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
            if(player == null)
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

            player.gameObject.transform.position = spawnPoint.position;
            return true;
        }
        return false;
    }
}
