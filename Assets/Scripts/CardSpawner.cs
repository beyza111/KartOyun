using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardSpawner : MonoBehaviour
{
    public DeckManager deckManager;
    public TurnUIManager uiManager; // UI Manager referansı


    public List<Transform> playerCardPositionsLevel1;
    public List<Transform> npcCardPositionsLevel1;
    public List<Transform> playerCardPositionsLevel2;
    public List<Transform> npcCardPositionsLevel2;
    public List<Transform> playerCardPositionsLevel3;
    public List<Transform> npcCardPositionsLevel3;

    [SerializeField] public int CurrentLevel = 1; // Varsayılan olarak Level 1


    public List<Transform> CurrentPlayerPositions { get; private set; }
    public List<Transform> CurrentNPCPositions { get; private set; }

    public int playerScore;
    public int npcScore;

    public CardValue SelectedPlayerSwapCard { get; private set; }
    public CardValue SelectedNPCSwapCard { get; private set; }
    public CardValue NPCLockedCard { get; set; }

    public void StartLevel(int level = 1)
    {
        Debug.Log("Starting level: " + level);
        CurrentLevel = level;
        deckManager.ResetDeckForNewLevel();

        // Yeni levelde kart pozisyonlarını ayarla
        if (level == 1)
        {
            CurrentPlayerPositions = playerCardPositionsLevel1;
            CurrentNPCPositions = npcCardPositionsLevel1;
        }
        else if (level == 2)
        {
            CurrentPlayerPositions = playerCardPositionsLevel2;
            CurrentNPCPositions = npcCardPositionsLevel2;
        }
        else if (level == 3)
        {
            CurrentPlayerPositions = playerCardPositionsLevel3;
            CurrentNPCPositions = npcCardPositionsLevel3;
        }
        else
        {
            Debug.LogError("Invalid level!");
            return;
        }


        SpawnCards(CurrentPlayerPositions, "Player");
        SpawnCards(CurrentNPCPositions, "NPC");
        Debug.Log($"Level {level} started.");

        // Skorları hesapla ve UI'yı güncelle
        CalculateScores();
        uiManager.UpdateScores(playerScore, npcScore);
    }
    


    private void SpawnCards(List<Transform> positions, string cardType)
    {
        for (int i = 0; i < positions.Count; i++)
        {
            CardData cardData = deckManager.DrawCard();
            if (cardData == null)
            {
                Debug.LogWarning($"Not enough cards for {cardType}");
                break;
            }

            Transform position = positions[i];
            GameObject card = Instantiate(cardData.cardPrefab, position.position, Quaternion.identity, position);
            card.name = $"{cardType}_Card_{i}";

            CardValue cardValue = card.AddComponent<CardValue>();
            cardValue.value = cardData.cardValue;
            cardValue.cardData = cardData;

            Debug.Log($"Spawned {cardType} card at {position.name}, Value: {cardValue.value}");
        }
    }



    public void ReplaceLowestCard(List<Transform> positions, CardData newCardData, ref int score)
    {
        Transform lowestCardTransform = null;
        int lowestValue = int.MaxValue;
        CardValue lowestCardValue = null;

        foreach (Transform position in positions)
        {
            var cardValue = position.GetComponentInChildren<CardValue>();
            if (cardValue != null && cardValue.value < lowestValue)
            {
                lowestValue = cardValue.value;
                lowestCardTransform = position;
                lowestCardValue = cardValue;
            }
        }

        if (lowestCardTransform != null && lowestCardValue != null)
        {
            Destroy(lowestCardValue.gameObject);

            GameObject newCard = Instantiate(newCardData.cardPrefab, lowestCardTransform.position, Quaternion.identity, lowestCardTransform);
            newCard.name = $"New_Card_{newCardData.cardValue}";

            CardValue newCardValue = newCard.AddComponent<CardValue>();
            newCardValue.value = newCardData.cardValue;
            newCardValue.cardData = newCardData;

            score -= lowestCardValue.value;
            score += newCardValue.value;

            Debug.Log($"Replaced lowest card with value {lowestCardValue.value} by new card with value {newCardValue.value}.");
        }
        else
        {
            Debug.LogWarning("No valid card found to replace.");
        }
    }

    public void NPCLockAndSwapSelection()
    {
        Debug.Log("NPC Lock and Swap Selection Phase");

        // NPC en iyi 3 karttan birini kilitler
        var npcTopCards = GetTopCards(CurrentNPCPositions, 3);
        NPCLockedCard = npcTopCards[Random.Range(0, npcTopCards.Count)];
        NPCLockedCard.IsLocked = true;
        Debug.Log($"NPC locked card: {NPCLockedCard.value}");

        // NPC, oyuncunun en düşük 3 kartından birini seçer
        var playerTopCards = GetTopCards(CurrentPlayerPositions, 3);
        SelectedNPCSwapCard = playerTopCards[Random.Range(0, playerTopCards.Count)];
        Debug.Log($"NPC selected Player card for swap: {SelectedNPCSwapCard.value}");
    }


    public List<CardValue> GetTopCards(List<Transform> positions, int count)
    {
        List<CardValue> topCards = new List<CardValue>();

        foreach (var position in positions)
        {
            var cardValue = position.GetComponentInChildren<CardValue>();
            if (cardValue != null)
                topCards.Add(cardValue);
        }

        topCards.Sort((a, b) => b.value.CompareTo(a.value));
        return topCards.GetRange(0, Mathf.Min(count, topCards.Count));
    }




    public string EvaluateSwapAndLock(CardValue playerLockedCard, CardValue playerSelectedSwapCard)
    {
        if (playerSelectedSwapCard == null)
        {
            Debug.LogError("Player must select a card for swap.");
            return "Swap failed. No card selected.";
        }

        // Player'ın en düşük değerli kartını bul
        var playerLowestCard = GetLowestCard(CurrentPlayerPositions);

        if (playerLowestCard == null)
        {
            Debug.LogError("Player has no valid card to swap.");
            return "Swap failed. No valid player card.";
        }

        // Swap işlemini gerçekleştir ve animasyonu başlat
        SwapCards(playerLowestCard, playerSelectedSwapCard);

        Debug.Log($"Forced swap: Player's lowest card ({playerLowestCard.value}) swapped with NPC's card ({playerSelectedSwapCard.value}).");
        return $"Player's lowest card ({playerLowestCard.value}) swapped with NPC's card ({playerSelectedSwapCard.value}).";
    }

    public void SwapCards(CardValue playerCard, CardValue npcCard)
    {
        Debug.Log($"Performing swap. PlayerCard: {playerCard.name}, NPC_Card: {npcCard.name}");

        // Swap pozisyonlarını güncelle
        Transform playerParent = playerCard.transform.parent;
        Transform npcParent = npcCard.transform.parent;

        playerCard.transform.SetParent(npcParent);
        npcCard.transform.SetParent(playerParent);

        // Animasyonu başlat
        StartCoroutine(SwapCardsWithAnimation(playerCard, npcCard));

        // Swap tamamlandıktan sonra skoru güncelle
        CalculateScores();
        uiManager.UpdateScores(playerScore, npcScore);

        Debug.Log($"Swap completed: Player's card ({playerCard.value}) and NPC's card ({npcCard.value}) swapped positions. Scores updated.");
    }


    private IEnumerator SwapCardsWithAnimation(CardValue playerCard, CardValue npcCard)
    {
        if (playerCard == null || npcCard == null)
        {
            Debug.LogError("SwapCardsWithAnimation: One or both cards are null.");
            yield break;
        }

        Transform playerTransform = playerCard.transform;
        Transform npcTransform = npcCard.transform;

        Vector3 playerStartPos = playerTransform.position;
        Vector3 npcStartPos = npcTransform.position;

        float duration = 1.5f; // Animasyon süresi
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            playerTransform.position = Vector3.Lerp(playerStartPos, npcStartPos, t);
            npcTransform.position = Vector3.Lerp(npcStartPos, playerStartPos, t);

            yield return null;
        }

        playerTransform.position = npcStartPos;
        npcTransform.position = playerStartPos;

        Debug.Log($"Swapped Player card ({playerCard.value}) with NPC card ({npcCard.value}). Animation completed.");
    }




    public CardValue GetLowestCard(List<Transform> positions)
    {
        CardValue lowestCard = null;
        int lowestValue = int.MaxValue;

        foreach (Transform position in positions)
        {
            var cardValue = position.GetComponentInChildren<CardValue>();
            if (cardValue != null && cardValue.value < lowestValue && !cardValue.IsLocked)
            {
                lowestValue = cardValue.value;
                lowestCard = cardValue;
            }
        }

        if (lowestCard != null)
        {
            Debug.Log($"Lowest card found: {lowestCard.name}, Value: {lowestCard.value}");
        }
        else
        {
            Debug.LogWarning("No valid lowest card found.");
        }

        return lowestCard;
    }


    [ContextMenu("Test Level")]
    public void TestLevel()
    {
        StartLevel(CurrentLevel);
    }

    public void DisableCardInteractivity()
    {
        foreach (var card in FindObjectsOfType<CardValue>())
        {
            card.SetInteractivity(false); // Kartların interaktivitesini kapat
        }
        Debug.Log("All cards are now non-interactive.");
    }

    public void EnableCardInteractivity()
    {
        foreach (var card in FindObjectsOfType<CardValue>())
        {
            card.SetInteractivity(true); // Kartların interaktivitesini aç
        }
        Debug.Log("All cards are now interactive.");
    }

    public void CalculateScores()
    {
        playerScore = 0;
        npcScore = 0;

        foreach (var position in CurrentPlayerPositions)
        {
            var cardValue = position.GetComponentInChildren<CardValue>();
            if (cardValue != null)
                playerScore += cardValue.value;
        }

        foreach (var position in CurrentNPCPositions)
        {
            var cardValue = position.GetComponentInChildren<CardValue>();
            if (cardValue != null)
                npcScore += cardValue.value;
        }

        Debug.Log($"Scores updated: Player Score = {playerScore}, NPC Score = {npcScore}");
    }




}