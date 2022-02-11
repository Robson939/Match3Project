using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameManager>();
            }
            return instance;
        }
    }

    public int Score
    {
        get => score;
        set
        {
            score = value;
            GameEvents.ObtainScore(score);
        }
    }
    private int score = 0;

    [SerializeField]
    private List<TileSO> tileScriptableObjects = new List<TileSO>();

    public List<TileSO> GetTilesSO() => tileScriptableObjects;
}
