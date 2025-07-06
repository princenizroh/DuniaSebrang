using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using DS.Data.Save;

namespace DS
{
    /// <summary>
    /// Core save system manager - handles all save/load operations
    /// </summary>
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        [Header("=== SAVE SETTINGS ===")]
        [Tooltip("Auto save interval (seconds, 0 = disabled)")]
        [SerializeField] private float autoSaveInterval = 30f; // 30 seconds untuk testing

        [Tooltip("Maximum save slots")]
        [SerializeField] private int maxSaveSlots = 5;

        [Tooltip("Current active save slot")]
        [SerializeField] private int currentSaveSlot = 0;

        [Tooltip("Save file prefix")]
        [SerializeField] private string saveFilePrefix = "DuniaSebrang_Save";

        [Header("=== CHECKPOINTS ===")]
        [Tooltip("Checkpoint library containing all checkpoints")]
        [SerializeField] private ScriptableObject checkpointLibrary;

        [Header("=== REFERENCES ===")]
        [Tooltip("Player GameObject reference")]
        [SerializeField] private GameObject player;

        [Tooltip("Player death handler reference")]
        [SerializeField] private PlayerDeathHandler playerDeathHandler;

        [Header("=== DEBUG ===")]
        [Tooltip("Show debug messages")]
        [SerializeField] private bool showDebug = true;

        [Tooltip("Enable debug GUI")]
        [SerializeField] private bool enableDebugGUI = true;

        // Runtime data
        private SaveData currentSaveData;
        private CheckpointData currentCheckpoint;
        private float lastAutoSaveTime;
        private bool isCreatingNewGame = false; // Flag to track new game creation
        private bool forceSkipLoadOnStart = false; // Force skip load on next Start()
        private bool skipAutoLoadOnStart = false; // Flag to skip auto-load in Start()
        private string saveDirectory;

        // Death state tracking
        private bool isPlayerDead = false;

        // Play time tracking
        private float sessionStartTime;
        private bool isTrackingTime = false;

        // Events
        public event Action<SaveData> OnGameSaved;
        public event Action<SaveData> OnGameLoaded;
        public event Action<CheckpointData> OnCheckpointActivated;

        // Singleton pattern
        public static SaveManager Instance { get; private set; }

        // Persistent slot tracking (static to survive scene changes)
        private static int persistentCurrentSlot = 0;

        private void Awake()
        {
            // Singleton setup
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);

                // Restore persistent slot on new instance
                currentSaveSlot = persistentCurrentSlot;
                if (showDebug) Debug.Log($"★ SaveManager Awake: Restored slot to {currentSaveSlot}");

                InitializeSaveSystem();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // Auto-find references if not assigned
            if (player == null)
                player = GameObject.FindGameObjectWithTag("Player");

            if (playerDeathHandler == null && player != null)
                playerDeathHandler = player.GetComponent<PlayerDeathHandler>();

            // CRITICAL: Check if we should skip auto-load (for new game scenarios)
            if (forceSkipLoadOnStart)
            {
                if (showDebug) Debug.Log($"★ Start() - FORCE SKIPPING auto-load for slot {currentSaveSlot} (NEW GAME)");
                forceSkipLoadOnStart = false; // Reset flag

                // Start tracking play time without loading anything
                StartTimeTracking();
                return;
            }

            // Check if we're creating a new game
            if (isCreatingNewGame)
            {
                if (showDebug) Debug.Log($"★ Start() - SKIPPING auto-load (creating new game in slot {currentSaveSlot})");

                // Start tracking play time without loading
                StartTimeTracking();
                return;
            }

            // PREVENT AUTO-LOADING IN MAIN MENU SCENE
            string currentScene = SceneManager.GetActiveScene().name;
            if (currentScene.Contains("MainMenu") || currentScene.Contains("Menu"))
            {
                if (showDebug) Debug.Log($"★ Start() - SKIPPING auto-load (in main menu scene: {currentScene})");

                // Just start time tracking, don't load anything
                StartTimeTracking();
                return;
            }

            // Only load existing save if we're not in skip mode AND not in main menu
            if (!skipAutoLoadOnStart)
            {
                if (showDebug) Debug.Log($"★ Start() - Loading existing save for slot {currentSaveSlot}");
                LoadOrCreateSave();
            }
            else
            {
                if (showDebug) Debug.Log($"★ Start() - Skipping auto-load (skip flag set for slot {currentSaveSlot})");
                skipAutoLoadOnStart = false; // Reset flag
            }

