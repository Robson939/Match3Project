using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TileEffectFactory
{
    public static ITileEffect GetEffect(TileTypeEnum tileType)
    {
        switch (tileType)
        {
            case TileTypeEnum.Normal:
                return null;
            case TileTypeEnum.TakeAllById:
                return new TileEffectTakeAllById();
            case TileTypeEnum.TakeAround:
                return new TileEffectTakeAround();
            default:
                return null;
        }
    }
}