using System;
using UnityEngine;

[CreateAssetMenu(fileName = "SOTileRules", menuName = "Scriptable Objects/SOTileRules")]
public class SOTileRules : ScriptableObject
{
    [Serializable]

    public struct Rules
    {
        public string TileName;
        public SOTileData.TileType OwnTileType;
        public SOTileData.TileType[] UpPossibleTiles;
        public SOTileData.TileType[] RightPossibleTiles;
        public SOTileData.TileType[] DownPossibleTiles;
        public SOTileData.TileType[] LeftPossibleTiles;
    }

    public Rules[] tileRules;
}
