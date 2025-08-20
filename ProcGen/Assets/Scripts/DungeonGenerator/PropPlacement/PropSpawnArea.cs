using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

[RequireComponent(typeof(Tilemap))]
public class PropSpawnArea : MonoBehaviour
{
    [SerializeField] Tilemap propSpawnAreaTilemap;
    [SerializeField] Tilemap chestSpawnAreaTilemap;
    [SerializeField] Tilemap trapSpawnAreaTilemap;
    [SerializeField] Tilemap wallMap;
    [SerializeField] Transform objectHasColliderTransform;
    [SerializeField] Transform objectsHasNoColliderTransform;

    [Header("Props")]
    [SerializeField] int maxNumberOfProps = 10;
    [SerializeField] int minNumberOfProps = 5;

    [Header("Chests")]
    [SerializeField] int maxNumberOfChests = 2;
    [SerializeField] int minNumberOfChests = 1;

    [Header("Traps")]
    [SerializeField] int maxNumberOfTraps = 2;
    [SerializeField] int minNumberOfTraps = 1;

    HashSet<Vector3Int> placedPropsCellPosition;
    Dictionary<Tilemap, List<Vector3Int>> areaSpawnCells;

    public void InitPropSpawn()
    {
        placedPropsCellPosition = new HashSet<Vector3Int>();

        // get all the cells that has been marked for potential placement
        areaSpawnCells = PopulateAreaSpawnCellsDict(
            new List<Tilemap>
            {
                propSpawnAreaTilemap,
                chestSpawnAreaTilemap,
                trapSpawnAreaTilemap
            },
            wallMap
        );
    }

    private Dictionary<Tilemap, List<Vector3Int>> PopulateAreaSpawnCellsDict(List<Tilemap> tileMaps, Tilemap wallMap)
    {
        Dictionary<Tilemap, List<Vector3Int>> areaSpawnCells = new Dictionary<Tilemap, List<Vector3Int>>();
        foreach (var tileMap in tileMaps)
        {
            if (tileMap == null || areaSpawnCells.ContainsKey(tileMap)) continue;

            tileMap.CompressBounds();
            // add tileMap to dict
            areaSpawnCells.Add(tileMap, new List<Vector3Int>());

            foreach (var cell in tileMap.cellBounds.allPositionsWithin)
            {
                // check if cell contains a wall cell
                bool isWallFree = (wallMap != null && wallMap.GetTile(cell) == null);

                // cells that are painted in the tilemap are marked as valid spawn points
                // add the cells that have been marked
                if (tileMap.GetTile(cell) != null && isWallFree)
                    areaSpawnCells[tileMap].Add(cell);
            }
        }

        return areaSpawnCells;
    }


