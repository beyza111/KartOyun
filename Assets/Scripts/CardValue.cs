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
    private bool isSwapTurnActive = false; // Swap turu aktif mi?
    private bool isSelected = false; // Kart seçili mi?
    private Color originalColor; // Kartın orijinal rengi
    private TurnManager turnManager;
    private Coroutine lockCoroutine;
    private static CardValue selectedPlayerCard;
    private static CardValue selectedNPCCard;

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

        if (Owner == CardOwner.Player && selectedPlayerCard == null)
        {
            selectedPlayerCard = this;
            Debug.Log($"Player selected card with value: {value}");
            SelectCard(true);
        }
        else if (Owner == CardOwner.NPC && selectedPlayerCard != null && selectedNPCCard == null)
        {
            selectedNPCCard = this;
            Debug.Log($"NPC card selected with value: {value}");
            SelectCard(true);

            if (!selectedNPCCard.IsLocked)
            {
                PerformSwap(selectedPlayerCard, selectedNPCCard);
            }
            else
            {
                Debug.Log("NPC card is locked. Swap will not occur.");
            }

            // Seçim tamamlandı, seçimleri sıfırla
            ResetSelections();
        }
        else
        {
            Debug.LogWarning("Invalid or duplicate selection detected.");
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
            if (lockCoroutine == null)
            {
                lockCoroutine = StartCoroutine(LockCardAfterDelay());
            }
        }
    }

    private void OnMouseExit()
    {
        if (!isSelected)
        {
            HighlightCard(false);
        }
        if (lockCoroutine != null)
        {
            StopCoroutine(lockCoroutine);
            lockCoroutine = null;
            Debug.Log($"{gameObject.name}: Lock process canceled.");
        }
    }

    private IEnumerator LockCardAfterDelay()
    {
        Debug.Log($"{gameObject.name}: Locking process started.");
        yield return new WaitForSeconds(3f); // 3 saniye bekle

        if (!IsLocked)
        {
            Debug.Log($"{gameObject.name}: Card locked after 3 seconds.");
            IsLocked = true;
            turnManager.LockPlayerCard(this); // Kart kilitlendiğinde PlayerLockedCard set edilir

            if (Owner == CardOwner.Player && selectedPlayerCard == null)
            {
                selectedPlayerCard = this;
                Debug.Log($"Player auto-selected card with value: {value}");
            }
            else if (Owner == CardOwner.NPC && selectedNPCCard == null && selectedPlayerCard != null)
            {
                selectedNPCCard = this;
                Debug.Log($"NPC auto-selected card with value: {value}");

                if (!selectedNPCCard.IsLocked)
                {
                    PerformSwap(selectedPlayerCard, selectedNPCCard);
                }
                else
                {
                    Debug.Log("NPC card is locked. Swap will not occur.");
                }

                ResetSelections();
            }
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

    private void ResetSelections()
    {
        selectedPlayerCard = null;
        selectedNPCCard = null;
        Debug.Log("Selections reset.");
    }

    private void PerformSwap(CardValue playerCard, CardValue npcCard)
    {
        Debug.Log($"Swapping cards: Player card {playerCard.value} with NPC card {npcCard.value}");

        // Kart pozisyonlarını değiştir
        Vector3 tempPosition = playerCard.transform.position;
        playerCard.transform.position = npcCard.transform.position;
        npcCard.transform.position = tempPosition;

        Debug.Log("Swap completed.");
    }
}










