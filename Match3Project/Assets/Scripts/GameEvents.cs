using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEvents : MonoBehaviour
{
    public delegate TileFrame[,] GridAction();

    public static event GridAction OnGetGrid;
    public static TileFrame[,] GetGrid() => OnGetGrid?.Invoke();



    public delegate void TileFrameAction((byte, byte) tile);

    public static event TileFrameAction OnBeginSwap;
    public static void BeginSwap((byte, byte) tile) => OnBeginSwap?.Invoke(tile);



    public delegate void TileSwapAction((byte, byte) tileOrigin, DirectionEnum direction);

    public static event TileSwapAction OnKeepSwap;
    public static void KeepSwap((byte, byte) tileOrigin, DirectionEnum direction) => OnKeepSwap?.Invoke(tileOrigin, direction);

    public static event TileSwapAction OnFinishSwap;
    public static void FinishSwap((byte, byte) tileOrigin, DirectionEnum direction) => OnFinishSwap?.Invoke(tileOrigin, direction);


    public delegate void ScoreAction (int amount);
    public static event ScoreAction OnObtainScore;
    public static void ObtainScore(int amount) => OnObtainScore?.Invoke(amount);
}