using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace DS.UI
{
    /// <summary>
    /// Generic confirmation dialog for save system actions
    /// </summary>
    public class ConfirmationDialog : MonoBehaviour
    {
        [Header("=== UI REFERENCES ===")]
        [Tooltip("Dialog panel")]
        [SerializeField] private GameObject dialogPanel;
        
        [Tooltip("Title text")]
        [SerializeField] private TextMeshProUGUI titleText;
        
        [Tooltip("Message text")]
        [SerializeField] private TextMeshProUGUI messageText;
        
        [Tooltip("Confirm button")]
        [SerializeField] private Button confirmButton;
        
        [Tooltip("Cancel button")]
        [SerializeField] private Button cancelButton;
        
        [Tooltip("Background blocker")]
        [SerializeField] private GameObject backgroundBlocker;
        
        [Header("=== SETTINGS ===")]
        [Tooltip("Close on background click")]
        [SerializeField] private bool closeOnBackgroundClick = true;
        
        [Header("=== DEBUG ===")]
        [Tooltip("Show debug messages")]
        [SerializeField] private bool showDebug = true;
        
        // Current dialog data
        private System.Action onConfirm;
        private System.Action onCancel;
        
        private void Awake()
        {
            // Validate UI components
            if (dialogPanel == null || titleText == null || messageText == null || confirmButton == null || cancelButton == null)
            {
                Debug.LogWarning("ConfirmationDialog: Some UI components are missing. Please assign them in the Inspector.");
                Debug.LogWarning("ConfirmationDialog will use fallback behavior until UI is properly set up.");
            }
            
            // Setup button events
            if (confirmButton != null)
                confirmButton.onClick.AddListener(OnConfirmClicked);
                
            if (cancelButton != null)
                cancelButton.onClick.AddListener(OnCancelClicked);
                
            // Setup background blocker
            if (backgroundBlocker != null && closeOnBackgroundClick)
            {
                Button backgroundButton = backgroundBlocker.GetComponent<Button>();
                if (backgroundButton == null)
                    backgroundButton = backgroundBlocker.AddComponent<Button>();
                    
                backgroundButton.onClick.AddListener(OnCancelClicked);
            }
            
            // Start hidden
            Hide();
        }
        
        /// <summary>
        /// Show confirmation dialog
        /// </summary>
        public void Show(string title, string message, System.Action onConfirm, System.Action onCancel = null)
        {
            // Check if UI components are available
            if (dialogPanel == null || titleText == null || messageText == null)
            {
                Debug.LogWarning("ConfirmationDialog: UI components not set up. Showing fallback debug confirmation.");
                Debug.LogWarning($"CONFIRMATION DIALOG: {title}");
                Debug.LogWarning($"MESSAGE: {message}");
                Debug.LogWarning("PROCEEDING WITH CONFIRM ACTION (no UI available)");
                
                // Execute confirm action as fallback
                onConfirm?.Invoke();
                return;
            }
            
            // Set text
            if (titleText != null)
                titleText.text = title;
                
            if (messageText != null)
                messageText.text = message;
            
            // Store callbacks
            this.onConfirm = onConfirm;
            this.onCancel = onCancel;
            
            // Show dialog
            if (dialogPanel != null)
                dialogPanel.SetActive(true);
                
            if (backgroundBlocker != null)
                backgroundBlocker.SetActive(true);
                
            if (showDebug) Debug.Log($"Confirmation dialog shown: {title}");
        }
        
        /// <summary>
        /// Hide dialog
        /// </summary>
        public void Hide()
        {
            if (dialogPanel != null)
                dialogPanel.SetActive(false);
                
            if (backgroundBlocker != null)
                backgroundBlocker.SetActive(false);
                
            // Clear callbacks
            onConfirm = null;
            onCancel = null;
            
            if (showDebug) Debug.Log("Confirmation dialog hidden");
        }
        
        /// <summary>
        /// Called when confirm button is clicked
        /// </summary>
        private void OnConfirmClicked()
        {
            if (showDebug) Debug.Log("Confirmation dialog: CONFIRMED");
            
            // Execute callback
            onConfirm?.Invoke();
            
            // Hide dialog
            Hide();
        }
        
        /// <summary>
        /// Called when cancel button is clicked
        /// </summary>
        private void OnCancelClicked()
        {
            if (showDebug) Debug.Log("Confirmation dialog: CANCELLED");
            
            // Execute callback
            onCancel?.Invoke();
            
            // Hide dialog
            Hide();
        }
        
        /// <summary>
        /// Show overwrite save confirmation
        /// </summary>
        public void ShowOverwriteConfirmation(int slotIndex, System.Action onConfirm, System.Action onCancel = null)
        {
            string title = "Overwrite Save";
            string message = $"Save slot {slotIndex + 1} already contains saved data.\n\nDo you want to overwrite it?";
            
            Show(title, message, onConfirm, onCancel);
        }
        
        /// <summary>
        /// Show delete save confirmation
        /// </summary>
        public void ShowDeleteConfirmation(int slotIndex, System.Action onConfirm, System.Action onCancel = null)
        {
            string title = "Delete Save";
            string message = $"Are you sure you want to delete the saved data in slot {slotIndex + 1}?\n\nThis action cannot be undone.";
            
            Show(title, message, onConfirm, onCancel);
        }
        
        #if UNITY_EDITOR
        /// <summary>
        /// Test overwrite dialog (Editor only)
        /// </summary>
        [ContextMenu("Test Overwrite Dialog")]
        private void TestOverwriteDialog()
        {
            ShowOverwriteConfirmation(0, 
                () => Debug.Log("TEST: Overwrite confirmed"),
                () => Debug.Log("TEST: Overwrite cancelled"));
        }
        
        /// <summary>
        /// Test delete dialog (Editor only)
        /// </summary>
        [ContextMenu("Test Delete Dialog")]
        private void TestDeleteDialog()
        {
            ShowDeleteConfirmation(2,
                () => Debug.Log("TEST: Delete confirmed"),
                () => Debug.Log("TEST: Delete cancelled"));
        }
        #endif
    }
}
