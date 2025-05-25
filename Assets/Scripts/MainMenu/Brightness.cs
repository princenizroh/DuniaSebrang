using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;
using TMPro;

namespace DS
{
    public class Brightness : MonoBehaviour
    {
        public Slider brightnessSlider;
        public PostProcessProfile brightnessProfile;
        public TextMeshProUGUI brightnessText; // Reference to the TextMeshProUGUI element

        private AutoExposure exposure;

        private const string brightnessPrefsKey = "Brightness";

        // Start is called before the first frame update
        void Start()
        {
            if (brightnessProfile.TryGetSettings(out exposure))
            {
                // Try to get the brightness value from PlayerPrefs
                if (PlayerPrefs.HasKey(brightnessPrefsKey))
                {
                    float savedBrightness = PlayerPrefs.GetFloat(brightnessPrefsKey);
                    brightnessSlider.value = savedBrightness;
                    AdjustBrightness(savedBrightness);
                }
                else
                {
                    float defaultBrightness = brightnessSlider.value;
                    PlayerPrefs.SetFloat(brightnessPrefsKey, defaultBrightness);
                    AdjustBrightness(defaultBrightness);
                }

                brightnessSlider.onValueChanged.AddListener(AdjustBrightness);
            }
            else
            {
                Debug.LogError("AutoExposure not found in the PostProcessProfile.");
            }

            UpdateBrightnessText(brightnessSlider.value); // Initial update of the brightness text
        }

        // Update is called once per frame
        public void AdjustBrightness(float value)
        {
            if (exposure != null)
            {
                exposure.keyValue.value = value;
                PlayerPrefs.SetFloat(brightnessPrefsKey, value); // Save the value to PlayerPrefs
                UpdateBrightnessText(value); // Update the brightness text
            }
        }

        private void UpdateBrightnessText(float value)
        {
            int percentage = Mathf.RoundToInt(value * 10); // Convert the slider value to a percentage
            brightnessText.text = percentage.ToString() + "%"; // Update the text
        }
    }
}
