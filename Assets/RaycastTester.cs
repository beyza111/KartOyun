using UnityEngine;

public class RaycastTester : MonoBehaviour
{
    public LayerMask cardLayer; // Kartlar için özel layer

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Sol fare tuşu
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, cardLayer)) // Sadece Card layer'ını kontrol eder
            {
                Debug.Log($"Hit Object: {hit.transform.name}");
            }
            else
            {
                Debug.Log("No object hit.");
            }
        }
    }
}

