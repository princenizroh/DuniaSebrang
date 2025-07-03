using UnityEngine;
using UnityEngine.UI;

namespace DS
{
    public class ResolutionButtonControl : MonoBehaviour
    {
        [Header("Buttons")]
        public Button button720p;
        public Button button1360;
        public Button button1080p;

        private void Start()
        {
            Debug.Log("ResolutionButtonControl: Start");

            // Tambahkan listener tombol
            button720p.onClick.AddListener(() =>
            {
                SetResolution(1280, 720);
            });

            button1360.onClick.AddListener(() =>
            {
                SetResolution(1366, 768);
            });

            button1080p.onClick.AddListener(() =>
            {
                SetResolution(1920, 1080);
            });

            // Coba load resolusi terakhir
            if (PlayerPrefs.HasKey("SavedWidth") && PlayerPrefs.HasKey("SavedHeight"))
            {
                int savedWidth = PlayerPrefs.GetInt("SavedWidth");
                int savedHeight = PlayerPrefs.GetInt("SavedHeight");
                Debug.Log($"Loading saved resolution: {savedWidth}x{savedHeight}");
                SetResolution(savedWidth, savedHeight);
            }
            else
            {
                Debug.Log("No saved resolution found in PlayerPrefs.");
            }
        }

        public void SetResolution(int width, int height)
        {
            Debug.Log($"Setting resolution to: {width}x{height}");
            Screen.SetResolution(width, height, FullScreenMode.FullScreenWindow); // Ganti FullScreenWindow jika perlu

            PlayerPrefs.SetInt("SavedWidth", width);
            PlayerPrefs.SetInt("SavedHeight", height);
            PlayerPrefs.Save();
        }
    }
}
