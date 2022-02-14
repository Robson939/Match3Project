using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TileScriptableObject", menuName = "ScriptableObjects/TileScriptableObject", order = 1)]
public class TileSO : ScriptableObject
{
    public TileTypeEnum tileType;
    public int valueId;
    public Sprite icon;
    public int scoreValue = 1;
}
