using UnityEngine;

namespace DS
{
    /// <summary>
    /// Handles player death logic, animations, and state management.
    /// This script should be attached to the player GameObject.
    /// </summary>
    public class PlayerDeathHandler : MonoBehaviour
    {
        [Header("=== DEATH ANIMATION ===")]
        [Tooltip("Animator component untuk player death animation")]
        [SerializeField] private Animator playerAnimator;
        
        [Tooltip("Nama state animation untuk death")]
        [SerializeField] private string deathAnimationState = "FlyingBack";
        
        [Tooltip("Durasi death animation (detik)")]
        [SerializeField] private float deathAnimationDuration = 3f;
        
        [Header("=== DEATH BEHAVIOR ===")]
        [Tooltip("Apakah player sudah mati")]
        [SerializeField] private bool isDead = false;
        
        [Tooltip("Disable player movement saat mati")]
        [SerializeField] private bool disableMovementOnDeath = true;
        
        [Tooltip("Disable player input saat mati")]
        [SerializeField] private bool disableInputOnDeath = true;

        [Header("=== VISUAL EFFECTS ===")]
        [Tooltip("Screen effect atau UI untuk death")]
        [SerializeField] private GameObject deathScreenUI;
        
        [Tooltip("Death screen effect component (will be auto-found if not assigned)")]
        [SerializeField] private DeathScreenEffect deathScreenEffect;
        
        [Header("=== DEBUG ===")]
        [Tooltip("Show debug messages")]
        [SerializeField] private bool showDebug = true;
        
        // Runtime variables
        private float deathStartTime;
        private bool deathAnimationPlaying = false;
        
        // Component references
        private Rigidbody playerRigidbody;
        private Collider playerCollider;
        private MonoBehaviour[] playerScripts;
        
        // Properties
        public bool IsDead => isDead;
        public bool IsDeathAnimationPlaying => deathAnimationPlaying;
        
        private void Awake()
        {
            // Get component references
            if (playerAnimator == null)
                playerAnimator = GetComponent<Animator>();
            
            // Auto-find death screen effect if not assigned
            if (deathScreenEffect == null)
            {
                // Try to find DeathScreenEffect component
                GameObject deathEffectObj = GameObject.Find("DeathScreenEffect");
                if (deathEffectObj != null)
                    deathScreenEffect = deathEffectObj.GetComponent<DeathScreenEffect>();
            }
        
            playerRigidbody = GetComponent<Rigidbody>();
            playerCollider = GetComponent<Collider>();
            
            // Get all player scripts for disabling (optional)
            playerScripts = GetComponents<MonoBehaviour>();
            
            // Validate components
            if (playerAnimator == null && showDebug)
                Debug.LogWarning($"PlayerDeathHandler: No Animator found on {gameObject.name}");
        }
        
        private void Start()
        {
        }
        
        private void Update()
        {
            // Check death animation progress
            if (deathAnimationPlaying && !isDead)
            {
                CheckDeathAnimationProgress();
            }
        }
        
        /// <summary>
        /// Main method to kill the player - call this from external scripts
        /// </summary>
        public void Die()
        {
            if (isDead)
            {
                if (showDebug) Debug.LogWarning("PlayerDeathHandler: Player is already dead!");
                return;
            }
            
            if (showDebug) 
            {
                Debug.Log("★★★ PLAYER DEATH TRIGGERED! ★★★");
                Debug.Log($"[DEATH DEBUG] Die() called from: {System.Environment.StackTrace}");
            }
            
            // Set death state
            isDead = true;
            deathStartTime = Time.time;
            
            // Play death animation
            PlayDeathAnimation();
            
            // Disable player controls and movement
            DisablePlayerControls();
            
            // Show death UI
            ShowDeathUI();
            
            if (showDebug) Debug.Log("Player death sequence initiated");
        }
        
        /// <summary>
        /// Alternative method to die with specific cause
        /// </summary>
        public void Die(string cause)
        {
            if (showDebug) 
            {
                Debug.Log($"★★★ PLAYER DIED FROM: {cause} ★★★");
                Debug.Log($"[DEATH DEBUG] Die(cause) called from: {System.Environment.StackTrace}");
            }
            Die();
        }
        
        private void PlayDeathAnimation()
        {
            if (playerAnimator == null)
            {
                if (showDebug) Debug.LogWarning("PlayerDeathHandler: No animator to play death animation");
                return;
            }
            
            if (showDebug) Debug.Log($"Playing death animation: {deathAnimationState}");
            
            try
            {
                // Play death animation directly by name
                if (!string.IsNullOrEmpty(deathAnimationState))
                {
                    playerAnimator.Play(deathAnimationState);
                    deathAnimationPlaying = true;
                    if (showDebug) Debug.Log("Death animation played successfully");
                }
                else
                {
                    if (showDebug) Debug.LogWarning("PlayerDeathHandler: Death animation state name is empty");
                }
            }
            catch (System.Exception e)
            {
                if (showDebug) Debug.LogError($"PlayerDeathHandler: Error playing death animation: {e.Message}");
            }
        }
        

        private void DisablePlayerControls()
        {
            if (disableMovementOnDeath)
            {
                // Stop player movement
                if (playerRigidbody != null)
                {
                    playerRigidbody.linearVelocity = Vector3.zero;
                    playerRigidbody.angularVelocity = Vector3.zero;
                    playerRigidbody.isKinematic = true; // Prevent physics movement
                }
                
                if (showDebug) Debug.Log("Player movement disabled");
            }
            
            if (disableInputOnDeath)
            {
                // Disable specific player control scripts
                DisablePlayerScripts();
                
                if (showDebug) Debug.Log("Player input disabled");
            }
        }
        
