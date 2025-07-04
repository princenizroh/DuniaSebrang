using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using DS.Data.Save;

namespace DS.UI
{
    /// <summary>
    /// Data structure for save slot information
    /// </summary>
    [System.Serializable]
    public struct SaveSlotInfo
    {
        public int slotIndex;
        public bool isEmpty;
        public string areaName;
        public float playTime;
        public string saveDateTime;
        public float lastSavePlayTime; // From CheckpointData
        public string lastSaveDateTime; // From CheckpointData
    }

    /// <summary>
    /// Manager for save slot selection UI - handles New Game flow
    /// </summary>
    public class SaveSlotManager : MonoBehaviour
    {
        [Header("=== UI REFERENCES ===")]
        [Tooltip("Array of save slot UI components")]
        [SerializeField] private SaveSlotUI[] saveSlots;
        
        [Tooltip("Save manager reference")]
        [SerializeField] private SaveManager saveManager;
        
        [Header("=== NEW GAME SETTINGS ===")]
        [Tooltip("Scene to load for new game")]
        [SerializeField] private string newGameScene = "GameScene";
        
        [Header("=== DEBUG ===")]
        [Tooltip("Show debug messages")]
        [SerializeField] private bool showDebug = true;
        
        [Header("=== QUICK FIX CONTROLS ===")]
        [Tooltip("Key to force refresh all slots")]
        [SerializeField] private KeyCode forceRefreshKey = KeyCode.F5;
        
        [Tooltip("Key to debug save files")]
        [SerializeField] private KeyCode debugSaveFilesKey = KeyCode.F12;
        
        // Events
        public event System.Action<int> OnNewGameSlotSelected;
        public event System.Action<int> OnLoadGameSlotSelected;
        
        // Current mode
        public enum SlotSelectionMode
        {
            NewGame,    // For "Mulai Permainan"
            Continue    // For "Lanjutkan" 
        }
        
        private SlotSelectionMode currentMode = SlotSelectionMode.NewGame;
        
        private void Awake()
        {        // Auto-find SaveManager if not assigned
        if (saveManager == null)
            saveManager = FindFirstObjectByType<SaveManager>();
            
            // Initialize save slots
            InitializeSaveSlots();
        }
        
        private void Start()
        {
            // Delay initial refresh to ensure everything is loaded
            StartCoroutine(DelayedInitialRefresh());
        }
        
        /// <summary>
        /// Delayed initial refresh to ensure all components are ready
        /// </summary>
        private System.Collections.IEnumerator DelayedInitialRefresh()
        {
            // Wait for SaveManager to initialize
            yield return new UnityEngine.WaitForSeconds(0.5f);
            
            Debug.Log("★ INITIAL DELAYED REFRESH STARTING ★");
            
            // Force refresh multiple times if needed
            RefreshSaveSlots();
            
            yield return new UnityEngine.WaitForSeconds(0.2f);
            RefreshSaveSlots();
            
            yield return new UnityEngine.WaitForSeconds(0.3f);
            RefreshSaveSlots();
            
            Debug.Log("★ INITIAL DELAYED REFRESH COMPLETE ★");
        }
        
        /// <summary>
        /// Initialize save slot UI components
        /// </summary>
        private void InitializeSaveSlots()
        {
            for (int i = 0; i < saveSlots.Length; i++)
            {
                if (saveSlots[i] != null)
                {
                    // Set slot index
                    saveSlots[i].SetSlotIndex(i);
                    
                    // Subscribe to slot click events
                    int slotIndex = i; // Capture for closure
                    saveSlots[i].OnSlotClicked += (index) => OnSaveSlotClicked(index);
                }
            }
            
            if (showDebug) Debug.Log($"Initialized {saveSlots.Length} save slots");
        }
          /// <summary>
        /// Refresh all save slot displays with current save data
        /// </summary>
        public void RefreshSaveSlots()
        {
            Debug.Log("★★★ REFRESHING SAVE SLOTS ★★★");
            
            if (saveManager == null)
            {
                Debug.LogError("SaveSlotManager: No SaveManager found!");
                return;
            }

            for (int i = 0; i < saveSlots.Length; i++)
            {
                if (saveSlots[i] != null)
                {
                    // Get enhanced save slot info (includes CheckpointData info)
                    SaveSlotInfo slotInfo = GetEnhancedSaveSlotInfo(i);
                    
                    Debug.Log($"★ Slot {i}: {(slotInfo.isEmpty ? "EMPTY" : "HAS DATA")}");
                    
                    // Use the new enhanced method
                    saveSlots[i].SetSaveSlotInfo(slotInfo);
                    
                    if (!slotInfo.isEmpty)
                    {
                        Debug.Log($"★ Slot {i} DATA: Area='{slotInfo.areaName}', PlayTime={slotInfo.playTime}s, LastSave='{slotInfo.lastSaveDateTime}'");
                    }
                    
                    // Verify the UI state matches our expectation
                    Debug.Log($"★ Slot {i} UI State: IsEmpty={saveSlots[i].IsEmpty}");
                }
                else
                {
                    Debug.LogWarning($"★ Slot {i} is NULL!");
                }
            }
            
            Debug.Log("★★★ Save slots refresh complete ★★★");
        }
        
