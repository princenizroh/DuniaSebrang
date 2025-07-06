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

        [Header("=== SAVE SYSTEM ===")]
        [Tooltip("Auto respawn to checkpoint after death animation")]
        [SerializeField] private bool autoRespawnToCheckpoint = true;

        [Tooltip("Delay before respawn (after death animation)")]
        [SerializeField] private float respawnDelay = 2f;

        [Header("=== DEBUG ===")]
        [Tooltip("Show debug messages")]
        [SerializeField] private bool showDebug = true;

        // Runtime variables
        private float deathStartTime;
        private bool deathAnimationPlaying = false;
        private bool respawnScheduled = false;

        // Component references
        private Rigidbody playerRigidbody;
        private Collider playerCollider;
        private MonoBehaviour[] playerScripts;

        // Save system reference (will be found at runtime)
        private object saveManager; // Using object to avoid compile errors if SaveManager not found

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

            // Find SaveManager using reflection to avoid compile errors
            GameObject saveManagerObj = GameObject.Find("SaveManager");
            if (saveManagerObj != null)
                saveManager = saveManagerObj.GetComponent("SaveManager");

            if (saveManager == null && showDebug)
                Debug.LogWarning("PlayerDeathHandler: No SaveManager found in scene!");

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

            // IMPORTANT: Prevent any save operations during death to avoid overwriting checkpoint
            PreventAutoSaveDuringDeath();

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
        /// Prevent SaveManager from auto-saving during death to preserve checkpoint position
        /// </summary>
        private void PreventAutoSaveDuringDeath()
        {
            if (saveManager != null)
            {
                try
                {
                    // Call method to temporarily disable auto-save during death
                    var preventSaveMethod = saveManager.GetType().GetMethod("SetDeathState");
                    if (preventSaveMethod != null)
                    {
                        preventSaveMethod.Invoke(saveManager, new object[] { true });
                        if (showDebug) Debug.Log("★ Death state set in SaveManager - auto-save disabled");
                    }
                }
                catch (System.Exception e)
                {
                    if (showDebug) Debug.LogWarning($"Could not prevent auto-save during death: {e.Message}");
                }
            }
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

            // Trigger death screen effect with respawn callback
            if (deathScreenEffect != null)
            {
                // Pass respawn callback to death screen effect
                deathScreenEffect.TriggerDeathFade(OnRespawnRequested);
                if (showDebug) Debug.Log("★ Death screen fade effect triggered with respawn callback!");
            }
            else
            {
                if (showDebug) Debug.LogWarning("No death screen effect assigned - create UI with DeathScreenEffect component");

                // Fallback: schedule respawn directly if no death effect
                if (autoRespawnToCheckpoint && !respawnScheduled)
                {
                    respawnScheduled = true;
                    float totalDelay = deathAnimationDuration + respawnDelay;
                    Invoke(nameof(TriggerRespawnToCheckpoint), totalDelay);
                    if (showDebug) Debug.Log($"Fallback: Scheduled respawn in {totalDelay}s");
                }
            }
        }

        /// <summary>
        /// Callback method called by DeathScreenEffect when respawn should happen
        /// </summary>
        private void OnRespawnRequested()
        {
            if (showDebug) Debug.Log("★★★ RESPAWN REQUESTED BY DEATH SCREEN EFFECT ★★★");

            // CRITICAL: Prevent any main menu or scene loading during respawn!
            if (showDebug) Debug.Log("Blocking any potential scene transitions during respawn...");

            if (!autoRespawnToCheckpoint)
            {
                if (showDebug) Debug.Log("Auto respawn disabled - waiting for manual respawn");
                return;
            }

            if (respawnScheduled)
            {
                if (showDebug) Debug.Log("Respawn already scheduled - ignoring duplicate request");
                return;
            }

            // Trigger respawn immediately - SHOULD ONLY MOVE PLAYER, NOT CHANGE SCENES
            respawnScheduled = true;
            TriggerRespawnToCheckpoint();
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

            // The respawn is now triggered by the DeathScreenEffect callback system
            // No longer scheduling respawn here to avoid conflicts
            if (showDebug) Debug.Log("=== WAITING FOR DEATH SCREEN EFFECT TO TRIGGER RESPAWN ===");
        }

        /// <summary>
        /// Trigger respawn to last checkpoint
        /// </summary>
        private void TriggerRespawnToCheckpoint()
        {
            if (showDebug) Debug.Log("★★★ TRIGGERING RESPAWN TO CHECKPOINT ★★★");

            // Prevent any scene loading or main menu navigation
            // This should ONLY respawn at checkpoint, not go to main menu!

            // Try to respawn via SaveManager
            if (saveManager != null)
            {
                try
                {
                    // Use reflection to call RespawnAtLastCheckpoint method
                    var respawnMethod = saveManager.GetType().GetMethod("RespawnAtLastCheckpoint");
                    if (respawnMethod != null)
                    {
                        if (showDebug) Debug.Log("★ Calling SaveManager.RespawnAtLastCheckpoint()");
                        respawnMethod.Invoke(saveManager, null);
                        if (showDebug) Debug.Log("★ SaveManager respawn completed successfully");

                        // Trigger fade out to restore gameplay
                        TriggerFadeOutAfterRespawn();
                    }
                    else
                    {
                        if (showDebug) Debug.LogError("RespawnAtLastCheckpoint method not found on SaveManager!");
                        FallbackRespawn();
                    }
                }
                catch (System.Exception e)
                {
                    if (showDebug) Debug.LogError($"Error calling SaveManager respawn: {e.Message}");
                    FallbackRespawn();
                }
            }
            else
            {
                if (showDebug) Debug.LogError("No SaveManager available - using fallback respawn");
                FallbackRespawn();
            }

            // Reset respawn state
            respawnScheduled = false;
        }

        /// <summary>
        /// Trigger fade out effect after respawn to restore normal gameplay
        /// </summary>
        private void TriggerFadeOutAfterRespawn()
        {
            if (deathScreenEffect != null)
            {
                // Small delay to ensure player position is set before fade out
                StartCoroutine(DelayedFadeOut());
                if (showDebug) Debug.Log("★ Fade out triggered after respawn!");
            }
            else
            {
                // If no death effect, just reset player immediately
                ResetPlayerAfterRespawn();
                if (showDebug) Debug.Log("No death effect - resetting player immediately");
            }
        }

        /// <summary>
        /// Coroutine to add small delay before fade out (ensures player is positioned correctly)
        /// </summary>
        private System.Collections.IEnumerator DelayedFadeOut()
        {
            // Wait one frame to ensure SaveManager has positioned player
            yield return null;
            yield return null; // Wait additional frame for safety

            if (showDebug) Debug.Log("Starting fade out to restore gameplay...");

            // Trigger fade out
            deathScreenEffect.TriggerFadeOut();

            // Wait for fade out to complete before fully resetting player
            yield return new WaitForSecondsRealtime(deathScreenEffect != null ? 2f : 0.5f);

            // Final player reset
            ResetPlayerAfterRespawn();
        }

        /// <summary>
        /// Reset player state after respawn and fade out
        /// </summary>
        private void ResetPlayerAfterRespawn()
        {
            if (showDebug) Debug.Log("Resetting player state after respawn...");

            // Reset death states
            isDead = false;
            deathAnimationPlaying = false;
            respawnScheduled = false;

            // Re-enable auto-save in SaveManager
            RestoreAutoSaveAfterRespawn();

            // Re-enable player controls
            if (playerRigidbody != null)
                playerRigidbody.isKinematic = false;

            // Re-enable player scripts
            EnablePlayerScripts();

            // Reset animator
            if (playerAnimator != null)
                playerAnimator.Rebind();

            if (showDebug) Debug.Log("★★★ PLAYER RESPAWN COMPLETE - READY FOR GAMEPLAY ★★★");
        }

        /// <summary>
        /// Re-enable auto-save in SaveManager after respawn is complete
        /// </summary>
        private void RestoreAutoSaveAfterRespawn()
        {
            if (saveManager != null)
            {
                try
                {
                    // Call method to re-enable auto-save after respawn
                    var restoreSaveMethod = saveManager.GetType().GetMethod("SetDeathState");
                    if (restoreSaveMethod != null)
                    {
                        restoreSaveMethod.Invoke(saveManager, new object[] { false });
                        if (showDebug) Debug.Log("★ Death state cleared in SaveManager - auto-save restored");
                    }
                }
                catch (System.Exception e)
                {
                    if (showDebug) Debug.LogWarning($"Could not restore auto-save after respawn: {e.Message}");
                }
            }
        }

        /// <summary>
        /// Fallback respawn if SaveManager is not available
        /// </summary>
        private void FallbackRespawn()
        {
            if (showDebug) Debug.LogWarning("Using fallback respawn - SaveManager not available!");

            // CRITICAL: DO NOT load main menu or change scenes!
            // This should only reset player state and position

            // Try to find a spawn point in the current scene
            GameObject spawnPoint = GameObject.FindWithTag("Respawn");
            if (spawnPoint == null)
                spawnPoint = GameObject.Find("SpawnPoint");
            if (spawnPoint == null)
                spawnPoint = GameObject.Find("PlayerSpawn");

            if (spawnPoint != null)
            {
                if (showDebug) Debug.Log($"Found fallback spawn point: {spawnPoint.name}");
                gameObject.transform.position = spawnPoint.transform.position;
                gameObject.transform.rotation = spawnPoint.transform.rotation;
            }
            else
            {
                if (showDebug) Debug.LogWarning("No spawn point found for fallback respawn - keeping current position");
            }

            // Trigger fade out to restore gameplay
            TriggerFadeOutAfterRespawn();
        }

        /// <summary>
        /// Method to reset player state (for restart/checkpoint logic)
        /// </summary>
        public void ResetPlayer()
        {
            if (showDebug) Debug.Log("Resetting player state...");

            // Cancel any pending respawn
            CancelInvoke(nameof(TriggerRespawnToCheckpoint));

            // DON'T stop all coroutines as it will interrupt fade-out
            // StopAllCoroutines(); // REMOVED - this was causing fade-out to be interrupted

            // Reset death screen effect
            if (deathScreenEffect != null)
            {
                deathScreenEffect.ResetDeathEffect();
                if (showDebug) Debug.Log("Death screen effect reset");
            }

            // Reset player state after respawn
            ResetPlayerAfterRespawn();

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

        /// <summary>
        /// Debug method to test checkpoint respawn
        /// </summary>
        [ContextMenu("Test Checkpoint Respawn")]
        private void TestCheckpointRespawn()
        {
            TriggerRespawnToCheckpoint();
        }

        /// <summary>
        /// Debug method to force save at current position
        /// </summary>
        [ContextMenu("Force Save Current Position")]
        private void ForceSaveCurrentPosition()
        {
            if (saveManager != null)
            {
                try
                {
                    var quickSaveMethod = saveManager.GetType().GetMethod("QuickSave");
                    if (quickSaveMethod != null)
                    {
                        quickSaveMethod.Invoke(saveManager, null);
                        if (showDebug) Debug.Log("Force save completed");
                    }
                }
                catch (System.Exception e)
                {
                    if (showDebug) Debug.LogError($"Error force saving: {e.Message}");
                }
            }
        }

        /// <summary>
        /// Debug method to validate current checkpoint setup
        /// </summary>
        [ContextMenu("Debug: Validate Checkpoint Setup")]
        private void DebugValidateCheckpointSetup()
        {
            Debug.Log("=== CHECKPOINT VALIDATION DEBUG ===");

            if (saveManager == null)
            {
                Debug.LogError("❌ SaveManager not found! Cannot respawn without SaveManager.");
                return;
            }

            try
            {
                // Use reflection to check current checkpoint
                var currentCheckpointField = saveManager.GetType().GetField("currentCheckpoint",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (currentCheckpointField != null)
                {
                    var checkpoint = currentCheckpointField.GetValue(saveManager);

                    if (checkpoint == null)
                    {
                        Debug.LogError("❌ No checkpoint saved! Player must pass through a CheckpointTrigger first.");
                    }
                    else
                    {
                        // Get checkpoint details
                        var checkpointType = checkpoint.GetType();
                        var nameField = checkpointType.GetField("checkpointName");
                        var sceneField = checkpointType.GetField("sceneName");
                        var positionField = checkpointType.GetField("spawnPosition");

                        string checkpointName = nameField?.GetValue(checkpoint)?.ToString() ?? "Unknown";
                        string sceneName = sceneField?.GetValue(checkpoint)?.ToString() ?? "Unknown";
                        object position = positionField?.GetValue(checkpoint);

                        Debug.Log($"✅ Checkpoint found: {checkpointName}");
                        Debug.Log($"📍 Spawn position: {position}");
                        Debug.Log($"🎬 Scene name: {sceneName}");
                        Debug.Log($"🎮 Current scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");

                        if (sceneName != UnityEngine.SceneManagement.SceneManager.GetActiveScene().name)
                        {
                            Debug.LogWarning("⚠️ WARNING: Checkpoint scene name doesn't match current scene! This might cause main menu issue!");
                        }
                        else
                        {
                            Debug.Log("✅ Scene names match - respawn should work correctly");
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error validating checkpoint: {e.Message}");
            }

            Debug.Log("=== END CHECKPOINT VALIDATION ===");
        }

        // Debug GUI
#if UNITY_EDITOR
        private void OnGUI()
        {
            if (!showDebug) return;

            GUILayout.BeginArea(new Rect(10, 450, 350, 250));
            GUILayout.Label("=== PLAYER DEATH DEBUG ===");
            GUILayout.Label($"Is Dead: {isDead}");
            GUILayout.Label($"Death Animation Playing: {deathAnimationPlaying}");
            GUILayout.Label($"Respawn Scheduled: {respawnScheduled}");
            GUILayout.Label($"Auto Respawn: {autoRespawnToCheckpoint}");
            GUILayout.Label($"SaveManager Found: {saveManager != null}");

            if (isDead)
            {
                float timeSinceDeath = Time.time - deathStartTime;
                GUILayout.Label($"Time Since Death: {timeSinceDeath:F1}s");
                GUILayout.Label($"Animation Duration: {deathAnimationDuration:F1}s");

                if (respawnScheduled)
                {
                    GUILayout.Label($"Respawn Delay: {respawnDelay:F1}s");
                }
            }

            GUILayout.Space(5);

            if (GUILayout.Button("Test Death"))
            {
                Die("Manual Test");
            }

            if (GUILayout.Button("Reset Player"))
            {
                ResetPlayer();
            }

            if (GUILayout.Button("Force Respawn to Checkpoint"))
            {
                TriggerRespawnToCheckpoint();
            }

            if (GUILayout.Button("Force Save Current Position"))
            {
                ForceSaveCurrentPosition();
            }

            GUILayout.EndArea();
        }
        #endif
    }
    
}
