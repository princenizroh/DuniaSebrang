using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DS.UI
{
    /// <summary>
    /// Simple confirmation dialog using Unity's built-in EditorUtility (Editor only)
    /// and fallback for runtime
    /// </summary>
    public class SimpleConfirmationDialog : MonoBehaviour
    {
        [Header("=== SETTINGS ===")]
        [SerializeField] private bool showDebug = true;
        
        /// <summary>
        /// Show dialog for new game on occupied slot
        /// </summary>
        public void ShowNewGameConflictDialog(int slotIndex, SaveSlotInfo existingData, 
            System.Action onContinue, System.Action onStartFresh, System.Action onCancel = null)
        {
            if (showDebug) Debug.Log($"★ ShowNewGameConflictDialog: Slot {slotIndex}");
            
            string playTimeStr = FormatPlayTime(existingData.playTime);
            string title = $"Slot {slotIndex + 1} Sudah Berisi Data";
            string message = $"Slot ini berisi:\n\n" +
                           $"Area: {existingData.areaName}\n" +
                           $"Waktu Main: {playTimeStr}\n" +
                           $"Terakhir Disimpan: {existingData.saveDateTime}\n\n" +
                           $"Apa yang ingin Anda lakukan?";
            
#if UNITY_EDITOR
            // In Editor: Use EditorUtility for quick testing
            int choice = EditorUtility.DisplayDialogComplex(
                title,
                message,
                "Lanjutkan Game yang Ada",  // option 0
                "Batal",                    // option 1  
                "Mulai Baru (Hapus Data)"   // option 2
            );
            
            switch (choice)
            {
                case 0: // Continue
                    if (showDebug) Debug.Log("★ User chose: Continue Existing Game");
                    onContinue?.Invoke();
                    break;
                case 1: // Cancel
                    if (showDebug) Debug.Log("★ User chose: Cancel");
                    onCancel?.Invoke();
                    break;
                case 2: // Start Fresh
                    if (showDebug) Debug.Log("★ User chose: Start Fresh");
                    onStartFresh?.Invoke();
                    break;
            }
#else
            // In Build: Show simple choice dialog
            Debug.Log($"★ RUNTIME DIALOG: {title}");
            Debug.Log($"★ {message}");
            Debug.Log($"★ Options: [1] Continue Existing [2] Start Fresh [3] Cancel");
            Debug.LogWarning($"★ FALLBACK: Defaulting to CONTINUE EXISTING for safety");
            
            // Default to continue existing for safety
            onContinue?.Invoke();
#endif
        }
        
        /// <summary>
        /// Show dialog for continue on empty slot
        /// </summary>
        public void ShowEmptySlotDialog(int slotIndex, System.Action onCancel = null)
        {
            if (showDebug) Debug.Log($"★ ShowEmptySlotDialog: Slot {slotIndex}");
            
            string title = $"Slot {slotIndex + 1} Kosong";
            string message = $"Slot ini tidak berisi data game.\n\n" +
                           $"Coba pilih 'Mulai Permainan' untuk memulai game baru,\n" +
                           $"atau pilih slot lain yang berisi save data.";
            
#if UNITY_EDITOR
            // In Editor: Simple OK dialog
            EditorUtility.DisplayDialog(title, message, "OK");
            onCancel?.Invoke();
#else
            // In Build: Log message
            Debug.LogWarning($"★ {title}: {message}");
            onCancel?.Invoke();
#endif
        }
        
        /// <summary>
        /// Show error dialog for general errors
        /// </summary>
        public void ShowErrorDialog(string title, string message, System.Action onOK = null)
        {
            if (showDebug) Debug.Log($"★ ShowErrorDialog: {title}");
            
#if UNITY_EDITOR
            EditorUtility.DisplayDialog(title, message, "OK");
            onOK?.Invoke();
#else
            Debug.LogError($"★ ERROR: {title} - {message}");
            onOK?.Invoke();
#endif
        }
        
        private string FormatPlayTime(float totalSeconds)
        {
            if (totalSeconds <= 0) return "0m";
            
            int hours = Mathf.FloorToInt(totalSeconds / 3600f);
            int minutes = Mathf.FloorToInt((totalSeconds % 3600f) / 60f);
            
            if (hours > 0)
                return $"{hours}h {minutes}m";
            else
                return $"{minutes}m";
        }
    }
}
