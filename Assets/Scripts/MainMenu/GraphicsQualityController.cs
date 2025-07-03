using UnityEngine;
using UnityEngine.UI;

namespace DS
{
    public class GraphicsQualityController : MonoBehaviour
    {
        [Header("Tombol Grafik")]
        public Button lowButton;
        public Button mediumButton;
        public Button highButton;

        private const string GraphicsQualityKey = "GraphicsQuality"; // Simpan index kualitas grafik

        private void Start()
        {
            // Tambahkan event ke tombol
            lowButton.onClick.AddListener(() => SetQuality(1));    // Low
            mediumButton.onClick.AddListener(() => SetQuality(2)); // Medium
            highButton.onClick.AddListener(() => SetQuality(3));   // High

            // Cek jika ada preferensi yang tersimpan
            if (PlayerPrefs.HasKey(GraphicsQualityKey))
            {
                int savedQuality = PlayerPrefs.GetInt(GraphicsQualityKey);
                SetQuality(savedQuality);
                Debug.Log($"Terapkan kualitas grafik tersimpan: {QualitySettings.names[savedQuality]}");
            }
            else
            {
                Debug.Log("Tidak ada preferensi grafik tersimpan, menggunakan default.");
            }
        }

        public void SetQuality(int qualityIndex)
        {
            // Batasi agar tidak melebihi jumlah level yang tersedia
            qualityIndex = Mathf.Clamp(qualityIndex, 0, QualitySettings.names.Length - 1);

            QualitySettings.SetQualityLevel(qualityIndex, true);
            PlayerPrefs.SetInt(GraphicsQualityKey, qualityIndex);
            PlayerPrefs.Save();

            Debug.Log($"Kualitas grafik diatur ke: {QualitySettings.names[qualityIndex]}");
        }
    }
}
