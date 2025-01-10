using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterController2D : MonoBehaviour
{
    [SerializeField] private float stepDuration = .4f;
    [SerializeField] private Transform characterVisual;
    [SerializeField] private Ease stepEase;
    private Animator anim;
    
    private Vector2Int currentPosition;
    private Vector2Int moveDirection;

    private bool isAllowedToMove = true;
    
    private void Start()
    {
        anim = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        if (moveDirection != Vector2Int.zero && isAllowedToMove)
        {
            MoveCharacter();   
        }
    }

    void OnMove(InputValue inputValue)
    {
        moveDirection = new Vector2Int(Mathf.RoundToInt(inputValue.Get<Vector2>().x), 
            Mathf.RoundToInt(inputValue.Get<Vector2>().y));
    }

    void MoveCharacter()
    {
        if (moveDirection.y == 1)
        {
            anim.SetTrigger("MoveUp");
        }
        else if (moveDirection.y == -1)
        {
            anim.SetTrigger("MoveDown");
        }
        else if (moveDirection.x == 1)
        {
            anim.SetTrigger("MoveRight");
            transform.GetChild(0).localScale = new Vector3(1, 1, 1);
        }
        else if(moveDirection.x == -1)
        {
            anim.SetTrigger("MoveRight");
            transform.GetChild(0).localScale = new Vector3(-1, 1, 1);
        }

        Vector2Int newPosition = currentPosition + moveDirection;

        DungeonTile dungeonTile = TileCheck(newPosition);
        if(!dungeonTile) return;

        if (dungeonTile.walkable)
        {
            isAllowedToMove = false;

            transform.DOMove(newPosition.ToVector3(), stepDuration).SetEase(stepEase).OnComplete(() =>
            {
                currentPosition = newPosition;
                isAllowedToMove = true;
            });
            characterVisual.DOLocalMoveY(.3f, stepDuration / 2).SetEase(Ease.InBack).SetLoops(2, LoopType.Yoyo);
        }
    }
    
    DungeonTile TileCheck(Vector2Int checkPosition)
    {
        RaycastHit2D hit = Physics2D.Raycast(checkPosition.ToVector3() + Vector3.back * 0.2f, Vector3.forward);

        return hit ? hit.transform.GetComponent<DungeonTile>() : null;
    }
}
