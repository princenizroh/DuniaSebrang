using UnityEngine;
using UnityEngine.UI;

namespace DS
{
    public class GraphicsQualityController : MonoBehaviour
    {
        public static GraphicsQualityController Instance { get; private set; }

        [Header("Tombol Grafik")]
        public Button lowButton;
        public Button mediumButton;
        public Button highButton;

        private const string GraphicsQualityKey = "GraphicsQuality";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject); // Hapus duplikat
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            SetupButtonListeners();

            // Terapkan preferensi grafik sebelumnya
            int savedQuality = PlayerPrefs.GetInt(GraphicsQualityKey, QualitySettings.GetQualityLevel());
            SetQuality(savedQuality);
        }

        private void SetupButtonListeners()
        {
            if (lowButton != null)
                lowButton.onClick.AddListener(() => SetQuality(1));

            if (mediumButton != null)
                mediumButton.onClick.AddListener(() => SetQuality(2));

            if (highButton != null)
                highButton.onClick.AddListener(() => SetQuality(3));
        }

        public void SetQuality(int qualityIndex)
        {
            qualityIndex = Mathf.Clamp(qualityIndex, 0, QualitySettings.names.Length - 1);

            QualitySettings.SetQualityLevel(qualityIndex, true);
            PlayerPrefs.SetInt(GraphicsQualityKey, qualityIndex);
            PlayerPrefs.Save();

            Debug.Log($"Kualitas grafik diatur ke: {QualitySettings.names[qualityIndex]}");
        }

        /// <summary>
        /// Dipanggil saat scene baru jika tombol UI berubah.
        /// </summary>
        public void RebindButtons(Button low, Button medium, Button high)
        {
            // Bersihkan listener lama
            if (lowButton != null) lowButton.onClick.RemoveAllListeners();
            if (mediumButton != null) mediumButton.onClick.RemoveAllListeners();
            if (highButton != null) highButton.onClick.RemoveAllListeners();

            // Set tombol baru
            lowButton = low;
            mediumButton = medium;
            highButton = high;

            // Pasang ulang listener
            SetupButtonListeners();
        }
    }
}
