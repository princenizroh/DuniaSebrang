// using UnityEngine;
// using UnityEngine.UI;
// using TMPro;
// using DS.Data.Save;

// namespace DS.UI
// {
//     /// <summary>
//     /// UI component for Load/Delete actions on filled save slots in Continue mode
//     /// </summary>
//     public class SaveSlotActions : MonoBehaviour
//     {
//         [Header("=== UI REFERENCES ===")]
//         [Tooltip("Actions panel")]
//         [SerializeField] private GameObject actionsPanel;
        
//         [Tooltip("Load button")]
//         [SerializeField] private Button loadButton;
        
//         [Tooltip("Delete button")]
//         [SerializeField] private Button deleteButton;
        
//         [Tooltip("Close/Back button")]
//         [SerializeField] private Button closeButton;
        
//         [Tooltip("Slot info text")]
//         [SerializeField] private TextMeshProUGUI slotInfoText;
        
//         [Header("=== REFERENCES ===")]
//         [Tooltip("Save manager")]
//         [SerializeField] private SaveManager saveManager;
        
//         [Tooltip("Confirmation dialog")]
//         [SerializeField] private ConfirmationDialog confirmationDialog;
        
//         [Header("=== SETTINGS ===")]
//         [Tooltip("Scene to load for continuing game")]
//         [SerializeField] private string gameScene = "GameScene";
        
//         [Header("=== DEBUG ===")]
//         [Tooltip("Show debug messages")]
//         [SerializeField] private bool showDebug = true;
        
//         // Events
//         public event System.Action<int> OnSlotLoaded;
//         public event System.Action<int> OnSlotDeleted;
//         public event System.Action OnActionsClosed;
        
//         // Current slot data
//         private int currentSlotIndex = -1;
//         private SaveData currentSlotData;
        
//         private void Awake()
//         {
//             // Auto-find references if not assigned
//             if (saveManager == null)
//                 saveManager = FindFirstObjectByType<SaveManager>();
                
//             if (confirmationDialog == null)
//                 confirmationDialog = FindFirstObjectByType<ConfirmationDialog>();
            
//             // Setup button events
//             if (loadButton != null)
//                 loadButton.onClick.AddListener(OnLoadButtonClicked);
                
//             if (deleteButton != null)
//                 deleteButton.onClick.AddListener(OnDeleteButtonClicked);
                
//             if (closeButton != null)
//                 closeButton.onClick.AddListener(OnCloseButtonClicked);
            
//             // Start hidden
//             Hide();
//         }
        
//         /// <summary>
//         /// Show actions for specific slot
//         /// </summary>
//         public void ShowActionsForSlot(int slotIndex, SaveData slotData)
//         {
//             currentSlotIndex = slotIndex;
//             currentSlotData = slotData;
            
//             // Update slot info display
//             UpdateSlotInfoDisplay();
            
//             // Show actions panel
//             if (actionsPanel != null)
//                 actionsPanel.SetActive(true);
                
//             if (showDebug) Debug.Log($"Showing actions for slot {slotIndex}");
//         }
        
//         /// <summary>
//         /// Hide actions panel
//         /// </summary>
//         public void Hide()
//         {
//             if (actionsPanel != null)
//                 actionsPanel.SetActive(false);
                
//             currentSlotIndex = -1;
//             currentSlotData = null;
            
//             if (showDebug) Debug.Log("Save slot actions hidden");
//         }
        
//         /// <summary>
//         /// Update slot info display
//         /// </summary>
//         private void UpdateSlotInfoDisplay()
//         {
//             if (slotInfoText == null || currentSlotData == null) return;
            
//             string areaName = GetAreaNameFromSaveData(currentSlotData);
//             string playTime = FormatPlayTime(currentSlotData.totalPlayTime);
            
//             slotInfoText.text = $"Slot {currentSlotIndex + 1}\n{areaName}\n{playTime}";
//         }
        
//         /// <summary>
//         /// Get area name from save data
//         /// </summary>
//         private string GetAreaNameFromSaveData(SaveData saveData)
//         {
//             if (!string.IsNullOrEmpty(saveData.checkpointData.lastCheckpointName))
//             {
//                 if (!string.IsNullOrEmpty(saveData.playerData.currentScene))
//                 {
//                     return saveData.playerData.currentScene.Replace("_", " ");
//                 }
//                 return saveData.checkpointData.lastCheckpointName;
//             }
            
//             return !string.IsNullOrEmpty(saveData.playerData.currentScene) 
//                 ? saveData.playerData.currentScene.Replace("_", " ") 
//                 : "Unknown Area";
//         }
        
//         /// <summary>
//         /// Format play time to HH:MM:SS format
//         /// </summary>
//         private string FormatPlayTime(float totalSeconds)
//         {
//             int hours = Mathf.FloorToInt(totalSeconds / 3600f);
//             int minutes = Mathf.FloorToInt((totalSeconds % 3600f) / 60f);
//             int seconds = Mathf.FloorToInt(totalSeconds % 60f);
            
//             return $"{hours:00}:{minutes:00}:{seconds:00}";
//         }
        
//         /// <summary>
//         /// Called when Load button is clicked
//         /// </summary>
//         private void OnLoadButtonClicked()
//         {
//             if (currentSlotIndex < 0 || saveManager == null)
//             {
//                 Debug.LogError("SaveSlotActions: Cannot load - invalid slot or no SaveManager!");
//                 return;
//             }
            