        /// <summary>
        /// Load save data for specific slot
        /// </summary>
        private SaveData LoadSaveDataForSlot(int slotIndex)
        {
            try
            {
                // USE SAME PATH AS SAVEMANAGER - VERY IMPORTANT!
                string saveDirectory = saveManager != null ? saveManager.SaveDirectory : Path.Combine(Application.persistentDataPath, "Saves");
                string fileName = $"DuniaSebrang_Save_Slot{slotIndex:00}.json";
                string filePath = Path.Combine(saveDirectory, fileName);
                
                Debug.Log($"★ Checking save file: {filePath}");
                Debug.Log($"★ Save directory exists: {Directory.Exists(saveDirectory)}");
                Debug.Log($"★ SaveManager.SaveDirectory: {(saveManager != null ? saveManager.SaveDirectory : "NULL")}");
                
                if (Directory.Exists(saveDirectory))
                {
                    Debug.Log($"★ Files in save directory:");
                    string[] files = Directory.GetFiles(saveDirectory, "*.json");
                    foreach (string file in files)
                    {
                        Debug.Log($"  - {Path.GetFileName(file)}");
                    }
                    
                    Debug.Log($"★ Total JSON files found: {files.Length}");
                }
                else
                {
                    Debug.LogError($"★ Save directory does not exist: {saveDirectory}");
                }
                
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    SaveData saveData = JsonUtility.FromJson<SaveData>(json);
                    
                    Debug.Log($"★ Found save data for slot {slotIndex}: Scene={saveData.playerData.currentScene}, Time={saveData.totalPlayTime}");
                    return saveData;
                }
                else
                {
                    Debug.Log($"★ No save file found for slot {slotIndex} at: {filePath}");
                    return null;
                }
            }
            catch (System.Exception e)
            {
                Debug.Log($"★ Error loading slot {slotIndex}: {e.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Set the current selection mode
        /// </summary>
        public void SetMode(SlotSelectionMode mode)
        {
            currentMode = mode;
            if (showDebug) Debug.Log($"Save slot mode set to: {mode}");
        }
        
        /// <summary>
        /// Called when a save slot is clicked
        /// </summary>
        private void OnSaveSlotClicked(int slotIndex)
        {
            if (showDebug) Debug.Log($"Save slot {slotIndex} clicked in {currentMode} mode");
            
            switch (currentMode)
            {
                case SlotSelectionMode.NewGame:
                    HandleNewGameSlotClick(slotIndex);
                    break;
                    
                case SlotSelectionMode.Continue:
                    HandleContinueSlotClick(slotIndex);
                    break;
            }
        }
        
        /// <summary>
        /// Handle slot click in New Game mode
        /// </summary>
        private void HandleNewGameSlotClick(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= saveSlots.Length) 
            {
                Debug.LogError($"★ Invalid slot index: {slotIndex} (valid range: 0-{saveSlots.Length-1})");
                return;
            }
            
            SaveSlotUI clickedSlot = saveSlots[slotIndex];
            
            Debug.Log($"★ HandleNewGameSlotClick - Slot {slotIndex}: IsEmpty={clickedSlot.IsEmpty}");
            
            // Double-check by loading data directly
            SaveData slotData = LoadSaveDataForSlot(slotIndex);
            bool actuallyEmpty = (slotData == null);
            Debug.Log($"★ Slot {slotIndex} data check: HasFile={!actuallyEmpty}");
            
            if (clickedSlot.IsEmpty && actuallyEmpty)
            {
                Debug.Log($"★ Slot {slotIndex} is empty - starting new game");
                // Empty slot - start new game immediately
                StartNewGameInSlot(slotIndex);
            }
            else if (!clickedSlot.IsEmpty || !actuallyEmpty)
            {
                Debug.Log($"★ Slot {slotIndex} has data - showing overwrite confirmation");
                // Filled slot - show override confirmation
                ShowOverwriteConfirmation(slotIndex);
            }
            else
            {
                Debug.LogWarning($"★ Slot {slotIndex} state mismatch: UI says empty={clickedSlot.IsEmpty}, but file exists={!actuallyEmpty}");
                // Refresh the slot and try again
                RefreshSlot(slotIndex);
            }
        }
        
        /// <summary>
        /// Handle slot click in Continue mode
        /// </summary>
        private void HandleContinueSlotClick(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= saveSlots.Length) return;
            
            SaveSlotUI clickedSlot = saveSlots[slotIndex];
            
            if (!clickedSlot.IsEmpty)
            {
                // Filled slot - show load/delete options
                ShowSaveSlotActions(slotIndex, clickedSlot.SlotSaveData);
            }
            else
            {
                // Empty slot - nothing to continue from
                if (showDebug) Debug.Log($"Slot {slotIndex} is empty - cannot continue");
            }
        }
        
