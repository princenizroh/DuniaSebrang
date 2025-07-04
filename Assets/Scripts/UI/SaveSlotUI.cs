using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using DS.Data.Save;

namespace DS.UI
{
    /// <summary>
    /// UI component for displaying individual save slot information
    /// </summary>
    public class SaveSlotUI : MonoBehaviour
    {
        [Header("=== UI REFERENCES ===")]
        [Tooltip("Button component for slot interaction")]
        [SerializeField] private Button slotButton;
        
        [Tooltip("Text display for slot info (area + time)")]
        [SerializeField] private TextMeshProUGUI slotInfoText;
        
        [Header("=== SLOT SETTINGS ===")]
        [Tooltip("Slot index (0-4)")]
        [SerializeField] private int slotIndex;
        
        [Tooltip("Empty slot text")]
        [SerializeField] private string emptySlotText = "Empty";
        
        [Header("=== DISPLAY OPTIONS ===")]
        [Tooltip("Show last save date/time in slot info")]
        [SerializeField] private bool showLastSaveDate = true;
        
        [Tooltip("Format for displaying save date (short format)")]
        [SerializeField] private string dateFormat = "MM/dd HH:mm";

        [Header("=== DEBUG ===")]
        [Tooltip("Show debug messages")]
        [SerializeField] private bool showDebug = false; // Default false untuk production
        
        // Events
        public event System.Action<int> OnSlotClicked;
        
        // Properties
        public int SlotIndex => slotIndex;
        public bool IsEmpty { get; private set; } = true;
        public SaveData SlotSaveData { get; private set; }
        
        private void Awake()
        {
            // Auto-assign components if not set
            if (slotButton == null)
                slotButton = GetComponent<Button>();
                
            // Setup button click
            if (slotButton != null)
                slotButton.onClick.AddListener(OnSlotButtonClicked);
        }
        
        private void Start()
        {
            // Initialize as empty slot
            SetAsEmptySlot();
        }
        
        /// <summary>
        /// Set this slot as empty
        /// </summary>
        public void SetAsEmptySlot()
        {
            IsEmpty = true;
            SlotSaveData = null;
            
            // Set empty text
            if (slotInfoText != null)
                slotInfoText.text = emptySlotText;
                
            if (showDebug) Debug.Log($"Save slot {slotIndex} set as empty");
        }
        
        /// <summary>
        /// Set this slot with save data
        /// </summary>
        public void SetSaveData(SaveData saveData)
        {
            if (saveData == null)
            {
                SetAsEmptySlot();
                return;
            }
            
            IsEmpty = false;
            SlotSaveData = saveData;
            
            // Update slot info (area + time in one text)
            string areaName = GetAreaNameFromSaveData(saveData);
            string playTimeString = FormatPlayTime(saveData.totalPlayTime);
            
            // For SaveData, we can't show CheckpointData's lastSaveDateTime, so show save time
            string lastSaveDateString = "";
            if (showLastSaveDate)
            {
                lastSaveDateString = $"\n{saveData.saveTime.ToString(dateFormat)}";
            }
            
            if (slotInfoText != null)
                slotInfoText.text = $"{areaName}\n{playTimeString}{lastSaveDateString}";
                
            if (showDebug) Debug.Log($"Save slot {slotIndex} updated with area: {areaName}, time: {playTimeString}");
        }
        
        /// <summary>
        /// Set this slot with enhanced save slot info
        /// </summary>
        public void SetSaveSlotInfo(SaveSlotInfo slotInfo)
        {
            if (showDebug) 
            {
                Debug.Log($"★ SaveSlotUI[{slotIndex}].SetSaveSlotInfo called:");
                Debug.Log($"  isEmpty: {slotInfo.isEmpty}, areaName: '{slotInfo.areaName}'");
            }
            
            if (slotInfo.isEmpty)
            {
                SetAsEmptySlot();
                return;
            }

            IsEmpty = false;
            // Note: SlotSaveData might be null when using SaveSlotInfo, but that's okay

            // Decide which play time to show: lastSavePlayTime from CheckpointData or current totalPlayTime
            float displayPlayTime = slotInfo.lastSavePlayTime > 0 ? slotInfo.lastSavePlayTime : slotInfo.playTime;
            string playTimeString = FormatPlayTime(displayPlayTime);
            string lastSaveDateString = FormatLastSaveDate(slotInfo);

            string finalText = $"{slotInfo.areaName}\n{playTimeString}{lastSaveDateString}";
            
            if (slotInfoText != null)
            {
                slotInfoText.text = finalText;
                
                // Force UI refresh
                slotInfoText.SetAllDirty();
                slotInfoText.ForceMeshUpdate();
                
                if (showDebug) Debug.Log($"★ Slot {slotIndex} text updated: '{finalText}'");
            }
            else
            {
                Debug.LogError($"★ Slot {slotIndex} slotInfoText is NULL!");
            }

            if (showDebug) 
            {
                Debug.Log($"Save slot {slotIndex} updated: {slotInfo.areaName}, playtime: {playTimeString}");
            }
        }
        
