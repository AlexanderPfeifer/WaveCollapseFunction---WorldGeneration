using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class DungeonGeneration : MonoBehaviour
{
    //important parameters for implementation
    /*
     * Room count (vector2)
     * 
     * Room size (vector2)
     * How many exits (vector2)
     * Corridor curve (bool)
     * Branching corridors (bool)
     * Probability (if corridor branches - float 0 - 1)
     *
     * Corridor length
     *
     * Start in room or corridor(bool)
     * 
     * Non-walkable areas in rooms (float as probability) -> but all exits have to be reachable
     * 
     * Door location
     */

    enum Himmelsrichtung
    {
        North,
        East,
        South,
        West
    }

    [Header("Tiles")]
    [SerializeField] private DungeonTile floorTile;
    [SerializeField] private DungeonTile wallTile;
    [SerializeField] private DungeonTile backgroundTile;

    [Header("Seed")]
    [SerializeField] private int seed;
    [SerializeField] private bool useSeed;

    [Header("Room")]    
    [SerializeField, Tooltip("X:From, Y:To")] private Vector2Int roomCount;
    private int totalRoomCount;
    private int currentRoomCount;
    [SerializeField, Tooltip("X:From, Y:To")] private Vector2Int roomSize;
    [SerializeField, Tooltip("X:From, Y:To")] private Vector2Int possibleExitsPerRoom;
    
    [Header("Corridor")]
    [SerializeField, Tooltip("X:From, Y:To")] private Vector2Int corridorSize;
    [FormerlySerializedAs("corridorCurves")] [SerializeField, Range(0f, 1f)] float corridorCurveProbability;

    [SerializeField] private Transform parentDungeon;

    [Header("Background")]
    private Vector2Int upperRightCorner;
    private Vector2Int lowerLeftCorner;

    [Header("Character")] 
    [SerializeField] private Transform prefabCharacter;
    [SerializeField] private CinemachineBrain cineMachineBrain;
    [SerializeField] private CinemachineCamera playerCam;

    [SerializeField] private float generatingTileTime;
    
    [SerializeField] private int runningCoroutine;
    public bool isGenerationFinished { get; private set; }

    struct RoomInfo
    {
        public Vector2Int startingPosition;
        public Vector2Int size;
        public int exitCount;
    }    
    
    struct CorridorInfo
    {
        public Vector2Int startingPosition;
        public int length;
        public Himmelsrichtung direction;

        public CorridorInfo(Vector2Int newStartingPosition, Himmelsrichtung newHimmelsrichtung)
        {
            startingPosition = newStartingPosition;
            direction = newHimmelsrichtung;
            length = 0;
        }
    }
    
    void Start()
    {
        GenerateDungeon();
    }

    void GenerateDungeon()
    {
        if (useSeed)
        {
            Random.InitState(seed);
        }
        
        totalRoomCount = Random.Range(roomCount.x, roomCount.y + 1);
        
        StartCoroutine(GenerateDungeonRoom(RandomRoomInfo(new Vector2Int(0,0))));
    }

    void Reset()
    {
        upperRightCorner = new Vector2Int();
        lowerLeftCorner = new Vector2Int();
        StopAllCoroutines();
        Destroy(parentDungeon.gameObject);
        currentRoomCount = 0;
        isGenerationFinished = false;
    }

    void Update()
    {
        if (!isGenerationFinished && runningCoroutine == 0)
        {
            if (currentRoomCount == totalRoomCount)
            {
                isGenerationFinished = true;
                StartCoroutine(GenerateBackground());
            }
            else
            {
                Reset();
                GenerateDungeon();
            }
        }
    }

    IEnumerator GenerateDungeonRoom(RoomInfo roomInfo)
    {
        runningCoroutine++;
        for (int x = 0; x < roomInfo.size.x; x++)
        {
            for (int y = 0; y < roomInfo.size.y; y++)
            {
                Vector2Int newTilePos = new Vector2Int(x, y) + roomInfo.startingPosition;
                if (TileCheck(newTilePos))
                {
                    continue;
                }
                
                Instantiate(floorTile, new Vector3(x, y) + roomInfo.startingPosition.ToVector3(), Quaternion.identity, parentDungeon);
                yield return new WaitForSeconds(generatingTileTime);
                AdjustMapBoundaries(newTilePos);
            }
        }
        
        yield return null; // For longer generation, add new WaitForSeconds(0.2f)
        
        currentRoomCount++;
        if (totalRoomCount == currentRoomCount)
        {
            StopAllCoroutines();
            
            //Make the cam focus the center of the map
            /*Vector3 sumVector = new Vector3(0f,0f,0f);

            foreach (Transform child in parentDungeon.transform)
            {          
                sumVector += child.position;        
            }

            Vector3 groupCenter = sumVector / parentDungeon.transform.childCount;
            Camera.main.transform.position = groupCenter;
            Camera.main.transform.position = new Vector3(groupCenter.x, groupCenter.y, -10);*/

            runningCoroutine = 0;
            yield break;
        }

        int maxAttempts = 100;
        for (int i = 0; i < roomInfo.exitCount;)
        {
            CorridorInfo? newCorridorInfo = GetCorridorStartingPosition(roomInfo);

            if (newCorridorInfo != null)
            {
                CorridorInfo ci = (CorridorInfo)newCorridorInfo;
                //Add 1 on the Y size because Random Range is maxExclusive
                ci.length = Random.Range(corridorSize.x, corridorSize.y + 1);
                StartCoroutine(GenerateCorridor(ci));
                i++;
            }

            maxAttempts--;
            if(maxAttempts <= 0) 
                break;
        }

        runningCoroutine--;
    }

    IEnumerator GenerateBackground()
    {
        for (int x = lowerLeftCorner.x; x < upperRightCorner.x + 1; x++)
        {
            for (int y = lowerLeftCorner.y; y < upperRightCorner.y + 2; y++)
            {
                Vector2Int currentPos = new Vector2Int(x, y);
                
                DungeonTile dungeonTile = TileCheck(currentPos);
                
                if (dungeonTile && dungeonTile.walkable)
                {
                    if (!TileCheck(currentPos + Vector2Int.up))
                    {
                        Instantiate(wallTile, (currentPos + Vector2Int.up).ToVector3(), Quaternion.identity, parentDungeon);
                    }
                }
                else if(!dungeonTile)
                {
                    Instantiate(backgroundTile, currentPos.ToVector3(), Quaternion.identity, parentDungeon);
                }
                
            }
            
            yield return null;
        }
        
        GetComponent<ZoomOutCamera>().ChangeZoomCamPriority();

        StartCoroutine(WaitForCameraSwitch());
    }

    private IEnumerator WaitForCameraSwitch()
    {
        yield return new WaitUntil(() => cineMachineBrain.IsBlending); 
        
        yield return new WaitUntil(() => !cineMachineBrain.IsBlending);

        playerCam.Follow = Instantiate(prefabCharacter).transform;
    }

    CorridorInfo? GetCorridorStartingPosition(RoomInfo roomInfo)
    {
        // Find possible exits
        //Corridors are not allowed next to each other

        (Vector2Int, Himmelsrichtung) value = GetPossibleCorridorPosition(roomInfo.size.x, roomInfo.size.y);
        Vector2Int corridorPosition = roomInfo.startingPosition + value.Item1;
        Himmelsrichtung direction = value.Item2;

        CorridorInfo? corridorInfo = new CorridorInfo(corridorPosition, direction);

        if(!TileCheck(corridorPosition))
        {
            Vector2Int checkDirection = direction is Himmelsrichtung.North or Himmelsrichtung.South ? Vector2Int.right : Vector2Int.up;
            
            if(!TileCheck(corridorPosition + checkDirection) && !TileCheck(corridorPosition + -checkDirection))
            {
                return corridorInfo;
            }        
        }

        return null;
    }

    IEnumerator GenerateCorridor(CorridorInfo corridorInfo)
    {
        runningCoroutine++;

        Vector2Int currentPos = corridorInfo.startingPosition;
        Himmelsrichtung currentDirection = corridorInfo.direction;
        
        for (int i = 0; i < corridorInfo.length; i++)
        {
            if (TileCheck(currentPos))
            {
                runningCoroutine--;
                yield break;
            }
            
            Instantiate(floorTile, currentPos.ToVector3(), Quaternion.identity, parentDungeon);

            yield return new WaitForSeconds(generatingTileTime);
            
            AdjustMapBoundaries(currentPos);

            if (Random.value < corridorCurveProbability)
            {
                // Determine curve direction: left (-1) or right (+1)
                currentDirection += Random.value > 0.5f ? 1 : -1;

                // Wrap the direction to stay within bounds (0 to 3)
                currentDirection = (Himmelsrichtung)(((int)currentDirection + 4) % 4);
            }
            
            currentPos += HimmelsrichtungToVector(currentDirection);
        }
        
        yield return null;

        StartCoroutine(GenerateDungeonRoom(RandomRoomInfo(currentPos)));
        
        runningCoroutine--;
    }

    (Vector2Int, Himmelsrichtung) GetPossibleCorridorPosition(int xSize, int ySize)
    {
        Vector2Int[] possiblePositions =
        {
            new(Random.Range(0, xSize), ySize),
            new(xSize, Random.Range(0 ,ySize)),
            new(Random.Range(0, xSize), -1),
            new(-1, Random.Range(0, ySize))
        };

        int randomNumber = Random.Range(0, possiblePositions.Length);
        
        return (possiblePositions[randomNumber], (Himmelsrichtung) randomNumber);
    }

    Vector2Int HimmelsrichtungToVector(Himmelsrichtung himmelsrichtung)
    {
        return himmelsrichtung switch
        {
            Himmelsrichtung.North => Vector2Int.up,
            Himmelsrichtung.East => Vector2Int.right,
            Himmelsrichtung.South => Vector2Int.down,
            Himmelsrichtung.West => Vector2Int.left,
            _ => Vector2Int.zero
        };
    }

    RoomInfo RandomRoomInfo(Vector2Int newStartingPosition)
    {
        return new RoomInfo
        {
            startingPosition = newStartingPosition,
            size = new Vector2Int(Random.Range(roomSize.x, roomSize.y), Random.Range(roomSize.x, roomSize.y)),
            //we add 1 in the y because RandomRange is maxExclusive
            exitCount = Random.Range(possibleExitsPerRoom.x, possibleExitsPerRoom.y + 1)
        };
    }

    DungeonTile TileCheck(Vector2Int checkPosition)
    {
        RaycastHit2D hit = Physics2D.Raycast(checkPosition.ToVector3() + Vector3.back * 0.2f, Vector3.forward);

        return hit ? hit.transform.GetComponent<DungeonTile>() : null;
    }

    void AdjustMapBoundaries(Vector2Int currentPoint)
    {
        upperRightCorner.x = Mathf.Max(upperRightCorner.x, currentPoint.x);
        lowerLeftCorner.x = Mathf.Min(lowerLeftCorner.x, currentPoint.x);

        upperRightCorner.y = Mathf.Max(upperRightCorner.y, currentPoint.y);
        lowerLeftCorner.y = Mathf.Min(lowerLeftCorner.y, currentPoint.y);

        StartCoroutine(GetComponent<ZoomOutCamera>()
            .ZoomOut(new Vector2Int(upperRightCorner.x, upperRightCorner.y), new Vector2Int( lowerLeftCorner.x + upperRightCorner.x, lowerLeftCorner.y + upperRightCorner.y)));
    }
}

public static class Extensions
{
    public static Vector3 ToVector3(this Vector2Int vector2Int)
    {
        return new Vector3(vector2Int.x, vector2Int.y);
    }
}
