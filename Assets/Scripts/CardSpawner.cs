using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardSpawner : MonoBehaviour
{
    public DeckManager deckManager;
    public List<Transform> playerCardPositionsLevel1;
    public List<Transform> npcCardPositionsLevel1;

    public List<Transform> CurrentPlayerPositions { get; private set; }
    public List<Transform> CurrentNPCPositions { get; private set; }

    public int playerScore;
    public int npcScore;

    public CardValue SelectedPlayerSwapCard { get; private set; }
    public CardValue SelectedNPCSwapCard { get; private set; }
    public CardValue NPCLockedCard { get; private set; }

    public void StartLevel(int level = 1)
    {
        Debug.Log("Starting level: " + level);

        switch (level)
        {
            case 1:
                CurrentPlayerPositions = playerCardPositionsLevel1;
                CurrentNPCPositions = npcCardPositionsLevel1;
                break;
            default:
                Debug.LogError("Invalid level!");
                return;
        }

        Debug.Log($"Level {level} started. Spawning cards...");

        if (deckManager.DeckCount < CurrentPlayerPositions.Count + CurrentNPCPositions.Count)
        {
            Debug.LogError("Not enough cards in the deck!");
            return;
        }

        SpawnCards(CurrentPlayerPositions, "Player");
        SpawnCards(CurrentNPCPositions, "NPC");
    }

    private void SpawnCards(List<Transform> positions, string cardType)
    {
        Debug.Log($"Spawning cards for: {cardType}");

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
            cardValue.Owner = cardType == "Player" ? CardValue.CardOwner.Player : CardValue.CardOwner.NPC;

            if (cardType == "Player")
                playerScore += cardValue.value;
            else if (cardType == "NPC")
                npcScore += cardValue.value;
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
        }
    }

    public void StartFlipBackProcess()
    {
        StartCoroutine(FlipCardsToBackAfterDelay());
    }

    private IEnumerator FlipCardsToBackAfterDelay()
    {
        yield return new WaitForSeconds(5f);

        foreach (Transform position in CurrentNPCPositions)
        {
            var card = position.GetComponentInChildren<CardValue>();
            if (card != null)
            {
                // Görünürlüğü kapatmadan kartın arka yüzünü ekle
                GameObject backCard = Instantiate(card.cardData.backcardPrefab,
                                                  card.transform.position + Vector3.up * 0.2f,
                                                  Quaternion.identity,
                                                  card.transform.parent);

                backCard.name = $"BackCard_{position.name}";
                backCard.SetActive(true);

                Debug.Log($"Card at {position.name} flipped to back.");
            }
        }
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

    public void SwapCards(CardValue playerCard, CardValue npcCard)
    {
        int tempValue = playerCard.value;
        playerCard.value = npcCard.value;
        npcCard.value = tempValue;
    }

    public void NPCLockAndSwapSelection()
    {
        Debug.Log("NPC Lock and Swap Selection Phase");

        var npcTopCards = GetTopCards(CurrentNPCPositions, 3);
        NPCLockedCard = npcTopCards[Random.Range(0, npcTopCards.Count)];
        NPCLockedCard.IsLocked = true;
        Debug.Log($"NPC locked card: {NPCLockedCard.value}");

        var playerTopCards = GetTopCards(CurrentPlayerPositions, 3);
        SelectedNPCSwapCard = playerTopCards[Random.Range(0, playerTopCards.Count)];
        Debug.Log($"NPC selected Player card for swap: {SelectedNPCSwapCard.value}");
    }

    public string EvaluateSwapAndLock(CardValue playerLockedCard, CardValue playerSelectedSwapCard)
    {
        string swapDetails = "";

        if (playerSelectedSwapCard != null && !playerSelectedSwapCard.IsLocked)
        {
            var npcSwapCard = GetRandomTopCard(CurrentNPCPositions);
            SwapCards(playerSelectedSwapCard, npcSwapCard);
            swapDetails += $"Player's card ({playerSelectedSwapCard.value}) swapped with NPC's card ({npcSwapCard.value}). ";
        }
        else
        {
            swapDetails += "Player's selected card was locked. No swap occurred. ";
        }

        if (SelectedNPCSwapCard != null && !SelectedNPCSwapCard.IsLocked)
        {
            var playerSwapCard = GetRandomTopCard(CurrentPlayerPositions);
            SwapCards(SelectedNPCSwapCard, playerSwapCard);
            swapDetails += $"NPC's card ({SelectedNPCSwapCard.value}) swapped with Player's card ({playerSwapCard.value}). ";
        }
        else
        {
            swapDetails += "NPC's selected card was locked. No swap occurred.";
        }

        return swapDetails;
    }


    private CardValue GetRandomTopCard(List<Transform> positions)
    {
        var topCards = GetTopCards(positions, 3);
        return topCards[Random.Range(0, topCards.Count)];
    }
}







