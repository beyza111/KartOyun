using UnityEngine;

public class ObjectManager : MonoBehaviour
{
    public GameObject[] objects; // Tüm objeleri buraya ekle
    public float rotationSpeed = 100f; // Döndürme hızı
    private GameObject selectedObject; // Seçilen obje
    private int currentLevel = 0;

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
            objects[i].SetActive(i < objectCount);
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

    // Obje seçimini kontrol et
    private void HandleObjectSelection()
    {
        if (Input.GetMouseButtonDown(0)) // Sol mouse tıklandığında
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.CompareTag("Inspectable"))
                {
                    selectedObject = hit.transform.gameObject;
                }
            }
        }
    }

    // Seçilen objeyi döndür
    private void RotateSelectedObject()
    {
        if (selectedObject != null && Input.GetMouseButton(0)) // Sol mouse basılıyken döndür
        {
            float rotX = Input.GetAxis("Mouse X") * rotationSpeed * Mathf.Deg2Rad;
            float rotY = Input.GetAxis("Mouse Y") * rotationSpeed * Mathf.Deg2Rad;

            selectedObject.transform.Rotate(Vector3.up, -rotX, Space.World);
            selectedObject.transform.Rotate(Vector3.right, rotY, Space.World);
        }
    }
}
