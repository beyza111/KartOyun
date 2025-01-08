using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardSpawner : MonoBehaviour
{
    public DeckManager deckManager;
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
    public CardValue NPCLockedCard { get; private set; }

    public void StartLevel(int level = 1)
    {
        Debug.Log("Starting level: " + level);
        CurrentLevel = level;
        deckManager.ResetDeckForNewLevel();

        // Yeni levelde kart çekmeye devam edilebilir
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

            if (cardType == "Player")
                playerScore += cardValue.value;
            else
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

        var npcTopCards = GetTopCards(CurrentNPCPositions, 3);
        NPCLockedCard = npcTopCards[Random.Range(0, npcTopCards.Count)];
        NPCLockedCard.IsLocked = true;
        Debug.Log($"NPC locked card: {NPCLockedCard.value}");

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
        string swapDetails = "";

        var npcLowestCard = GetLowestCard(CurrentNPCPositions);
        var playerLowestCard = GetLowestCard(CurrentPlayerPositions);


        if (playerSelectedSwapCard != null && !playerSelectedSwapCard.IsLocked && npcLowestCard != null)
        {
            Debug.Log($"Attempting swap: NPC's card selected by player ({playerSelectedSwapCard.value}) with Player's card ({playerLowestCard.value})");


            SwapCards(playerSelectedSwapCard, playerLowestCard);
            swapDetails += $"Player's card ({playerSelectedSwapCard.value}) swapped with NPC's card ({playerLowestCard.value}). ";


        }
        else
        {
            swapDetails += "Player's swap failed or locked card selected. ";
        }

        if (SelectedNPCSwapCard != null && !SelectedNPCSwapCard.IsLocked && playerLowestCard != null)
        {
            Debug.Log($"\"Attempting swap: Player's card selected by NPC ({SelectedNPCSwapCard.value}) with NPC's card ({npcLowestCard.value})");


            SwapCards(SelectedNPCSwapCard, npcLowestCard);
            swapDetails += $"NPC's card ({SelectedNPCSwapCard.value}) swapped with Player's card ({npcLowestCard.value}). ";

        }
        else
        {
            swapDetails += "NPC's swap failed or locked card selected. ";
        }

        return swapDetails;
    }

    public void SwapCards(CardValue playerCard, CardValue npcCard)
    {

        Debug.Log($"Swapping Player's card ({playerCard.value}) with NPC's card ({npcCard.value}).");
        StartCoroutine(SwapCardsWithAnimation(playerCard, npcCard));

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

        return lowestCard;
    }



    private IEnumerator SwapCardsWithAnimation(CardValue playerCard, CardValue npcCard)
    {
        Transform playerTransform = playerCard.transform;
        Transform npcTransform = npcCard.transform;

        Vector3 playerStartPos = playerTransform.position;
        Vector3 npcStartPos = npcTransform.position;

        float duration = 1.5f;
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

   

}
