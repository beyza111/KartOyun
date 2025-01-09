using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    public List<CardData> cardDatas;
    private List<CardData> deck = new List<CardData>();
    [SerializeField] private int cardsPerLevel = 3;

    public int DeckCount => deck.Count;

    void Start()
    {
        CreateDeck();
        ShuffleDeck();
        Debug.Log($"Each card will have {cardsPerLevel} copies in the deck.");
    }

    public void CreateDeck()
    {
        deck.Clear();
        foreach (CardData card in cardDatas)
        {
            for (int i = 0; i < cardsPerLevel; i++) // cardsPerLevel kullanıldı
            {
                deck.Add(card);
            }
        }
        Debug.Log($"Deck created with {deck.Count} cards. Each card has {cardsPerLevel} copies.");
    }

    public void ShuffleDeck()
    {
        for (int i = deck.Count - 1; i > 0; i--) // Fisher-Yates algoritması
        {
            int randomIndex = Random.Range(0, i + 1);
            (deck[i], deck[randomIndex]) = (deck[randomIndex], deck[i]);
        }
        Debug.Log("Deck shuffled (Fisher-Yates).");
    }
    //bu fonks her el kart cekme saglar
    public CardData DrawCard()
    {
        if (deck.Count == 0)
        {
            Debug.LogWarning("Deck is empty. Resetting deck...");
            ResetDeckForNewLevel();
            return DrawCard();
        }

        CardData card = deck[0];
        deck.RemoveAt(0);
        return card;
    }

    public void ResetDeckForNewLevel()
    {
        Debug.Log($"Resetting deck for new level with {cardsPerLevel} copies per card...");
        CreateDeck();
        ShuffleDeck();
    }
}
