using UnityEngine;
using System.Collections.Generic;
public class DungeonGenerator : MonoBehaviour
{
    [SerializeField]
    private DungeonLayout layout;

    [SerializeField]
    private Grid grid;

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
            if (RandomRoomPlacement.GenerateRooms(layout, grid.transform, maxPlacementFailCount: 3))
            {
                // corridor generation
                return;
            }
        }
    }
}
