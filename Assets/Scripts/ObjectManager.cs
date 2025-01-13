using UnityEngine;
using TMPro; // TextMeshPro için gerekli

public class ObjectManager : MonoBehaviour
{
    public GameObject[] objects; // Tüm objeleri buraya ekle
    public float rotationSpeed = 100f; // Döndürme hızı
    public TextMeshProUGUI descriptionText; // Obje açıklamaları için TextMeshPro

    private GameObject selectedObject; // Seçilen obje
    private int currentLevel = 0;

    public static bool isSelectingObject = false; // Obje seçimi aktif mi?

    void Start()
    {
        if (descriptionText != null)
        {
            descriptionText.text = ""; // Başlangıçta boş bırak
            descriptionText.gameObject.SetActive(false); // Başlangıçta gizle
        }
    }

    void Update()
    {
        HandleObjectSelection();
        RotateSelectedObject();
    }

    // Objeleri seviyeye göre aktif hale getir
    public void ShowObjectsForLevel(int level)
    {
        currentLevel = level;
        int objectCount = GetObjectCountForLevel(level);

        for (int i = 0; i < objects.Length; i++)
        {
            bool isActive = i < objectCount;
            objects[i].SetActive(isActive);

            // Mesh Renderer'ı aktif hale getir
            var meshRenderer = objects[i].GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                meshRenderer.enabled = isActive;
            }
        }
    }


    // Her seviyeye uygun obje sayısını döndür
    private int GetObjectCountForLevel(int level)
    {
        switch (level)
        {
            case 1: return 3;
            case 2: return 7; // 3+4
            case 3: return 12; // 3+4+5
            default: return 0;
        }
    }

    // Obje veya kart seçimini kontrol et
    private void HandleObjectSelection()
    {
        Camera activeCamera = Camera.main; // Varsayılan olarak MainCamera
        var playerController = FindObjectOfType<PlayerController>();
        if (playerController != null && playerController.isPlayingCardGame)
        {
            activeCamera = playerController.cardGameCamera; // Kart oyun modundaysa kart kamerasını kullan
        }

        if (Input.GetMouseButtonDown(0)) // Sol mouse tıklandığında
        {
            Ray ray = activeCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                // Kart mı seçildi?
                if (hit.transform.CompareTag("Card"))
                {
                    CardValue selectedCard = hit.transform.GetComponent<CardValue>();
                    if (selectedCard != null)
                    {
                        selectedCard.SelectCard(true); // Kart seçimi
                        Debug.Log($"Card selected: {selectedCard.value}");
                        return;
                    }
                }

                // Obje mi seçildi?
                if (hit.transform.CompareTag("Inspectable"))
                {
                    selectedObject = hit.transform.gameObject;
                    isSelectingObject = true; // Obje seçimi başladı
                    Debug.Log($"Object selected: {selectedObject.name}");

                    // Obje açıklamasını göster
                    var objectDescription = selectedObject.GetComponent<ObjectDescription>();
                    if (objectDescription != null && descriptionText != null)
                    {
                        descriptionText.text = objectDescription.description;
                        descriptionText.gameObject.SetActive(true); // Açıklamayı göster
                    }
                }
            }
        }

        if (Input.GetMouseButtonUp(0)) // Mouse bırakıldığında seçim biter
        {
            isSelectingObject = false;

            // Açıklamayı gizle
            if (descriptionText != null)
            {
                descriptionText.gameObject.SetActive(false);
            }
        }
    }

    // Seçilen objeyi döndür
    private void RotateSelectedObject()
    {
        if (selectedObject != null && isSelectingObject && Input.GetMouseButton(0)) // Sol mouse basılıyken döndür
        {
            float rotX = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
            float rotY = Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;

            selectedObject.transform.Rotate(Vector3.up, -rotX, Space.World);
            selectedObject.transform.Rotate(Vector3.right, rotY, Space.World);
        }
    }

}
