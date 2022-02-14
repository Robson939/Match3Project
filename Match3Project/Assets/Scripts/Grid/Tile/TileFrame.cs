using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TileFrame : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    public GridPosition pos;
    public Tile tile;

    [SerializeField] private Canvas canvas;
    private Vector2 startDragPos = new Vector2();

    #region touch

    public void OnBeginDrag(PointerEventData eventData)
    {
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        startDragPos = eventData.pressPosition;
        GameEvents.BeginSwap((pos.x, pos.y));
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
        DirectionEnum targetTileDirection = GetDirectionOfTarget((eventData.position - startDragPos).normalized);

        GameEvents.FinishSwap((pos.x, pos.y), targetTileDirection);
    }

    #endregion

    public void OverrideSorting(bool enable) => canvas.overrideSorting = enable;
    

    private DirectionEnum GetDirectionOfTarget(Vector2 direction)
    {
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))    //horizontal
        {
            return direction.x > 0 ? DirectionEnum.Right : DirectionEnum.Left;
        }
        else   //vertical
        {
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