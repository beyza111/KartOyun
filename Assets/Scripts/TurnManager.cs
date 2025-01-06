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

    private bool isSwapAndLockTurnActive = false;
    private bool isPlayerActionComplete = false; // Oyuncu aksiyonu tamamladı mı?

    public void StartGame()
    {
        Debug.Log("Starting Game...");
        cardSpawner.StartLevel();
        UpdateCardInteractivity();
        PlayTurn();
    }

    public void PlayTurn()
    {
        Debug.Log($"Turn {currentTurn} started.");
        isPlayerActionComplete = false; // Her tur başlangıcında sıfırla
        UpdateCardInteractivity();

        switch (currentTurn)
        {
            case 1:
            case 2:
            case 3:
                HandleDrawOrPass();
                break;
            case 4:
                isSwapAndLockTurnActive = true;
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

    public void StartLockSelection()
    {
        Debug.Log("Lock selection phase started.");
        PlayerLockedCard = null;
        uiManager.ShowNotification("Select a card to lock.");
    }

    public void StartSwapSelection()
    {
        Debug.Log("Swap selection phase started.");
        PlayerSelectedCardForSwap = null;
        uiManager.ShowNotification("Select an opponent's card to swap.");
    }

    public void PlayerTurn(bool isDraw)
    {
        if (currentTurn == 4 && !isPlayerActionComplete)
        {
            Debug.LogWarning("Player must complete their action during the Swap & Lock phase.");
            return;
        }

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
        Invoke(nameof(PlayNPCTurn), 1f);
    }

    public bool IsSwapAndLockTurn()
    {
        return isSwapAndLockTurnActive;
    }

    public int GetPlayerScore()
    {
        return cardSpawner.playerScore;
    }

    private void HandleDrawOrPass()
    {
        Debug.Log("Player turn: Draw or Pass.");
        uiManager.ShowUI();
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
        uiManager.ShowNotification("Select one of your cards to lock.");
        yield return StartCoroutine(WaitForPlayerLockSelection());

        cardSpawner.NPCLockAndSwapSelection();

        uiManager.ShowNotification("Select one of NPC's cards to swap.");
        yield return StartCoroutine(WaitForPlayerSwapSelection());

        string swapDetails = cardSpawner.EvaluateSwapAndLock(PlayerLockedCard, PlayerSelectedCardForSwap);
        if (!swapDetails.Contains("swapped"))
        {
            Debug.Log("Swap gerçekleşmedi, kartlar korundu.");
            uiManager.ShowNotification("Swap failed. Cards remained locked.");
        }
        else
        {
            Debug.Log(swapDetails);
            uiManager.ShowNotification(swapDetails);
        }

        isSwapAndLockTurnActive = false;
        Debug.Log("Swap and Lock Phase Completed.");
        currentTurn++;
        PlayTurn();
    }

    private IEnumerator WaitForPlayerLockSelection()
    {
        PlayerLockedCard = null;

        while (PlayerLockedCard == null)
        {
            yield return null;
        }

        Debug.Log($"Player locked the card with value: {PlayerLockedCard.value}");
        isPlayerActionComplete = true;
    }

    private IEnumerator WaitForPlayerSwapSelection()
    {
        PlayerSelectedCardForSwap = null;

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
        if (PlayerLockedCard == null)
        {
            PlayerLockedCard = card;
            Debug.Log($"Player locked card: {card.value}");
            card.SelectCard(true);
            isPlayerActionComplete = true;
        }
        else
        {
            Debug.LogWarning("Player has already locked a card.");
        }
    }

    public void SelectPlayerCardForSwap(CardValue card)
    {
        if (PlayerSelectedCardForSwap == null)
        {
            PlayerSelectedCardForSwap = card;
            Debug.Log($"Player selected card for swap: {card.value}");
            card.SelectCard(true);
        }
        else
        {
            Debug.LogWarning("Player has already selected a card for swap.");
        }
    }

    public void UpdateScores()
    {
        uiManager.UpdateScores(cardSpawner.playerScore, cardSpawner.npcScore);
        Debug.Log($"Player Score: {cardSpawner.playerScore}, NPC Score: {cardSpawner.npcScore}");
    }

    private void UpdateCardInteractivity()
    {
        foreach (var card in FindObjectsOfType<CardValue>())
        {
            card.SetSwapTurnActive(currentTurn == 4);
        }
    }
}





