using UnityEngine;
using UnityEngine.EventSystems;

public class CardValue : MonoBehaviour, IPointerClickHandler
{
    public int value; // Kart değeri
    public CardData cardData; // CardData referansı
    public bool IsLocked { get; set; } = false; // Kart kilitli mi?

    public enum CardOwner { Player, NPC }
    public CardOwner Owner; // Kartın sahibi

    private Renderer cardRenderer;
    private bool isSwapTurnActive = false; // Swap turu aktif mi?
    private bool isSelected = false; // Kart seçili mi?
    private Color originalColor; // Kartın orijinal rengi
    private TurnManager turnManager;

    public Texture2D cursorTexture; // Yeni imleç için Texture2D

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

    private void Update()
    {
        // 4. turda özel imleci aktif et
        if (turnManager.currentTurn == 4 && cursorTexture != null)
        {
            Cursor.SetCursor(cursorTexture, Vector2.zero, CursorMode.Auto);
        }
        else
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isSwapTurnActive)
        {
            Debug.LogWarning("Swap turn is not active. Cannot select card.");
            return;
        }

        if (IsLocked)
        {
            Debug.LogWarning($"Card {value} is locked and cannot be selected.");
            return;
        }

        if (Owner == CardOwner.Player && turnManager.PlayerLockedCard == null)
        {
            Debug.Log($"Player locked card with value: {value}");
            turnManager.LockPlayerCard(this);
            SelectCard(true);
        }
        else if (Owner == CardOwner.NPC && turnManager.PlayerSelectedCardForSwap == null)
        {
            Debug.Log($"Player selected NPC card for swap with value: {value}");
            turnManager.SelectPlayerCardForSwap(this);
            SelectCard(true);
        }
        else
        {
            Debug.LogWarning("Invalid action or duplicate selection detected.");
        }
    }

    public void HighlightCard(bool highlight)
    {
        if (cardRenderer != null && !isSelected)
        {
            cardRenderer.material.color = highlight ? originalColor * 1.5f : originalColor;
            if (highlight && turnManager.currentTurn == 4)
            {
                Debug.Log($"{gameObject.name} highlighting: {highlight} (4th turn)");
            }
        }
    }

    private void OnMouseEnter()
    {
        if (isSwapTurnActive && !IsLocked)
        {
            HighlightCard(true);
            if (cursorTexture != null)
            {
                Cursor.SetCursor(cursorTexture, Vector2.zero, CursorMode.Auto);
            }

            if (turnManager.currentTurn == 4)
            {
                Debug.Log($"{gameObject.name} hovered (4th turn, Swap Active).");
            }
        }
    }

    private void OnMouseExit()
    {
        if (!isSelected)
        {
            HighlightCard(false);
        }

        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        if (turnManager.currentTurn == 4)
        {
            Debug.Log($"{gameObject.name} hover ended (4th turn).");
        }
    }

    public void SetSwapTurnActive(bool isActive)
    {
        isSwapTurnActive = isActive;
        Debug.Log($"{gameObject.name} Swap Turn Active: {isActive}");

        if (!isActive)
        {
            HighlightCard(false);
            SelectCard(false); // Seçili durumu kaldır
        }
    }

    public void SelectCard(bool select)
    {
        isSelected = select;

        if (cardRenderer != null)
        {
            cardRenderer.material.color = select ? originalColor * 2f : originalColor; // Parlaklık artırılmış efekt
        }

        if (select)
        {
            Debug.Log($"{(Owner == CardOwner.Player ? "Player" : "NPC")} selected card with value: {value}");
        }
    }
    private void OnMouseDown()
    {
        Debug.Log($"{gameObject.name} clicked!");
    }

}










