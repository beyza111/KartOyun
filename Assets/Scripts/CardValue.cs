using UnityEngine;
using UnityEngine.EventSystems;

public class CardValue : MonoBehaviour, IPointerClickHandler
{
    public int value; // Kart değeri
    public CardData cardData; // CardData referansı
    public bool IsLocked { get; set; } = false; // Kart kilitli mi?

    private Renderer cardRenderer;
    private bool isSelected = false; // Kart seçili mi?
    private Color originalColor; // Kartın orijinal rengi
    private TurnManager turnManager;

   
    private bool isInteractable = false; // Kartın tıklanabilir olup olmadığını kontrol etmek için

    private void Awake()
    {
        cardRenderer = GetComponent<Renderer>();
        if (cardRenderer == null)
        {
            Debug.LogWarning($"CardValue: Renderer not found on {gameObject.name}. Highlighting won't work.");
            return;
        }

        originalColor = cardRenderer.material.color;
        turnManager = FindObjectOfType<TurnManager>();

        if (!GetComponent<Collider>())
        {
            Debug.LogWarning($"CardValue: No collider found on {gameObject.name}. Adding BoxCollider.");
            gameObject.AddComponent<BoxCollider>();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Kart tıklamaları sadece Swap ve Lock turunda işlenir
        if (!turnManager.IsSwapAndLockTurn() || IsLocked)
        {
            Debug.Log("Card click ignored: Not swap turn or locked.");
            return;
        }

        // Oyuncunun kartına tıklanırsa ve henüz kilitli bir kart yoksa
        if (IsPlayerCard())
        {
            if (turnManager.PlayerLockedCard == null)
            {
                turnManager.LockPlayerCard(this);
                turnManager.UpdateCardInteractivity();
                Debug.Log($"Player locked card: {value}");
            }
            else
            {
                Debug.LogWarning("Player can only lock one card. Ignoring click.");
            }
        }
        // NPC'nin kartına tıklanırsa ve henüz bir kart seçilmediyse
        else if (IsNPCCard())
        {
            if (turnManager.PlayerSelectedCardForSwap == null)
            {
                turnManager.SelectPlayerCardForSwap(this);
                turnManager.UpdateCardInteractivity();
                Debug.Log($"Player selected NPC card for swap: {value}");
            }
            else
            {
                Debug.LogWarning("Player can only select one NPC card for swap. Ignoring click.");
            }
        }
        else
        {
            Debug.LogWarning("Invalid card selection.");
        }
    }






    public void HighlightCard(bool highlight)
    {
        if (isInteractable && cardRenderer != null && !isSelected)
        {
            cardRenderer.material.color = highlight ? originalColor * 1.5f : originalColor;
        }
    }

    private void OnMouseEnter()
    {
        if (isInteractable && !isSelected)
        {
            HighlightCard(true);
        }
    }

    private void OnMouseExit()
    {
        if (!isSelected)
        {
            HighlightCard(false);
        }
    }

    public void SelectCard(bool select, bool isSwapPhase = false)
    {
        isSelected = select;
        cardRenderer.material.color = select ? originalColor * 2f : originalColor;

        if (select)
        {
            Debug.Log($"Selected card with value: {value}");
        }
    }

    public void SetInteractivity(bool isInteractive)
    {
        isInteractable = isInteractive;
        GetComponent<Collider>().enabled = isInteractive;
        Debug.Log($"Card {name} interactivity set to {isInteractive}");
    }


    public bool IsPlayerCard()
    {
        bool isPlayer = turnManager.cardSpawner.CurrentPlayerPositions.Contains(transform.parent);
        Debug.Log($"{name} (Value: {value}) isPlayer: {isPlayer}, Parent: {transform.parent.name}");
        return isPlayer;
    }

    public bool IsNPCCard()
    {
        bool isNPC = turnManager.cardSpawner.CurrentNPCPositions.Contains(transform.parent);
        Debug.Log($"{name} (Value: {value}) isNPC: {isNPC}, Parent: {transform.parent.name}");
        return isNPC;
    }




    public void SetSwapTurnActive(bool isActive)
    {
        if (cardRenderer != null)
        {
            cardRenderer.material.color = isActive ? originalColor * 1.2f : originalColor;
        }
    }

    public bool IsInteractable()
    {
        return !IsLocked && isInteractable; // Kart kilitli değil ve interaktifse
    }

}

