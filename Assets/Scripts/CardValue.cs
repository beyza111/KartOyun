using UnityEngine;

public class CardValue : MonoBehaviour
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

    private void Awake()
    {
        // Renderer bileşenini al
        cardRenderer = GetComponent<Renderer>();
        if (cardRenderer == null)
        {
            Debug.LogWarning($"CardValue: Renderer not found on {gameObject.name}. Highlighting won't work.");
            return;
        }

        // Kartın orijinal rengini sakla
        originalColor = cardRenderer.material.color;

        // TurnManager'ı bul
        turnManager = FindObjectOfType<TurnManager>();

        // Collider kontrolü
        if (!GetComponent<Collider>())
        {
            Debug.LogWarning($"CardValue: No collider found on {gameObject.name}. Adding BoxCollider.");
            gameObject.AddComponent<BoxCollider>();
        }
    }

    public void FlipCard(bool showBack)
    {
        if (cardData == null)
        {
            Debug.LogError($"CardValue: Missing cardData on {gameObject.name}.");
            return;
        }

        // Kartı çevir
        cardData.cardPrefab.SetActive(!showBack);
        cardData.backcardPrefab.SetActive(showBack);
    }

    public void HighlightCard(bool highlight)
    {
        if (cardRenderer != null && !isSelected) // Eğer kart seçili değilse highlight uygula
        {
            cardRenderer.material.color = highlight ? Color.yellow : originalColor;
        }
    }

    private void OnMouseEnter()
    {
        // Sadece swap turunda ve kart kilitli değilken highlight
        if (isSwapTurnActive && !IsLocked)
        {
            HighlightCard(true);
        }
    }

    private void OnMouseExit()
    {
        // Highlight kaldır
        HighlightCard(false);
    }

    private void OnMouseDown()
    {
        // Yalnızca swap turunda çalış ve kart kilitli değilse işlem yap
        if (!isSwapTurnActive || IsLocked) return;

        if (Owner == CardOwner.Player && turnManager.PlayerLockedCard == null)
        {
            Debug.Log($"Player clicked to lock card with value: {value}");
            turnManager.LockPlayerCard(this); // Oyuncunun kilitleme seçimini bildir
            SelectCard(true); // Kartı seçili olarak işaretle
        }
        else if (Owner == CardOwner.NPC && turnManager.PlayerSelectedCardForSwap == null)
        {
            Debug.Log($"Player selected NPC card for swap with value: {value}");
            turnManager.SelectPlayerCardForSwap(this); // Oyuncunun swap seçimini bildir
            SelectCard(true); // Kartı seçili olarak işaretle
        }
        else
        {
            Debug.LogWarning("Invalid action or duplicate selection detected.");
        }
    }

    public void SetSwapTurnActive(bool isActive)
    {
        isSwapTurnActive = isActive;

        // Swap turu sona erdiğinde tüm görsel değişiklikleri sıfırla
        if (!isActive)
        {
            HighlightCard(false);
            SelectCard(false); // Seçili durumu kaldır
        }
    }

    public void SelectCard(bool select)
    {
        isSelected = select;

        // Seçili kart yeşil renkte gösterilir
        if (cardRenderer != null)
        {
            cardRenderer.material.color = select ? Color.green : originalColor;
        }
    }
}







