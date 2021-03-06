using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ScoreUI : MonoBehaviour
{
    [SerializeField] Text scoreText;


    private void OnEnable()
    {
        GameEvents.OnObtainScore += GameEvents_OnObtainScore;       
    }
    private void OnDisable()
    {
        GameEvents.OnObtainScore -= GameEvents_OnObtainScore;
    }

    private void GameEvents_OnObtainScore(int amount)
    {
        scoreText.text = amount.ToString();
        scoreText.rectTransform.DOKill();
        scoreText.rectTransform.DOPunchScale(new Vector3(1.05f, 1.05f, 1.05f), 1.3f, 3, 0).OnKill(() => scoreText.rectTransform.localScale = Vector3.one);
    }
}