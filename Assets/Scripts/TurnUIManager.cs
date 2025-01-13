using UnityEngine;
using TMPro;

public class TurnUIManager : MonoBehaviour
{
    public GameObject uiPanel; // UI Panel
    public TMP_Text playerScoreText; // Player score text
    public TMP_Text npcScoreText; // NPC score text
    public TMP_Text notificationText; // Notification text
    public GameObject promptUI; // Prompt UI objesi
    public TMPro.TextMeshProUGUI promptText; // Prompt mesajını gösterecek Text


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
        playerScoreText.text = $"Erica Skor: {playerScore}";
        npcScoreText.text = $"Cin Skor: {npcScore}";
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
        Debug.Log("Bir kart çektiniz!");
        if (turnManager != null)
        {
            turnManager.PlayerTurn(true);
        }
        HideUI();
    }

    public void OnPlayerPassButton()
    {
        Debug.Log("Turu pas geçtiniz.");
        if (turnManager != null)
        {
            turnManager.PlayerTurn(false);
        }
        HideUI();
    }

    public void StartLockCardSelection()
    {
        ShowNotification("Korumak için bir kart seçiniz.");
        turnManager.StartLockSelection();
    }

    public void StartSwapCardSelection()
    {
        ShowNotification("Takas için bir kart seçiniz.");
        turnManager.StartSwapSelection();
    }
}






