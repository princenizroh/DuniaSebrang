using System;
using System.Collections.Generic;
using UnityEngine;

namespace DS.Data.Save
{
    [System.Serializable]
    public class SaveData
    {
        [Header("=== SAVE METADATA ===")]
        public string saveFileName;
        public DateTime saveTime;
        public string gameVersion = "1.0.0";
        public int saveSlot = 0;
        
        [Header("=== PLAY TIME TRACKING ===")]
        public float totalPlayTime = 0f; // Total play time in seconds
        public DateTime lastCheckpointTime; // When last checkpoint was reached
        
        [Header("=== PLAYER DATA ===")]
        public PlayerSaveData playerData;
        
        [Header("=== CHECKPOINT DATA ===")]
        public CheckpointSaveData checkpointData;
        
        [Header("=== GAME PROGRESS ===")]
        public GameProgressData gameProgress;
        
        [Header("=== COLLECTIBLES ===")]
        public CollectiblesSaveData collectibles;
        
        [Header("=== SETTINGS ===")]
        public GameSettingsData settings;
        
        public SaveData()
        {
            playerData = new PlayerSaveData();
            checkpointData = new CheckpointSaveData();
            gameProgress = new GameProgressData();
            collectibles = new CollectiblesSaveData();
            settings = new GameSettingsData();
            saveTime = DateTime.Now;
            lastCheckpointTime = DateTime.Now;
            totalPlayTime = 0f;
        }
        
        public void UpdateSaveTime()
        {
            saveTime = DateTime.Now;
        }
    }
    
    [System.Serializable]
    public class PlayerSaveData
    {
        [Header("=== POSITION ===")]
        public Vector3 position = Vector3.zero;
        public Vector3 rotation = Vector3.zero;
        public string currentScene = "";
        
        [Header("=== STATS ===")]
        public float health = 100f;
        public float maxHealth = 100f;
        public float stamina = 100f;
        public float maxStamina = 100f;
        
        [Header("=== PLAYER STATE ===")]
        public bool isDead = false;
        public float playTime = 0f;
        public int deathCount = 0;
        
        public PlayerSaveData()
        {
            // Constructor untuk PlayerSaveData
        }
    }
    
    [System.Serializable]
    public class CheckpointSaveData
    {
        public string lastCheckpointId = "";
        public string lastCheckpointName = "";
        public Vector3 lastCheckpointPosition = Vector3.zero;
        public Vector3 lastCheckpointRotation = Vector3.zero;
        public string lastCheckpointScene = "";
        public DateTime lastCheckpointTime;
        public List<string> activatedCheckpoints = new List<string>();
        
        public CheckpointSaveData()
        {
            activatedCheckpoints = new List<string>();
            lastCheckpointTime = DateTime.Now;
        }
    }
    
    [System.Serializable]
    public class GameProgressData
    {
        [Header("=== LEVEL PROGRESS ===")]
        public List<string> completedLevels = new List<string>();
        public List<string> unlockedAreas = new List<string>();
        public string currentLevel = "";
        public float levelProgress = 0f;
        
        [Header("=== STORY PROGRESS ===")]
        public List<string> triggeredCutscenes = new List<string>();
        public List<string> completedDialogues = new List<string>();
        public List<string> discoveredSecrets = new List<string>();
        
        [Header("=== ACHIEVEMENTS ===")]
        public List<string> unlockedAchievements = new List<string>();
        
        public GameProgressData()
        {
            completedLevels = new List<string>();
            unlockedAreas = new List<string>();
            triggeredCutscenes = new List<string>();
            completedDialogues = new List<string>();
            discoveredSecrets = new List<string>();
            unlockedAchievements = new List<string>();
        }
    }
    
    [System.Serializable]
    public class CollectiblesSaveData
    {
        [Header("=== COLLECTIBLES ===")]
        public List<string> collectedItemIds = new List<string>();
        public Dictionary<string, bool> collectibleStates = new Dictionary<string, bool>();
        public int totalCollectibles = 0;
        public int collectedCount = 0;
        
        [Header("=== AUDIO TRACKING ===")]
        public List<string> playedAudioDataIds = new List<string>();
        public Dictionary<string, bool> audioPlayStates = new Dictionary<string, bool>();
        
        public CollectiblesSaveData()
        {
            collectedItemIds = new List<string>();
            collectibleStates = new Dictionary<string, bool>();
            playedAudioDataIds = new List<string>();
            audioPlayStates = new Dictionary<string, bool>();
        }
        
        /// <summary>
        /// Mark collectible as collected
        /// </summary>
        public void CollectItem(string itemId)
        {
            if (!collectedItemIds.Contains(itemId))
            {
                collectedItemIds.Add(itemId);
                collectibleStates[itemId] = true;
                collectedCount++;
            }
        }
        
        /// <summary>
        /// Check if collectible is collected
        /// </summary>
        public bool IsItemCollected(string itemId)
        {
            return collectedItemIds.Contains(itemId);
        }
        
        /// <summary>
        /// Mark audio as played
        /// </summary>
        public void MarkAudioAsPlayed(string audioDataId)
        {
            if (!playedAudioDataIds.Contains(audioDataId))
            {
                playedAudioDataIds.Add(audioDataId);
                audioPlayStates[audioDataId] = true;
            }
        }
        
        /// <summary>
        /// Check if audio has been played
        /// </summary>
        public bool IsAudioPlayed(string audioDataId)
        {
            return playedAudioDataIds.Contains(audioDataId);
        }
    }
    
    [System.Serializable]
    public class GameSettingsData
    {
        [Header("=== AUDIO ===")]
        public float masterVolume = 1f;
        public float musicVolume = 1f;
        public float sfxVolume = 1f;
        
        [Header("=== GRAPHICS ===")]
        public int qualityLevel = 2;
        public bool fullscreen = true;
        public string resolution = "1920x1080";
        
        [Header("=== GAMEPLAY ===")]
        public float mouseSensitivity = 1f;
        public bool invertMouse = false;
        
        public GameSettingsData()
        {
            // Default values already set above
        }
    }
}
