using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileEffectTakeAllById : ITileEffect
{
    public List<(byte, byte)> Effect((byte, byte) originPos, TileFrame[,] tileFrames)
    {
        List<(byte, byte)> result = new List<(byte, byte)>();
        result.Add(originPos);

        for (byte x = 0; x < tileFrames.GetLength(0); x++)
        {
            for (byte y = 0; y < tileFrames.GetLength(1); y++)
            {
                if (tileFrames[x,y].tile.GetTileSO().valueId == tileFrames[originPos.Item1, originPos.Item2].tile.GetTileSO().valueId &&
                    tileFrames[x, y].tile.GetTileSO().tileType == TileTypeEnum.Normal)
                {
                    result.Add((x, y));
                }
            }
        }

        return result;
    }
}