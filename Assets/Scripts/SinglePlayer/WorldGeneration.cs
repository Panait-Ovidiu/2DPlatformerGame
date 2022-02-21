using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldGeneration : MonoBehaviour
{
    [SerializeField] private bool multiplayerMode;
    [SerializeField] private Player player;

    [SerializeField] Transform enemies;
    [SerializeField] List<Enemy> enemiesPrefabs;
    [SerializeField] int maxNrOfEnemies;
    private int enemiesNr;
    private int enemyOffsetX = 6;
    [SerializeField] Transform projectiles;

    [SerializeField] int width = 100;
    [SerializeField] int height = 30;
    [SerializeField] List<TileBase> groundTiles = new List<TileBase>();
    [SerializeField] Tilemap groundTilemap;
    [SerializeField] float smoothness = 30;
    [SerializeField] float seed = 0;
    int seedRange = 100000;
    int groundTileIndex;
    int[,] map;

    List<Vector3> terrainBase = new List<Vector3>();
    private int terrainStartOffsetX = 2;
    private int terrainEndOffsetX = 2;
    private int heightOffset = 5;

    private float enviromentXPosOffset = 0.5f;
    private int enviromentStartEndOffset = 3;

    [SerializeField] Transform trees;
    [SerializeField] List<GameObject> treesPrefabs = new List<GameObject>();
    [SerializeField] public int treesOffset = 9;
    private float[] treesPrefabOffestY = { 0.9f, 3f };

    [SerializeField] Transform bushes;
    [SerializeField] List<GameObject> bushesPrefabs = new List<GameObject>();
    [SerializeField] public int bushesOffset = 6;
    private float[] bushesPrefabOffestY = { 1.1f, 1.5f, 1.5f };

    [SerializeField] Transform rocks;
    [SerializeField] List<GameObject> rocksPrefabs = new List<GameObject>();
    [SerializeField] public int rocksOffset = 14;
    private float[] rocksPrefabOffestY = { 0.95f, 0.95f, .65f };

    [SerializeField] Transform props;
    [SerializeField] List<GameObject> propsSmallPrefabs = new List<GameObject>();
    [SerializeField] List<GameObject> propsLargePrefabs = new List<GameObject>();
    [SerializeField] public int propsOffset = 8;
    private float propsPrefabOffestY = 0.95f;

    [SerializeField] Transform traps;
    [SerializeField] List<GameObject> trapsPrefabs = new List<GameObject>();
    private float[] trapsPrefabOffestY = { 1f };

    [SerializeField] Transform collectibles;
    [SerializeField] GameObject collectiblesPrefab;
    [SerializeField] public int collectiblesOffset = 5;
    private float collectiblesYOffset = 2.0f;

    [SerializeField] Transform endFlags;
    [SerializeField] GameObject flag;
    private float flagOffsetY = 1.95f;
    private float flagOffsetX = 0.5f;


    // Start is called before the first frame update
    void Start()
    {
        //GenerateNewLevel();
        groundTileIndex = PlayerPrefs.GetInt("groundTileIndex");
        seed = PlayerPrefs.GetFloat("seed");
        if (!multiplayerMode)
        {
            enemiesNr = PlayerPrefs.GetInt("enemiesNr");
        }
        Generation();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void GenerateNewLevel()
    {
        groundTileIndex = Random.Range(0, groundTiles.Count);
        PlayerPrefs.SetInt("groundTileIndex", groundTileIndex);
        seed = Random.Range(0, seedRange);
        PlayerPrefs.SetFloat("seed", seed);
        if (!multiplayerMode)
        {
            enemiesNr = Random.Range(0, maxNrOfEnemies);
            PlayerPrefs.SetInt("enemiesNr", enemiesNr);
        }
    }

    private void ClearPreviousLevel()
    {
        groundTilemap.ClearAllTiles();
        terrainBase.Clear();
        if (!multiplayerMode)
        {
            foreach (Transform child in enemies)
            {
                GameObject.Destroy(child.gameObject);
            }
            foreach (Transform child in collectibles)
            {
                GameObject.Destroy(child.gameObject);
            }
            foreach (Transform child in endFlags)
            {
                GameObject.Destroy(child.gameObject);
            }
        }
        foreach (Transform child in trees)
        {
            GameObject.Destroy(child.gameObject);
        }
        foreach (Transform child in bushes)
        {
            GameObject.Destroy(child.gameObject);
        }
        foreach (Transform child in rocks)
        {
            GameObject.Destroy(child.gameObject);
        }
        foreach (Transform child in props)
        {
            GameObject.Destroy(child.gameObject);
        }
        foreach (Transform child in traps)
        {
            GameObject.Destroy(child.gameObject);
        }
    }

    public void Generation()
    {
        ClearPreviousLevel();
        map = GenerateArray(width, height, true);
        map = TerrainGeneration(map);
        RenderMap(map, groundTilemap, groundTiles[groundTileIndex]);
        MakeTerrainBase(map);
        AddEnviroment(map);
        if (!multiplayerMode)
        {
            AddCollectibles(map);
            AddEnemies();
        }
    }

    private int[,] GenerateArray(int width, int height, bool empty)
    {
        int[,] map = new int[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                map[x, y] = (empty) ? 0 : 1;
            }
        }

        return map;
    }

    private int[,] TerrainGeneration(int[,] map)
    {
        int perlinHeight;
        for (int x = 0; x < width; x++)
        {
            perlinHeight = Mathf.RoundToInt(Mathf.PerlinNoise(x / smoothness, seed) * height / 2);
            perlinHeight += height / 2;
            for (int y = 0; y < perlinHeight; y++)
            {
                map[x, y] = 1;
            }
        }
        return map;
    }

    private void RenderMap(int[,] map, Tilemap groundTilemap, TileBase groundTileBase)
    {
        for (int x = 0 - terrainStartOffsetX; x < 0; x++)
        {
            for (int y = 0; y < height + heightOffset; y++)
            {
                groundTilemap.SetTile(new Vector3Int(x, y, 0), groundTileBase);
            }
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (map[x, y] == 1)
                {
                    groundTilemap.SetTile(new Vector3Int(x, y, 0), groundTileBase);
                }
            }
        }

        for (int x = width; x < width + terrainEndOffsetX; x++)
        {
            for (int y = 0; y < height + heightOffset; y++)
            {
                groundTilemap.SetTile(new Vector3Int(x, y, 0), groundTileBase);
            }
        }
    }

    private void MakeTerrainBase(int[,] map)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = height - 1; y > 0; y--)
            {
                if (map[x, y] == 1)
                {
                    terrainBase.Add(new Vector3(x, y, 0f));
                    break;
                }
            }
        }
    }

    private void AddEnviroment(int[,] map)
    {
        for (int x = enviromentStartEndOffset; x < width - enviromentStartEndOffset; x++)
        {
            if (x % treesOffset == 0)
            {
                int indexSelected = Random.Range(0, treesPrefabs.Count);
                Vector3 pos = new Vector3(terrainBase[x].x + enviromentXPosOffset, terrainBase[x].y + treesPrefabOffestY[indexSelected], terrainBase[x].z);
                GameObject newTree = Instantiate(treesPrefabs[indexSelected], pos, Quaternion.identity);
                newTree.transform.SetParent(trees);
            }

            if (x % bushesOffset == 0)
            {
                if (terrainBase[x].y == terrainBase[x + 1].y && terrainBase[x].y == terrainBase[x - 1].y)
                {
                    int indexSelected = Random.Range(1, bushesPrefabs.Count);
                    Vector3 pos = new Vector3(terrainBase[x].x + enviromentXPosOffset, terrainBase[x].y + bushesPrefabOffestY[indexSelected], terrainBase[x].z);
                    GameObject newTree = Instantiate(bushesPrefabs[indexSelected], pos, Quaternion.identity);
                    newTree.transform.SetParent(bushes);
                }
                else
                {
                    Vector3 pos = new Vector3(terrainBase[x].x + enviromentXPosOffset, terrainBase[x].y + bushesPrefabOffestY[0], terrainBase[x].z);
                    GameObject newTree = Instantiate(bushesPrefabs[0], pos, Quaternion.identity);
                    newTree.transform.SetParent(bushes);
                }
            }

            if (x % rocksOffset == 0)
            {
                int indexSelected = Random.Range(1, rocksPrefabs.Count);
                Vector3 pos = new Vector3(terrainBase[x].x + enviromentXPosOffset, terrainBase[x].y + rocksPrefabOffestY[indexSelected], terrainBase[x].z);
                GameObject newRock = Instantiate(rocksPrefabs[indexSelected], pos, Quaternion.identity);
                newRock.transform.SetParent(rocks);
            }

            if (x % propsOffset == 0)
            {
                if (terrainBase[x].y == terrainBase[x + 1].y && terrainBase[x].y == terrainBase[x - 1].y &&
                    terrainBase[x + 2].y == terrainBase[x + 1].y && terrainBase[x].y == terrainBase[x - 2].y)
                {
                    int indexSelected = Random.Range(0, propsLargePrefabs.Count);
                    Vector3 pos = new Vector3(terrainBase[x].x + enviromentXPosOffset, terrainBase[x].y + propsPrefabOffestY, terrainBase[x].z);
                    GameObject newProp = Instantiate(propsLargePrefabs[indexSelected], pos, Quaternion.identity);
                    newProp.transform.SetParent(props);
                }
                else
                {
                    int indexSelected = Random.Range(1, propsSmallPrefabs.Count);
                    Vector3 pos = new Vector3(terrainBase[x].x + enviromentXPosOffset, terrainBase[x].y + propsPrefabOffestY, terrainBase[x].z);
                    GameObject newProp = Instantiate(propsSmallPrefabs[indexSelected], pos, Quaternion.identity);
                    newProp.transform.SetParent(props);
                }
            }

            if (terrainBase[x].y < terrainBase[x + 1].y && terrainBase[x].y < terrainBase[x - 1].y)
            {
                int indexSelected = Random.Range(trapsPrefabs.Count, trapsPrefabs.Count);
                Vector3 pos = new Vector3(terrainBase[x].x + enviromentXPosOffset, terrainBase[x].y + trapsPrefabOffestY[0], terrainBase[x].z);
                GameObject newTrap = Instantiate(trapsPrefabs[0], pos, Quaternion.identity);
                newTrap.transform.SetParent(traps);
            }

        }

        if (!multiplayerMode)
        {
            Vector3 flagPos = new Vector3(width - flagOffsetX, terrainBase[width - 1].y + flagOffsetY);
            GameObject endFlag = Instantiate(flag, flagPos, Quaternion.identity);
            endFlag.transform.SetParent(endFlags);
        }
    }

    private void AddEnemies()
    {
        for (int i = 0; i < enemiesNr; i++)
        {
            float posX = Random.Range(enemyOffsetX, width - enemyOffsetX);
            int pickedEnemy = Random.Range(0, enemiesPrefabs.Count);
            Vector3 pos = new Vector3(posX, enemiesPrefabs[pickedEnemy].transform.position.y, enemiesPrefabs[pickedEnemy].transform.position.z);
            Enemy newEnemy = Instantiate(enemiesPrefabs[pickedEnemy], pos, Quaternion.identity);
            newEnemy.Instantiate(projectiles, this, player.transform);
            newEnemy.transform.SetParent(enemies);
        }
    }

    private void AddCollectibles(int[,] map)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = height - 1; y > 0; y--)
            {
                if (map[x, y] == 1 && (x != 0) && (x % collectiblesOffset == 0))
                {
                    Vector3 pos = new Vector3(x, y + collectiblesYOffset, 0f);
                    GameObject newCollectible = Instantiate(collectiblesPrefab, pos, Quaternion.identity);
                    newCollectible.transform.SetParent(collectibles);
                    break;
                }
            }
        }
    }

    public void GenerateCollectibles(Vector3 postion)
    {
        GameObject newCollectible = Instantiate(collectiblesPrefab, postion, Quaternion.identity);
        newCollectible.transform.SetParent(collectibles);

    }

    public float GetTerrainHeight(int xPosition)
    {
        if (xPosition < 2)
        {
            return height;
        }
        else if (xPosition > width - 2)
        {
            return height;
        }
        else
        {
            return terrainBase[xPosition].y;
        }

    }

    public int GetWidth()
    {
        return width;
    }

    public List<Vector3> GetTerrainBase()
    {
        return terrainBase;
    }
}
