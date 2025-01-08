using System.Collections.Generic;
using UnityEngine;

public class ScoreCounter : MonoBehaviour
{
    public int playerScore = 0;
    public int npcScore = 0;

    public int CalculateScoreFromPositions(List<Transform> positions)
    {
        if (positions == null || positions.Count == 0)
        {
            Debug.LogWarning("Positions list is empty or null. Score will be 0.");
            return 0;
        }

        int score = 0;
        foreach (var position in positions)
        {
            var cardValue = position.GetComponentInChildren<CardValue>();
            if (cardValue != null)
            {
                score += cardValue.value;
                Debug.Log($"Added {cardValue.value} from {position.name}. Current score: {score}");
            }
            else
            {
                Debug.LogWarning($"No CardValue component found in child of position {position.name}.");
            }
        }
        return score;
    }

    public void ResetScores()
    {
        playerScore = 0;
        npcScore = 0;
        Debug.Log("Scores reset to 0 for new level.");
    }

    public void UpdateScore(ref int score, Transform position)
    {
        var cardValue = position.GetComponentInChildren<CardValue>();
        if (cardValue != null)
        {
            score += cardValue.value;
            Debug.Log($"Updated score by adding {cardValue.value} from {position.name}. New score: {score}");
        }
        else
        {
            Debug.LogWarning($"No CardValue found in child of position {position.name}. Score remains unchanged.");
        }
    }

    public string CompareScores(int playerScore, int npcScore)
    {
        if (playerScore > npcScore)
            return "Player is leading!";
        else if (playerScore < npcScore)
            return "NPC is leading!";
        else
            return "It's a tie!";
    }
}