        /// <summary>
        /// Get area name from save data
        /// </summary>
        private string GetAreaNameFromSaveData(SaveData saveData)
        {
            // Prioritas 1: Gunakan areaName yang tersimpan di checkpoint data (dari CheckpointData.areaName)
            if (!string.IsNullOrEmpty(saveData.checkpointData.lastCheckpointName))
            {
                // Cek apakah ini area name (bukan checkpoint name biasa)
                string checkpointName = saveData.checkpointData.lastCheckpointName;
                
                // Jika mengandung "Area" atau "Level", kemungkinan ini checkpoint name, bukan area name
                if (!checkpointName.Contains("Area") && !checkpointName.Contains("Level") && !checkpointName.Contains("SS"))
                {
                    return checkpointName; // Ini kemungkinan area name seperti "Parking", "Forest", dll
                }
            }
            
            // Prioritas 2: Gunakan scene name yang di-format
            if (!string.IsNullOrEmpty(saveData.playerData.currentScene))
            {
                string sceneName = saveData.playerData.currentScene.Replace("_", " ").Replace("-", " ");
                return sceneName;
            }
            
            // Prioritas 3: Fallback ke checkpoint name
            if (!string.IsNullOrEmpty(saveData.checkpointData.lastCheckpointName))
            {
                return saveData.checkpointData.lastCheckpointName;
            }
            
            // Fallback terakhir
            return "Unknown Area";
        }
        
        /// <summary>
        /// Format play time to HH:MM:SS format
        /// </summary>
        private string FormatPlayTime(float totalSeconds)
        {
            int hours = Mathf.FloorToInt(totalSeconds / 3600f);
            int minutes = Mathf.FloorToInt((totalSeconds % 3600f) / 60f);
            int seconds = Mathf.FloorToInt(totalSeconds % 60f);
            
            return $"{hours:00}:{minutes:00}:{seconds:00}";
        }

        /// <summary>
        /// Format last save date/time to string from SaveSlotInfo
        /// </summary>
        private string FormatLastSaveDate(SaveSlotInfo slotInfo)
        {
            if (!showLastSaveDate || string.IsNullOrEmpty(slotInfo.lastSaveDateTime))
                return string.Empty;
            
            try
            {
                // Parse the datetime string and format it
                if (System.DateTime.TryParse(slotInfo.lastSaveDateTime, out System.DateTime saveDateTime))
                {
                    return $"\n{saveDateTime.ToString(dateFormat)}";
                }
            }
            catch (System.Exception e)
            {
                if (showDebug) Debug.LogWarning($"Error parsing save date '{slotInfo.lastSaveDateTime}': {e.Message}");
            }
            
            return string.Empty;
        }
        
        /// <summary>
        /// Called when slot button is clicked
        /// </summary>
        private void OnSlotButtonClicked()
        {
            if (showDebug) Debug.Log($"Save slot {slotIndex} clicked - IsEmpty: {IsEmpty}");
            
            OnSlotClicked?.Invoke(slotIndex);
        }
        
        /// <summary>
        /// Set slot index programmatically
        /// </summary>
        public void SetSlotIndex(int index)
        {
            slotIndex = index;
            if (showDebug) Debug.Log($"Save slot index set to: {index}");
        }
        
        /// <summary>
        /// Enable/disable slot interaction
        /// </summary>
        public void SetInteractable(bool interactable)
        {
            if (slotButton != null)
                slotButton.interactable = interactable;
        }
        
        /// <summary>
        /// Force refresh this slot's UI (public method for debugging)
        /// </summary>
        [ContextMenu("Force Refresh This Slot")]
        public void ForceRefreshThisSlot()
        {
            Debug.Log($"★ FORCE REFRESH SLOT {slotIndex} ★");
            
            if (slotInfoText != null)
            {
                string currentText = slotInfoText.text;
                Debug.Log($"★ Current text: '{currentText}'");
                
                // Force multiple refresh methods
                slotInfoText.text = "";
                slotInfoText.text = currentText;
                slotInfoText.SetAllDirty();
                slotInfoText.ForceMeshUpdate();
                
                Debug.Log($"★ After refresh: '{slotInfoText.text}'");
            }
        }

        #if UNITY_EDITOR
        /// <summary>
        /// Test method to simulate save data (Editor only)
        /// </summary>
        [ContextMenu("Test Fill Slot")]
        private void TestFillSlot()
        {
            var testSaveData = new SaveData();
            testSaveData.playerData.currentScene = "Prison_Level_01";
            testSaveData.totalPlayTime = 937f; // 00:15:37
            testSaveData.checkpointData.lastCheckpointName = "Prison Entrance";
            testSaveData.saveTime = System.DateTime.Now;
            
            SetSaveData(testSaveData);
        }
        
        /// <summary>
        /// Test method to set as empty (Editor only)
        /// </summary>
        [ContextMenu("Test Empty Slot")]
        private void TestEmptySlot()
        {
            SetAsEmptySlot();
        }
        #endif
    }
}
