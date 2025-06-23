using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
public class DungeonGenerator : MonoBehaviour
{
    [SerializeField]
    private DungeonLayout layout;

    [SerializeField]
    private Grid grid;

    [SerializeField]
    private Tilemap corridorTilemap;

    [SerializeField]
    private Tile corridorFloor;

    [SerializeField]
    private Tile corridorWallHorizontal;

    [SerializeField]
    private Tile corridorWallVertical;

    private Dictionary<DungeonRoom, Tilemap> rooms;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GenerateDungeon();
    }



    void GenerateDungeon()
    {
        if (layout == null) return;

        while(true)
        {
            // room generation
            if (RandomRoomPlacement.GenerateRooms(layout, grid.transform, out rooms, maxPlacementFailCount: 3))
            {
                // corridor generation
                RandomWalk.GenerateCorridors(layout, grid.transform, rooms, corridorTilemap);
                return;
            }
        }
    }
}
