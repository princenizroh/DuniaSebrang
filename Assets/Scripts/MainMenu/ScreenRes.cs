using UnityEngine;
using UnityEngine.UI;

namespace DS
{
    public class ScreenRes : MonoBehaviour
    {
        public static ScreenRes Instance { get; private set; }

        [Header("Tombol UI")]
        public Button windowedButton;
        public Button fullscreenButton;

        private const string ScreenModeKey = "ScreenMode"; // 0 = Windowed, 1 = Fullscreen

        private void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject); // Hapus duplikat saat pindah scene
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject); // Pertahankan antar scene
        }

        private void Start()
        {
            // Pasang listener tombol jika tombol terhubung
            if (windowedButton != null)
                windowedButton.onClick.AddListener(SetWindowedMode);

            if (fullscreenButton != null)
                fullscreenButton.onClick.AddListener(SetFullscreenMode);

            // Terapkan preferensi sebelumnya
            int savedMode = PlayerPrefs.GetInt(ScreenModeKey, 0); // Default: Windowed
            if (savedMode == 1)
            {
                SetFullscreenMode();
            }
            else
            {
                SetWindowedMode();
            }
        }

        public void SetWindowedMode()
        {
            Screen.fullScreenMode = FullScreenMode.Windowed;
            PlayerPrefs.SetInt(ScreenModeKey, 0);
            PlayerPrefs.Save();
            Debug.Log("Mode layar diatur ke: Windowed");
        }

        public void SetFullscreenMode()
        {
            Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
            PlayerPrefs.SetInt(ScreenModeKey, 1);
            PlayerPrefs.Save();
            Debug.Log("Mode layar diatur ke: Fullscreen");
        }
    }
}
