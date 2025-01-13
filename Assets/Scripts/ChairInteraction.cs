using UnityEngine;
using TMPro; // TMP için gerekli

public class ChairInteraction : MonoBehaviour
{
    public Transform sitPoint;
    public TurnManager turnManager;
    public TurnUIManager uiManager;
    public Transform leaveSpawnPoint; // Sandalyeden ayrılınca spawn olunacak nokta
    public TMP_Text interactionText; // [E] Otur metni (TMP Pro kullanıyor)

    private bool isPlayerNear = false;
    private Transform playerTransform;
    private bool welcomeMessageShown = false;
    private bool isSitting = false;

    void Start()
    {
        if (interactionText != null)
        {
            interactionText.enabled = false; // Başlangıçta metni gizle
        }
    }

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

        var playerController = playerTransform.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.enabled = false;
            playerController.isPlayingCardGame = true;
            playerController.mainCamera.enabled = false;
            playerController.cardGameCamera.enabled = true;
        }

        isSitting = true;

        if (!welcomeMessageShown)
        {
            uiManager.ShowNotification("Oyuna Hoş Geldiniz!");
            welcomeMessageShown = true;
        }

        // Değişkenleri sıfırla ve oyunu başlat
        turnManager.isLevelComplete = false;
        turnManager.ResetCardSelections(); // Kart seçimlerini sıfırla
        turnManager.StartGame(); // Yeni seviyeyi başlat

        if (interactionText != null)
        {
            interactionText.enabled = false; // Oturunca metni gizle
        }
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

        var playerController = playerTransform.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.enabled = true;
            playerController.isPlayingCardGame = false;
            playerController.mainCamera.enabled = true;
            playerController.cardGameCamera.enabled = false;
        }

        var characterController = playerTransform.GetComponent<CharacterController>();
        if (characterController != null)
        {
            characterController.enabled = true;
        }

        isSitting = false;

        uiManager.ShowNotification("Oyundan ayrıldınız. Yeni bir seviyeye geçmek için tekrar E'ye basın.");

        if (interactionText != null)
        {
            interactionText.enabled = true;
        }
    }




    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            playerTransform = other.transform;

            if (interactionText != null && !isSitting)
            {
                interactionText.enabled = true; // Oyuncu yaklaştığında metni göster
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            playerTransform = null;

            if (interactionText != null)
            {
                interactionText.enabled = false; // Oyuncu uzaklaştığında metni gizle
            }
        }
    }
}