    public void SpawnProps(List<GameObject> propPrefabs, int maxAttempts = 5)
    {
        if (propPrefabs == null ||
            propPrefabs.Count == 0 ||
            !propSpawnAreaTilemap ||
            areaSpawnCells == null ||
            areaSpawnCells.Count == 0 ||
            !areaSpawnCells.ContainsKey(propSpawnAreaTilemap) ||
            areaSpawnCells.Values == null ||
            placedPropsCellPosition == null)
        {
            Debug.Log("Failed to spawn props");
            return;
        }

        List<Vector3Int> spawnCells = areaSpawnCells[propSpawnAreaTilemap];

        minNumberOfProps = Mathf.Max(minNumberOfProps, 1);
        maxNumberOfProps = Mathf.Max(maxNumberOfProps, 1);

        if (maxNumberOfProps < minNumberOfProps)
            maxNumberOfProps = minNumberOfProps;

        int numberOfProps = Random.Range(minNumberOfProps, maxNumberOfProps + 1);

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

                if (placedPropsCellPosition.Contains(cell))
                {
                    attempts++;
                    continue;
                }


                // spawn the props in the world or in the tilemap
                Vector3 worldPosition = propSpawnAreaTilemap.GetCellCenterWorld(cell);

                // get random prefab
                int propPrefabIndex = Random.Range(0, propPrefabs.Count);

                GameObject propGO;
                GameObject propPrefab = propPrefabs[propPrefabIndex];

                // check if prop has collider
                bool hasCollider = CheckColliderPresent(propPrefab);

                // choose the parent transform
                Transform parent = null;

                if (hasCollider && objectHasColliderTransform != null)
                {
                    parent = objectHasColliderTransform;
                }
                else if (!hasCollider && objectsHasNoColliderTransform != null)
                {
                    parent = objectsHasNoColliderTransform;
                }

                // instantiate under chosen parent or root if parent is null
                propGO = Instantiate(propPrefab, worldPosition, Quaternion.identity, parent);

                // update cell trackers
                spawnCells.Remove(cell);
                placedPropsCellPosition.Add(cell);
                break;
            }
        }
    }

    private bool CheckColliderPresent(GameObject gameObject)
    {
        if (gameObject == null) return false;
        return gameObject.TryGetComponent<Collider2D>(out Collider2D collider);
    }


    public void SpawnTraps(List<GameObject> propPrefabs, int maxAttempts = 5)
    {
        if (propPrefabs == null ||
            propPrefabs.Count == 0 ||
            !trapSpawnAreaTilemap ||
            areaSpawnCells == null ||
            !areaSpawnCells.ContainsKey(trapSpawnAreaTilemap) ||
            areaSpawnCells[trapSpawnAreaTilemap] == null ||
            placedPropsCellPosition == null)
        {
            Debug.Log("Failed to spawn traps");
            return;
        }

        List<Vector3Int> spawnCells = areaSpawnCells[trapSpawnAreaTilemap];

        minNumberOfTraps = Mathf.Max(minNumberOfTraps, 1);
        maxNumberOfTraps = Mathf.Max(maxNumberOfTraps, 1);

        if (maxNumberOfTraps < minNumberOfTraps)
            maxNumberOfTraps = minNumberOfTraps;

        int numberOfChests = Random.Range(minNumberOfTraps, maxNumberOfTraps + 1);

        if (spawnCells.Count < numberOfChests)
            numberOfChests = spawnCells.Count;

        maxAttempts = Mathf.Max(maxAttempts, 1);

        for (int i = 0; i < numberOfChests; ++i)
        {
            int attempts = 0;

            while (attempts < maxAttempts)
            {
                int randomCellIndex = Random.Range(0, spawnCells.Count);

                // identify cells to spawn props on
                Vector3Int cell = spawnCells[randomCellIndex];

                if (placedPropsCellPosition.Contains(cell))
                {
                    attempts++;
                    continue;
                }

                // spawn the props in the world or in the tilemap
                Vector3 worldPosition = trapSpawnAreaTilemap.GetCellCenterWorld(cell);

                // get random prefab
                int propPrefabIndex = Random.Range(0, propPrefabs.Count);

                GameObject propGO;
                GameObject propPrefab = propPrefabs[propPrefabIndex];

                if (objectHasColliderTransform)
                    propGO = Instantiate(propPrefab, worldPosition, Quaternion.identity, objectHasColliderTransform);
                else
                    propGO = Instantiate(propPrefab, worldPosition, Quaternion.identity);

                // once spawn, can optionally clear the tilemap.
                spawnCells.Remove(cell);
                placedPropsCellPosition.Add(cell);
                break;
            }
        }
    }

    public void SpawnChests(List<GameObject> propPrefabs, int maxAttempts = 5)
    {
        if (propPrefabs == null ||
            propPrefabs.Count == 0 ||
            !chestSpawnAreaTilemap ||
            !areaSpawnCells.ContainsKey(chestSpawnAreaTilemap) ||
            areaSpawnCells[chestSpawnAreaTilemap] == null ||
            placedPropsCellPosition == null)
        {
            Debug.Log("Failed to spawn props");
            return;
        }


        List<Vector3Int> spawnCells = areaSpawnCells[chestSpawnAreaTilemap];

        minNumberOfChests = Mathf.Max(minNumberOfChests, 1);
        maxNumberOfChests = Mathf.Max(maxNumberOfChests, 1);

        if (maxNumberOfChests < minNumberOfChests)
            maxNumberOfChests = minNumberOfChests;

        int numberOfChests = Random.Range(minNumberOfChests, maxNumberOfChests + 1);

        if (spawnCells.Count < numberOfChests)
            numberOfChests = spawnCells.Count;

        maxAttempts = Mathf.Max(maxAttempts, 1);

        for (int i = 0; i < numberOfChests; ++i)
        {
            int attempts = 0;

            while (attempts < maxAttempts)
            {
                int randomCellIndex = Random.Range(0, spawnCells.Count);

                // identify cells to spawn props on
                Vector3Int cell = spawnCells[randomCellIndex];

                if (placedPropsCellPosition.Contains(cell))
                {
                    attempts++;
                    continue;
                }


                // spawn the props in the world or in the tilemap
                Vector3 worldPosition = chestSpawnAreaTilemap.GetCellCenterWorld(cell);

                // get random prefab
                int propPrefabIndex = Random.Range(0, propPrefabs.Count);

                GameObject propGO;

                if (objectHasColliderTransform)
                {
                    propGO = Instantiate(propPrefabs[propPrefabIndex], worldPosition, Quaternion.identity, objectHasColliderTransform);
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
