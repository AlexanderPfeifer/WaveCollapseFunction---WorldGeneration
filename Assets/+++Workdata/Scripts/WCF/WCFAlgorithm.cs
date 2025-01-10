using UnityEngine;
using System.Collections.Generic;
using System.Linq;


public class WCFAlgorithm : MonoBehaviour
{
    private WorldGrid worldGrid;

    int countSetTiles;

    [SerializeField] SOTileRules soTileRules;
    [SerializeField] float waitForLineTime;

    private LineRenderer lineRenderer;
    private List<BaseTile> currentPath = new List<BaseTile>();

    void Start()
    {
        lineRenderer = GetComponentInChildren<LineRenderer>();

        StartGeneration();
    }

    async Awaitable StartGeneration()
    {
        worldGrid = FindAnyObjectByType<WorldGrid>();

        while(countSetTiles < worldGrid.GetTotalTileCount())
        {
            BaseTile startingTile = worldGrid.GetRandomBaseTile();
            while (startingTile.soTileData != null)
            {
                startingTile = worldGrid.GetRandomBaseTile();
            }

            currentPath.Clear();
            currentPath.Add(startingTile);
            UpdateLine();

            startingTile.InitTile(startingTile.possibleTiles[Random.Range(0, startingTile.possibleTiles.Count)]);
            countSetTiles++;
            worldGrid.TileWasSet(startingTile);
            //startingTile.possibleTiles.Remove(startingTile.soTileData);

            List<SOTileData> currentPossibleTileData = new List<SOTileData>(startingTile.possibleTiles);
            for (int i = 0; i < startingTile.possibleTiles.Count; i++)
            {
                if (startingTile.possibleTiles[i] == startingTile.soTileData)
                {
                    continue;
                }

                currentPossibleTileData.Remove(startingTile.possibleTiles[i]);

                PossibleNeighbourInfo possibleNeighbourInfo = CalculatePossibleTileNeighbours(currentPossibleTileData);
                await CheckNeighbours(startingTile.gridPosition, possibleNeighbourInfo);
            }
            startingTile.possibleTiles = new List<SOTileData>(currentPossibleTileData);
        }

        Destroy(lineRenderer);
    }

    async Awaitable CheckNeighbours(Vector2Int currentPos, PossibleNeighbourInfo possibleNeighbourInfo)
    {
        await CheckNeighbourPossibleTiles(currentPos + Vector2Int.up, possibleNeighbourInfo.possibleTileTypesUp);
        await CheckNeighbourPossibleTiles(currentPos + Vector2Int.right, possibleNeighbourInfo.possibleTileTypesRight);
        await CheckNeighbourPossibleTiles(currentPos + Vector2Int.down, possibleNeighbourInfo.possibleTileTypesDown);
        await CheckNeighbourPossibleTiles(currentPos + Vector2Int.left, possibleNeighbourInfo.possibleTileTypesLeft);
    }

    async Awaitable CheckNeighbourPossibleTiles(Vector2Int currentPos, HashSet<SOTileData.TileType> possibleNeighbours)
    {
        //one to the left
        BaseTile neighbourTile = worldGrid.GetBaseTileAt(currentPos);

        if(!neighbourTile || neighbourTile.soTileData != null || currentPath.Contains(neighbourTile))
        {
            return;
        }

        currentPath.Add(neighbourTile);
        UpdateLine();

        await Awaitable.WaitForSecondsAsync(waitForLineTime);

        List<SOTileData> currentPossibleTileDatas = new List<SOTileData>(neighbourTile.possibleTiles);

        foreach (SOTileData tileData in neighbourTile.possibleTiles)
        {
            if (possibleNeighbours.Contains(tileData.type))
            {
                continue; 
            }

            currentPossibleTileDatas.Remove(tileData);
            await CheckNeighbours(neighbourTile.gridPosition, CalculatePossibleTileNeighbours(currentPossibleTileDatas));

            if (currentPossibleTileDatas.Count == 1)
            {
                neighbourTile.InitTile(currentPossibleTileDatas[0]);
                countSetTiles++;
                worldGrid.TileWasSet(neighbourTile);
            }
        }

        neighbourTile.possibleTiles = new List<SOTileData>(currentPossibleTileDatas);

        currentPath.Remove(neighbourTile);
    }

    struct PossibleNeighbourInfo
    {
        public HashSet<SOTileData.TileType> possibleTileTypesUp;
        public HashSet<SOTileData.TileType> possibleTileTypesRight;
        public HashSet<SOTileData.TileType> possibleTileTypesDown;
        public HashSet<SOTileData.TileType> possibleTileTypesLeft;
    }

    PossibleNeighbourInfo CalculatePossibleTileNeighbours(List<SOTileData> listTileData)
    {
        PossibleNeighbourInfo possibleNeighbourInfo = new PossibleNeighbourInfo();

        possibleNeighbourInfo.possibleTileTypesUp = new HashSet<SOTileData.TileType>();
        possibleNeighbourInfo.possibleTileTypesRight = new HashSet<SOTileData.TileType>();
        possibleNeighbourInfo.possibleTileTypesDown = new HashSet<SOTileData.TileType>();
        possibleNeighbourInfo.possibleTileTypesLeft = new HashSet<SOTileData.TileType>();


        foreach (SOTileData tileData in listTileData)
        {
            foreach (SOTileRules.Rules rule in soTileRules.tileRules)
            {
                if (rule.OwnTileType == tileData.type)
                {
                    foreach(var types in rule.UpPossibleTiles)
                    {
                        possibleNeighbourInfo.possibleTileTypesUp.Add(types);
                    }

                    foreach (var types in rule.RightPossibleTiles)
                    {
                        possibleNeighbourInfo.possibleTileTypesRight.Add(types);
                    }

                    foreach (var types in rule.DownPossibleTiles)
                    {
                        possibleNeighbourInfo.possibleTileTypesDown.Add(types);
                    }

                    foreach (var types in rule.LeftPossibleTiles)
                    {
                        possibleNeighbourInfo.possibleTileTypesLeft.Add(types);
                    }
                }
            }
        }

        return possibleNeighbourInfo;
    }

    void UpdateLine()
    {
        Vector3[] points = currentPath.Select(item => item.gridPosition.ToVector3()).ToArray();
        lineRenderer.positionCount = points.Length;
        lineRenderer.SetPositions(points);
    }
}
