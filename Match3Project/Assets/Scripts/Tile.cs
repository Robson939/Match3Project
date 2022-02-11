using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{    
    public Image icon;
    [SerializeField] private TileSO tileSO;

    public TileSO GetTileSO() => tileSO;
    public void SetTileSO(TileSO tileSO)
    {
        this.tileSO = tileSO;
        icon.sprite = tileSO == null ? null : tileSO.icon;
    }
}