using UnityEngine;
using DS.Data.Save;
using DS.Data.Audio;

namespace DS
{
    /// <summary>
    /// Checkpoint trigger that saves game when player enters
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class CheckpointTrigger : MonoBehaviour
    {
        [Header("=== CHECKPOINT REFERENCE ===")]
        [Tooltip("Checkpoint data asset untuk checkpoint ini")]
        [SerializeField] private CheckpointData checkpointData;
        
        [Header("=== TRIGGER SETTINGS ===")]
        [Tooltip("Tag player yang akan trigger checkpoint")]
        [SerializeField] private string playerTag = "Player";
        
        [Tooltip("Hanya trigger sekali per session")]
        [SerializeField] private bool triggerOnce = false;
        
        [Tooltip("Auto-save saat triggered")]
        [SerializeField] private bool autoSave = true;
        
        [Tooltip("Show save confirmation UI")]
        [SerializeField] private bool showSaveConfirmation = true;
        
        [Header("=== VISUAL FEEDBACK ===")]
        [Tooltip("Particle effect saat save")]
        [SerializeField] private ParticleSystem saveParticles;
        
        [Tooltip("Audio source untuk save sound")]
        [SerializeField] private AudioSource audioSource;
        
        [Tooltip("Visual indicator checkpoint")]
        [SerializeField] private GameObject visualIndicator;
        
        [Header("=== DEBUG ===")]
        [Tooltip("Show debug messages")]
        [SerializeField] private bool showDebug = true;
        
        // Runtime state
        private bool hasBeenTriggered = false;
        private SaveManager saveManager;
        private Collider triggerCollider;
        
        private void Awake()
        {
            // Get components
            triggerCollider = GetComponent<Collider>();
            triggerCollider.isTrigger = true;
            
            // Find save manager
            saveManager = FindFirstObjectByType<SaveManager>();
            if (saveManager == null && showDebug)
                Debug.LogWarning($"CheckpointTrigger: No SaveManager found in scene!");
                
            // Setup audio source
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
        }
        
        private void Start()
        {
            // Validate checkpoint data
            if (checkpointData == null)
            {
                Debug.LogError($"CheckpointTrigger: No checkpoint data assigned to {gameObject.name}!");
                return;
            }
            
            // Set position dari checkpoint data jika tidak diset manual
            if (checkpointData.spawnPosition == Vector3.zero)
            {
                checkpointData.spawnPosition = transform.position;
                checkpointData.spawnRotation = transform.eulerAngles;
                if (showDebug) Debug.Log($"Auto-set checkpoint position: {transform.position}");
            }
            
            // Setup visual indicator
            SetupVisualIndicator();
        }
        
        private void OnTriggerEnter(Collider other)
        {
            // Check if it's player
            if (!other.CompareTag(playerTag))
                return;
                
            // Check if already triggered (if triggerOnce is enabled)
            if (triggerOnce && hasBeenTriggered)
                return;
                
            // Trigger checkpoint
            TriggerCheckpoint(other.gameObject);
        }
        
        private void TriggerCheckpoint(GameObject player)
        {
            if (showDebug) 
                Debug.Log($"★★★ CHECKPOINT TRIGGERED: {checkpointData.checkpointName} ★★★");
            
            // Mark as triggered
            hasBeenTriggered = true;
            
            // Play visual/audio effects
            PlaySaveEffects();
            
            // Save game through SaveManager
            if (saveManager != null && autoSave)
            {
                saveManager.SaveGameAtCheckpoint(checkpointData);
                
                if (showSaveConfirmation)
                {
                    // TODO: Show save confirmation UI
                    Debug.Log($"Game saved at checkpoint: {checkpointData.checkpointName}");
                }
            }
            else
            {
                if (showDebug) Debug.LogWarning("CheckpointTrigger: Cannot auto-save (SaveManager not found or autoSave disabled)");
            }
            
            // Update visual state
            UpdateVisualState();
        }
        
        private void PlaySaveEffects()
        {
            // Play particle effect
            if (saveParticles != null)
            {
                saveParticles.Play();
                if (showDebug) Debug.Log("Save particles played");
            }
            
            // Play save audio using AudioData
            if (audioSource != null && checkpointData.saveAudioData != null)
            {
                audioSource.clip = checkpointData.saveAudioData.audioClip;
                audioSource.volume = checkpointData.saveAudioData.volume;
                audioSource.Play();
                if (showDebug) Debug.Log($"Save audio played: {checkpointData.saveAudioData.AudioName}");
            }
        }
        
        private void SetupVisualIndicator()
        {
            // Setup visual state (no more automatic visual from checkpoint data)
            UpdateVisualState();
        }
        
        private void UpdateVisualState()
        {
            if (visualIndicator == null) return;
            
            // Change visual based on triggered state
            if (hasBeenTriggered)
            {
                // Checkpoint has been activated
                Color indicatorColor = Color.green;
                Renderer renderer = visualIndicator.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = indicatorColor;
                }
            }
        }
        
        /// <summary>
        /// Manual trigger for testing
        /// </summary>
        [ContextMenu("Manual Trigger")]
        public void ManualTrigger()
        {
            GameObject player = GameObject.FindGameObjectWithTag(playerTag);
            if (player != null)
            {
                TriggerCheckpoint(player);
            }
            else
            {
                Debug.LogWarning("No player found for manual trigger!");
            }
        }
        
        /// <summary>
        /// Reset checkpoint trigger state
        /// </summary>
        [ContextMenu("Reset Trigger")]
        public void ResetTrigger()
        {
            hasBeenTriggered = false;
            UpdateVisualState();
            if (showDebug) Debug.Log("Checkpoint trigger reset");
        }
        
        /// <summary>
        /// Get checkpoint world position
        /// </summary>
        public Vector3 GetCheckpointPosition()
        {
            return checkpointData != null ? checkpointData.spawnPosition : transform.position;
        }
        
        /// <summary>
        /// Get checkpoint world rotation
        /// </summary>
        public Vector3 GetCheckpointRotation()
        {
            return checkpointData != null ? checkpointData.spawnRotation : transform.eulerAngles;
        }
        
        // Properties
        public CheckpointData CheckpointData => checkpointData;
        public bool HasBeenTriggered => hasBeenTriggered;
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            // Auto-setup trigger collider
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.isTrigger = true;
            }
        }
        
        private void OnDrawGizmos()
        {
            // Draw checkpoint gizmo
            Gizmos.color = hasBeenTriggered ? Color.green : Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 1f);
            
            // Draw spawn position if different
            if (checkpointData != null && checkpointData.spawnPosition != Vector3.zero)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireCube(checkpointData.spawnPosition, Vector3.one * 0.5f);
                Gizmos.DrawLine(transform.position, checkpointData.spawnPosition);
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            // Draw detailed gizmo when selected
            if (checkpointData != null)
            {
                UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, 
                    $"Checkpoint: {checkpointData.checkpointName}");
            }
        }
#endif
    }
}
