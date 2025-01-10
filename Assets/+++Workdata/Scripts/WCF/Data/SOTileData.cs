using UnityEngine;

[CreateAssetMenu(fileName = "SOTileData", menuName = "Scriptable Objects/SOTileData")]
public class SOTileData : ScriptableObject
{
    public enum TileType
    {
        Water, 
        Land,
        Coast,
        WaterShallow,
        LandInner,
        WaterToRight,
        WaterToLeft,
        WaterToDown,
        WaterToUp,
        LandToRight,
        LandToLeft,
        LandToDown,
        LandToUp,
        WaterToDownRight,
        WaterToUpRight,
        WaterToDownLeft,
        WaterToUpLeft,
        LandToDownRight,
        LandToUpRight,
        LandToDownLeft,
        LandToUpLeft,
        WaterShallowSecond,
        WaterShallowThird,
        LandInnerSecond,
        LandInnerThird,
    }

    public TileType type;
    //todo: Change this later to array
    public Sprite tileSprite;
    public bool walkable;
}
