using System.Collections;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public int currentTurn = 1;
    public int turnsPerLevel = 7; // Toplam tur sayısı

    public CardSpawner cardSpawner;
    public TurnUIManager uiManager;
    public NPCController npcController;

    public CardValue PlayerLockedCard { get; private set; }
    public CardValue PlayerSelectedCardForSwap { get; private set; }

    private bool isPlayerTurnComplete = false;

    public void StartGame()
    {
        Debug.Log("Starting Game...");
        cardSpawner.StartLevel();
        UpdateCardInteractivity();
        PlayTurn();
    }

    private void PlayTurn()
    {
        Debug.Log($"Turn {currentTurn} started.");

        // Tüm kartların interaktiflik durumunu güncelle
        UpdateCardInteractivity();

        switch (currentTurn)
        {
            case 1:
            case 2:
            case 3:
                HandleDrawOrPass();
                break;
            case 4:
                HandleSwapAndLock();
                break;
            case 5:
            case 6:
                HandleDrawWithHint();
                break;
            case 7:
                HandleFinalTurn();
                break;
            default:
                EndLevel();
                break;
        }
    }

    private void HandleDrawOrPass()
    {
        Debug.Log("Player turn: Draw or Pass.");
        isPlayerTurnComplete = false;
        uiManager.ShowUI(); // Oyuncu için arayüzü aç
    }

    private void HandleSwapAndLock()
    {
        Debug.Log("Swap and Lock Phase Started.");
        uiManager.ShowNotification("Phase: Swap & Lock");
        StartCoroutine(SwapAndLockPhase());
    }

    private IEnumerator SwapAndLockPhase()
    {
        Debug.Log("Starting Swap and Lock Phase...");

        // Oyuncuya bildirim gönder: Kartını kilitle
        uiManager.ShowNotification("Select one of your cards to lock.");
        yield return StartCoroutine(WaitForPlayerLockSelection());

        // NPC kendi kartını kilitler ve takas için bir oyuncu kartı seçer
        cardSpawner.NPCLockAndSwapSelection();

        // Oyuncuya bildirim gönder: NPC'den bir kart seç
        uiManager.ShowNotification("Select one of NPC's cards to swap.");
        yield return StartCoroutine(WaitForPlayerSwapSelection());

        // Swap ve lock işlemlerini değerlendir
        string swapDetails = cardSpawner.EvaluateSwapAndLock(PlayerLockedCard, PlayerSelectedCardForSwap);
        Debug.Log(swapDetails);
        uiManager.ShowNotification(swapDetails); // Swap detaylarını göster

        // Swap sonrası NPC'nin kartlarını ters çevirme
        StartCoroutine(FlipNPCBackCards());

        Debug.Log("Swap and Lock Phase Completed.");
        currentTurn++;
        PlayTurn();
    }

    private IEnumerator FlipNPCBackCards()
    {
        yield return new WaitForSeconds(5f);

        Debug.Log("Flipping NPC cards to back.");
        cardSpawner.StartFlipBackProcess(); // NPC kartlarını ters çevir
    }

    private IEnumerator WaitForPlayerLockSelection()
    {
        PlayerLockedCard = null;

        // Oyuncunun seçim yapmasını bekle
        while (PlayerLockedCard == null)
        {
            yield return null;
        }

        Debug.Log($"Player locked the card with value: {PlayerLockedCard.value}");
    }

    private IEnumerator WaitForPlayerSwapSelection()
    {
        PlayerSelectedCardForSwap = null;

        // Oyuncunun seçim yapmasını bekle
        while (PlayerSelectedCardForSwap == null)
        {
            yield return null;
        }

        Debug.Log($"Player selected NPC's card with value: {PlayerSelectedCardForSwap.value} for swap.");
    }

    private void HandleDrawWithHint()
    {
        Debug.Log($"Handling Draw With Hint for Turn {currentTurn}...");

        if (currentTurn == 6)
        {
            var npcLastCard = npcController.GetLastDrawnCard();
            if (npcLastCard != null)
            {
                string hint = npcLastCard.cardValue > 5 ? "5 > x" : "5 < x";
                uiManager.ShowNotification($"NPC's card is {hint}");
            }
        }

        HandleDrawOrPass();
    }

    private void HandleFinalTurn()
    {
        Debug.Log("Handling Final Turn...");
        uiManager.ShowNotification("Final Turn!");
        HandleDrawOrPass();
    }

    private void EndLevel()
    {
        Debug.Log("Level Complete!");
        uiManager.ShowNotification("Level Complete!");
    }

    public void PlayerTurn(bool isDraw)
    {
        if (isDraw)
        {
            var newCard = cardSpawner.deckManager.DrawCard();
            cardSpawner.ReplaceLowestCard(cardSpawner.CurrentPlayerPositions, newCard, ref cardSpawner.playerScore);
            uiManager.ShowNotification("Player drew a card.");
        }
        else
        {
            uiManager.ShowNotification("Player passed the turn.");
        }

        UpdateScores();
        isPlayerTurnComplete = true;

        // Oyuncu hamlesi tamamlandıktan sonra NPC'nin sırası
        Invoke(nameof(PlayNPCTurn), 1f);
    }

    private void PlayNPCTurn()
    {
        Debug.Log("NPC Turn Started...");
        npcController.PerformTurn(() =>
        {
            UpdateScores();
            Debug.Log($"Turn {currentTurn} ended.");
            currentTurn++;
            PlayTurn();
        });
    }

    public void LockPlayerCard(CardValue card)
    {
        PlayerLockedCard = card;
        Debug.Log($"Player locked card: {card.value}");
    }

    public void SelectPlayerCardForSwap(CardValue card)
    {
        PlayerSelectedCardForSwap = card;
        Debug.Log($"Player selected card for swap: {card.value}");
    }

    public bool IsSwapAndLockTurn()
    {
        return currentTurn == 4;
    }

    public int GetPlayerScore()
    {
        return cardSpawner.playerScore;
    }

    private void UpdateScores()
    {
        uiManager.UpdateScores(cardSpawner.playerScore, cardSpawner.npcScore);
        Debug.Log($"Player Score: {cardSpawner.playerScore}, NPC Score: {cardSpawner.npcScore}");
    }

    private void UpdateCardInteractivity()
    {
        foreach (var card in FindObjectsOfType<CardValue>())
        {
            // Yalnızca 4. turda swap veya lock işlemleri için kartlar interaktif
            card.SetSwapTurnActive(currentTurn == 4);
        }
    }
}








