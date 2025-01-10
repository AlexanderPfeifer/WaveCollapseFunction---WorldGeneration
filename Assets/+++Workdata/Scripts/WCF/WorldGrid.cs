using UnityEngine;
using System.Collections.Generic;

public class WorldGrid : MonoBehaviour
{
    [SerializeField] private Vector2Int gridSize;
    private BaseTile[,] worldGrid;

    private List<BaseTile> allNotSetTiles = new List<BaseTile>();

    [SerializeField] private BaseTile prefabBaseTile;
    [SerializeField] private Transform tileParent;

    private void Awake()
    {
        worldGrid = new BaseTile[gridSize.x, gridSize.y];

        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                worldGrid[x, y] = Instantiate(prefabBaseTile, new Vector2(x, y), Quaternion.identity, tileParent);
                worldGrid[x, y].gridPosition = new Vector2Int(x, y);
                allNotSetTiles.Add(worldGrid[x, y]);
            }
        }
        
        StartCoroutine(GetComponent<ZoomOutCamera>().ZoomOut(gridSize, gridSize));
    }

    public void TileWasSet(BaseTile setBaseTile)
    {
        allNotSetTiles.Remove(setBaseTile);
    }

    public BaseTile GetBaseTileAt(Vector2Int position)
    {
        if(position.x < 0 || position.y < 0 || position.x >= gridSize.x || position.y >= gridSize.y) 
            return null;

        return worldGrid[position.x, position.y];
    }

    public BaseTile GetRandomBaseTile()
    {
        return allNotSetTiles[Random.Range(0, allNotSetTiles.Count)];
    }

    public int GetTotalTileCount()
    {
        return gridSize.x * gridSize.y;
    }
}
