using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DS
{
    public class Brightness : MonoBehaviour
    {
        public static Brightness Instance { get; private set; }

        [Header("UI Elements")]
        public Slider brightnessSlider;

        [Header("Volume Settings (URP)")]
        public Volume volume;

        private ColorAdjustments colorAdjustments;
        private const string BrightnessPrefsKey = "Brightness";

        private void Awake()
        {
            // Singleton & DontDestroy
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject); // mencegah duplikat saat pindah scene
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (volume == null)
            {
                Debug.LogError("Volume belum di-assign.");
                return;
            }

            // Ambil ColorAdjustments dari Volume
            if (volume.profile.TryGet(out colorAdjustments))
            {
                float savedValue = PlayerPrefs.GetFloat(BrightnessPrefsKey, 1f);
                if (brightnessSlider != null)
                {
                    brightnessSlider.value = savedValue;
                    brightnessSlider.onValueChanged.AddListener(AdjustBrightness);
                }

                AdjustBrightness(savedValue);
            }
            else
            {
                Debug.LogError("ColorAdjustments tidak ditemukan di Volume Profile.");
            }
        }

        public void AdjustBrightness(float value)
        {
            if (colorAdjustments != null)
            {
                colorAdjustments.postExposure.value = value;
                PlayerPrefs.SetFloat(BrightnessPrefsKey, value);
            }
        }
    }
}
