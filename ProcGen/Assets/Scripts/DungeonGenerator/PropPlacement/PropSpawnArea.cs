using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

[RequireComponent(typeof(Tilemap))]
public class PropSpawnArea : MonoBehaviour
{
    public Tilemap PropSpawnAreaTilemap { get; private set; }
    [SerializeField] Transform propTransform;
    [SerializeField] Tilemap wallMap;
    [SerializeField] int maxNumberOfProps = 10;
    [SerializeField] int minNumberOfProps = 5;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        PropSpawnAreaTilemap = GetComponent<Tilemap>();
    }

    public void SpawnProps(List<GameObject> propPrefabs, int maxAttempts = 5)
    {
        if (propPrefabs == null || propPrefabs.Count == 0 || !PropSpawnAreaTilemap)
        {
            Debug.Log("Failed to spawn props");
            return;
        }


        PropSpawnAreaTilemap.CompressBounds();


        minNumberOfProps = Mathf.Max(minNumberOfProps, 1);
        maxNumberOfProps = Mathf.Max(maxNumberOfProps, 1);

        if (maxNumberOfProps < minNumberOfProps)
            maxNumberOfProps = minNumberOfProps;

        int numberOfProps = Random.Range(minNumberOfProps, maxNumberOfProps + 1);

        HashSet<Vector3Int> placedPropsCellPosition = new HashSet<Vector3Int>();

        List<Vector3Int> spawnCells = new List<Vector3Int>();

        // get all the cells that has been marked for potential placement
        foreach(var cell in PropSpawnAreaTilemap.cellBounds.allPositionsWithin)
        {
            if (PropSpawnAreaTilemap.GetTile(cell) != null)
                spawnCells.Add(cell);
        }


        if (spawnCells.Count < numberOfProps)
            numberOfProps = spawnCells.Count;

        maxAttempts = Mathf.Max(maxAttempts, 1);

        for (int i = 0; i < numberOfProps; ++i)
        {
            int attempts = 0;

            while(attempts < maxAttempts)
            {
                int randomCellIndex = Random.Range(0, spawnCells.Count);

                // identify cells to spawn props on
                Vector3Int cell = spawnCells[randomCellIndex];

                if (placedPropsCellPosition.Contains(cell) || (wallMap && wallMap.GetTile(cell) != null))
                {
                    attempts++;
                    continue;
                }


                // spawn the props in the world or in the tilemap
                Vector3 worldPosition = PropSpawnAreaTilemap.GetCellCenterWorld(cell);

                // get random prefab
                int propPrefabIndex = Random.Range(0, propPrefabs.Count);

                GameObject propGO;

                if (propTransform)
                {
                    propGO = Instantiate(propPrefabs[propPrefabIndex], worldPosition, Quaternion.identity, propTransform);
                }
                else
                {
                    propGO = Instantiate(propPrefabs[propPrefabIndex], worldPosition, Quaternion.identity);
                }

                // once spawn, can optionally clear the tilemap.
                spawnCells.Remove(cell);
                placedPropsCellPosition.Add(cell);
                break;
            }
        }


    }

}
