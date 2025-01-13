using UnityEngine;
using UnityEngine.EventSystems;

public class CardManager : MonoBehaviour
{
    public Camera cardGameCamera; // Kart oyunu kamerası

    void Update()
    {
        if (FindObjectOfType<PlayerController>().IsCardGameMode()) // Kart oyun modunda çalış
        {
            HandleCardClick();
        }
    }

    private void HandleCardClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cardGameCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit) && hit.transform.CompareTag("Card"))
            {
                CardValue cardValue = hit.transform.GetComponent<CardValue>();
                if (cardValue != null && cardValue.IsInteractable())
                {
                    if (TurnManager.Instance.IsSwapAndLockTurn())
                    {
                        if (cardValue.IsPlayerCard() && TurnManager.Instance.PlayerLockedCard == null)
                        {
                            TurnManager.Instance.LockPlayerCard(cardValue);
                            Debug.Log($"Player card locked: {cardValue.value}");
                        }
                        else if (cardValue.IsNPCCard() && TurnManager.Instance.PlayerSelectedCardForSwap == null)
                        {
                            TurnManager.Instance.SelectPlayerCardForSwap(cardValue);
                            Debug.Log($"NPC card selected for swap: {cardValue.value}");
                        }
                        else
                        {
                            Debug.LogWarning("Kart seçimi zaten tamamlandı.");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Swap and Lock fazında değilsiniz.");
                    }
                }
            }
        }
    }



}
