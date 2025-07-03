using UnityEngine;
using UnityEngine.UI;

namespace Lilu
{
    public class ScreenRes : MonoBehaviour
    {
        [Header("Tombol UI")]
        public Button windowedButton;
        public Button fullscreenButton;

        private const string ScreenModeKey = "ScreenMode"; // 0 = Windowed, 1 = Fullscreen

        private void Start()
        {
            // Tambahkan event klik
            windowedButton.onClick.AddListener(SetWindowedMode);
            fullscreenButton.onClick.AddListener(SetFullscreenMode);

            // Terapkan mode layar terakhir jika ada
            if (PlayerPrefs.HasKey(ScreenModeKey))
            {
                int savedMode = PlayerPrefs.GetInt(ScreenModeKey);
                if (savedMode == 1)
                {
                    SetFullscreenMode();
                }
                else
                {
                    SetWindowedMode();
                }
            }
            else
            {
                Debug.Log("Tidak ada preferensi mode layar tersimpan. Menggunakan default.");
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
            Screen.fullScreenMode = FullScreenMode.FullScreenWindow; // atau FullScreenMode.ExclusiveFullScreen
            PlayerPrefs.SetInt(ScreenModeKey, 1);
            PlayerPrefs.Save();
            Debug.Log("Mode layar diatur ke: Fullscreen");
        }
    }
}
