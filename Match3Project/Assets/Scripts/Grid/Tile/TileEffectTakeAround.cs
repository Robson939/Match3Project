using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileEffectTakeAround : ITileEffect
{
    private (sbyte, sbyte)[] directionValues = new (sbyte, sbyte)[] { 
        (-1, 1), (0, 1), (1, 1), 
        (-1, 0), (0, 0), (1, 0), 
        (-1, -1), (0, -1), (1, -1) };


    public List<(byte, byte)> Effect((byte, byte) pos, TileFrame[,] tileFrames)
    {
        TileFrame tileFrame = tileFrames[pos.Item1, pos.Item2];
        List<(byte, byte)> result = new List<(byte, byte)>();

        result.Add(pos);

        for (int i = 0; i < directionValues.Length; i++)
        {
            (byte, byte) targetPos = (
                (byte)(pos.Item1 + directionValues[i].Item1),
                (byte)(pos.Item2 + directionValues[i].Item2));

            if (IsPositionInBounds(targetPos, (byte)tileFrames.GetLength(0)) &&
                tileFrames[targetPos.Item1, targetPos.Item2].tile?.GetTileSO()?.tileType == TileTypeEnum.Normal)
            {
                result.Add(targetPos);
            }
        }

        return result;
    }

    private bool IsPositionInBounds((byte, byte) pos, byte sideSize) =>
        (0 <= pos.Item1 && pos.Item1 < sideSize) &&
        (0 <= pos.Item2 && pos.Item2 < sideSize);
}