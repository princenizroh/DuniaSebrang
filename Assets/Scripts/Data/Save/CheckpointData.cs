using UnityEngine;
using Game.Core;
using DS.Data.Audio;

namespace DS.Data.Save
{
    [CreateAssetMenu(fileName = "NewCheckpointData", menuName = "Game Data/Save/Checkpoint Data")]
    public class CheckpointData : BaseDataObject
    {
        [Header("=== CHECKPOINT INFO ===")]
        [Tooltip("Nama checkpoint untuk identifikasi")]
        public string checkpointName;
        
        [Tooltip("Deskripsi checkpoint")]
        [TextArea(2, 4)]
        public string description;
        
        [Header("=== SPAWN DATA ===")]
        [Tooltip("Posisi spawn player")]
        public Vector3 spawnPosition;
        
        [Tooltip("Rotasi spawn player")]
        public Vector3 spawnRotation;
        
        [Tooltip("Scene name checkpoint ini berada")]
        public string sceneName;
        
        [Header("=== CHECKPOINT AUDIO ===")]
        [Tooltip("Audio yang akan diplay saat checkpoint triggered (opsional)")]
        public AudioData saveAudioData;

        [Header("=== AREA INFO ===")]
        [Tooltip("Nama area untuk display di save slot (e.g., Prison, Forest, Cave)")]
        public string areaName;
        
        [Header("=== SAVE INFO ===")]
        [Tooltip("Waktu bermain terakhir saat save di checkpoint ini (dalam detik)")]
        public float lastSavePlayTime;
        
        [Tooltip("Tanggal dan waktu terakhir save di checkpoint ini")]
        public string lastSaveDateTime;

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Auto-set scene name jika kosong
            if (string.IsNullOrEmpty(sceneName))
            {
                sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            }
            
            // Auto-set checkpoint name dari asset name
            if (string.IsNullOrEmpty(checkpointName))
            {
                checkpointName = name.Replace("_", " ");
            }
        }
        
        [ContextMenu("Set Current Transform as Spawn Point")]
        private void SetCurrentTransformAsSpawn()
        {
            if (UnityEditor.Selection.activeGameObject != null)
            {
                Transform selectedTransform = UnityEditor.Selection.activeGameObject.transform;
                spawnPosition = selectedTransform.position;
                spawnRotation = selectedTransform.eulerAngles;
                
                // Mark dirty to save changes
                UnityEditor.EditorUtility.SetDirty(this);
                
                Debug.Log($"★ Checkpoint spawn point set to: {spawnPosition}, Rotation: {spawnRotation}");
            }
            else
            {
                Debug.LogWarning("No GameObject selected! Select a GameObject in scene first.");
            }
        }
        
        /// <summary>
        /// Auto-find CheckpointTrigger in scene and sync position
        /// </summary>
        [ContextMenu("Sync with CheckpointTrigger")]
        private void SyncWithCheckpointTrigger()
        {
            // Find CheckpointTrigger that references this CheckpointData
            CheckpointTrigger[] triggers = UnityEngine.Object.FindObjectsByType<CheckpointTrigger>(FindObjectsSortMode.None);
            
            foreach (var trigger in triggers)
            {
                // Use reflection to check if this trigger references this checkpoint data
                var field = trigger.GetType().GetField("checkpointData", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (field != null)
                {
                    var referencedData = field.GetValue(trigger) as CheckpointData;
                    if (referencedData == this)
                    {
                        spawnPosition = trigger.transform.position;
                        spawnRotation = trigger.transform.eulerAngles;
                        UnityEditor.EditorUtility.SetDirty(this);
                        
                        Debug.Log($"★ Synced with CheckpointTrigger '{trigger.name}': {spawnPosition}");
                        return;
                    }
                }
            }
            
            Debug.LogWarning("No CheckpointTrigger found that references this CheckpointData!");
        }
#endif
    }
}