        /// <summary>
        /// Show overwrite confirmation dialog
        /// </summary>
        private void ShowOverwriteConfirmation(int slotIndex)
        {
            Debug.Log($"★ SHOWING OVERWRITE CONFIRMATION FOR SLOT {slotIndex} ★");
            Debug.LogWarning($"★ SLOT {slotIndex} HAS DATA - PROCEEDING WITH OVERWRITE ★");
            
            // Just proceed with overwrite - no dialog needed for now
            StartNewGameInSlot(slotIndex);
        }
        
        /// <summary>
        /// Show save slot actions (Load/Delete)
        /// </summary>
        private void ShowSaveSlotActions(int slotIndex, SaveData slotData)
        {
            if (showDebug) Debug.Log($"Showing save slot actions for slot {slotIndex}");
            
            // Try to find save slot actions at runtime
            MonoBehaviour saveSlotActions = null;
            var allComponents = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            
            foreach (var component in allComponents)
            {
                if (component.GetType().Name == "SaveSlotActions")
                {
                    saveSlotActions = component;
                    break;
                }
            }
            
            if (saveSlotActions != null)
            {
                // Use reflection to call ShowActionsForSlot
                var showMethod = saveSlotActions.GetType().GetMethod("ShowActionsForSlot");
                if (showMethod != null)
                {
                    showMethod.Invoke(saveSlotActions, new object[] { slotIndex, slotData });
                }
            }
            else
            {
                // Fallback: load directly
                Debug.LogWarning("No SaveSlotActions found! Loading game directly.");
                LoadGameFromSlot(slotIndex);
            }
        }
        
        /// <summary>
        /// Handle slot loaded event
        /// </summary>
        private void OnSlotLoadedHandler(int slotIndex)
        {
            if (showDebug) Debug.Log($"Slot {slotIndex} loaded successfully");
            OnLoadGameSlotSelected?.Invoke(slotIndex);
        }
        
        /// <summary>
        /// Handle slot deleted event
        /// </summary>
        private void OnSlotDeletedHandler(int slotIndex)
        {
            if (showDebug) Debug.Log($"Slot {slotIndex} deleted - refreshing slots");
            
            // Refresh the specific slot
            RefreshSlot(slotIndex);
        }
        
