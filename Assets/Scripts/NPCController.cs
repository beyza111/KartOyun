using System;
using System.Collections;
using UnityEngine;

public class NPCController : MonoBehaviour
{
    public CardSpawner cardSpawner;
    public TurnUIManager uiManager;
    public DeckManager deckManager;
    private TurnManager turnManager;

    private CardData lastDrawnCard; // NPC'nin son çektiği kart

    void Start()
    {
        turnManager = FindObjectOfType<TurnManager>() ?? throw new Exception("TurnManager bulunamadı!");
    }

    public void PerformTurn(Action onTurnComplete)
    {
        if (turnManager.IsSwapAndLockTurn())
        {
            PerformSwapAndLock(onTurnComplete);
        }
        else
        {
            StartCoroutine(NPCTurnSequence(onTurnComplete));
        }
    }

    public void PerformSwapAndLock(Action onTurnComplete)
    {
        uiManager.ShowNotification("NPC is swapping and locking cards...");

        // NPC'nin en yüksek 3 kartını bul ve birini kilitle
        var npcTopCards = cardSpawner.GetTopCards(cardSpawner.CurrentNPCPositions, 3);
        var lockedCard = npcTopCards[UnityEngine.Random.Range(0, npcTopCards.Count)];
        lockedCard.IsLocked = true;
        Debug.Log($"NPC locked card: {lockedCard.value}");

        // Oyuncunun kartlarından birini seç
        var playerTopCards = cardSpawner.GetTopCards(cardSpawner.CurrentPlayerPositions, 3);
        var chosenCard = playerTopCards[UnityEngine.Random.Range(0, playerTopCards.Count)];

        // Kartların pozisyonlarını kontrol et
        if (!cardSpawner.CurrentPlayerPositions.Contains(chosenCard.transform.parent) ||
            !cardSpawner.CurrentNPCPositions.Contains(lockedCard.transform.parent))
        {
            Debug.LogError("Invalid card positions during swap.");
            uiManager.ShowNotification("Swap failed due to invalid card positions.");
            onTurnComplete?.Invoke();
            return;
        }

        // Takas işlemi
        if (!chosenCard.IsLocked)
        {
            var npcSwapCard = npcTopCards[UnityEngine.Random.Range(0, npcTopCards.Count)];
            cardSpawner.SwapCards(chosenCard, npcSwapCard);
            Debug.Log($"NPC swapped Player Card {chosenCard.value} with NPC Card {npcSwapCard.value}.");
            uiManager.ShowNotification("NPC swapped a card.");
        }
        else
        {
            Debug.Log("NPC's swap attempt failed. Player's card was locked.");
            uiManager.ShowNotification("NPC's swap attempt failed. Card is locked.");
        }

        CalculateNPCScore();
        onTurnComplete?.Invoke();
    }

    private IEnumerator NPCTurnSequence(Action onTurnComplete)
    {
        uiManager.ShowNotification("NPC is thinking...");
        yield return new WaitForSeconds(2f); // Düşünme süresi

        // NPC'nin kart çekme veya pas geçme kararı
        var drawCard = DecideNPCMove();
        if (drawCard)
        {
            lastDrawnCard = deckManager.DrawCard(); // Çekilen kartı sakla
            cardSpawner.ReplaceLowestCard(cardSpawner.CurrentNPCPositions, lastDrawnCard, ref cardSpawner.npcScore);
            Debug.Log($"NPC drew card with value: {lastDrawnCard.cardValue}");
            uiManager.ShowNotification("NPC drew a card.");
        }
        else
        {
            lastDrawnCard = null; // Pas geçildiği için son çekilen kart yok
            Debug.Log("NPC passed the turn.");
            uiManager.ShowNotification("NPC passed this turn.");
        }

        CalculateNPCScore();
        onTurnComplete?.Invoke();
    }

    public CardData GetLastDrawnCard()
    {
        return lastDrawnCard;
    }

    private bool DecideNPCMove()
    {
        var playerScore = turnManager.GetPlayerScore();
        var threshold = 15; // Minimum puan eşiği
        return cardSpawner.npcScore < playerScore || cardSpawner.npcScore < threshold;
    }

    private void CalculateNPCScore()
    {
        cardSpawner.npcScore = 0;
        foreach (var position in cardSpawner.CurrentNPCPositions)
        {
            var cardValue = position.GetComponentInChildren<CardValue>();
            if (cardValue != null)
            {
                cardSpawner.npcScore += cardValue.value;
            }
        }

        uiManager.UpdateScores(turnManager.GetPlayerScore(), cardSpawner.npcScore);
        Debug.Log($"[NPCController] Updated NPC Score: {cardSpawner.npcScore}");
    }
}
