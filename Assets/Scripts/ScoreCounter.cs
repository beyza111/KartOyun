using System.Collections.Generic;
using UnityEngine;

public class ScoreCounter : MonoBehaviour
{
    public int CalculateScoreFromPositions(List<Transform> positions)
    {
        int score = 0;
        foreach (var position in positions)
        {
            var cardValue = position.GetComponentInChildren<CardValue>();
            if (cardValue != null)
            {
                score += cardValue.value;
            }
        }
        return score;
    }
}