        /// <summary>
        /// Load game from specific slot
        /// </summary>
        public void LoadGameFromSlot(int slotIndex)
        {
            if (saveManager == null)
            {
                Debug.LogError("SaveSlotManager: Cannot load game - no SaveManager!");
                return;
            }
            
            if (showDebug) Debug.Log($"★★★ LOADING GAME FROM SLOT {slotIndex} ★★★");
            
            try
            {
                // Verify the slot has data first
                SaveData slotData = LoadSaveDataForSlot(slotIndex);
                if (slotData == null)
                {
                    Debug.LogError($"★ Cannot load from slot {slotIndex} - no save data found!");
                    return;
                }
                
                Debug.Log($"★ Found save data in slot {slotIndex}: Scene={slotData.playerData.currentScene}, Time={slotData.totalPlayTime}s");
                
                // Set the save slot in SaveManager with verification
                if (!SetSaveSlotInManager(slotIndex))
                {
                    Debug.LogError($"★ FAILED TO SET SAVE SLOT {slotIndex} FOR LOADING - ABORTING!");
                    return;
                }
                
                // Load from the slot
                var loadMethod = saveManager.GetType().GetMethod("LoadFromSlot");
                if (loadMethod != null)
                {
                    Debug.Log($"★ Attempting to load from slot {slotIndex}...");
                    bool loadSuccess = (bool)loadMethod.Invoke(saveManager, new object[] { slotIndex });
                    
                    if (loadSuccess)
                    {
                        Debug.Log($"★ Successfully loaded from slot {slotIndex}");
                        
                        // Notify about successful load
                        OnLoadGameSlotSelected?.Invoke(slotIndex);
                        
                        // Load the game scene
                        LoadNewGameScene();
                    }
                    else
                    {
                        Debug.LogError($"★ Failed to load save data from slot {slotIndex}");
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error loading game: {e.Message}");
            }
        }
        
        /// <summary>
        /// Load the game scene
        /// </summary>
        private void LoadGameScene()
        {
            LoadNewGameScene();
        }
          /// <summary>
        /// Start new game in specified slot
        /// </summary>
        public void StartNewGameInSlot(int slotIndex)
        {
            if (saveManager == null)
            {
                Debug.LogError("SaveSlotManager: Cannot start new game - no SaveManager!");
                return;
            }

            Debug.Log($"★★★ STARTING NEW GAME IN SLOT {slotIndex} ★★★");
            
            // DISABLE AUTO-SAVE to prevent unwanted saves during creation
            float originalAutoSaveInterval = DisableAutoSave();
            
            try
            {
                // STEP 0: Nuclear reset to ensure complete isolation
                var forceResetMethod = saveManager.GetType().GetMethod("ForceResetForNewGame");
                if (forceResetMethod != null)
                {
                    forceResetMethod.Invoke(saveManager, new object[] { slotIndex });
                    Debug.Log($"★ NUCLEAR RESET SaveManager for slot {slotIndex}");
                }
                
                // Wait for reset to complete
                System.Threading.Thread.Sleep(300);
                
                // STEP 1: Force clear any existing data first
                var forceClearMethod = saveManager.GetType().GetMethod("ForceClearSaveData");
                if (forceClearMethod != null)
                {
                    forceClearMethod.Invoke(saveManager, null);
                    Debug.Log($"★ Forced clear save data from memory");
                }
                
                // STEP 2: Force set slot and prevent any loading
                var forceSetSlotMethod = saveManager.GetType().GetMethod("ForceSetSlotAndPreventLoad");
                if (forceSetSlotMethod != null)
                {
                    forceSetSlotMethod.Invoke(saveManager, new object[] { slotIndex });
                    Debug.Log($"★ Force set slot to {slotIndex} and prevent load");
                }
                
                // STEP 3: Use the new public method that ensures proper slot isolation
                var startNewGameMethod = saveManager.GetType().GetMethod("StartNewGameInSlot");
                if (startNewGameMethod != null)
                {
                    Debug.Log($"★ Starting new game using SaveManager.StartNewGameInSlot({slotIndex})");
                    startNewGameMethod.Invoke(saveManager, new object[] { slotIndex });
                    Debug.Log($"★ New game started successfully in slot {slotIndex}");
                }
                else
                {
                    Debug.LogError("★ SaveManager.StartNewGameInSlot method not found!");
                    return;
                }
                
                // STEP 4: Triple-check slot setting
                if (!VerifySlotInSaveManager(slotIndex))
                {
                    Debug.LogError($"★ CRITICAL: Slot verification failed after new game creation!");
                    return;
                }
                
                // Notify that new game was started
                OnNewGameSlotSelected?.Invoke(slotIndex);
                
                // IMMEDIATE refresh to show the new save
                Debug.Log("★ IMMEDIATE REFRESH - After new game creation:");
                RefreshSaveSlots();
                
                // Force refresh slots to show the new save with delay
                ForceRefreshAllSlots();
                
                // Load the game scene
                LoadNewGameScene();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error starting new game: {e.Message}");
            }
            finally
            {
                // Re-enable auto-save
                EnableAutoSave(originalAutoSaveInterval);
            }
        }
        
        /// <summary>
        /// Load the new game scene
        /// </summary>
        private void LoadNewGameScene()
        {
            if (string.IsNullOrEmpty(newGameScene))
            {
                Debug.LogError("SaveSlotManager: No new game scene specified!");
                return;
            }
            
            if (showDebug) Debug.Log($"Loading new game scene: {newGameScene}");
            
            // Try to use loading screen for smooth transition
            MonoBehaviour loadingScreen = null;
            var allComponents = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            
            foreach (var component in allComponents)
            {
                if (component.GetType().Name == "LoadingScreen")
                {
                    loadingScreen = component;
                    break;
                }
            }
            
            if (loadingScreen != null)
            {
                // Use loading screen
                var loadSceneMethod = loadingScreen.GetType().GetMethod("LoadScene", new System.Type[] { typeof(string), typeof(string), typeof(string) });
                if (loadSceneMethod != null)
                {
                    loadSceneMethod.Invoke(loadingScreen, new object[] { newGameScene, "Starting new adventure...", "Your journey begins now!" });
                    return;
                }
            }
            
            // Fallback: direct scene load
            UnityEngine.SceneManagement.SceneManager.LoadScene(newGameScene);
        }
        
        /// <summary>
        /// Refresh specific save slot
        /// </summary>
        public void RefreshSlot(int slotIndex)
        {
            if (slotIndex >= 0 && slotIndex < saveSlots.Length && saveSlots[slotIndex] != null)
            {
                // Use enhanced save slot info (same as RefreshSaveSlots)
                SaveSlotInfo slotInfo = GetEnhancedSaveSlotInfo(slotIndex);
                
                // Use the enhanced method
                saveSlots[slotIndex].SetSaveSlotInfo(slotInfo);
                
                if (showDebug) 
                {
                    Debug.Log($"★ Refreshed slot {slotIndex}: {(slotInfo.isEmpty ? "EMPTY" : $"Area={slotInfo.areaName}, Time={slotInfo.playTime}s")}");
                }
            }
        }
        
        /// <summary>
        /// Get checkpoint data by ID from SaveManager
        /// </summary>
        private CheckpointData GetCheckpointById(string checkpointId)
        {
            if (saveManager == null || string.IsNullOrEmpty(checkpointId))
                return null;
            
            try
            {
                // Access checkpointLibrary from SaveManager
                var libraryField = saveManager.GetType().GetField("checkpointLibrary", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (libraryField != null)
                {
                    var checkpointLibrary = libraryField.GetValue(saveManager);
                    if (checkpointLibrary != null)
                    {
                        var getByIdMethod = checkpointLibrary.GetType().GetMethod("GetCheckpointById");
                        if (getByIdMethod != null)
                        {
                            return getByIdMethod.Invoke(checkpointLibrary, new object[] { checkpointId }) as CheckpointData;
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                if (showDebug) Debug.LogWarning($"Error getting checkpoint by ID '{checkpointId}': {e.Message}");
            }
            
            return null;
        }
        
        /// <summary>
        /// Get enhanced save slot info that includes CheckpointData info
        /// </summary>
        public SaveSlotInfo GetEnhancedSaveSlotInfo(int slotIndex)
        {
            SaveData saveData = LoadSaveDataForSlot(slotIndex);
            
            if (saveData == null)
            {
                return new SaveSlotInfo
                {
                    slotIndex = slotIndex,
                    isEmpty = true,
                    areaName = "Empty",
                    playTime = 0f,
                    saveDateTime = "",
                    lastSavePlayTime = 0f,
                    lastSaveDateTime = ""
                };
            }
            
            // Get CheckpointData for additional info
            CheckpointData checkpointData = null;
            if (!string.IsNullOrEmpty(saveData.checkpointData.lastCheckpointId))
            {
                checkpointData = GetCheckpointById(saveData.checkpointData.lastCheckpointId);
            }
            
            // Determine area name (prioritize CheckpointData.areaName)
            string areaName = "Unknown Area";
            if (checkpointData != null && !string.IsNullOrEmpty(checkpointData.areaName))
            {
                areaName = checkpointData.areaName;
            }
            else if (!string.IsNullOrEmpty(saveData.checkpointData.lastCheckpointName))
            {
                areaName = saveData.checkpointData.lastCheckpointName;
            }
            else if (!string.IsNullOrEmpty(saveData.playerData.currentScene))
            {
                areaName = saveData.playerData.currentScene.Replace("_", " ").Replace("-", " ");
            }
            
            return new SaveSlotInfo
            {
                slotIndex = slotIndex,
                isEmpty = false,
                areaName = areaName,
                playTime = saveData.totalPlayTime,
                saveDateTime = saveData.saveTime.ToString("yyyy-MM-dd HH:mm:ss"),
                lastSavePlayTime = checkpointData?.lastSavePlayTime ?? saveData.totalPlayTime,
                lastSaveDateTime = checkpointData?.lastSaveDateTime ?? saveData.saveTime.ToString("yyyy-MM-dd HH:mm:ss")
            };
        }
        
        /// <summary>
        /// Force refresh all slots - useful after creating new save or deleting
        /// </summary>
        public void ForceRefreshAllSlots()
        {
            Debug.Log("★★★ FORCE REFRESHING ALL SLOTS ★★★");
            
            // Add small delay to ensure file operations are complete
            StartCoroutine(DelayedRefresh());
        }
        
        private System.Collections.IEnumerator DelayedRefresh()
        {
            Debug.Log("★ Waiting 0.3 seconds for file operations to complete...");
            yield return new UnityEngine.WaitForSeconds(0.3f);
            Debug.Log("★ Starting delayed refresh...");
            RefreshSaveSlots();
            
            // Double-check after refresh
            yield return new UnityEngine.WaitForSeconds(0.1f);
            Debug.Log("★★★ POST-REFRESH VALIDATION ★★★");
            for (int i = 0; i < saveSlots.Length; i++)
            {
                if (saveSlots[i] != null)
                {
                    Debug.Log($"★ Slot {i} UI: IsEmpty={saveSlots[i].IsEmpty}, Text='{saveSlots[i].GetComponentInChildren<TextMeshProUGUI>()?.text}'");
                }
            }
        }
        
        /// <summary>
        /// Force refresh slots immediately (public method for testing)
        /// </summary>
        [ContextMenu("Force Refresh All Slots NOW")]
        public void ForceRefreshNow()
        {
            Debug.Log("★★★ FORCE REFRESH NOW - MANUAL TRIGGER ★★★");
            
            if (saveManager == null)
            {
                saveManager = FindFirstObjectByType<SaveManager>();
                Debug.Log($"★ Re-found SaveManager: {saveManager != null}");
            }
            
            RefreshSaveSlots();
            
            // Also force individual slot refresh
            for (int i = 0; i < saveSlots.Length; i++)
            {
                if (saveSlots[i] != null)
                {
                    RefreshSlot(i);
                }
            }
        }
        
        /// <summary>
        /// Debug method to check UI component states
        /// </summary>
        [ContextMenu("Debug UI Component States")]
        public void DebugUIComponentStates()
        {
            Debug.Log("★★★ UI COMPONENT DEBUG ★★★");
            
            for (int i = 0; i < saveSlots.Length; i++)
            {
                if (saveSlots[i] != null)
                {
                    var textComponent = saveSlots[i].GetComponentInChildren<TextMeshProUGUI>();
                    Debug.Log($"★ Slot {i}:");
                    Debug.Log($"  - SaveSlotUI exists: {saveSlots[i] != null}");
                    Debug.Log($"  - TextComponent exists: {textComponent != null}");
                    Debug.Log($"  - Current text: '{textComponent?.text}'");
                    Debug.Log($"  - GameObject active: {saveSlots[i].gameObject.activeInHierarchy}");
                    Debug.Log($"  - IsEmpty property: {saveSlots[i].IsEmpty}");
                }
                else
                {
                    Debug.LogError($"★ Slot {i} is NULL!");
                }
            }
        }
        
        /// <summary>
        /// Set save slot in SaveManager with verification
        /// </summary>
        private bool SetSaveSlotInManager(int slotIndex)
        {
            if (saveManager == null)
            {
                Debug.LogError("★ SaveManager is null!");
                return false;
            }
            
            try
            {
                // First try using the new public method
                var ensureSlotMethod = saveManager.GetType().GetMethod("EnsureSaveSlot");
                if (ensureSlotMethod != null)
                {
                    Debug.Log($"★ Using SaveManager.EnsureSaveSlot({slotIndex})");
                    bool success = (bool)ensureSlotMethod.Invoke(saveManager, new object[] { slotIndex });
                    
                    if (success)
                    {
                        Debug.Log($"★ SUCCESS: SaveManager slot ensured to {slotIndex}");
                        return true;
                    }
                    else
                    {
                        Debug.LogError($"★ EnsureSaveSlot({slotIndex}) returned false!");
                        return false;
                    }
                }
                
                // Fallback to reflection method
                Debug.Log("★ EnsureSaveSlot method not found, using reflection fallback");
                
                // Get the field using reflection
                var slotField = saveManager.GetType().GetField("currentSaveSlot", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (slotField == null)
                {
                    Debug.LogError("★ Could not find currentSaveSlot field in SaveManager!");
                    return false;
                }
                
                // Get current value
                int oldSlot = (int)slotField.GetValue(saveManager);
                Debug.Log($"★ SaveManager currentSaveSlot: {oldSlot} → {slotIndex}");
                
                // Set new value
                slotField.SetValue(saveManager, slotIndex);
                
                // Verify the change
                int verifySlot = (int)slotField.GetValue(saveManager);
                if (verifySlot != slotIndex)
                {
                    Debug.LogError($"★ FAILED TO SET SLOT! Expected {slotIndex}, got {verifySlot}");
                    return false;
                }
                
                Debug.Log($"★ SUCCESS: SaveManager slot set to {verifySlot}");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"★ Error setting save slot: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Debug method to show current SaveManager state
        /// </summary>
        private void DebugSaveManagerState()
        {
            if (saveManager == null) return;
            
            try
            {
                // Get current slot
                var slotField = saveManager.GetType().GetField("currentSaveSlot", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (slotField != null)
                {
                    int currentSlot = (int)slotField.GetValue(saveManager);
                    Debug.Log($"★ SaveManager currentSaveSlot: {currentSlot}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"★ Error debugging SaveManager state: {e.Message}");
            }
        }
        
        /// <summary>
        /// Verify slot and fix if necessary
        /// </summary>
        private void VerifyAndFixSlot(int expectedSlot)
        {
            var slotField = saveManager.GetType().GetField("currentSaveSlot", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (slotField != null)
            {
                int actualSlot = (int)slotField.GetValue(saveManager);
                if (actualSlot != expectedSlot)
                {
                    Debug.LogError($"★ SLOT RESET DETECTED! Expected {expectedSlot}, found {actualSlot}. Re-setting...");
                    slotField.SetValue(saveManager, expectedSlot);
                    Debug.Log($"★ Fixed slot to {expectedSlot}");
                }
                else
                {
                    Debug.Log($"★ Slot verification OK: {actualSlot}");
                }
            }
        }
        
        /// <summary>
        /// Verify that save was created in the correct slot
        /// </summary>
        private System.Collections.IEnumerator VerifySaveCreation(int expectedSlot)
        {
            yield return new UnityEngine.WaitForSeconds(0.5f); // Wait for file write
            
            SaveData verifyData = LoadSaveDataForSlot(expectedSlot);
            if (verifyData != null)
            {
                Debug.Log($"★ VERIFICATION SUCCESS: Save file exists in slot {expectedSlot}");
                
                // Force UI refresh after verification
                RefreshSlot(expectedSlot);
            }
            else
            {
                Debug.LogError($"★ VERIFICATION FAILED: No save file found in slot {expectedSlot}!");
            }
        }
        
        /// <summary>
        /// Temporarily disable auto-save to prevent interference
        /// </summary>
        private float DisableAutoSave()
        {
            var autoSaveField = saveManager.GetType().GetField("autoSaveInterval", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (autoSaveField != null)
            {
                float originalInterval = (float)autoSaveField.GetValue(saveManager);
                autoSaveField.SetValue(saveManager, 0f); // Disable auto-save
                Debug.Log($"★ Auto-save disabled (was {originalInterval}s)");
                return originalInterval;
            }
            
            return 0f;
        }
        
        /// <summary>
        /// Re-enable auto-save with original interval
        /// </summary>
        private void EnableAutoSave(float interval)
        {
            var autoSaveField = saveManager.GetType().GetField("autoSaveInterval", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (autoSaveField != null)
            {
                autoSaveField.SetValue(saveManager, interval);
                Debug.Log($"★ Auto-save re-enabled ({interval}s)");
            }
        }

        private void Update()
        {
            // Quick fix controls
            if (Input.GetKeyDown(forceRefreshKey))
            {
                Debug.Log("★ F5 pressed - Force refreshing all slots!");
                ForceRefreshNow();
            }
            
            if (Input.GetKeyDown(debugSaveFilesKey))
            {
                Debug.Log("★ F12 pressed - Debugging save files!");
                DebugSaveFiles();
            }
        }
        
        /// <summary>
        /// Debug save files and paths
        /// </summary>
        private void DebugSaveFiles()
        {
            Debug.Log("★★★ SAVE FILES DEBUG ★★★");
            
            string saveDir = saveManager != null ? saveManager.SaveDirectory : Path.Combine(Application.persistentDataPath, "Saves");
            Debug.Log($"★ Save Directory: {saveDir}");
            Debug.Log($"★ Directory Exists: {Directory.Exists(saveDir)}");
            
            if (Directory.Exists(saveDir))
            {
                string[] files = Directory.GetFiles(saveDir, "*.json");
                Debug.Log($"★ Found {files.Length} JSON files:");
                
                foreach (string file in files)
                {
                    string fileName = Path.GetFileName(file);
                    long fileSize = new FileInfo(file).Length;
                    Debug.Log($"  - {fileName} ({fileSize} bytes)");
                }
            }
            
            // Debug each slot
            for (int i = 0; i < saveSlots.Length; i++)
            {
                SaveData data = LoadSaveDataForSlot(i);
                Debug.Log($"★ Slot {i}: {(data != null ? $"HAS DATA - {data.playerData.currentScene}" : "EMPTY")}");
            }
        }
        
        /// <summary>
        /// Verify that SaveManager has the correct slot set
        /// </summary>
        private bool VerifySlotInSaveManager(int expectedSlot)
        {
            if (saveManager == null) return false;
            
            try
            {
                var slotField = saveManager.GetType().GetField("currentSaveSlot", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (slotField != null)
                {
                    int actualSlot = (int)slotField.GetValue(saveManager);
                    if (actualSlot == expectedSlot)
                    {
                        Debug.Log($"★ Slot verification SUCCESS: {actualSlot}");
                        return true;
                    }
                    else
                    {
                        Debug.LogError($"★ Slot verification FAILED: Expected {expectedSlot}, got {actualSlot}");
                        
                        // Try to fix it
                        slotField.SetValue(saveManager, expectedSlot);
                        Debug.Log($"★ Attempted to fix slot to {expectedSlot}");
                        
                        // Verify fix
                        int fixedSlot = (int)slotField.GetValue(saveManager);
                        return fixedSlot == expectedSlot;
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"★ Error verifying slot: {e.Message}");
            }
            
            return false;
        }
        
        /// <summary>
        /// Debug method to show complete SaveManager state
        /// </summary>
        [ContextMenu("Debug Complete SaveManager State")]
        public void DebugCompleteSaveManagerState()
        {
            Debug.Log("★★★ COMPLETE SAVEMANAGER STATE DEBUG ★★★");
            
            if (saveManager == null)
            {
                Debug.LogError("★ SaveManager is NULL!");
                return;
            }
            
            try
            {
                // Get current slot
                var slotField = saveManager.GetType().GetField("currentSaveSlot", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                var currentSlot = slotField?.GetValue(saveManager) ?? "UNKNOWN";
                Debug.Log($"★ SaveManager.currentSaveSlot: {currentSlot}");
                
                // Get current save data
                var saveDataProperty = saveManager.GetType().GetProperty("CurrentSaveData");
                var currentSaveData = saveDataProperty?.GetValue(saveManager);
                Debug.Log($"★ SaveManager.CurrentSaveData: {(currentSaveData != null ? "HAS DATA" : "NULL")}");
                
                if (currentSaveData != null)
                {
                    var saveDataSlot = currentSaveData.GetType().GetField("saveSlot")?.GetValue(currentSaveData) ?? "UNKNOWN";
                    Debug.Log($"★ CurrentSaveData.saveSlot: {saveDataSlot}");
                }
                
                // Get flags
                var isCreatingField = saveManager.GetType().GetField("isCreatingNewGame", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var isCreating = isCreatingField?.GetValue(saveManager) ?? "UNKNOWN";
                Debug.Log($"★ SaveManager.isCreatingNewGame: {isCreating}");
                
                var skipLoadField = saveManager.GetType().GetField("forceSkipLoadOnStart", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var skipLoad = skipLoadField?.GetValue(saveManager) ?? "UNKNOWN";
                Debug.Log($"★ SaveManager.forceSkipLoadOnStart: {skipLoad}");
                
                Debug.Log("★★★ DEBUG COMPLETE ★★★");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"★ Error debugging SaveManager: {e.Message}");
            }
        }
    }
}
