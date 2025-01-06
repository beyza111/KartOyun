using UnityEngine;
using TMPro;

public class TurnUIManager : MonoBehaviour
{
    public GameObject uiPanel; // UI Panel
    public TMP_Text playerScoreText; // Player score text
    public TMP_Text npcScoreText; // NPC score text
    public TMP_Text notificationText; // Notification text

    private TurnManager turnManager;

    void Start()
    {
        turnManager = FindObjectOfType<TurnManager>();
        HideUI();
        HideNotification();
    }

    void Update()
    {
        // Draw card with "X" key
        if (Input.GetKeyDown(KeyCode.X))
        {
            OnPlayerDrawButton();
        }

        // Pass turn with "Y" key
        if (Input.GetKeyDown(KeyCode.Y))
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
        turnManager.StartLockSelection();
    }

    public void StartSwapCardSelection()
    {
        ShowNotification("Select an opponent's card to swap.");
        turnManager.StartSwapSelection();
    }
}






