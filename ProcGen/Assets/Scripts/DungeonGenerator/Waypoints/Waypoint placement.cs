using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class Waypointplacement : MonoBehaviour
{
    [SerializeField] Tilemap wallMap;
    [SerializeField] Tilemap groundMap;
    [SerializeField] int numWaypoints;
    [SerializeField] int maxAttempts = 5;
    [SerializeField] float minGap;
    [SerializeField] GameObject wayPointPrefab;

    HashSet<GameObject> wayPoints = new HashSet<GameObject>();

    public void PlaceWaypoints()
    {
        if(wallMap == null || groundMap == null || wayPointPrefab == null)
        {
            Debug.Log("Could not spawn waypoints");
            return;
        }


        // sanitization of inputs
        numWaypoints = Mathf.Max(numWaypoints, 2);
        maxAttempts = Mathf.Max(maxAttempts, 1);

        // get cells that can contain waypoints
        groundMap.CompressBounds();
        wallMap.CompressBounds();

        HashSet<Vector3Int> spawnCellsHashSet = GetSpawnableCells(groundMap, wallMap);

        List<Vector3Int> spawnCells = new List<Vector3Int>(spawnCellsHashSet);

        for(int wayPointIndex = 0; wayPointIndex < numWaypoints; ++wayPointIndex)
        {
            Vector3 position;
            bool positionPicked = PickWaypointPosition(spawnCells, out position);

            // instantiate the waypoint
            if (positionPicked)
            {
                GameObject wayPointGO = Instantiate(wayPointPrefab, position, Quaternion.identity, transform);
                wayPoints.Add(wayPointGO);
            }
        }


    }

    private HashSet<Vector3Int> GetSpawnableCells(Tilemap groundMap, Tilemap wallMap)
    {
        groundMap.CompressBounds();
        wallMap.CompressBounds();

        HashSet<Vector3Int> spawnCells = new HashSet<Vector3Int>();
        foreach (var cell in groundMap.cellBounds.allPositionsWithin)
        {
            if (groundMap.GetTile(cell) != null && wallMap.GetTile(cell) == null)
                spawnCells.Add(cell);
        }

        return spawnCells;
    }


    private bool PickWaypointPosition(List<Vector3Int> cells, out Vector3 position, int maxAttempts = 5)
    {
        position = Vector3.zero;
        if(groundMap == null || wallMap == null)
        {
            Debug.Log("Failed to pick waypoint position, tilemaps missing");
            return false;
        }



        int attempts = 0;
        while (attempts < maxAttempts)
        {
            int randomCellIndex = Random.Range(0, cells.Count);
            Vector3Int cell = cells[randomCellIndex];
            Vector3 candidatePosition = groundMap.GetCellCenterWorld(cell);

            // maybe ensure minimum distance
            bool tooClose = false;
            foreach(var wayPoint in wayPoints)
            {
                if ((wayPoint.transform.position - candidatePosition).sqrMagnitude < minGap * minGap)
                {
                    tooClose = true;
                    break;
                }
            }

            if (tooClose)
                attempts++;
            else
            {
                position = candidatePosition;
                cells.Remove(cell);
                return true;
            }
        }

        return false;
    }

}
