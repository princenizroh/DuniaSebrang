using UnityEngine;
using UnityEngine.UI;

namespace DS
{
    public class ResolutionButtonControl : MonoBehaviour
    {
        public static ResolutionButtonControl Instance { get; private set; }

        [Header("Buttons")]
        public Button button720p;
        public Button button1360;
        public Button button1080p;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject); // Hapus duplikat saat pindah scene
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            SetupButtonListeners();

            // Load resolusi terakhir jika ada
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

        private void SetupButtonListeners()
        {
            if (button720p != null)
                button720p.onClick.AddListener(() => SetResolution(1280, 720));

            if (button1360 != null)
                button1360.onClick.AddListener(() => SetResolution(1366, 768));

            if (button1080p != null)
                button1080p.onClick.AddListener(() => SetResolution(1920, 1080));
        }

        public void SetResolution(int width, int height)
        {
            Debug.Log($"Setting resolution to: {width}x{height}");
            Screen.SetResolution(width, height, FullScreenMode.FullScreenWindow); // Ganti jika ingin windowed

            PlayerPrefs.SetInt("SavedWidth", width);
            PlayerPrefs.SetInt("SavedHeight", height);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Jika scene baru memiliki button baru, sambungkan ulang di sini.
        /// </summary>
        public void RebindButtons(Button btn720, Button btn1360, Button btn1080)
        {
            if (button720p != null)
                button720p.onClick.RemoveAllListeners();
            if (button1360 != null)
                button1360.onClick.RemoveAllListeners();
            if (button1080p != null)
                button1080p.onClick.RemoveAllListeners();

            button720p = btn720;
            button1360 = btn1360;
            button1080p = btn1080;

            SetupButtonListeners();
        }
    }
}
