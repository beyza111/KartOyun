using System.Collections;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public int currentTurn = 1;
    public int turnsPerLevel = 7; // Toplam tur sayısı

    public CardSpawner cardSpawner;
    public TurnUIManager uiManager;
    public NPCController npcController;

    public GameObject selectionCursor; // Seçim için imleç objesi

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

    private void PlayTurn()
    {
        Debug.Log($"Turn {currentTurn} started.");
        isPlayerActionComplete = false; // Her tur başlangıcında sıfırla (yalnızca swap turunda kullanılacak)
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

    public void PlayerTurn(bool isDraw)
    {
        // Swap ve Lock turunda oyuncu kart seçmeden devam edemez
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
        Debug.Log(swapDetails);
        uiManager.ShowNotification(swapDetails);

        yield return StartCoroutine(AnimateSwap(PlayerLockedCard.transform, PlayerSelectedCardForSwap.transform));

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
        isPlayerActionComplete = true; // Oyuncu aksiyonunu tamamladı
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

    private IEnumerator AnimateSwap(Transform playerCard, Transform npcCard)
    {
        float duration = 1.5f;
        Vector3 playerStartPos = playerCard.position;
        Vector3 npcStartPos = npcCard.position;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            playerCard.position = Vector3.Lerp(playerStartPos, npcStartPos, t);
            npcCard.position = Vector3.Lerp(npcStartPos, playerStartPos, t);

            yield return null;
        }

        playerCard.position = npcStartPos;
        npcCard.position = playerStartPos;

        Debug.Log("Swap animation completed.");
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
        PlayerLockedCard = card;
        Debug.Log($"Player locked card: {card.value}");
        card.SelectCard(true);
        isPlayerActionComplete = true; // Oyuncu aksiyonunu tamamladı
    }

    public void SelectPlayerCardForSwap(CardValue card)
    {
        PlayerSelectedCardForSwap = card;
        Debug.Log($"Player selected card for swap: {card.value}");
        card.SelectCard(true);
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
            card.SetSwapTurnActive(currentTurn == 4);
        }
    }
}








