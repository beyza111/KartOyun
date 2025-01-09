using UnityEngine;

public class ChairInteraction : MonoBehaviour
{
    public Transform sitPoint; // Oyuncunun oturacağı nokta
    public TurnManager turnManager; // TurnManager referansı
    public TurnUIManager uiManager; // UI Manager referansı
    public Transform leaveSpawnPoint; // Oyuncunun kalktıktan sonra spawn olacağı nokta

    private bool isPlayerNear = false; // Oyuncu sandalyeye yakın mı?
    private Transform playerTransform; // Oyuncunun transform referansı
    private bool isSitting = false; // Oyuncu oturuyor mu?

    private void Start()
    {
        // Oyuncuyu bul ve referans atamasını yap
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
                Debug.Log("Player Transform başarıyla atandı.");
            }
            else
            {
                Debug.LogError("Player GameObject sahnede bulunamadı!");
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && isPlayerNear)
        {
            if (isSitting)
            {
                Leave(); // Kalk
            }
            else
            {
                Sit(); // Otur
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

        isSitting = true;

        uiManager.ShowNotification("Oyuna Hoş Geldiniz!");
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

        isSitting = false;

        uiManager.ShowNotification("Oyundan ayrıldınız. Yeni bir seviyeye geçmek için tekrar E'ye basın.");
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player sandalyeye yaklaştı.");
            isPlayerNear = true;
            playerTransform = other.transform;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player sandalyeden uzaklaştı.");
            isPlayerNear = false;
            playerTransform = null;
        }
    }
}
