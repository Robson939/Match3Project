using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TileFrame : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    public GridPosition pos;
    public Tile tile;

    private Vector2 startDragPos = new Vector2();
    [SerializeField] private Canvas canvas;

    #region touch

    public void OnBeginDrag(PointerEventData eventData)
    {
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        startDragPos = eventData.pressPosition;

        //Debug.Log($"{this.name} startPos: {startDragPos}");

        GameEvents.BeginSwap((pos.x, pos.y));

        //tile.transform.DOScale(new Vector2(1.3f, 1.3f), 2).SetLoops(-1, LoopType.Yoyo);
    }

    public void OnDrag(PointerEventData eventData)
    {
        GameEvents.KeepSwap((pos.x, pos.y), GetDirectionOfTarget((eventData.position - startDragPos).normalized));
    }

    public void OnEndDrag(PointerEventData eventData)
    {
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        //Debug.Log($"{this.name} endDrag: {eventData.position}");
        DirectionEnum targetTileDirection = GetDirectionOfTarget((eventData.position - startDragPos).normalized);

        GameEvents.FinishSwap((pos.x, pos.y), targetTileDirection);
    }

    #endregion

    public void OverrideSorting(bool enable) => canvas.overrideSorting = enable;
    

    private DirectionEnum GetDirectionOfTarget(Vector2 direction)
    {
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))    //horizontal
        {
            //Debug.Log(direction.x > 0 ? "Right" : "Left");
            return direction.x > 0 ? DirectionEnum.Right : DirectionEnum.Left;
        }
        else   //vertical
        {
            //Debug.Log(direction.y > 0 ? "Up" : "Down");
            return direction.y > 0 ? DirectionEnum.Up : DirectionEnum.Down;
        }
    }
}

[System.Serializable]
public struct GridPosition
{
    public byte x;
    public byte y;

    public GridPosition(byte x, byte y)
    {
        this.x = x;
        this.y = y;
    }
}