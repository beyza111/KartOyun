using System.Collections;
using UnityEngine;
using System;
using System.Runtime.Serialization;

public class TurnManager : MonoBehaviour
{
    public int currentTurn = 1;
    public int turnsPerLevel = 7; // Toplam tur sayısı

    public CardSpawner cardSpawner;
    public TurnUIManager uiManager;
    public NPCController npcController;

    public event Action OnLevelCompleted; // Level tamamlandığında tetiklenecek event

    public ObjectManager objectManager; // ObjectManager referansı


    public CardValue PlayerLockedCard { get; private set; }
    public CardValue PlayerSelectedCardForSwap { get; private set; }

    private bool isSwapAndLockTurnActive = false;
    private bool isPlayerActionComplete = false; // Oyuncu aksiyonu tamamladı mı?
    public bool isLevelComplete = true;


    public void StartGame()
    {
        Debug.Log("Starting Game...");
        cardSpawner.StartLevel();
        UpdateCardInteractivity();
        cardSpawner.EnableCardInteractivity(); // Kartları interaktif hale getir
        PlayTurn();
    }

    public void PlayTurn()
    {
        if (isLevelComplete)
        {
            Debug.LogWarning("Cannot play turn. Level is complete.");
            return;
        }

        Debug.Log($"Turn {currentTurn} started.");
        isPlayerActionComplete = false; // Her tur başlangıcında sıfırla
        UpdateCardInteractivity();

        int maxTurns = GetTurnsForCurrentLevel();

        if (currentTurn > maxTurns)
        {
            EndLevel();
            return;
        }

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
                HandleDrawOrPass();
                break;
            case 6:
                HandleDrawWithHint();
                break;
            case 7:
                HandleFinalTurn();
                break;
            case 8:
                if (cardSpawner.CurrentLevel == 2 || cardSpawner.CurrentLevel == 3)
                {
                    HandleDrawOrPass();
                }
                else
                {
                    Debug.LogWarning("Turn 8 is only valid for Level 2 or Level 3.");
                    EndLevel();
                }
                break;
            case 9:
                if (cardSpawner.CurrentLevel == 3)
                {
                    ShowScoreDifference();
                }
                else
                {
                    Debug.LogWarning("Turn 9 is only valid for Level 3.");
                    EndLevel();
                }
                break;
            case 10:
                if (cardSpawner.CurrentLevel == 3)
                {
                    HandleDrawOrPass();
                }
                else
                {
                    Debug.LogWarning("Turn 10 is only valid for Level 3.");
                    EndLevel();
                }
                break;
            default:
                EndLevel();
                break;

        }
    }

    private int GetTurnsForCurrentLevel()
    {
        switch (cardSpawner.CurrentLevel)
        {
            case 1: return 7; // 1. Level için 7 tur
            case 2: return 8; // 2. Level için 8 tur
            case 3: return 10; // 3. Level için 10 tur
            default:
                Debug.LogError("Invalid level!");
                return 7; // Varsayılan olarak 7 tur
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
            uiManager.ShowNotification("Bir kart çektiniz!");
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

        if (!cardSpawner.CurrentPlayerPositions.Contains(PlayerLockedCard.transform.parent) ||
            !cardSpawner.CurrentNPCPositions.Contains(PlayerSelectedCardForSwap.transform.parent))
        {
            Debug.LogError("Invalid card positions during Swap and Lock.");
            uiManager.ShowNotification("Invalid card positions. Swap failed.");
            currentTurn++;
            PlayTurn();
            yield break;
        }

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

        isLevelComplete = true; // Level tamamlandığında işaretle
        OnLevelCompleted?.Invoke(); // Level tamamlandığını bildir
        cardSpawner.DisableCardInteractivity(); // Kart etkileşimlerini kapat

        // Objeleri seviyeye göre etkinleştir
        objectManager.ShowObjectsForLevel(cardSpawner.CurrentLevel);

        currentTurn = 1; // Turu sıfırla
        cardSpawner.CurrentLevel++; // Yeni seviyeye geç
        Debug.Log($"Next level: {cardSpawner.CurrentLevel}. Sit to start the new level.");
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

    //calismazsa cikar
    private void ShowScoreDifference()
    {
        int scoreDifference = Mathf.Abs(cardSpawner.playerScore - cardSpawner.npcScore);
        string leader = cardSpawner.playerScore > cardSpawner.npcScore ? "Player" : "NPC";

        uiManager.ShowNotification($"Score difference: {scoreDifference}. {leader} is leading.");
        Debug.Log($"Score difference: {scoreDifference}. {leader} is leading.");

        currentTurn++;
        PlayTurn();
    }

}
