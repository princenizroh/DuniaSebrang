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
                Debug.Log($"Checkpoint spawn point set to: {spawnPosition}");
            }
            else
            {
                Debug.LogWarning("No GameObject selected! Select a GameObject in scene first.");
            }
        }
#endif
    }
}
