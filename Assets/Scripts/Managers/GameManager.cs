using UnityEngine;

namespace DS
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private GameObject startText; // Referensi ke teks awal
        [SerializeField] private PlayerOpeningGuide playerOpeningGuide; // Referensi ke PlayerOpeningGuide

        private void Awake()
        {
            // Pastikan teks awal tidak aktif di awal
            if (startText != null)
            {
                startText.SetActive(false);
            }

            // Berlangganan ke event PlayerOpeningGuide jika tersedia
            if (playerOpeningGuide != null)
            {
                playerOpeningGuide.OnPlayerDetected += HandlePlayerDetected;
            }
        }

        private void Start()
        {
            // Tampilkan teks awal
            if (startText != null)
            {
                startText.SetActive(true);
            }
        }

        private void HandlePlayerDetected()
        {
            // Sembunyikan teks awal ketika player terdeteksi
            if (startText != null)
            {
                startText.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe dari event untuk mencegah memory leak
            if (playerOpeningGuide != null)
            {
                playerOpeningGuide.OnPlayerDetected -= HandlePlayerDetected;
            }
        }
    }
}