//             if (showDebug) Debug.Log($"★★★ LOADING GAME FROM SLOT {currentSlotIndex} ★★★");
            
//             try
//             {
//                 // Set the save slot in SaveManager
//                 var slotField = saveManager.GetType().GetField("currentSaveSlot", 
//                     System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
//                 if (slotField != null)
//                 {
//                     slotField.SetValue(saveManager, currentSlotIndex);
//                 }
                
//                 // Load from the slot
//                 var loadMethod = saveManager.GetType().GetMethod("LoadFromSlot");
//                 if (loadMethod != null)
//                 {
//                     bool loadSuccess = (bool)loadMethod.Invoke(saveManager, new object[] { currentSlotIndex });
                    
//                     if (loadSuccess)
//                     {
//                         // Notify about successful load
//                         OnSlotLoaded?.Invoke(currentSlotIndex);
                        
//                         // Load the game scene
//                         LoadGameScene();
//                     }
//                     else
//                     {
//                         Debug.LogError($"Failed to load save data from slot {currentSlotIndex}");
//                     }
//                 }
//             }
//             catch (System.Exception e)
//             {
//                 Debug.LogError($"Error loading game: {e.Message}");
//             }
//         }
        
//         /// <summary>
//         /// Called when Delete button is clicked
//         /// </summary>
//         private void OnDeleteButtonClicked()
//         {
//             if (currentSlotIndex < 0)
//             {
//                 Debug.LogError("SaveSlotActions: Cannot delete - invalid slot!");
//                 return;
//             }
            
//             if (confirmationDialog == null)
//             {
//                 Debug.LogError("SaveSlotActions: No ConfirmationDialog found!");
//                 return;
//             }
            
//             // Show delete confirmation
//             confirmationDialog.ShowDeleteConfirmation(currentSlotIndex,
//                 () => ConfirmDeleteSlot(),
//                 () => {
//                     if (showDebug) Debug.Log($"Delete cancelled for slot {currentSlotIndex}");
//                 });
//         }
        
//         /// <summary>
//         /// Confirm delete slot
//         /// </summary>
//         private void ConfirmDeleteSlot()
//         {
//             if (currentSlotIndex < 0 || saveManager == null) return;
            
//             if (showDebug) Debug.Log($"★★★ DELETING SAVE SLOT {currentSlotIndex} ★★★");
            
//             try
//             {
//                 // Delete the save file
//                 var deleteMethod = saveManager.GetType().GetMethod("DeleteSaveSlot");
//                 if (deleteMethod != null)
//                 {
//                     deleteMethod.Invoke(saveManager, new object[] { currentSlotIndex });
                    
//                     // Notify about deletion
//                     OnSlotDeleted?.Invoke(currentSlotIndex);
                    
//                     // Hide actions panel
//                     Hide();
                    
//                     if (showDebug) Debug.Log($"Save slot {currentSlotIndex} deleted successfully");
//                 }
//                 else
//                 {
//                     Debug.LogError("SaveManager.DeleteSaveSlot method not found!");
//                 }
//             }
//             catch (System.Exception e)
//             {
//                 Debug.LogError($"Error deleting save slot: {e.Message}");
//             }
//         }
        
//         /// <summary>
//         /// Called when Close button is clicked
//         /// </summary>
//         private void OnCloseButtonClicked()
//         {
//             Hide();
//             OnActionsClosed?.Invoke();
//         }
        
//         /// <summary>
//         /// Load the game scene
//         /// </summary>
//         private void LoadGameScene()
//         {
//             if (string.IsNullOrEmpty(gameScene))
//             {
//                 Debug.LogError("SaveSlotActions: No game scene specified!");
//                 return;
//             }
            
//             if (showDebug) Debug.Log($"Loading game scene: {gameScene}");
            
//             // Try to use loading screen for smooth transition
//             MonoBehaviour loadingScreen = null;
//             var allComponents = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            
//             foreach (var component in allComponents)
//             {
//                 if (component.GetType().Name == "LoadingScreen")
//                 {
//                     loadingScreen = component;
//                     break;
//                 }
//             }
            
//             if (loadingScreen != null)
//             {
//                 // Use loading screen with save slot context
//                 var loadSceneMethod = loadingScreen.GetType().GetMethod("LoadScene", new System.Type[] { typeof(string), typeof(string), typeof(string) });
//                 if (loadSceneMethod != null)
//                 {
//                     string areaName = GetAreaNameFromSaveData(currentSlotData);
//                     loadSceneMethod.Invoke(loadingScreen, new object[] { gameScene, $"Returning to {areaName}...", "Welcome back, adventurer!" });
//                     return;
//                 }
//             }
            
//             // Fallback: direct scene load
//             UnityEngine.SceneManagement.SceneManager.LoadScene(gameScene);
//         }
        
//         #if UNITY_EDITOR
//         /// <summary>
//         /// Test showing actions (Editor only)
//         /// </summary>
//         [ContextMenu("Test Show Actions")]
//         private void TestShowActions()
//         {
//             var testSaveData = new SaveData();
//             testSaveData.playerData.currentScene = "Prison_Level_01";
//             testSaveData.totalPlayTime = 1337f; // 00:22:17
//             testSaveData.checkpointData.lastCheckpointName = "Prison Entrance";
            
//             ShowActionsForSlot(1, testSaveData);
//         }
//         #endif
//     }
// }
