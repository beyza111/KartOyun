using UnityEngine;

public class CardDrawSoundManager : MonoBehaviour
{
    public AudioSource audioSource; // Ses kaynağı
    public AudioClip cardDrawSound; // Kart çekme sesi

    private void Start()
    {
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void PlayCardDrawSound()
    {
        if (cardDrawSound != null)
        {
            audioSource.PlayOneShot(cardDrawSound); // Kart çekme sesi çal
        }
        else
        {
            Debug.LogWarning("Card draw sound is not assigned!");
        }
    }
}
