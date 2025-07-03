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
        [SerializeField] private float autoSaveInterval = 300f; // 5 minutes
        
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
        private string saveDirectory;
        
        // Death state tracking
        private bool isPlayerDead = false;

        // Events
        public event Action<SaveData> OnGameSaved;
        public event Action<SaveData> OnGameLoaded;
        public event Action<CheckpointData> OnCheckpointActivated;
        
        // Singleton pattern
        public static SaveManager Instance { get; private set; }
        
        private void Awake()
        {
            // Singleton setup
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
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
            
            // Load existing save or create new
            LoadOrCreateSave();
        }
        
        private void Update()
        {
            // Auto save check
            if (autoSaveInterval > 0 && Time.time - lastAutoSaveTime >= autoSaveInterval)
            {
                AutoSave();
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
            
            if (showDebug) Debug.Log($"Saving game at checkpoint: {checkpoint.checkpointName}");
            
            // Update current checkpoint
            currentCheckpoint = checkpoint;
            
            // Gather save data
            GatherSaveData();
            
            // Update checkpoint data
            UpdateCheckpointSaveData(checkpoint);
            
            // Save to file
            bool saveSuccess = SaveToFile();
            
            if (saveSuccess)
            {
                OnGameSaved?.Invoke(currentSaveData);
                OnCheckpointActivated?.Invoke(checkpoint);
                
                if (showDebug) Debug.Log($"★ Game saved successfully at {checkpoint.checkpointName}");
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
            if (showDebug) Debug.Log("Quick Save: Gathering current player position...");
            GatherCurrentPlayerPosition();
            
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
            
            bool success = QuickSave();
            
            if (!success)
            {
                currentSaveSlot = previousSlot;
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
                
                if (showDebug) Debug.Log($"★ LOADING from file: {filePath}");
                if (showDebug) Debug.Log($"★ JSON content preview: {json.Substring(0, Mathf.Min(200, json.Length))}...");
                
                currentSaveData = JsonUtility.FromJson<SaveData>(json);
                currentSaveSlot = slot;
                
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
        /// Load or create new save
        /// </summary>
        private void LoadOrCreateSave()
        {
            if (!LoadGame())
            {
                // Create new save with starting checkpoint
                CreateNewSave();
            }
        }
        
        /// <summary>
        /// Create new save file
        /// </summary>
        public void CreateNewSave()
        {
            if (showDebug) Debug.Log("Creating new save file...");
            
            currentSaveData = new SaveData();
            currentSaveData.saveSlot = currentSaveSlot;
            
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
                            currentCheckpoint = startingCheckpoint;
                            SaveGameAtCheckpoint(startingCheckpoint);
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
            
            if (showDebug) Debug.Log($"Respawning at checkpoint: {checkpoint.checkpointName}");
            
            // Load scene if different
            if (!string.IsNullOrEmpty(checkpoint.sceneName) && 
                SceneManager.GetActiveScene().name != checkpoint.sceneName)
            {
                SceneManager.LoadScene(checkpoint.sceneName);
            }
            
            // DON'T reset player state here - it will interrupt fade-out
            // The PlayerDeathHandler will handle the reset after fade-out is complete
            // if (playerDeathHandler != null)
            // {
            //     playerDeathHandler.ResetPlayer();
            // }
            
            // Move player to checkpoint position FIRST
            if (player != null)
            {
                player.transform.position = checkpoint.spawnPosition;
                player.transform.eulerAngles = checkpoint.spawnRotation;
                
                if (showDebug) Debug.Log($"Player moved to checkpoint position: {checkpoint.spawnPosition}");
            }
            
            // Update current checkpoint
            currentCheckpoint = checkpoint;
            
            if (showDebug) Debug.Log("★ Respawn positioning complete - PlayerDeathHandler will handle state reset after fade-out");
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
            
            // Add to activated checkpoints if not already present
            if (!currentSaveData.checkpointData.activatedCheckpoints.Contains(checkpoint.Id))
            {
                currentSaveData.checkpointData.activatedCheckpoints.Add(checkpoint.Id);
            }
        }
        
        /// <summary>
        /// Apply loaded data to game state
        /// </summary>
        private void ApplyLoadedData()
        {
            if (showDebug) Debug.Log("★ APPLYING LOADED DATA ★");
            
            // Find and set current checkpoint
            if (!string.IsNullOrEmpty(currentSaveData.checkpointData.lastCheckpointId))
            {
                currentCheckpoint = GetCheckpointById(currentSaveData.checkpointData.lastCheckpointId);
                if (showDebug) Debug.Log($"Loaded checkpoint: {currentCheckpoint?.checkpointName}");
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
            
            if (showDebug) Debug.Log("★ Loaded data applied successfully");
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
                
                string json = JsonUtility.ToJson(currentSaveData, true);
                File.WriteAllText(filePath, json);
                
                if (showDebug) Debug.Log($"Save file written: {filePath}");
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
            return $"{saveFilePrefix}_Slot{slot:00}.json";
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
        
        // Debug GUI
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
        
        #endregion

        /// <summary>
        /// Set death state to prevent auto-save during death/respawn
        /// </summary>
        public void SetDeathState(bool isDead)
        {
            isPlayerDead = isDead;
            if (showDebug) Debug.Log($"SaveManager death state set to: {isDead}");
        }
    }
}
