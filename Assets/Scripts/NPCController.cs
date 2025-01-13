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
    //public CardDrawSoundManager soundManager; // Ses yöneticisi

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

        // NPC kendi en yüksek 3 kartından birini kilitler
        var npcTopCards = cardSpawner.GetTopCards(cardSpawner.CurrentNPCPositions, 3);
        cardSpawner.NPCLockedCard = npcTopCards[UnityEngine.Random.Range(0, npcTopCards.Count)];
        cardSpawner.NPCLockedCard.IsLocked = true;
        Debug.Log($"NPC locked card: {cardSpawner.NPCLockedCard.value}");

        // Oyuncunun kartlarından birini seç
        var playerLowestCard = cardSpawner.GetLowestCard(cardSpawner.CurrentPlayerPositions);
        if (playerLowestCard == null || playerLowestCard.IsLocked)
        {
            Debug.LogWarning("NPC could not find a valid player card to swap.");
            uiManager.ShowNotification("NPC's swap attempt failed.");
            onTurnComplete?.Invoke();
            return;
        }

        // NPC en düşük kartını bul ve swap yap
        var npcSwapCard = cardSpawner.GetLowestCard(cardSpawner.CurrentNPCPositions);
        if (npcSwapCard != null && !npcSwapCard.IsLocked)
        {
            cardSpawner.SwapCards(playerLowestCard, npcSwapCard);
            Debug.Log($"NPC swapped Player card ({playerLowestCard.value}) with NPC card ({npcSwapCard.value}).");
            uiManager.ShowNotification("NPC swapped a card.");
        }
        else
        {
            Debug.LogWarning("NPC could not swap because its card was locked.");
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
          //  soundManager.PlayCardDrawSound(); // Kart çekme sesiaaaaaaaaaaaaaaaaaaaaaaaaaaa
        }
        else
        {
            lastDrawnCard = null; // Pas geçildiği için son çekilen kart yok
            Debug.Log("NPC passed the turn.");
            uiManager.ShowNotification("NPC passed this turn.");
            yield return new WaitForSeconds(0.5f); // İsteğe bağlı kısa bir bekleme
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
