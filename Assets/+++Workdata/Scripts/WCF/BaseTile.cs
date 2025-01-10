using UnityEngine;
using System.Collections.Generic;

public class BaseTile : MonoBehaviour
{
    public Vector2Int gridPosition;
    public SOTileData soTileData;

    public List<SOTileData> possibleTiles;

    public void InitTile(SOTileData newTileData)
    {
        soTileData = newTileData;
        GetComponent<SpriteRenderer>().sprite = soTileData.tileSprite;
    }
}
