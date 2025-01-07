using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class CardValue : MonoBehaviour, IPointerClickHandler
{
    public int value; // Kart değeri
    public CardData cardData; // CardData referansı
    public bool IsLocked { get; set; } = false; // Kart kilitli mi?

    private Renderer cardRenderer;
    private bool isSelected = false; // Kart seçili mi?
    private Color originalColor; // Kartın orijinal rengi
    private TurnManager turnManager;
    private Coroutine hoverCoroutine; // Bekleme kontrolü için coroutine

    private static bool playerCardLocked = false; // Oyuncu kartını kilitledi mi?
    private static bool npcCardSelected = false; // NPC kartı seçildi mi?

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
        Debug.LogWarning("Manual selection is disabled. Use hover to select.");
    }

    public void HighlightCard(bool highlight)
    {
        if (cardRenderer != null && !isSelected)
        {
            cardRenderer.material.color = highlight ? originalColor * 1.5f : originalColor;
        }
    }

    private void OnMouseEnter()
    {
        if (hoverCoroutine == null && turnManager.IsSwapAndLockTurn())
        {
            HighlightCard(true);

            if (IsPlayerCard() && !playerCardLocked)
            {
                hoverCoroutine = StartCoroutine(LockAfterDelay());
            }
            else if (IsNPCCard() && !npcCardSelected)
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

        if (!isSelected)
        {
            HighlightCard(false);
        }
    }

    private IEnumerator LockAfterDelay()
    {
        yield return new WaitForSeconds(3f);

        if (!IsLocked && !playerCardLocked)
        {
            turnManager.LockPlayerCard(this);
            playerCardLocked = true;
            isSelected = true;
            HighlightCard(true);
            Debug.Log($"Card {value} locked after 3 seconds.");
        }
    }

    private IEnumerator SwapSelectionAfterDelay()
    {
        yield return new WaitForSeconds(3f);

        if (!npcCardSelected)
        {
            turnManager.SelectPlayerCardForSwap(this);
            npcCardSelected = true;
            isSelected = true;
            HighlightCard(true);
            Debug.Log($"Player selected NPC card {value} for swap after 3 seconds.");
        }
    }

    public void SelectCard(bool select)
    {
        isSelected = select;
        cardRenderer.material.color = select ? originalColor * 2f : originalColor;

        if (select)
        {
            Debug.Log($"Selected card with value: {value}");
        }
    }

    public void SetSwapTurnActive(bool isActive)
    {
        if (cardRenderer != null)
        {
            cardRenderer.material.color = isActive ? originalColor * 1.2f : originalColor;
        }
    }

    public static void ResetTurnSelections()
    {
        playerCardLocked = false;
        npcCardSelected = false;
    }

    private bool IsPlayerCard()
    {
        return turnManager.cardSpawner.CurrentPlayerPositions.Contains(transform.parent);
    }

    private bool IsNPCCard()
    {
        return turnManager.cardSpawner.CurrentNPCPositions.Contains(transform.parent);
    }
}













