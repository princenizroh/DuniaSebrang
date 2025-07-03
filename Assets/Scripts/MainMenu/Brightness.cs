using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DS
{
    public class Brightness : MonoBehaviour
    {
        [Header("UI Elements")]
        public Slider brightnessSlider;

        [Header("Volume Settings (URP)")]
        public Volume volume;

        private ColorAdjustments colorAdjustments;
        private const string BrightnessPrefsKey = "Brightness";

        private void Start()
        {
            // Ambil ColorAdjustments dari Volume
            if (volume.profile.TryGet<ColorAdjustments>(out colorAdjustments))
            {
                float savedValue = PlayerPrefs.GetFloat(BrightnessPrefsKey, 1f); // default: 0 exposure
                brightnessSlider.value = savedValue;
                AdjustBrightness(savedValue);

                brightnessSlider.onValueChanged.AddListener(AdjustBrightness);
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
