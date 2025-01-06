using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class CardValue : MonoBehaviour, IPointerClickHandler
{
    public int value; // Kart değeri
    public CardData cardData; // CardData referansı
    public bool IsLocked { get; set; } = false; // Kart kilitli mi?

    public enum CardOwner { Player, NPC }
    public CardOwner Owner; // Kartın sahibi

    private Renderer cardRenderer;
    private bool isSelected = false; // Kart seçili mi?
    private Color originalColor; // Kartın orijinal rengi
    private TurnManager turnManager;
    private Coroutine hoverCoroutine; // Bekleme kontrolü için coroutine

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
        if (turnManager.IsSwapAndLockTurn())
        {
            if (IsLocked)
            {
                Debug.LogWarning($"Card {value} is locked and cannot be selected.");
                return;
            }

            turnManager.HandleCardSelection(this);
        }
        else
        {
            Debug.LogWarning("Swap turn is not active. Cannot select card.");
        }
    }

    public void HighlightCard(bool highlight)
    {
        if (Owner == CardOwner.Player && cardRenderer != null && !isSelected)
        {
            cardRenderer.material.color = highlight ? originalColor * 1.5f : originalColor;
        }
    }

    private void OnMouseEnter()
    {
        if (turnManager.IsSwapAndLockTurn())
        {
            if (Owner == CardOwner.Player && !IsLocked)
            {
                hoverCoroutine = StartCoroutine(LockAfterDelay());
            }
            else if (Owner == CardOwner.NPC && !IsLocked)
            {
                hoverCoroutine = StartCoroutine(SwapSelectionAfterDelay());
            }
        }
    }

    private void OnMouseExit()
    {
        if (hoverCoroutine != null)
        {
            StopCoroutine(hoverCoroutine);
            hoverCoroutine = null;
        }
        HighlightCard(false);
    }

    private IEnumerator LockAfterDelay()
    {
        yield return new WaitForSeconds(3f); // 3 saniye bekle

        if (!IsLocked)
        {
            turnManager.LockPlayerCard(this);
            Debug.Log($"Card {value} locked after 3 seconds.");
        }
    }

    private IEnumerator SwapSelectionAfterDelay()
    {
        yield return new WaitForSeconds(3f); // 3 saniye bekle
        turnManager.SelectPlayerCardForSwap(this);
        Debug.Log($"Player selected NPC card {value} for swap.");
    }

    public void SelectCard(bool select)
    {
        isSelected = select;

        if (Owner == CardOwner.Player && cardRenderer != null)
        {
            cardRenderer.material.color = select ? originalColor * 2f : originalColor;
        }

        if (select)
        {
            Debug.Log($"{(Owner == CardOwner.Player ? "Player" : "NPC")} selected card with value: {value}");
        }
    }

    public void SetSwapTurnActive(bool isActive)
    {
        if (cardRenderer != null)
        {
            cardRenderer.material.color = isActive ? originalColor * 1.2f : originalColor;
        }
    }
}










