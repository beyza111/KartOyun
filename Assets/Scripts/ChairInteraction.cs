using UnityEngine;

public class ChairInteraction : MonoBehaviour
{
    public Transform sitPoint;
    public TurnManager turnManager;
    public TurnUIManager uiManager;
    public Transform leaveSpawnPoint; // Sandalyeden ayrılınca spawn olunacak nokta

    private bool isPlayerNear = false;
    private Transform playerTransform;
    private bool welcomeMessageShown = false;
    private bool isSitting = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && isPlayerNear)
        {
            if (isSitting)
            {
                Leave();
            }
            else
            {
                Sit();
            }
        }
    }

    void Sit()
    {
        if (playerTransform == null || sitPoint == null)
        {
            Debug.LogError("Player or SitPoint is not assigned!");
            return;
        }

        playerTransform.position = sitPoint.position;
        playerTransform.rotation = sitPoint.rotation;

        playerTransform.GetComponent<PlayerController>().enabled = false;
        isSitting = true;

        if (!welcomeMessageShown)
        {
            uiManager.ShowNotification("Oyuna Hoş Geldiniz!");
            welcomeMessageShown = true;
        }

        turnManager.StartGame();
    }

    void Leave()
    {
        if (playerTransform == null || leaveSpawnPoint == null)
        {
            Debug.LogError("Player or LeaveSpawnPoint is not assigned!");
            return;
        }

        playerTransform.position = leaveSpawnPoint.position;
        playerTransform.rotation = leaveSpawnPoint.rotation;

        playerTransform.GetComponent<PlayerController>().enabled = true;
        isSitting = false;

        uiManager.ShowNotification("Oyundan ayrıldınız. Yeni bir seviyeye geçmek için tekrar E'ye basın.");
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            playerTransform = other.transform;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            playerTransform = null;
        }
    }
}

