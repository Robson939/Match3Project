using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITileEffect
{
    public List<(byte, byte)> Effect((byte, byte) originPos, TileFrame[,] tileFrames);
}