using UnityEngine;

public class ChairInteraction : MonoBehaviour
{
    public Transform sitPoint;
    public TurnManager turnManager;
    public TurnUIManager uiManager;

    private bool isPlayerNear = false;
    private Transform playerTransform;
    private bool welcomeMessageShown = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && isPlayerNear)
        {
            Sit();
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

        if (!welcomeMessageShown)
        {
            uiManager.ShowNotification("Oyuna Hoş Geldiniz!");
            welcomeMessageShown = true;
        }

        turnManager.StartGame();
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



