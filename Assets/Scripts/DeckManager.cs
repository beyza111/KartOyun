using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    public List<CardData> cardDatas;
    private List<CardData> deck = new List<CardData>();

    public int DeckCount => deck.Count;

    void Start()
    {
        CreateDeck();
        ShuffleDeck();
    }

    public void CreateDeck()
    {
        deck.Clear();
        foreach (CardData card in cardDatas)
        {
            for (int i = 0; i < 3; i++) // Her karttan 3 tane ekle
            {
                deck.Add(card);
            }
        }
        Debug.Log($"Deck created with {deck.Count} cards.");
    }

    public void ShuffleDeck()
    {
        for (int i = 0; i < deck.Count; i++)
        {
            int randomIndex = Random.Range(0, deck.Count);
            (deck[i], deck[randomIndex]) = (deck[randomIndex], deck[i]);
        }
        Debug.Log("Deck shuffled.");
    }

    public CardData DrawCard()
    {
        if (deck.Count > 0)
        {
            CardData card = deck[0];
            deck.RemoveAt(0);
            return card; // Log'u kaldırdık.
        }
        Debug.LogError("Deck is empty!");
        return null;
    }
}


