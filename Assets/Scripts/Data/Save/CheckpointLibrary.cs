using System.Collections.Generic;
using UnityEngine;
using Game.Core;

namespace DS.Data.Save
{
    [CreateAssetMenu(fileName = "CheckpointLibrary", menuName = "Game Data/Save/Checkpoint Library")]
    public class CheckpointLibrary : BaseDataObject
    {
        [Header("=== CHECKPOINT COLLECTION ===")]
        [Tooltip("Semua checkpoint data dalam game")]
        [SerializeField] private List<CheckpointData> allCheckpoints = new List<CheckpointData>();
        
        [Header("=== STARTING SETTINGS ===")]
        [Tooltip("Default starting checkpoint (first time game)")]
        [SerializeField] private CheckpointData defaultStartingCheckpoint;
        
        [Header("=== LIBRARY INFO ===")]
        [Tooltip("Nama library untuk identifikasi")]
        public string libraryName = "Main Checkpoint Library";
        
        [Tooltip("Deskripsi library")]
        [TextArea(2, 4)]
        public string description = "Collection of all checkpoints in the game";
        
        // Properties untuk easy access
        public List<CheckpointData> AllCheckpoints => allCheckpoints;
        public CheckpointData DefaultStartingCheckpoint => defaultStartingCheckpoint;
        public int CheckpointCount => allCheckpoints.Count;
        
        /// <summary>
        /// Get checkpoint by ID
        /// </summary>
        public CheckpointData GetCheckpointById(string id)
        {
            foreach (CheckpointData checkpoint in allCheckpoints)
            {
                if (checkpoint != null && checkpoint.Id == id)
                    return checkpoint;
            }
            return null;
        }
        
        /// <summary>
        /// Get checkpoint by name
        /// </summary>
        public CheckpointData GetCheckpointByName(string name)
        {
            foreach (CheckpointData checkpoint in allCheckpoints)
            {
                if (checkpoint != null && checkpoint.checkpointName == name)
                    return checkpoint;
            }
            return null;
        }
        
        /// <summary>
        /// Add checkpoint to library
        /// </summary>
        public void AddCheckpoint(CheckpointData checkpoint)
        {
            if (checkpoint != null && !allCheckpoints.Contains(checkpoint))
            {
                allCheckpoints.Add(checkpoint);
                Debug.Log($"Added checkpoint to library: {checkpoint.checkpointName}");
            }
        }
        
        /// <summary>
        /// Remove checkpoint from library
        /// </summary>
        public void RemoveCheckpoint(CheckpointData checkpoint)
        {
            if (checkpoint != null && allCheckpoints.Contains(checkpoint))
            {
                allCheckpoints.Remove(checkpoint);
                Debug.Log($"Removed checkpoint from library: {checkpoint.checkpointName}");
            }
        }
        
        /// <summary>
        /// Get checkpoints by scene name
        /// </summary>
        public List<CheckpointData> GetCheckpointsByScene(string sceneName)
        {
            List<CheckpointData> sceneCheckpoints = new List<CheckpointData>();
            
            foreach (CheckpointData checkpoint in allCheckpoints)
            {
                if (checkpoint != null && checkpoint.sceneName == sceneName)
                {
                    sceneCheckpoints.Add(checkpoint);
                }
            }
            
            return sceneCheckpoints;
        }
        
        /// <summary>
        /// Validate all checkpoints in library
        /// </summary>
        public bool ValidateLibrary()
        {
            bool isValid = true;
            
            for (int i = 0; i < allCheckpoints.Count; i++)
            {
                CheckpointData checkpoint = allCheckpoints[i];
                
                if (checkpoint == null)
                {
                    Debug.LogError($"CheckpointLibrary: Null checkpoint at index {i}");
                    isValid = false;
                    continue;
                }
                
                if (string.IsNullOrEmpty(checkpoint.checkpointName))
                {
                    Debug.LogError($"CheckpointLibrary: Checkpoint at index {i} has empty name");
                    isValid = false;
                }
                
                if (string.IsNullOrEmpty(checkpoint.sceneName))
                {
                    Debug.LogError($"CheckpointLibrary: Checkpoint '{checkpoint.checkpointName}' has empty scene name");
                    isValid = false;
                }
            }
            
            if (defaultStartingCheckpoint == null)
            {
                Debug.LogWarning("CheckpointLibrary: No default starting checkpoint assigned");
            }
            
            return isValid;
        }
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            // Auto-set library name from asset name
            if (string.IsNullOrEmpty(libraryName))
            {
                libraryName = name.Replace("_", " ");
            }
            
            // Remove null entries
            allCheckpoints.RemoveAll(item => item == null);
            
            // Auto-set default starting checkpoint if not assigned and we have checkpoints
            if (defaultStartingCheckpoint == null && allCheckpoints.Count > 0)
            {
                defaultStartingCheckpoint = allCheckpoints[0];
                Debug.Log($"Auto-assigned default starting checkpoint: {defaultStartingCheckpoint.checkpointName}");
            }
        }
        
        [ContextMenu("Validate Library")]
        private void ValidateLibraryInEditor()
        {
            bool isValid = ValidateLibrary();
            Debug.Log($"Checkpoint library validation: {(isValid ? "PASSED" : "FAILED")}");
        }
        
        [ContextMenu("Sort Checkpoints by Name")]
        private void SortCheckpointsByName()
        {
            allCheckpoints.Sort((a, b) => 
            {
                if (a == null && b == null) return 0;
                if (a == null) return -1;
                if (b == null) return 1;
                return string.Compare(a.checkpointName, b.checkpointName);
            });
            
            Debug.Log("Checkpoints sorted by name");
        }
        
        [ContextMenu("Log All Checkpoints")]
        private void LogAllCheckpoints()
        {
            Debug.Log($"=== CHECKPOINT LIBRARY: {libraryName} ===");
            Debug.Log($"Total Checkpoints: {allCheckpoints.Count}");
            Debug.Log($"Default Starting: {(defaultStartingCheckpoint != null ? defaultStartingCheckpoint.checkpointName : "None")}");
            
            for (int i = 0; i < allCheckpoints.Count; i++)
            {
                CheckpointData checkpoint = allCheckpoints[i];
                if (checkpoint != null)
                {
                    Debug.Log($"[{i}] {checkpoint.checkpointName} (Scene: {checkpoint.sceneName})");
                }
                else
                {
                    Debug.Log($"[{i}] NULL CHECKPOINT");
                }
            }
        }
#endif
    }
}