            // Start tracking play time
            StartTimeTracking();
        }

        private void Update()
        {
            // Don't auto-save when creating new game
            if (isCreatingNewGame)
            {
                return;
            }

            // Auto save check
            if (autoSaveInterval > 0 && Time.time - lastAutoSaveTime >= autoSaveInterval)
            {
                AutoSave();
            }

            // Update play time more frequently (every 1 second) and auto-save
            if (isTrackingTime && Time.time - sessionStartTime >= 1f)
            {
                UpdatePlayTime();

                // Auto-save every 10 seconds to preserve time progress - but NOT when creating new game
                if (Time.time - lastAutoSaveTime >= 10f && !isCreatingNewGame)
                {
                    QuickSave();
                    lastAutoSaveTime = Time.time;
                }
            }
        }

        private void InitializeSaveSystem()
        {
            // Setup save directory
            saveDirectory = Path.Combine(Application.persistentDataPath, "Saves");
            if (!Directory.Exists(saveDirectory))
            {
                Directory.CreateDirectory(saveDirectory);
                if (showDebug) Debug.Log($"Created save directory: {saveDirectory}");
            }

            // Initialize save data
            currentSaveData = new SaveData();

            if (showDebug) Debug.Log("Save system initialized");
        }

        #region SAVE OPERATIONS

        /// <summary>
        /// Save game at specific checkpoint
        /// </summary>
        public bool SaveGameAtCheckpoint(CheckpointData checkpoint)
        {
            if (checkpoint == null)
            {
                Debug.LogError("SaveManager: Cannot save - checkpoint is null!");
                return false;
            }

            if (showDebug) Debug.Log($"★ Saving game at checkpoint: {checkpoint.checkpointName} to slot {currentSaveSlot}");

            // Update current checkpoint
            currentCheckpoint = checkpoint;

            // Gather save data
            GatherSaveData();

            // Update checkpoint data
            UpdateCheckpointSaveData(checkpoint);

            // Ensure save data has correct slot
            if (currentSaveData != null)
            {
                currentSaveData.saveSlot = currentSaveSlot;
                if (showDebug) Debug.Log($"★ SaveGameAtCheckpoint: Ensured SaveData.saveSlot = {currentSaveSlot}");
            }

            // Save to file
            bool saveSuccess = SaveToFile();

            if (saveSuccess)
            {
                OnGameSaved?.Invoke(currentSaveData);
                OnCheckpointActivated?.Invoke(checkpoint);

                if (showDebug) Debug.Log($"★ Game saved successfully at {checkpoint.checkpointName} to slot {currentSaveSlot}");
            }
            else
            {
                Debug.LogError($"★ FAILED to save game at {checkpoint.checkpointName} to slot {currentSaveSlot}");
            }

            return saveSuccess;
        }

        /// <summary>
        /// Quick save current game state
        /// </summary>
        public bool QuickSave()
        {
            if (currentCheckpoint == null)
            {
                Debug.LogWarning("SaveManager: No checkpoint available for quick save!");
                return false;
            }

            // Force gather current player position before saving
            if (showDebug) Debug.Log($"★ Quick Save to slot {currentSaveSlot}: Gathering current player position...");
            GatherCurrentPlayerPosition();

            // Ensure save data has correct slot before saving
            if (currentSaveData != null)
            {
                currentSaveData.saveSlot = currentSaveSlot;
                if (showDebug) Debug.Log($"★ QuickSave: Ensured SaveData.saveSlot = {currentSaveSlot}");
            }

            return SaveGameAtCheckpoint(currentCheckpoint);
        }

        /// <summary>
        /// Auto save (called by timer)
        /// </summary>
        private void AutoSave()
        {
            if (currentCheckpoint != null && !isPlayerDead)
            {
                if (showDebug) Debug.Log("Auto-saving game...");
                QuickSave();
            }

            lastAutoSaveTime = Time.time;
        }

        /// <summary>
        /// Save to specific slot
        /// </summary>
        public bool SaveToSlot(int slot)
        {
            if (slot < 0 || slot >= maxSaveSlots)
            {
                Debug.LogError($"SaveManager: Invalid save slot {slot}!");
                return false;
            }

            int previousSlot = currentSaveSlot;
            currentSaveSlot = slot;
            persistentCurrentSlot = slot; // Update persistent slot too!

            bool success = QuickSave();

            if (!success)
            {
                currentSaveSlot = previousSlot;
                persistentCurrentSlot = previousSlot; // Restore persistent too
            }

            return success;
        }

        #endregion

        #region LOAD OPERATIONS

        /// <summary>
        /// Load game from current save slot
        /// </summary>
        public bool LoadGame()
        {
            if (showDebug) Debug.Log($"★ LoadGame() called - currentSaveSlot: {currentSaveSlot}");
            return LoadFromSlot(currentSaveSlot);
        }

        /// <summary>
        /// Load game from specific slot
        /// </summary>
        public bool LoadFromSlot(int slot)
        {
            if (slot < 0 || slot >= maxSaveSlots)
            {
                Debug.LogError($"SaveManager: Invalid save slot {slot}!");
                return false;
            }

            string fileName = GetSaveFileName(slot);
            string filePath = Path.Combine(saveDirectory, fileName);

            if (!File.Exists(filePath))
            {
                if (showDebug) Debug.LogWarning($"Save file not found: {filePath}");
                return false;
            }

            try
            {
                string json = File.ReadAllText(filePath);

                if (showDebug) Debug.Log($"★ LOADING FROM SLOT {slot}: {filePath}");
                if (showDebug) Debug.Log($"★ JSON content preview: {json.Substring(0, Mathf.Min(200, json.Length))}...");

                currentSaveData = JsonUtility.FromJson<SaveData>(json);
                currentSaveSlot = slot;
                persistentCurrentSlot = slot; // Update persistent slot too!

                if (showDebug) Debug.Log($"★ Loaded SaveData.saveSlot = {currentSaveData.saveSlot}");
                if (showDebug) Debug.Log($"★ Current SaveManager slot = {currentSaveSlot}");
                if (showDebug) Debug.Log($"★ Loaded save data - Position in file: {currentSaveData.playerData.position}");

                // Apply loaded data to game
                ApplyLoadedData();

                OnGameLoaded?.Invoke(currentSaveData);

                if (showDebug) Debug.Log($"★ Game loaded successfully from slot {slot}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"SaveManager: Failed to load save file: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Load or create new save - respects new game creation flag
        /// </summary>
        private void LoadOrCreateSave()
        {
            if (showDebug) Debug.Log($"★ LoadOrCreateSave called - isCreatingNewGame: {isCreatingNewGame}, currentSaveSlot: {currentSaveSlot}");

            // If we're in the middle of creating a new game, don't try to load existing data
            if (isCreatingNewGame)
            {
                if (showDebug) Debug.Log("★ Skipping load because we're creating a new game");
                return;
            }

            if (!LoadGame())
            {
                // Create new save with starting checkpoint
                if (showDebug) Debug.Log("★ No existing save found, creating new save");
                CreateNewSave();
            }
            else
            {
                if (showDebug) Debug.Log("★ Existing save loaded successfully");
            }
        }

        /// <summary>
        /// Create new save file - force clean start
        /// </summary>
        public void CreateNewSave()
        {
            if (showDebug) Debug.Log($"★★★ CREATING COMPLETELY NEW SAVE FOR SLOT {currentSaveSlot} ★★★");
            if (showDebug) Debug.Log($"★ BEFORE CreateNewSave: currentSaveSlot = {currentSaveSlot}, persistentCurrentSlot = {persistentCurrentSlot}");

            // Set flag to indicate we're creating a new game
            isCreatingNewGame = true;

            // FORCE CLEAR ALL EXISTING DATA FIRST
            ForceClearSaveData();

            // DELETE any existing save file for this slot to ensure clean start
            DeleteSaveFile(currentSaveSlot);

            // Create completely fresh save data from scratch
            currentSaveData = new SaveData();
            currentSaveData.saveSlot = currentSaveSlot;
            currentSaveData.saveTime = System.DateTime.Now;
            currentSaveData.totalPlayTime = 0f;

            // Initialize player data with default values (already done in SaveData constructor)
            currentSaveData.playerData.currentScene = ""; // Will be set when checkpoint is applied
            currentSaveData.playerData.position = Vector3.zero;
            currentSaveData.playerData.rotation = Vector3.zero;

            // Initialize checkpoint data (already done in SaveData constructor)

            if (showDebug) Debug.Log($"★ Created fresh SaveData for slot {currentSaveSlot}");

            // Set starting checkpoint from library using reflection
            if (checkpointLibrary != null)
            {
                try
                {
                    var defaultProperty = checkpointLibrary.GetType().GetProperty("DefaultStartingCheckpoint");
                    if (defaultProperty != null)
                    {
                        CheckpointData startingCheckpoint = defaultProperty.GetValue(checkpointLibrary) as CheckpointData;
                        if (startingCheckpoint != null)
                        {
                            if (showDebug) Debug.Log($"★ Setting starting checkpoint: {startingCheckpoint.checkpointName}");

                            // Clear checkpoint reference first
                            currentCheckpoint = null;

                            // Set the starting checkpoint
                            currentCheckpoint = startingCheckpoint;

                            // Update save data with checkpoint info
                            currentSaveData.checkpointData.lastCheckpointId = startingCheckpoint.Id;
                            currentSaveData.checkpointData.lastCheckpointName = startingCheckpoint.checkpointName;
                            currentSaveData.playerData.currentScene = startingCheckpoint.sceneName;
                            currentSaveData.playerData.position = startingCheckpoint.spawnPosition;
                            currentSaveData.playerData.rotation = startingCheckpoint.spawnRotation;

                            // Force save the new clean data immediately with safety check
                            if (showDebug) Debug.Log($"★ Force saving new data to file for slot {currentSaveSlot}");

                            // Double-ensure the slot is correct before saving
                            currentSaveData.saveSlot = currentSaveSlot;

                            bool saveSuccess = SaveToFile();
                            if (saveSuccess)
                            {
                                if (showDebug) Debug.Log($"★ New save created and saved to file for slot {currentSaveSlot}");
                            }
                            else
                            {
                                Debug.LogError($"★ FAILED to save new game data to slot {currentSaveSlot}!");
                            }
                        }
                        else
                        {
                            Debug.LogWarning("SaveManager: No default starting checkpoint in library!");
                        }
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"SaveManager: Error accessing checkpoint library: {e.Message}");
                }
            }
            else
            {
                Debug.LogWarning("SaveManager: No checkpoint library assigned!");
            }

            // Clear the flag after a delay to ensure everything is set up
            StartCoroutine(ClearNewGameFlagDelayed());

            if (showDebug) Debug.Log($"★★★ NEW SAVE CREATION COMPLETE FOR SLOT {currentSaveSlot} ★★★");
        }

        /// <summary>
        /// Clear new game flag after delay
        /// </summary>
        private System.Collections.IEnumerator ClearNewGameFlagDelayed()
        {
            yield return new UnityEngine.WaitForSeconds(1f);
            isCreatingNewGame = false;
            if (showDebug) Debug.Log("★ New game flag cleared after delay");
        }

        #endregion

        #region RESPAWN/CHECKPOINT OPERATIONS

        /// <summary>
        /// Respawn player at last checkpoint
        /// </summary>
        public void RespawnAtLastCheckpoint()
        {
            if (currentCheckpoint == null)
            {
                Debug.LogError("SaveManager: No checkpoint available for respawn!");
                return;
            }

            RespawnAtCheckpoint(currentCheckpoint);
        }

        /// <summary>
        /// Respawn player at specific checkpoint
        /// </summary>
        public void RespawnAtCheckpoint(CheckpointData checkpoint)
        {
            if (checkpoint == null)
            {
                Debug.LogError("SaveManager: Cannot respawn - checkpoint is null!");
                return;
            }

            if (showDebug) Debug.Log($"★★★ RESPAWNING AT CHECKPOINT: {checkpoint.checkpointName} ★★★");

            // CRITICAL: Validate scene name before any scene loading
            string currentSceneName = SceneManager.GetActiveScene().name;

            if (showDebug) Debug.Log($"🎬 Current Scene: '{currentSceneName}'");
            if (showDebug) Debug.Log($"🎯 Checkpoint Scene: '{checkpoint.sceneName}'");

            // PREVENT MAIN MENU LOADING
            if (!string.IsNullOrEmpty(checkpoint.sceneName))
            {
                string checkpointSceneLower = checkpoint.sceneName.ToLower();
                if (checkpointSceneLower.Contains("menu") || checkpointSceneLower.Contains("main"))
                {
                    Debug.LogError($"❌ CRITICAL ERROR: Checkpoint points to menu scene '{checkpoint.sceneName}'!");
                    Debug.LogError($"❌ BLOCKING scene load to prevent going to main menu!");
                    Debug.LogError($"❌ FIX: Update CheckpointData '{checkpoint.checkpointName}' sceneName to current game scene!");

                    // Force use current scene instead
                    checkpoint.sceneName = currentSceneName;
                    Debug.LogWarning($"⚠️ TEMPORARY FIX: Using current scene '{currentSceneName}' for respawn");
                }

                // Check if scene change is really needed
                if (currentSceneName != checkpoint.sceneName)
                {
                    Debug.LogError($"❌ WARNING: Scene change required from '{currentSceneName}' to '{checkpoint.sceneName}'");
                    Debug.LogError($"❌ This might be the source of main menu issue!");
                    Debug.LogError($"❌ Recommended: Set checkpoint.sceneName = '{currentSceneName}'");

                    // EMERGENCY PREVENTION: Don't load scene if it might be main menu
                    if (checkpoint.sceneName != currentSceneName)
                    {
                        Debug.LogError($"❌ EMERGENCY PREVENTION: Blocking scene load to '{checkpoint.sceneName}'");
                        Debug.LogError($"❌ Respawning in current scene instead");
                        // Don't call SceneManager.LoadScene() - this is what causes main menu issue!
                    }
                }
                else
                {
                    if (showDebug) Debug.Log($"✅ Same scene respawn: {currentSceneName}");
                }
            }
            else
            {
                Debug.LogWarning("⚠️ Checkpoint sceneName is empty - using current scene");
            }

            // Move player to checkpoint position (THIS is what we want, not scene loading)
            if (player != null)
            {
                player.transform.position = checkpoint.spawnPosition;
                player.transform.eulerAngles = checkpoint.spawnRotation;

                if (showDebug) Debug.Log($"★ Player moved to checkpoint: {checkpoint.spawnPosition}");
            }
            else
            {
                Debug.LogError("SaveManager: Cannot respawn - player reference is null!");
            }

            // Update current checkpoint
            currentCheckpoint = checkpoint;

            if (showDebug) Debug.Log("★★★ RESPAWN POSITIONING COMPLETE ★★★");
        }

        #endregion

        #region DATA GATHERING & APPLYING

        /// <summary>
        /// Gather all save data from game state
        /// </summary>
        private void GatherSaveData()
        {
            if (currentSaveData == null)
                currentSaveData = new SaveData();

            // Update save metadata
            currentSaveData.UpdateSaveTime();
            currentSaveData.saveSlot = currentSaveSlot;

            // Gather player data
            GatherPlayerData();

            // Gather game progress
            GatherGameProgressData();

            // Gather collectibles
            GatherCollectiblesData();

            if (showDebug) Debug.Log("Save data gathered");
        }

        /// <summary>
        /// Gather player-specific data
        /// </summary>
        private void GatherPlayerData()
        {
            if (player == null) return;

            currentSaveData.playerData.position = player.transform.position;
            currentSaveData.playerData.rotation = player.transform.eulerAngles;
            currentSaveData.playerData.currentScene = SceneManager.GetActiveScene().name;

            // Gather player stats from PlayerDeathHandler or other components
            if (playerDeathHandler != null)
            {
                currentSaveData.playerData.isDead = playerDeathHandler.IsDead;
                // Add death count, health, etc. here based on your player systems
            }

            if (showDebug) Debug.Log($"Gathered player data - Position: {currentSaveData.playerData.position}");
        }

        /// <summary>
        /// Force gather current player position (for Quick Save)
        /// </summary>
        private void GatherCurrentPlayerPosition()
        {
            if (player == null) return;

            if (currentSaveData == null)
                currentSaveData = new SaveData();

            // Force update to current player position
            Vector3 currentPos = player.transform.position;
            Vector3 currentRot = player.transform.eulerAngles;

            currentSaveData.playerData.position = currentPos;
            currentSaveData.playerData.rotation = currentRot;
            currentSaveData.playerData.currentScene = SceneManager.GetActiveScene().name;

            if (showDebug) Debug.Log($"★ Force gathered CURRENT player position: {currentPos}");
        }

        /// <summary>
        /// Gather game progress data
        /// </summary>
        private void GatherGameProgressData()
        {
            currentSaveData.gameProgress.currentLevel = SceneManager.GetActiveScene().name;

            // TODO: Add level completion, unlocked areas, story progress, etc.
            // This will depend on your game progression systems
        }

        /// <summary>
        /// Gather collectibles data
        /// </summary>
        private void GatherCollectiblesData()
        {
            // TODO: Gather collectible states
            // This will depend on your collectible systems
        }

        /// <summary>
        /// Update checkpoint-specific save data
        /// </summary>
        private void UpdateCheckpointSaveData(CheckpointData checkpoint)
        {
            currentSaveData.checkpointData.lastCheckpointId = checkpoint.Id;
            currentSaveData.checkpointData.lastCheckpointName = checkpoint.checkpointName;
            currentSaveData.checkpointData.lastCheckpointPosition = checkpoint.spawnPosition;
            currentSaveData.checkpointData.lastCheckpointRotation = checkpoint.spawnRotation;
            currentSaveData.checkpointData.lastCheckpointScene = checkpoint.sceneName;
            currentSaveData.checkpointData.lastCheckpointTime = DateTime.Now;

            // ★ TAMBAHAN: Simpan area name dari CheckpointData
            if (!string.IsNullOrEmpty(checkpoint.areaName))
            {
                // Gunakan areaName dari CheckpointData
                currentSaveData.checkpointData.lastCheckpointName = checkpoint.areaName;
            }

            // ★ TAMBAHAN: Simpan play time ke CheckpointData ScriptableObject
            checkpoint.lastSavePlayTime = currentSaveData.totalPlayTime;
            checkpoint.lastSaveDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

#if UNITY_EDITOR
            // Mark checkpoint data as dirty untuk save perubahan
            UnityEditor.EditorUtility.SetDirty(checkpoint);
#endif

            // Add to activated checkpoints if not already present
            if (!currentSaveData.checkpointData.activatedCheckpoints.Contains(checkpoint.Id))
            {
                currentSaveData.checkpointData.activatedCheckpoints.Add(checkpoint.Id);
            }

            if (showDebug) Debug.Log($"★ Checkpoint data updated: Area={checkpoint.areaName}, Name={checkpoint.checkpointName}, PlayTime={FormatPlayTime(checkpoint.lastSavePlayTime)}");
        }

        /// <summary>
        /// Apply loaded data to game state
        /// </summary>
        private void ApplyLoadedData()
        {
            if (showDebug) Debug.Log($"★★★ APPLYING LOADED DATA FOR SLOT {currentSaveSlot} ★★★");

            if (currentSaveData == null)
            {
                Debug.LogError("★ Cannot apply loaded data - currentSaveData is null!");
                return;
            }

            if (showDebug) Debug.Log($"★ SaveData slot: {currentSaveData.saveSlot}, current slot: {currentSaveSlot}");

            // Verify that the loaded data is for the correct slot
            if (currentSaveData.saveSlot != currentSaveSlot)
            {
                Debug.LogWarning($"★ SLOT MISMATCH! SaveData is for slot {currentSaveData.saveSlot} but current slot is {currentSaveSlot}");
                Debug.LogWarning($"★ This could indicate slot contamination!");
            }

            // Find and set current checkpoint
            if (!string.IsNullOrEmpty(currentSaveData.checkpointData.lastCheckpointId))
            {
                currentCheckpoint = GetCheckpointById(currentSaveData.checkpointData.lastCheckpointId);
                if (showDebug) Debug.Log($"★ Loaded checkpoint: {currentCheckpoint?.checkpointName} (ID: {currentSaveData.checkpointData.lastCheckpointId})");
            }
            else
            {
                if (showDebug) Debug.Log("★ No checkpoint ID in save data");
            }

            // Apply player position - FORCE OVERRIDE current position
            if (player != null)
            {
                Vector3 savedPos = currentSaveData.playerData.position;
                Vector3 savedRot = currentSaveData.playerData.rotation;

                if (showDebug) Debug.Log($"★ MOVING PLAYER from {player.transform.position} to SAVED position {savedPos}");

                // DISABLE ALL MOVEMENT SCRIPTS TEMPORARILY
                DisablePlayerMovement();

                // FORCE SET POSITION MULTIPLE TIMES TO ENSURE IT STICKS
                player.transform.position = savedPos;
                player.transform.eulerAngles = savedRot;

                // Disable physics completely temporarily
                var rb = player.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = true;
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }

                // Use coroutine to ensure position sticks
                StartCoroutine(ForcePositionAfterLoad(savedPos, savedRot));

                if (showDebug) Debug.Log($"★ Player moved to loaded position: {savedPos}");
            }

            if (showDebug) Debug.Log($"★★★ LOADED DATA APPLIED SUCCESSFULLY FOR SLOT {currentSaveSlot} ★★★");
        }

        /// <summary>
        /// Coroutine to force position after load with multiple attempts
        /// </summary>
        private System.Collections.IEnumerator ForcePositionAfterLoad(Vector3 targetPos, Vector3 targetRot)
        {
            if (showDebug) Debug.Log("★ Starting ForcePositionAfterLoad coroutine");

            // Wait a frame
            yield return null;

            // Force position multiple times over several frames
            for (int i = 0; i < 10; i++)
            {
                if (player != null)
                {
                    Vector3 currentPos = player.transform.position;
                    float distance = Vector3.Distance(currentPos, targetPos);

                    if (distance > 0.1f) // If player moved away from target
                    {
                        if (showDebug) Debug.Log($"★ Force correction #{i}: Moving from {currentPos} to {targetPos} (distance: {distance:F2})");
                        player.transform.position = targetPos;
                        player.transform.eulerAngles = targetRot;
                    }
                    else
                    {
                        if (showDebug) Debug.Log($"★ Position stable at frame {i}");
                        break;
                    }
                }
                yield return null;
            }

            // Re-enable physics after position is stable
            var rb = player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
            }

            // Re-enable movement scripts after a delay
            yield return new WaitForSeconds(0.5f);
            EnablePlayerMovement();

            if (showDebug) Debug.Log("★ ForcePositionAfterLoad completed");
        }

        /// <summary>
        /// Disable player movement scripts temporarily
        /// </summary>
        private void DisablePlayerMovement()
        {
            if (player == null) return;

            string[] movementScripts = {
                "PlayerController", "PlayerMovement", "FirstPersonController",
                "ThirdPersonController", "PlayerInput", "CharacterController"
            };

            foreach (string scriptName in movementScripts)
            {
                Component script = player.GetComponent(scriptName);
                if (script != null && script is MonoBehaviour behaviour)
                {
                    behaviour.enabled = false;
                    if (showDebug) Debug.Log($"★ Temporarily disabled: {scriptName}");
                }
            }

            // Also disable CharacterController if present
            var charController = player.GetComponent<CharacterController>();
            if (charController != null)
            {
                charController.enabled = false;
                if (showDebug) Debug.Log("★ Temporarily disabled CharacterController");
            }
        }

        /// <summary>
        /// Re-enable player movement scripts
        /// </summary>
        private void EnablePlayerMovement()
        {
            if (player == null) return;

            string[] movementScripts = {
                "PlayerController", "PlayerMovement", "FirstPersonController",
                "ThirdPersonController", "PlayerInput", "CharacterController"
            };

            foreach (string scriptName in movementScripts)
            {
                Component script = player.GetComponent(scriptName);
                if (script != null && script is MonoBehaviour behaviour)
                {
                    behaviour.enabled = true;
                    if (showDebug) Debug.Log($"★ Re-enabled: {scriptName}");
                }
            }

            // Re-enable CharacterController if present
            var charController = player.GetComponent<CharacterController>();
            if (charController != null)
            {
                charController.enabled = true;
                if (showDebug) Debug.Log("★ Re-enabled CharacterController");
            }
        }

        #endregion

        #region FILE OPERATIONS

        /// <summary>
        /// Save current data to file
        /// </summary>
        private bool SaveToFile()
        {
            try
            {
                string fileName = GetSaveFileName(currentSaveSlot);
                string filePath = Path.Combine(saveDirectory, fileName);

                // MASSIVE DEBUG LOGGING TO FIND THE PROBLEM
                if (showDebug) Debug.Log($"★★★ SaveToFile() CALLED ★★★");
                if (showDebug) Debug.Log($"★ currentSaveSlot = {currentSaveSlot}");
                if (showDebug) Debug.Log($"★ persistentCurrentSlot = {persistentCurrentSlot}");
                if (showDebug) Debug.Log($"★ fileName = {fileName}");
                if (showDebug) Debug.Log($"★ filePath = {filePath}");
                if (showDebug) Debug.Log($"★ Call stack = {System.Environment.StackTrace}");

                // Ensure save data has correct slot
                if (currentSaveData != null)
                {
                    if (showDebug) Debug.Log($"★ BEFORE: currentSaveData.saveSlot = {currentSaveData.saveSlot}");
                    currentSaveData.saveSlot = currentSaveSlot;
                    if (showDebug) Debug.Log($"★ AFTER: currentSaveData.saveSlot = {currentSaveData.saveSlot}");
                }

                string json = JsonUtility.ToJson(currentSaveData, true);
                File.WriteAllText(filePath, json);

                if (showDebug) Debug.Log($"★ SAVE FILE WRITTEN TO SLOT {currentSaveSlot}: {filePath}");
                if (showDebug) Debug.Log($"★ SaveData.saveSlot = {(currentSaveData != null ? currentSaveData.saveSlot : "NULL")}");

                // VERIFY THE FILE WAS ACTUALLY WRITTEN TO THE CORRECT LOCATION
                if (File.Exists(filePath))
                {
                    if (showDebug) Debug.Log($"★ VERIFICATION: File exists at {filePath}");
                }
                else
                {
                    Debug.LogError($"★ ERROR: File NOT found at {filePath} after writing!");
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"SaveManager: Failed to save file: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get save file name for slot
        /// </summary>
        private string GetSaveFileName(int slot)
        {
            string fileName = $"{saveFilePrefix}_Slot{slot:00}.json";
            if (showDebug) Debug.Log($"★ GetSaveFileName({slot}) = {fileName}");
            return fileName;
        }

        /// <summary>
        /// Check if save file exists for slot
        /// </summary>
        public bool SaveFileExists(int slot)
        {
            string fileName = GetSaveFileName(slot);
            string filePath = Path.Combine(saveDirectory, fileName);
            return File.Exists(filePath);
        }

        /// <summary>
        /// Delete save file for slot
        /// </summary>
        public bool DeleteSaveFile(int slot)
        {
            try
            {
                string fileName = GetSaveFileName(slot);
                string filePath = Path.Combine(saveDirectory, fileName);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    if (showDebug) Debug.Log($"Deleted save file: {filePath}");
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                Debug.LogError($"SaveManager: Failed to delete save file: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Delete save slot completely
        /// </summary>
        public bool DeleteSaveSlot(int slot)
        {
            if (slot < 0 || slot >= maxSaveSlots)
            {
                Debug.LogError($"SaveManager: Invalid save slot {slot}!");
                return false;
            }

            bool success = DeleteSaveFile(slot);

            if (success && showDebug)
            {
                Debug.Log($"★★★ Save slot {slot} deleted successfully ★★★");
            }

            return success;
        }

        #endregion

        #region UTILITY METHODS

        /// <summary>
        /// Get checkpoint by ID
        /// </summary>
        private CheckpointData GetCheckpointById(string id)
        {
            if (checkpointLibrary != null)
            {
                try
                {
                    var getByIdMethod = checkpointLibrary.GetType().GetMethod("GetCheckpointById");
                    if (getByIdMethod != null)
                    {
                        return getByIdMethod.Invoke(checkpointLibrary, new object[] { id }) as CheckpointData;
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"SaveManager: Error getting checkpoint by ID: {e.Message}");
                }
            }
            return null;
        }

        /// <summary>
        /// Add checkpoint to library
        /// </summary>
        public void RegisterCheckpoint(CheckpointData checkpoint)
        {
            if (checkpointLibrary != null)
            {
                try
                {
                    var addMethod = checkpointLibrary.GetType().GetMethod("AddCheckpoint");
                    if (addMethod != null)
                    {
                        addMethod.Invoke(checkpointLibrary, new object[] { checkpoint });
                        if (showDebug) Debug.Log($"Registered checkpoint: {checkpoint.checkpointName}");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"SaveManager: Error registering checkpoint: {e.Message}");
                }
            }
            else
            {
                if (showDebug) Debug.LogWarning("No checkpoint library assigned to SaveManager!");
            }
        }

        #endregion

        #region PUBLIC PROPERTIES

        public SaveData CurrentSaveData => currentSaveData;
        public CheckpointData CurrentCheckpoint => currentCheckpoint;
        public int CurrentSaveSlot => currentSaveSlot;
        public string SaveDirectory => saveDirectory;
        public ScriptableObject CheckpointLibrary => checkpointLibrary;

        #endregion

        #region DEBUG METHODS

        [ContextMenu("Quick Save")]
        private void DebugQuickSave()
        {
            QuickSave();
        }

        [ContextMenu("Quick Load")]
        private void DebugQuickLoad()
        {
            LoadGame();
        }

        [ContextMenu("Force Load Test")]
        private void DebugForceLoadTest()
        {
            if (showDebug) Debug.Log("★★★ STARTING FORCE LOAD TEST ★★★");

            // Log current position before load
            if (player != null)
            {
                Debug.Log($"★ BEFORE LOAD - Player position: {player.transform.position}");
            }

            // Perform load
            bool success = LoadGame();

            if (success)
            {
                Debug.Log("★ LOAD COMPLETED - Check if position changed correctly");
            }
            else
            {
                Debug.LogError("★ LOAD FAILED!");
            }
        }

        [ContextMenu("Respawn at Checkpoint")]
        private void DebugRespawn()
        {
            RespawnAtLastCheckpoint();
        }


        #endregion

        /// <summary>
        /// Set death state to prevent auto-save during death/respawn
        /// </summary>
        public void SetDeathState(bool isDead)
        {
            isPlayerDead = isDead;
            if (showDebug) Debug.Log($"SaveManager death state set to: {isDead}");
        }

        /// <summary>
        /// Start tracking play time
        /// </summary>
        public void StartTimeTracking()
        {
            sessionStartTime = Time.time;
            isTrackingTime = true;
            if (showDebug) Debug.Log("Started play time tracking");
        }

        /// <summary>
        /// Stop tracking play time
        /// </summary>
        public void StopTimeTracking()
        {
            if (isTrackingTime)
            {
                UpdatePlayTime();
                isTrackingTime = false;
                if (showDebug) Debug.Log("Stopped play time tracking");
            }
        }

        /// <summary>
        /// Update total play time with current session
        /// </summary>
        private void UpdatePlayTime()
        {
            if (!isTrackingTime || currentSaveData == null) return;

            float sessionTime = Time.time - sessionStartTime;
            currentSaveData.totalPlayTime += sessionTime;
            sessionStartTime = Time.time; // Reset for next update

            if (showDebug) Debug.Log($"Play time updated: {FormatPlayTime(currentSaveData.totalPlayTime)}");
        }

        /// <summary>
        /// Format play time to readable string
        /// </summary>
        public string FormatPlayTime(float totalSeconds)
        {
            int hours = Mathf.FloorToInt(totalSeconds / 3600f);
            int minutes = Mathf.FloorToInt((totalSeconds % 3600f) / 60f);
            int seconds = Mathf.FloorToInt(totalSeconds % 60f);

            return $"{hours:00}:{minutes:00}:{seconds:00}";
        }

        /// <summary>
        /// Start a completely new game in specified slot (public method for UI)
        /// </summary>
        public void StartNewGameInSlot(int slot)
        {
            if (slot < 0 || slot >= maxSaveSlots)
            {
                Debug.LogError($"SaveManager: Invalid save slot {slot}!");
                return;
            }

            if (showDebug) Debug.Log($"★★★ STARTING NEW GAME IN SLOT {slot} ★★★");
            if (showDebug) Debug.Log($"★ BEFORE StartNewGameInSlot: currentSaveSlot = {currentSaveSlot}, persistentCurrentSlot = {persistentCurrentSlot}");

            try
            {
                // STEP 1: Only delete the save file for THIS slot (NOT all slots!)
                DeleteSaveFile(slot);
                System.Threading.Thread.Sleep(200); // Wait for file deletion

                // STEP 2: Set the slot using safety method
                if (!EnsureSaveSlot(slot))
                {
                    Debug.LogError($"★ FAILED to set save slot to {slot}!");
                    return;
                }

                if (showDebug) Debug.Log($"★ AFTER EnsureSaveSlot: currentSaveSlot = {currentSaveSlot}, persistentCurrentSlot = {persistentCurrentSlot}");

                // STEP 3: Set ALL skip flags
                isCreatingNewGame = true;
                forceSkipLoadOnStart = true;
                skipAutoLoadOnStart = true;

                // STEP 4: COMPLETELY CLEAR ALL EXISTING DATA
                currentSaveData = null;
                currentCheckpoint = null;
                if (showDebug) Debug.Log($"★ Cleared all memory data");

                // STEP 5: Create completely fresh save data
                CreateNewSave();

                if (showDebug) Debug.Log($"★ AFTER CreateNewSave: currentSaveSlot = {currentSaveSlot}, persistentCurrentSlot = {persistentCurrentSlot}");

                // STEP 6: Force save immediately to the correct slot with safety check
                if (currentSaveData != null)
                {
                    bool saveSuccess = ForceSaveToSlot(slot);
                    if (saveSuccess)
                    {
                        if (showDebug) Debug.Log($"★ Force saved new data to slot {slot}");
                    }
                    else
                    {
                        Debug.LogError($"★ FAILED to force save new data to slot {slot}!");
                    }
                }

                if (showDebug) Debug.Log($"★ FINAL: currentSaveSlot = {currentSaveSlot}, persistentCurrentSlot = {persistentCurrentSlot}");
                if (showDebug) Debug.Log($"★★★ NEW GAME SETUP COMPLETE FOR SLOT {slot} ★★★");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"SaveManager: Error starting new game in slot {slot}: {e.Message}");
            }
        }

        /// <summary>
        /// Force set save slot and prevent any loading (for new game isolation)
        /// </summary>
        public void ForceSetSlotAndPreventLoad(int slot)
        {
            if (showDebug) Debug.Log($"★ FORCE SET SLOT {slot} AND PREVENT LOAD ★");

            currentSaveSlot = slot;
            persistentCurrentSlot = slot; // Update persistent slot too!
            forceSkipLoadOnStart = true;
            skipAutoLoadOnStart = true;
            isCreatingNewGame = true;

            // Clear any existing data
            currentSaveData = null;
            currentCheckpoint = null;

            if (showDebug) Debug.Log($"★ Slot forced to {slot}, all load flags set to prevent contamination");
        }

        /// <summary>
        /// Set skip auto load flag (used before scene transitions)
        /// </summary>
        public void SetSkipAutoLoad(bool skip)
        {
            forceSkipLoadOnStart = skip;
            skipAutoLoadOnStart = skip;
            if (showDebug) Debug.Log($"★ SetSkipAutoLoad: {skip}");
        }

        /// <summary>
        /// Check if SaveManager is currently creating a new game
        /// </summary>
        public bool IsCreatingNewGame => isCreatingNewGame;

        /// <summary>
        /// Set the current save slot (public method)
        /// </summary>
        public void SetSaveSlot(int slot)
        {
            if (slot < 0 || slot >= maxSaveSlots)
            {
                Debug.LogError($"SaveManager: Invalid save slot {slot}!");
                return;
            }

            int previousSlot = currentSaveSlot;
            currentSaveSlot = slot;
            persistentCurrentSlot = slot; // Update persistent slot too!

            if (showDebug) Debug.Log($"★ SaveSlot changed from {previousSlot} to {slot} (persistent updated too)");
        }

        /// <summary>
        /// Force clear all save data from memory (for slot isolation)
        /// </summary>
        public void ForceClearSaveData()
        {
            if (showDebug) Debug.Log("★ FORCE CLEARING ALL SAVE DATA FROM MEMORY ★");

            currentSaveData = null;
            currentCheckpoint = null;

            // Stop any ongoing auto-save
            lastAutoSaveTime = 0f;

            if (showDebug) Debug.Log("★ Memory cleared successfully");
        }

        /// <summary>
        /// Get current save slot (for debugging)
        /// </summary>
        public int GetCurrentSaveSlot()
        {
            return currentSaveSlot;
        }

        /// <summary>
        /// Force reset SaveManager state for new game (nuclear option)
        /// </summary>
        public void ForceResetForNewGame(int slot)
        {
            if (showDebug) Debug.Log($"★★★ FORCE RESET SAVEMANAGER FOR NEW GAME SLOT {slot} ★★★");

            // Stop all ongoing operations
            isTrackingTime = false;
            lastAutoSaveTime = 0f;

            // Clear all data
            currentSaveData = null;
            currentCheckpoint = null;

            // Set flags
            isCreatingNewGame = true;
            forceSkipLoadOnStart = true;
            skipAutoLoadOnStart = true;

            // Set slot using safety method
            EnsureSaveSlot(slot);

            // Only delete the save file for THIS slot (not all slots!)
            DeleteSaveFile(slot);

            if (showDebug) Debug.Log($"★ SaveManager reset for slot {slot} (only this slot's file deleted)");
        }

        /// <summary>
        /// Ensure save slot is correctly set and verified (safety method)
        /// </summary>
        public bool EnsureSaveSlot(int slot)
        {
            if (slot < 0 || slot >= maxSaveSlots)
            {
                Debug.LogError($"SaveManager: Invalid save slot {slot}!");
                return false;
            }

            // Set the slot
            int previousSlot = currentSaveSlot;
            currentSaveSlot = slot;
            persistentCurrentSlot = slot; // Update persistent slot too!

            // Verify it was set correctly
            if (currentSaveSlot != slot)
            {
                Debug.LogError($"SaveManager: Failed to set slot to {slot}! Current slot is {currentSaveSlot}");
                currentSaveSlot = previousSlot; // Restore previous
                persistentCurrentSlot = previousSlot; // Restore persistent too
                return false;
            }

            if (showDebug) Debug.Log($"★ EnsureSaveSlot: Successfully set to slot {slot} (persistent updated too)");
            return true;
        }

        /// <summary>
        /// Force save to specific slot with safety checks
        /// </summary>
        public bool ForceSaveToSlot(int slot)
        {
            if (!EnsureSaveSlot(slot))
            {
                return false;
            }

            // Ensure save data has correct slot
            if (currentSaveData != null)
            {
                currentSaveData.saveSlot = slot;
            }

            if (showDebug) Debug.Log($"★ ForceSaveToSlot: Saving to slot {slot}");
            return SaveToFile();
        }

        /// <summary>
        /// Force load from specific slot with safety checks
        /// </summary>
        public bool ForceLoadFromSlot(int slot)
        {
            if (!EnsureSaveSlot(slot))
            {
                return false;
            }

            if (showDebug) Debug.Log($"★ ForceLoadFromSlot: Loading from slot {slot}");
            return LoadFromSlot(slot);
        }

        /// <summary>
        /// Explicitly load save data (for in-game use)
        /// Call this when actually entering game scene
        /// </summary>
        public bool ExplicitLoadGame()
        {
            if (showDebug) Debug.Log($"★ ExplicitLoadGame called for slot {currentSaveSlot}");

            // Don't load if we're creating a new game
            if (isCreatingNewGame)
            {
                if (showDebug) Debug.Log("★ Skipping explicit load - creating new game");
                return false;
            }

            return LoadGame();
        }

        /// <summary>
        /// Check if SaveManager should be active in current scene
        /// </summary>
        public bool ShouldBeActiveInCurrentScene()
        {
            string currentScene = SceneManager.GetActiveScene().name;

            // Don't be active in main menu scenes
            if (currentScene.Contains("MainMenu") || currentScene.Contains("Menu"))
            {
                return false;
            }

            // Be active in game scenes
            return true;
        }
        
               // Debug GUI
#if UNITY_EDITOR
        private void OnGUI()
        {
            if (!enableDebugGUI) return;

            GUILayout.BeginArea(new Rect(10, 10, 400, 450));
            GUILayout.Label("=== SAVE SYSTEM DEBUG ===");

            GUILayout.Label($"Current Slot: {currentSaveSlot}");
            GUILayout.Label($"Current Checkpoint: {(currentCheckpoint != null ? currentCheckpoint.checkpointName : "None")}");
            GUILayout.Label($"Checkpoint Library: {(checkpointLibrary != null ? "Assigned" : "None")}");
            GUILayout.Label($"Player Dead State: {isPlayerDead} {(isPlayerDead ? "(Auto-save disabled)" : "")}");

            // Show current player position
            if (player != null)
            {
                GUILayout.Label($"Current Player Pos: {player.transform.position:F1}");
            }

            // Show saved position
            if (currentSaveData != null && currentSaveData.playerData != null)
            {
                GUILayout.Label($"Saved Player Pos: {currentSaveData.playerData.position:F1}");
            }

            if (checkpointLibrary != null)
            {
                // Try to get checkpoint count using reflection
                try
                {
                    var countProperty = checkpointLibrary.GetType().GetProperty("CheckpointCount");
                    if (countProperty != null)
                    {
                        int count = (int)countProperty.GetValue(checkpointLibrary);
                        GUILayout.Label($"Library Checkpoints: {count}");
                    }
                }
                catch
                {
                    GUILayout.Label("Library Checkpoints: Unknown");
                }
            }
            GUILayout.Label($"Save Directory: {saveDirectory}");

            GUILayout.Space(10);

            // Save operations
            if (GUILayout.Button("Quick Save"))
            {
                QuickSave();
            }

            if (GUILayout.Button("Quick Load"))
            {
                LoadGame();
            }

            if (GUILayout.Button("FORCE Load Test"))
            {
                DebugForceLoadTest();
            }

            if (GUILayout.Button("Create New Save"))
            {
                CreateNewSave();
            }

            GUILayout.Space(10);

            // Respawn operations
            if (GUILayout.Button("Respawn at Checkpoint"))
            {
                RespawnAtLastCheckpoint();
            }

            GUILayout.Space(10);

            // Slot operations
            GUILayout.Label("Save Slots:");
            for (int i = 0; i < maxSaveSlots; i++)
            {
                GUILayout.BeginHorizontal();

                bool exists = SaveFileExists(i);
                string slotLabel = $"Slot {i} {(exists ? "(Exists)" : "(Empty)")} {(i == currentSaveSlot ? "*" : "")}";

                if (GUILayout.Button(slotLabel))
                {
                    if (exists)
                    {
                        LoadFromSlot(i);
                    }
                    else
                    {
                        currentSaveSlot = i;
                        CreateNewSave();
                    }
                }

                if (exists && GUILayout.Button("Delete", GUILayout.Width(60)))
                {
                    DeleteSaveFile(i);
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.EndArea();
        }
#endif
    }
}
