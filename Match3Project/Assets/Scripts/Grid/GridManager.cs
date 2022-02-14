using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridManager : MonoBehaviour
{
    [SerializeField] private Tile tilePrefab;

    private TileFrame[,] tileFrames = new TileFrame[byte.MaxValue, byte.MaxValue];
    private TileFrame TemporaryTargetTileFrame => tempFramePositon == null ? null : tileFrames[tempFramePositon.Value.x, tempFramePositon.Value.y];
    private GridPosition? tempFramePositon = null;
    private byte sideSize = 0;
    private bool allowUserAction = true;
    private (sbyte, sbyte)[] directionValues = new (sbyte, sbyte)[] { (1, 0), (-1, 0), (0, 1), (0, -1) };
    private Queue<(byte, byte)> queue = new Queue<(byte, byte)>();
    private List<TileSO> tilesSO = new List<TileSO>();
    private List<TileSO> temporarySpecialTileSOs = new List<TileSO>();
    private List<(TileTypeEnum, int)> temporaryTilesToAvoid = new List<(TileTypeEnum, int)>();
    private Vector3 tilePunchScale = new Vector3(1.2f, 1.2f, 1.2f);

    public void OnEnable()
    {
        GameEvents.OnBeginSwap += GameEvents_OnBeginSwap;
        GameEvents.OnKeepSwap += GameEvents_OnKeepSwap;
        GameEvents.OnFinishSwap += GameEvents_OnFinishSwap;
    }
    public void OnDisable()
    {
        GameEvents.OnBeginSwap -= GameEvents_OnBeginSwap;
        GameEvents.OnKeepSwap -= GameEvents_OnKeepSwap;
        GameEvents.OnFinishSwap -= GameEvents_OnFinishSwap;
    }

    public void Start()
    {
        tilesSO = GameManager.Instance.GetTilesSO();

        GenerateBoard();

        sideSize = (byte)tileFrames.GetLength(0);
    }

    #region events 

    private void GameEvents_OnBeginSwap((byte, byte) tile)
    {
        if (!allowUserAction)
        {
            return;
        }
        
        TileTypeEnum tileType = tileFrames[tile.Item1, tile.Item2].tile.GetTileSO().tileType;

        if (tileType != TileTypeEnum.Normal)
        {
            ITileEffect tileEffect = TileEffectFactory.GetEffect(tileType);
            List<(byte, byte)> effectResult = tileEffect.Effect(tile, tileFrames);
            Sequence effectSequence = DOTween.Sequence();
            int scoreHolder = 0;

            allowUserAction = false;

            foreach ((byte, byte) tileItem in effectResult)
            {
                scoreHolder += tileFrames[tileItem.Item1, tileItem.Item2].tile.GetTileSO().scoreValue;

                SetNewTile(effectSequence, tileItem, tilesSO);
            }

            GameManager.Instance.Score += scoreHolder;

            effectSequence.OnComplete(() =>
            {
                TryHandleBoardAfterFill(effectResult);
            });
        }
        else
        {
            tileFrames[tile.Item1, tile.Item2].tile.icon.transform.
                DOScale(tilePunchScale, 0.5f).
                SetEase(Ease.Flash).
                SetLoops(-1, LoopType.Yoyo).
                OnKill(() => tileFrames[tile.Item1, tile.Item2].tile.icon.transform.localScale = Vector3.one);
        }
    }
    private void GameEvents_OnKeepSwap((byte, byte) tileOrigin, DirectionEnum direction)
    {
        if (tileFrames[tileOrigin.Item1, tileOrigin.Item2].tile.GetTileSO().tileType != TileTypeEnum.Normal)
        {
            return;
        }

        if (!allowUserAction)
        {
            return;
        }

        (sbyte, sbyte) dir = GetValueByDirectionEnum(direction);
        byte posX = (byte)(tileOrigin.Item1 + dir.Item1);
        byte posY = (byte)(tileOrigin.Item2 + dir.Item2);

        if (IsPositionInBounds((posX, posY)))
        {
            if (TemporaryTargetTileFrame != tileFrames[posX, posY])
            {
                TemporaryTargetTileFrame?.tile.icon.transform.DOKill();
                tempFramePositon = new GridPosition(posX, posY);
                TemporaryTargetTileFrame.tile.icon.transform.
                    DOScale(tilePunchScale, 1).
                    SetEase(Ease.Flash).
                    SetLoops(-1, LoopType.Yoyo).
                    OnKill(() => TemporaryTargetTileFrame.tile.icon.transform.localScale = Vector3.one);
            }
        }
        else
        {
            TemporaryTargetTileFrame?.tile.icon.transform.DOKill();
            tempFramePositon = null;
        }
    }
    private void GameEvents_OnFinishSwap((byte, byte) tileOrigin, DirectionEnum direction)
    {
        if (tileFrames[tileOrigin.Item1, tileOrigin.Item2].tile.GetTileSO().tileType != TileTypeEnum.Normal)
        {
            return;
        }

        if (!allowUserAction)
        {
            return;
        }

        tileFrames[tileOrigin.Item1, tileOrigin.Item2].tile.icon.transform.DOKill();

        (sbyte, sbyte) dir = GetValueByDirectionEnum(direction);
        byte posX = (byte)(tileOrigin.Item1 + dir.Item1);
        byte posY = (byte)(tileOrigin.Item2 + dir.Item2);

        if (!IsPositionInBounds((posX, posY)) || TemporaryTargetTileFrame != tileFrames[posX, posY])
        {
            TemporaryTargetTileFrame?.tile.icon.transform.DOKill();
            return;
        }

        if (TemporaryTargetTileFrame != null)
        {
            TemporaryTargetTileFrame.tile.icon.transform.DOKill();

            TileSO tileSO1 = TemporaryTargetTileFrame.tile.GetTileSO();
            TileSO tileSO2 = tileFrames[tileOrigin.Item1, tileOrigin.Item2].tile.GetTileSO();

            Vector3 pos1 = TemporaryTargetTileFrame.tile.transform.position;
            Vector3 pos2 = tileFrames[tileOrigin.Item1, tileOrigin.Item2].tile.transform.position;

            allowUserAction = false;

            Sequence swapSequence = DOTween.Sequence();

            swapSequence.OnStart(() => {
                
                tileFrames[tileOrigin.Item1, tileOrigin.Item2].OverrideSorting(true);
            });

            swapSequence.
                Append(tileFrames[tileOrigin.Item1, tileOrigin.Item2].tile.icon.transform.DOMove(pos1, .5f)).
                Insert(0, TemporaryTargetTileFrame.tile.icon.transform.DOMove(pos2, .5f));

            swapSequence.OnComplete(() => {
                tileFrames[tileOrigin.Item1, tileOrigin.Item2].OverrideSorting(false);

                tileFrames[tileOrigin.Item1, tileOrigin.Item2].tile.icon.transform.localPosition = Vector3.zero;
                tileFrames[tileOrigin.Item1, tileOrigin.Item2].tile.SetTileSO(tileSO1);

                TemporaryTargetTileFrame.tile.icon.transform.localPosition = Vector3.zero;
                TemporaryTargetTileFrame.tile.SetTileSO(tileSO2);

                bool result1 = TryHandleMatch(tileFrames[tileOrigin.Item1, tileOrigin.Item2], out HashSet<(byte, byte)> match1);
                bool result2 = TryHandleMatch(TemporaryTargetTileFrame, out HashSet<(byte, byte)> match2);

                if (!result1 && !result2)
                {
                    Sequence unswapSequence = DOTween.Sequence();

                    unswapSequence.OnStart(() => {
                        tileFrames[tileOrigin.Item1, tileOrigin.Item2].OverrideSorting(true);
                    });

                    unswapSequence.
                        Append(tileFrames[tileOrigin.Item1, tileOrigin.Item2].tile.icon.transform.DOMove(pos1, .25f)).
                        Insert(0, TemporaryTargetTileFrame.tile.icon.transform.DOMove(pos2, .25f));

                    unswapSequence.OnComplete(() =>
                    {
                        tileFrames[tileOrigin.Item1, tileOrigin.Item2].OverrideSorting(false);

                        tileFrames[tileOrigin.Item1, tileOrigin.Item2].tile.icon.transform.localPosition = Vector3.zero;
                        tileFrames[tileOrigin.Item1, tileOrigin.Item2].tile.SetTileSO(tileSO2);

                        TemporaryTargetTileFrame.tile.icon.transform.localPosition = Vector3.zero;
                        TemporaryTargetTileFrame.tile.SetTileSO(tileSO1);

                        tempFramePositon = null;
                        allowUserAction = true;
                    });
                }
                else
                {
                    tempFramePositon = null;
                    match1.UnionWith(match2);

                    int scoreHolder = 0;

                    Sequence newTilesSequence = DOTween.Sequence();

                    foreach ((byte, byte) tile in match1)
                    {
                        scoreHolder += tileFrames[tile.Item1, tile.Item2].tile.GetTileSO().scoreValue;

                        SetNewTile(newTilesSequence, tile, tilesSO);
                    }

                    GameManager.Instance.Score += scoreHolder;

                    newTilesSequence.OnComplete(() => {
                        TryHandleBoardAfterFill(new List < (byte, byte) > (match1));
                    });
                }            
            });
        }
    }

    #endregion
        

    private void SetNewTile(Sequence sequenceRef, (byte, byte) tilePos, List<TileSO> randomPool)
    {
        tileFrames[tilePos.Item1, tilePos.Item2].tile.SetTileSO(randomPool[Random.Range(0, randomPool.Count)]);
        tileFrames[tilePos.Item1, tilePos.Item2].tile.icon.transform.DOKill();

        sequenceRef.Insert(0, tileFrames[tilePos.Item1, tilePos.Item2].tile.icon.transform.DOPunchScale(tilePunchScale, 1, 5, .3f).OnKill(() => tileFrames[tilePos.Item1, tilePos.Item2].tile.icon.transform.localScale = Vector3.one));
    }

    private void TryHandleBoardAfterFill(List<(byte, byte)> newTiles)
    {
        HashSet<(byte, byte)> newMatch = new HashSet<(byte, byte)>();
        Sequence newTilesSequence = DOTween.Sequence();
        int scoreHolder = 0;

        foreach ((byte, byte) tile in newTiles)
        {
            if (tileFrames[tile.Item1, tile.Item2].tile.GetTileSO().tileType != TileTypeEnum.Normal)
            {
                continue;
            }

            TryHandleMatch(tileFrames[tile.Item1, tile.Item2], out HashSet<(byte, byte)> tempMatch);
            newMatch.UnionWith(tempMatch);
        }

        if (newMatch.Count > 0)
        {
            foreach ((byte, byte) tile in newMatch)
            {
                scoreHolder += tileFrames[tile.Item1, tile.Item2].tile.GetTileSO().scoreValue;

                SetNewTile(newTilesSequence, tile, tilesSO);
            }

            GameManager.Instance.Score += scoreHolder;

            newTilesSequence.OnComplete(() => {
                TryHandleBoardAfterFill(new List<(byte, byte)>(newMatch));
            });
        }
        else
        {
            allowUserAction = true;
        }
    }

    private void GenerateBoard()
    {
        List<TileSO> tilesSO = GameManager.Instance.GetTilesSO();
            
        tileFrames = GameEvents.GetGrid();
            
        for (int y=0; y<tileFrames.GetLength(1); y++)
        {
            for (int x=0; x<tileFrames.GetLength(0); x++)
            {
                Tile tile = Instantiate(tilePrefab, tileFrames[x, y].transform);
                TileSO tileSO = null;

                if (x != 0 && y != 0)
                {
                    SetSpecialTileSOs(tileFrames[x, y]);
                    tileSO = temporarySpecialTileSOs[Random.Range(0, temporarySpecialTileSOs.Count)];
                }
                else
                {
                    tileSO = tilesSO[Random.Range(0, tilesSO.Count)];
                }

                tile.SetTileSO(tileSO);
                tileFrames[x, y].tile = tile;
            }
        }
    }

    private (sbyte, sbyte) GetValueByDirectionEnum(DirectionEnum direction)
    {
        if (direction == DirectionEnum.Right)
        {
            return  (1, 0);
        }
        else if (direction == DirectionEnum.Left)
        {
            return (-1, 0);
        }
        else if (direction == DirectionEnum.Up)
        {
            return (0, -1);
        }
        else if (direction == DirectionEnum.Down)
        {
            return (0, 1);
        }
        else
        {
            return (0, 0);
        }
    }

    private bool TryHandleMatch(TileFrame tileFrame, out HashSet<(byte, byte)> result)
    {
        result = BFS((tileFrame.pos.x, tileFrame.pos.y));

        bool isMatch = result.Count >= 3;

        if (!isMatch)
        {
            result.Clear();
        }

        return isMatch;
    }

    private List<(byte, byte)> GetProperNeighbours((byte, byte) pos)
    {
        TileFrame tileFrame = tileFrames[pos.Item1, pos.Item2];
        List<(byte, byte)> result = new List<(byte, byte)>();

        for (int i = 0; i < directionValues.Length; i++)
        {
            (byte, byte) targetPos = (
                (byte)(pos.Item1 + directionValues[i].Item1), 
                (byte)(pos.Item2 + directionValues[i].Item2));

            if (IsPositionInBounds(targetPos) && 
                tileFrame.tile?.GetTileSO()?.valueId == tileFrames[targetPos.Item1, targetPos.Item2].tile?.GetTileSO()?.valueId &&
                tileFrames[targetPos.Item1, targetPos.Item2].tile?.GetTileSO()?.tileType == TileTypeEnum.Normal)
            {
                result.Add(targetPos);
            }
        }

        return result;
    }

    private bool IsPositionInBounds((byte, byte) pos) => 
        (0 <= pos.Item1 && pos.Item1 < sideSize) && 
        (0 <= pos.Item2 && pos.Item2 < sideSize);

    private HashSet<(byte, byte)> BFS((byte, byte) startPoint)
    {
        HashSet<(byte, byte)> visited = new HashSet<(byte, byte)>();
        queue.Clear();
        queue.Enqueue(startPoint);

        while (queue.Count > 0)
        {
            (byte, byte) node = queue.Dequeue();

            if (visited.Contains(node))
            {
                continue;
            }

            visited.Add(node);

            foreach ((byte, byte) neighbor in GetProperNeighbours(node))
            {
                if (!visited.Contains(neighbor))
                {
                    queue.Enqueue(neighbor);
                }
            }
        }

        return visited;
    }

    private void SetSpecialTileSOs(TileFrame currentTile)
    {
        temporarySpecialTileSOs.Clear();
        temporaryTilesToAvoid.Clear();

        if (!(currentTile.pos.x == 0 && currentTile.pos.y == 0))
        {
            if (currentTile.pos.x != 0)
            {
                temporaryTilesToAvoid.Add((tileFrames[currentTile.pos.x - 1, currentTile.pos.y].tile.GetTileSO().tileType, tileFrames[currentTile.pos.x - 1, currentTile.pos.y].tile.GetTileSO().valueId));
            }

            if (currentTile.pos.y != 0)
            {
                temporaryTilesToAvoid.Add((tileFrames[currentTile.pos.x, currentTile.pos.y - 1].tile.GetTileSO().tileType, tileFrames[currentTile.pos.x, currentTile.pos.y - 1].tile.GetTileSO().valueId));
            }
        }

        for (int i = 0; i < tilesSO.Count; i++)
        {
            bool isTileAllowed = true;

            for (int j = 0; j < temporaryTilesToAvoid.Count; j++)
            {
                if (tilesSO[i].tileType == temporaryTilesToAvoid[j].Item1 &&
                    tilesSO[i].valueId == temporaryTilesToAvoid[j].Item2)
                {
                    isTileAllowed = false;
                    break;
                }
            }

            if (isTileAllowed)
            {
                temporarySpecialTileSOs.Add(tilesSO[i]);
            }
        }
    }
}