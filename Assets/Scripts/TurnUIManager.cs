using UnityEngine;
using TMPro;

public class TurnUIManager : MonoBehaviour
{
    public GameObject uiPanel; // UI Panel
    public TMP_Text playerScoreText; // Player score text
    public TMP_Text npcScoreText; // NPC score text
    public TMP_Text notificationText; // Notification text

    private TurnManager turnManager;
    private bool isSelectingCardToLock = false;
    private bool isSelectingCardToSwap = false;

    void Start()
    {
        turnManager = FindObjectOfType<TurnManager>();
        HideUI();
        HideNotification();
    }

    void Update()
    {
        if (isSelectingCardToLock || isSelectingCardToSwap)
        {
            HandleMouseSelection();
        }

        // Draw card with "X" key
        if (Input.GetKeyDown(KeyCode.X) && !isSelectingCardToLock && !isSelectingCardToSwap)
        {
            OnPlayerDrawButton();
        }

        // Pass turn with "Y" key
        if (Input.GetKeyDown(KeyCode.Y) && !isSelectingCardToLock && !isSelectingCardToSwap)
        {
            OnPlayerPassButton();
        }
    }

    public void ShowUI()
    {
        uiPanel.SetActive(true);
    }

    public void HideUI()
    {
        uiPanel.SetActive(false);
    }

    public void UpdateScores(int playerScore, int npcScore)
    {
        playerScoreText.text = $"Player Score: {playerScore}";
        npcScoreText.text = $"NPC Score: {npcScore}";
    }

    public void ShowNotification(string message, float duration = 3f)
    {
        notificationText.text = message;
        notificationText.gameObject.SetActive(true);

        CancelInvoke(nameof(HideNotification));
        Invoke(nameof(HideNotification), duration);
    }

    private void HideNotification()
    {
        notificationText.gameObject.SetActive(false);
    }

    public void OnPlayerDrawButton()
    {
        Debug.Log("Player drew a card.");
        if (turnManager != null)
        {
            turnManager.PlayerTurn(true);
        }
        HideUI();
    }

    public void OnPlayerPassButton()
    {
        Debug.Log("Player passed the turn.");
        if (turnManager != null)
        {
            turnManager.PlayerTurn(false);
        }
        HideUI();
    }

    public void StartLockCardSelection()
    {
        ShowNotification("Select a card to lock.");
        isSelectingCardToLock = true;
    }

    public void StartSwapCardSelection()
    {
        ShowNotification("Select an opponent's card to swap.");
        isSelectingCardToSwap = true;
    }

    private void HandleMouseSelection()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                CardValue cardValue = hit.collider.GetComponent<CardValue>();
                if (cardValue != null)
                {
                    if (isSelectingCardToLock)
                    {
                        LockPlayerCard(cardValue);
                    }
                    else if (isSelectingCardToSwap)
                    {
                        SwapWithNPC(cardValue);
                    }
                }
            }
        }
    }

    private void LockPlayerCard(CardValue card)
    {
        card.IsLocked = true;
        Debug.Log($"Player locked card with value: {card.value}");
        ShowNotification($"You locked card with value: {card.value}");
        isSelectingCardToLock = false;

        // Notify TurnManager that the player has locked their card
        turnManager.LockPlayerCard(card);
    }

    private void SwapWithNPC(CardValue npcCard)
    {
        Debug.Log($"Player selected NPC card for swapping.");
        ShowNotification($"You selected NPC card with value: {npcCard.value}");
        isSelectingCardToSwap = false;

        // Notify TurnManager to process the swap
        turnManager.SelectPlayerCardForSwap(npcCard);
    }
}





