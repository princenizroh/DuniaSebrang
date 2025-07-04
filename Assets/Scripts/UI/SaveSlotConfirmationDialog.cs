using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace DS.UI
{
    /// <summary>
    /// Dialog for handling save slot conflicts and confirmations
    /// </summary>
    public class SaveSlotConfirmationDialog : MonoBehaviour
    {
        [Header("=== UI REFERENCES ===")]
        [SerializeField] private GameObject dialogPanel;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Button yesButton;
        [SerializeField] private Button noButton;
        
        [Header("=== SETTINGS ===")]
        [SerializeField] private bool showDebug = false; // Default false untuk production
        
        // Callbacks
        private Action onYesCallback;
        private Action onNoCallback;
        
        private void Awake()
        {
            // Setup button listeners
            if (yesButton != null)
                yesButton.onClick.AddListener(OnYesClicked);
            if (noButton != null)
                noButton.onClick.AddListener(OnNoClicked);
            
            // Hide dialog initially
            HideDialog();
        }
        
        /// <summary>
        /// Show dialog for new game on occupied slot
        /// </summary>
        public void ShowNewGameConflictDialog(int slotIndex, SaveSlotInfo existingData, 
            Action onContinue, Action onStartFresh, Action onCancel = null)
        {
            if (showDebug) Debug.Log($"★ ShowNewGameConflictDialog: Slot {slotIndex}");
            
            // Set callbacks - Yes = Start Fresh (overwrite), No = Cancel
            onYesCallback = onStartFresh;
            onNoCallback = onCancel ?? (() => HideDialog());
            
            // Set UI text
            if (titleText != null)
                titleText.text = $"Slot {slotIndex + 1} sudah terisi.\nTimpa data?";
            
            // Set button text
            if (yesButton != null && yesButton.GetComponentInChildren<TextMeshProUGUI>() != null)
                yesButton.GetComponentInChildren<TextMeshProUGUI>().text = "Ya";
            if (noButton != null && noButton.GetComponentInChildren<TextMeshProUGUI>() != null)
                noButton.GetComponentInChildren<TextMeshProUGUI>().text = "Tidak";
            
            ShowDialog();
        }
        
        /// <summary>
        /// Show dialog for continue on empty slot
        /// </summary>
        public void ShowEmptySlotDialog(int slotIndex, Action onCancel = null)
        {
            if (showDebug) Debug.Log($"★ ShowEmptySlotDialog: Slot {slotIndex}");
            
            // Set callbacks (only No is relevant for OK)
            onYesCallback = null;
            onNoCallback = onCancel ?? (() => HideDialog());
            
            // Set UI text - corrected message
            if (titleText != null)
                titleText.text = $"Slot {slotIndex + 1} kosong.\nTidak ada data untuk dilanjutkan.";
            
            // Hide Yes button for empty slot, show only No as "OK"
            if (yesButton != null)
                yesButton.gameObject.SetActive(false);
            
            // Show only No button as "OK"
            if (noButton != null)
            {
                noButton.gameObject.SetActive(true);
                if (noButton.GetComponentInChildren<TextMeshProUGUI>() != null)
                    noButton.GetComponentInChildren<TextMeshProUGUI>().text = "OK";
            }
            
            ShowDialog();
        }
        
        /// <summary>
        /// Show dialog for general errors
        /// </summary>
        public void ShowErrorDialog(string title, string message, Action onOK = null)
        {
            if (showDebug) Debug.Log($"★ ShowErrorDialog: {title}");
            
            // Set callbacks
            onYesCallback = null;
            onNoCallback = onOK ?? (() => HideDialog());
            
            // Set UI text
            if (titleText != null)
                titleText.text = title;
            
            // Hide Yes button, show only No as "OK"
            if (yesButton != null)
                yesButton.gameObject.SetActive(false);
            
            if (noButton != null)
            {
                noButton.gameObject.SetActive(true);
                if (noButton.GetComponentInChildren<TextMeshProUGUI>() != null)
                    noButton.GetComponentInChildren<TextMeshProUGUI>().text = "OK";
            }
            
            ShowDialog();
        }
        
        /// <summary>
        /// Show dialog for continue on occupied slot - Load or Delete choice
        /// </summary>
        public void ShowLoadOrDeleteDialog(int slotIndex, SaveSlotInfo existingData, 
            Action onLoad, Action onDelete, Action onCancel = null)
        {
            if (showDebug) Debug.Log($"★ ShowLoadOrDeleteDialog: Slot {slotIndex}");
            
            // Set callbacks - Yes = Load, No = Delete (we'll change button text)
            onYesCallback = onLoad;
            onNoCallback = onDelete;
            
            // Set UI text
            if (titleText != null)
                titleText.text = $"Slot {slotIndex + 1} berisi data.\nLoad atau hapus?";
            
            // Set button text for Load/Delete choice
            if (yesButton != null && yesButton.GetComponentInChildren<TextMeshProUGUI>() != null)
                yesButton.GetComponentInChildren<TextMeshProUGUI>().text = "Load";
            if (noButton != null && noButton.GetComponentInChildren<TextMeshProUGUI>() != null)
                noButton.GetComponentInChildren<TextMeshProUGUI>().text = "Hapus";
            
            ShowDialog();
        }
        
        private void ShowDialog()
        {
            if (dialogPanel != null)
            {
                dialogPanel.SetActive(true);
                
                // Reset button visibility (in case they were hidden)
                if (yesButton != null)
                    yesButton.gameObject.SetActive(true);
                if (noButton != null)
                    noButton.gameObject.SetActive(true);
            }
        }
        
        private void HideDialog()
        {
            if (dialogPanel != null)
                dialogPanel.SetActive(false);
            
            // Clear callbacks
            onYesCallback = null;
            onNoCallback = null;
        }
        
        /// <summary>
        /// Public method to close dialog and deactivate panel
        /// Call this when exiting save slot panel
        /// </summary>
        public void ForceCloseDialog()
        {
            if (showDebug) Debug.Log("★ ForceCloseDialog called");
            HideDialog();
        }
        
        private void OnYesClicked()
        {
            if (showDebug) Debug.Log("★ Dialog: Yes clicked");
            onYesCallback?.Invoke();
            HideDialog();
        }
        
        private void OnNoClicked()
        {
            if (showDebug) Debug.Log("★ Dialog: No/Cancel clicked");
            onNoCallback?.Invoke();
            HideDialog();
        }
    }
}
