using UnityEngine;
using Random = UnityEngine.Random;

public class DungeonTile : MonoBehaviour
{
    [SerializeField] private Sprite[] possibleTileSprite;
    [SerializeField, Range(0, 1)] private float probabilityDifferentSprite;
    public bool walkable;
    public enum TileType
    {
        Water, 
        Grass,
        Mountain,
        Floor,
        Wall,
        Background,
        Tree
    }

    public TileType tileType;

    private void Start()
    {
        if (possibleTileSprite.Length > 0)
        {
            GetComponent<SpriteRenderer>().sprite = Random.value < probabilityDifferentSprite ? 
                possibleTileSprite[Random.Range(0, possibleTileSprite.Length - 1)] : possibleTileSprite[0];
        }
    }
}
