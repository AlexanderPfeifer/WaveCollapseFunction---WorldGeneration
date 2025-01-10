using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Unity.Cinemachine;
using UnityEngine;

public class WorldGeneration : MonoBehaviour
{
    [SerializeField] private DungeonTile prefabWater;
    [SerializeField] private DungeonTile prefabMountain;
    [SerializeField] private DungeonTile prefabGrass;
    [SerializeField] private DungeonTile prefabTrees;
    [SerializeField] private Transform prefabCharacter;

    [SerializeField] private Vector2Int worldSize;
    [SerializeField] private Vector2Int treePlacementSize;

    [SerializeField] private float scaleNoiseWorld = 0.1f;
    [SerializeField] private float scaleNoiseTrees = 0.1f;

    [SerializeField] private bool spawnTrees;

    [SerializeField] private Transform tileParent;

    [SerializeField] private CinemachineCamera playerCam;
    
    [SerializeField] private float generatingTileTime;

    private void Start()
    {
        StartCoroutine(GetComponent<ZoomOutCamera>().ZoomOut(worldSize, worldSize));

        StartCoroutine(GenerateWorld());
    }

    private IEnumerator GenerateWorld()
    {
        for (int x = 0; x < worldSize.x; x++)
        {
            for (int y = 0; y < worldSize.y; y++)
            {
                float value = Mathf.PerlinNoise(x * scaleNoiseWorld, y * scaleNoiseWorld);

                switch (value)
                {
                    case > .3f and < .7f:
                        Instantiate(prefabGrass, new Vector3(x, y), Quaternion.identity, tileParent);
                        break;
                    case > .7f:
                        Instantiate(prefabWater, new Vector3(x, y), Quaternion.identity, tileParent);
                        yield return null;
                        break;
                    case < .3f:
                        Instantiate(prefabMountain, new Vector3(x, y), Quaternion.identity, tileParent);
                        break;
                }
            }
        }

        playerCam.Follow = Instantiate(prefabCharacter);

        if(spawnTrees)
            StartCoroutine(GenerateTrees());
    }

    private IEnumerator GenerateTrees()
    {
        for (int x = 0; x < treePlacementSize.x; x++)
        {
            for (int y = 0; y < treePlacementSize.y; y++)
            {
                float value = Mathf.PerlinNoise(x * scaleNoiseTrees, y * scaleNoiseTrees);

                if (TileCheck(new Vector2Int(x, y), DungeonTile.TileType.Grass) && value > .7f)
                {
                    Instantiate(prefabTrees, new Vector3(x, y), Quaternion.identity, tileParent);
                    yield return null;
                }
            }
        }
        
        GetComponent<ZoomOutCamera>().ChangeZoomCamPriority();
    }

    bool TileCheck(Vector2Int checkPosition, DungeonTile.TileType checkForTile)
    {
        RaycastHit2D hit = Physics2D.Raycast(checkPosition.ToVector3() + Vector3.back * 0.2f, Vector3.forward);

        return hit.transform.GetComponent<DungeonTile>().tileType == checkForTile;
    }
}
