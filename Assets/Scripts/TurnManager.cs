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
    public static TurnManager Instance { get; set; } // Singleton Instance


    public void StartGame()
    {
        Debug.Log("Starting Game...");
        cardSpawner.StartLevel();
        UpdateCardInteractivity();
        PlayTurn();
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Birden fazla Instance varsa yok et
        }
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

        switch (cardSpawner.CurrentLevel)
        {
            case 1:
                HandleLevel1Turns();
                break;
            case 2:
                HandleLevel2Turns();
                break;
            case 3:
                HandleLevel3Turns();
                break;
            default:
                Debug.LogError("Invalid level!");
                break;
        }
    }

    private void HandleLevel1Turns()
    {
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
                HandleDrawOrPass();
                break;
            case 7:
                HandleFinalTurn();
                break;
            default:
                EndLevel();
                break;
        }
    }

    private void HandleLevel2Turns()
    {
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
                HandleDrawOrPass();
                break;
            case 7:
                HandleDrawWithHint();
                break;
            case 8:
                HandleFinalTurn();
                break;
            default:
                EndLevel();
                break;
        }
    }

    private void HandleLevel3Turns()
    {
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
                HandleDrawOrPass();
                break;
            case 7:
                HandleDrawWithHint();
                break;
            case 8:
                HandleDrawOrPass();
                break;
            case 9:
                ShowScoreDifference();
                break;
            case 10:
                HandleFinalTurn();
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
        UpdateCardInteractivity(); // Sadece 4. turda kartları interaktif yap
        uiManager.ShowNotification("Select one of your cards to lock.");
        yield return StartCoroutine(WaitForPlayerLockSelection());

        cardSpawner.NPCLockAndSwapSelection();

        uiManager.ShowNotification("Select one of NPC's cards to swap.");
        yield return StartCoroutine(WaitForPlayerSwapSelection());

        if (PlayerLockedCard == null || PlayerSelectedCardForSwap == null)
        {
            Debug.LogError("Player did not complete the lock or swap selection.");
            uiManager.ShowNotification("Selection failed. Turn skipped.");
            currentTurn++;
            PlayTurn();
            yield break;
        }

        string swapDetails = cardSpawner.EvaluateSwapAndLock(PlayerLockedCard, PlayerSelectedCardForSwap);
        uiManager.ShowNotification(swapDetails);

        isSwapAndLockTurnActive = false; // Faz tamamlandı
        UpdateCardInteractivity(); // Kartları tekrar pasif yap
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

        isLevelComplete = true; // Level tamamlandığını işaretle
        OnLevelCompleted?.Invoke(); // Level tamamlandığını bildir
        cardSpawner.DisableCardInteractivity(); // Kart etkileşimlerini kapat

        // Objeleri seviyeye göre etkinleştir
        objectManager.ShowObjectsForLevel(cardSpawner.CurrentLevel);

        // Yeni seviyeye geç
        currentTurn = 1; // Turu sıfırla
        cardSpawner.CurrentLevel++; // Seviyeyi artır
        isLevelComplete = false; // Yeni seviye için level tamamlanmadı olarak işaretle
        ResetCardSelections(); // Kart seçimlerini sıfırla

        Debug.Log($"Next level: {cardSpawner.CurrentLevel}. Sit to start the new level.");
        StartGame(); // Yeni seviyeyi başlat
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
        if (PlayerLockedCard == null && card.IsPlayerCard())
        {
            PlayerLockedCard = card;
            card.SelectCard(true);
            Debug.Log($"Player locked card: {card.value}");
        }
        else
        {
            Debug.LogWarning("You can only lock one player card.");
        }
    }



    public void SelectPlayerCardForSwap(CardValue card)
    {
        if (PlayerSelectedCardForSwap == null && card.IsNPCCard())
        {
            PlayerSelectedCardForSwap = card;
            card.SelectCard(true);
            Debug.Log($"Player selected NPC card for swap: {card.value}");
        }
        else
        {
            Debug.LogWarning("You can only select one NPC card for swap.");
        }
    }



    public void UpdateScores()
{
    uiManager.UpdateScores(cardSpawner.playerScore, cardSpawner.npcScore);
    Debug.Log($"Player Score: {cardSpawner.playerScore}, NPC Score: {cardSpawner.npcScore}");
}

    public void UpdateCardInteractivity()
    {
        foreach (var card in FindObjectsOfType<CardValue>())
        {
            if (currentTurn == 4)
            {
                if (card.IsPlayerCard() && PlayerLockedCard == null)
                {
                    card.SetInteractivity(true);
                }
                else if (card.IsNPCCard() && PlayerSelectedCardForSwap == null)
                {
                    card.SetInteractivity(true);
                }
                else
                {
                    card.SetInteractivity(false);
                }
            }
            else
            {
                card.SetInteractivity(false); // Diğer turlarda tüm kartlar pasif
            }
        }

        Debug.Log($"Kartların interaktivitesi {currentTurn}. tur için güncellendi.");
    }


    private void ShowScoreDifference()
{
    int scoreDifference = Mathf.Abs(cardSpawner.playerScore - cardSpawner.npcScore);
    string leader = cardSpawner.playerScore > cardSpawner.npcScore ? "Player" : "NPC";

    uiManager.ShowNotification($"Score difference: {scoreDifference}. {leader} is leading.");
    Debug.Log($"Score difference: {scoreDifference}. {leader} is leading.");

    currentTurn++;
    PlayTurn();
}

public void CheckSwapTurn()
{
    if (IsSwapAndLockTurn())
    {
        Cursor.visible = true; // 4. turda mouse görünür
        Cursor.lockState = CursorLockMode.None;
    }
    else
    {
        Cursor.visible = false; // Diğer turlarda mouse gizli
        Cursor.lockState = CursorLockMode.Locked;
    }
}

    //yeni
    public int GetCurrentTurn()
    {
        return currentTurn;
    }

    private void DisableOtherCardsInCategory(string category)
    {
        foreach (var card in FindObjectsOfType<CardValue>())
        {
            if (category == "Player" && card.IsPlayerCard() && card != PlayerLockedCard)
            {
                card.SetInteractivity(false);
            }
            else if (category == "NPC" && card.IsNPCCard() && card != PlayerSelectedCardForSwap)
            {
                card.SetInteractivity(false);
            }
        }
    }
    public void ResetCardSelections()
    {
        PlayerLockedCard = null;
        PlayerSelectedCardForSwap = null;

        foreach (var card in FindObjectsOfType<CardValue>())
        {
            card.SelectCard(false);
            card.SetInteractivity(false);
        }

        Debug.Log("All card selections have been reset.");
    }

}