        private void DisablePlayerScripts()
        {
            // List of common player script names to disable
            string[] scriptsToDisable = {
                "PlayerController",
                "PlayerMovement", 
                "FirstPersonController",
                "ThirdPersonController",
                "PlayerInput",
                "PlayerInteraction",
                "MouseLook",
                "CameraController"
            };
            
            foreach (string scriptName in scriptsToDisable)
            {
                Component script = GetComponent(scriptName);
                if (script != null && script is MonoBehaviour)
                {
                    ((MonoBehaviour)script).enabled = false;
                    if (showDebug) Debug.Log($"Disabled script: {scriptName}");
                }
            }
            
        }

        private void ShowDeathUI()
        {
            if (showDebug) Debug.Log("Showing death UI and effects...");
            
            // Trigger death screen effect (Little Nightmares style fade)
            if (deathScreenEffect != null)
            {
                // Call TriggerDeathFade method directly
                deathScreenEffect.TriggerDeathFade();
                if (showDebug) Debug.Log("★ Death screen fade effect triggered!");
            }
            else
            {
                if (showDebug) Debug.LogWarning("No death screen effect assigned - create UI with DeathScreenEffect component");
            }
            
        }
        
        private void CheckDeathAnimationProgress()
        {
            if (playerAnimator == null) return;
            
            // Check if death animation is still playing
            AnimatorStateInfo stateInfo = playerAnimator.GetCurrentAnimatorStateInfo(0);
            
            // Check by animation name or use timer fallback
            bool animationFinished = false;
            
            if (!string.IsNullOrEmpty(deathAnimationState))
            {
                // Check if we're in death state and animation is complete
                if (stateInfo.IsName(deathAnimationState))
                {
                    animationFinished = stateInfo.normalizedTime >= 1.0f;
                }
                else if (Time.time - deathStartTime >= deathAnimationDuration)
                {
                    // Fallback timer
                    animationFinished = true;
                }
            }
            else
            {
                // Use timer only
                animationFinished = Time.time - deathStartTime >= deathAnimationDuration;
            }
            
            if (animationFinished)
            {
                deathAnimationPlaying = false;
                OnDeathAnimationComplete();
            }
        }
        
        private void OnDeathAnimationComplete()
        {
            if (showDebug) Debug.Log("Death animation completed");
            
            // Death animation finished - ready for restart/checkpoint logic
            // This is where future checkpoint/restart logic will go
            
            // For now, just log completion
            if (showDebug) Debug.Log("=== READY FOR RESTART/CHECKPOINT LOGIC ===");
        }
        
        /// <summary>
        /// Method to reset player state (for future restart logic)
        /// </summary>
        public void ResetPlayer()
        {
            if (showDebug) Debug.Log("Resetting player state...");
            
            isDead = false;
            deathAnimationPlaying = false;
            
            // Reset death screen effect
            if (deathScreenEffect != null)
            {
                deathScreenEffect.ResetDeathEffect();
                if (showDebug) Debug.Log("Death screen effect reset");
            }
            // Re-enable player controls
            if (playerRigidbody != null)
                playerRigidbody.isKinematic = false;
            
            // Re-enable player scripts
            EnablePlayerScripts();
            
            // Reset animator
            if (playerAnimator != null)
                playerAnimator.Rebind();
            
            if (showDebug) Debug.Log("Player reset completed");
        }
        
        private void EnablePlayerScripts()
        {
            // Re-enable previously disabled scripts
            string[] scriptsToEnable = {
                "PlayerController",
                "PlayerMovement", 
                "FirstPersonController",
                "ThirdPersonController",
                "PlayerInput",
                "PlayerInteraction",
                "MouseLook",
                "CameraController"
            };
            
            foreach (string scriptName in scriptsToEnable)
            {
                Component script = GetComponent(scriptName);
                if (script != null && script is MonoBehaviour)
                {
                    ((MonoBehaviour)script).enabled = true;
                    if (showDebug) Debug.Log($"Re-enabled script: {scriptName}");
                }
            }
        }
        
        /// <summary>
        /// Check if player can die (for external validation)
        /// </summary>
        public bool CanDie()
        {
            return !isDead;
        }
        
        /// <summary>
        /// Force stop death animation (emergency)
        /// </summary>
        public void StopDeathAnimation()
        {
            deathAnimationPlaying = false;
            if (showDebug) Debug.Log("Death animation force stopped");
        }
        
        /// <summary>
        /// Debug method to test death from inspector
        /// </summary>
        [ContextMenu("Test Death")]
        private void TestDeath()
        {
            Die("Debug Test");
        }
        
        /// <summary>
        /// Debug method to test reset from inspector
        /// </summary>
        [ContextMenu("Test Reset")]
        private void TestReset()
        {
            ResetPlayer();
        }
        
        // Debug GUI
        private void OnGUI()
        {
            if (!showDebug) return;
            
            GUILayout.BeginArea(new Rect(10, 450, 300, 200));
            GUILayout.Label("=== PLAYER DEATH DEBUG ===");
            GUILayout.Label($"Is Dead: {isDead}");
            GUILayout.Label($"Death Animation Playing: {deathAnimationPlaying}");
            
            if (isDead)
            {
                float timeSinceDeath = Time.time - deathStartTime;
                GUILayout.Label($"Time Since Death: {timeSinceDeath:F1}s");
                GUILayout.Label($"Animation Duration: {deathAnimationDuration:F1}s");
            }
            
            if (GUILayout.Button("Test Death"))
            {
                Die("Manual Test");
            }
            
            if (GUILayout.Button("Reset Player"))
            {
                ResetPlayer();
            }
            
            GUILayout.EndArea();
        }
    }
}
