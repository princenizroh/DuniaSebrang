using UnityEngine;

namespace DS
{
    /// <summary>
    /// Hit collider yang diletakkan di tangan Takau untuk mendeteksi hit dengan player saat attack.
    /// Collider ini hanya aktif selama attack animation.
    /// </summary>
    public class KuntiAttackHitCollider : MonoBehaviour
    {
        [Header("=== HIT DETECTION ===")]
        [Tooltip("Layer mask untuk player yang bisa terkena hit (set ke layer 'Player')")]
        [SerializeField] private LayerMask playerLayerMask = 1 << 3; // Layer 3 = "Player" default

        [Tooltip("Tag player yang bisa terkena hit")]
        [SerializeField] private string playerTag = "Player";

        [Header("=== HIT BEHAVIOR ===")]
        [Tooltip("Collider hanya aktif saat attack mode")]
        [SerializeField] private bool onlyActiveInAttackMode = true;

        [Tooltip("Disable collider setelah hit pertama (prevent multiple hits)")]
        [SerializeField] private bool disableAfterHit = true;

        [Tooltip("Durasi collider aktif setelah attack dimulai (detik)")]
        [SerializeField] private float activeDuration = 1f;

        [Header("=== REFERENCES ===")]
        [Tooltip("Reference ke TakauAI script")]
        [SerializeField] private KuntiAI takauAI;

        [Tooltip("Collider yang akan diaktifkan/nonaktifkan")]
        [SerializeField] private Collider hitCollider;

        [Header("=== DEBUG ===")]
        [Tooltip("Show debug messages")]
        [SerializeField] private bool showDebug = true;

        [Tooltip("Visual indicator saat collider aktif")]
        [SerializeField] private bool showGizmosWhenActive = true;

        // Runtime variables
        private bool hasHitPlayer = false;
        private float activationTime;
        private bool isCurrentlyActive = false;
        private MoveMode lastKnownMode;

        // Properties
        public bool IsActive => isCurrentlyActive;
        public bool HasHitPlayer => hasHitPlayer;

        private void Awake()
        {
            // Get component references
            if (takauAI == null)
                takauAI = GetComponentInParent<KuntiAI>();

            if (hitCollider == null)
                hitCollider = GetComponent<Collider>();

            // Validation
            if (takauAI == null)
            {
                Debug.LogError($"TakauAttackHitCollider: No TakauAI found in parent of {gameObject.name}");
            }
            else
            {
                if (showDebug) Debug.Log($"✅ TakauAI found: {takauAI.name}");
            }

            if (hitCollider == null)
            {
                Debug.LogError($"TakauAttackHitCollider: No Collider found on {gameObject.name}");
            }
            else
            {
                if (showDebug) Debug.Log($"✅ Hit Collider found: {hitCollider.GetType().Name}");

                // Auto-set as trigger if not already
                if (!hitCollider.isTrigger)
                {
                    hitCollider.isTrigger = true;
                    if (showDebug) Debug.Log("✅ Auto-set collider as Trigger");
                }
            }
        }

        private void Start()
        {
            // Disable collider by default
            DisableHitCollider();

            if (showDebug)
            {
                Debug.Log($"TakauAttackHitCollider initialized on {gameObject.name}");
                Debug.Log($"Player detection: LayerMask={playerLayerMask}, Tag={playerTag}");

                // Show setup instructions if components missing
                if (takauAI == null || hitCollider == null)
                {
                    Debug.LogWarning("=== SETUP REQUIRED ===");
                    if (takauAI == null)
                        Debug.LogWarning("• TakauAI not found - make sure this is child of Takau GameObject");
                    if (hitCollider == null)
                        Debug.LogWarning("• Add BoxCollider/SphereCollider to this GameObject and set IsTrigger = true");
                    Debug.LogWarning("• Right-click this component → Validate Setup for detailed instructions");
                }
            }
        }

        private void Update()
        {
            if (takauAI == null) return;

            // Check if we should activate/deactivate based on Takau's mode
            CheckAttackModeStatus();

            // Check duration timeout
            if (isCurrentlyActive && activeDuration > 0)
            {
                CheckActiveDuration();
            }
        }

        private void CheckAttackModeStatus()
        {
            MoveMode currentMode = takauAI.moveMode;

            // Detect mode change
            if (currentMode != lastKnownMode)
            {
                if (showDebug)
                {
                    Debug.Log($"TakauAttackHitCollider: Mode changed from {lastKnownMode} to {currentMode}");
                }

                lastKnownMode = currentMode;

                // Enable collider when entering attack mode
                if (currentMode == MoveMode.attack)
                {
                    EnableHitCollider();
                }
                else
                {
                    DisableHitCollider();
                }
            }
        }

        private void CheckActiveDuration()
        {
            if (Time.time - activationTime >= activeDuration)
            {
                if (showDebug) Debug.Log("TakauAttackHitCollider: Active duration expired, disabling");
                DisableHitCollider();
            }
        }

        public void EnableHitCollider()
        {
            if (hitCollider == null) return;

            hitCollider.enabled = true;
            isCurrentlyActive = true;
            hasHitPlayer = false; // Reset hit status
            activationTime = Time.time;

            if (showDebug)
            {
                Debug.Log($"★ TakauAttackHitCollider ENABLED - Ready to hit player!");
                Debug.Log($"Active duration: {activeDuration}s, Disable after hit: {disableAfterHit}");
            }
        }

        public void DisableHitCollider()
        {
            if (hitCollider == null) return;

            hitCollider.enabled = false;
            isCurrentlyActive = false;

            if (showDebug && isCurrentlyActive)
            {
                Debug.Log("TakauAttackHitCollider DISABLED");
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!isCurrentlyActive) return;
            if (hasHitPlayer && disableAfterHit) return;

            // Check if it's a player
            bool isPlayer = IsPlayerObject(other);

            if (!isPlayer)
            {
                if (showDebug) Debug.Log($"TakauAttackHitCollider: Hit non-player object: {other.name}");
                return;
            }

            if (showDebug)
            {
                Debug.Log($"★★★ TAKAU HIT DETECTED! ★★★");
                Debug.Log($"Hit object: {other.name}");
                Debug.Log($"Hit position: {other.transform.position}");
            }

            // Try to kill the player
            bool playerKilled = TryKillPlayer(other);

            if (playerKilled)
            {
                hasHitPlayer = true;

                if (disableAfterHit)
                {
                    DisableHitCollider();
                    if (showDebug) Debug.Log("Hit collider disabled after successful hit");
                }
            }
        }

        private bool IsPlayerObject(Collider other)
        {
            // Check by tag
            if (!string.IsNullOrEmpty(playerTag) && other.CompareTag(playerTag))
            {
                return true;
            }

            // Check by layer mask
            int objectLayer = other.gameObject.layer;
            bool isInLayerMask = (playerLayerMask & (1 << objectLayer)) != 0;

            if (showDebug && !isInLayerMask)
            {
                Debug.Log($"Object {other.name} layer {objectLayer} not in player LayerMask {playerLayerMask}");
            }

            return isInLayerMask;
        }

        private bool TryKillPlayer(Collider playerCollider)
        {
            // Try to find PlayerDeathHandler
            PlayerDeathHandler deathHandler = playerCollider.GetComponent<PlayerDeathHandler>();

            if (deathHandler == null)
            {
                // Try in parent
                deathHandler = playerCollider.GetComponentInParent<PlayerDeathHandler>();
            }

            if (deathHandler == null)
            {
                // Try in children
                deathHandler = playerCollider.GetComponentInChildren<PlayerDeathHandler>();
            }

            if (deathHandler != null)
            {
                // Check if player can die
                if (deathHandler.CanDie())
                {
                    if (showDebug)
                    {
                        Debug.Log($"★★★ KILLING PLAYER via PlayerDeathHandler! ★★★");
                        Debug.Log($"Death cause: Takau Attack Hit");
                    }

                    // Kill the player
                    deathHandler.Die("Takau Attack Hit");
                    return true;
                }
                else
                {
                    if (showDebug) Debug.Log("PlayerDeathHandler found but player cannot die (already dead?)");
                    return false;
                }
            }
            else
            {
                if (showDebug)
                {
                    Debug.LogWarning($"No PlayerDeathHandler found on player object: {playerCollider.name}");
                    Debug.LogWarning("Player hit detected but cannot trigger death - missing PlayerDeathHandler component!");
                }
                return false;
            }
        }

        /// <summary>
        /// Manual method to enable hit collider (can be called from animation events)
        /// </summary>
        public void ActivateHitCollider()
        {
            if (showDebug) Debug.Log("TakauAttackHitCollider: Manual activation via ActivateHitCollider()");
            EnableHitCollider();
        }

        /// <summary>
        /// Manual method to disable hit collider (can be called from animation events)
        /// </summary>
        public void DeactivateHitCollider()
        {
            if (showDebug) Debug.Log("TakauAttackHitCollider: Manual deactivation via DeactivateHitCollider()");
            DisableHitCollider();
        }

        /// <summary>
        /// Reset hit status (for testing or multiple attacks)
        /// </summary>
        public void ResetHitStatus()
        {
            hasHitPlayer = false;
            if (showDebug) Debug.Log("TakauAttackHitCollider: Hit status reset");
        }

        /// <summary>
        /// Check if player is currently in hit range
        /// </summary>
        public bool IsPlayerInRange()
        {
            if (!isCurrentlyActive || hitCollider == null) return false;

            // Find all colliders in trigger range
            Collider[] overlapping = Physics.OverlapBox(
                hitCollider.bounds.center,
                hitCollider.bounds.extents,
                transform.rotation,
                playerLayerMask
            );

            foreach (Collider col in overlapping)
            {
                if (IsPlayerObject(col))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Debug method to test hit detection
        /// </summary>
        [ContextMenu("Test Hit Detection")]
        private void TestHitDetection()
        {
            EnableHitCollider();
            Debug.Log("Hit collider enabled for testing");
        }

        /// <summary>
        /// Debug method to force disable
        /// </summary>
        [ContextMenu("Force Disable")]
        private void ForceDisable()
        {
            DisableHitCollider();
            Debug.Log("Hit collider force disabled");
        }

        /// <summary>
        /// Helper method to set player layer mask by layer name
        /// </summary>
        public void SetPlayerLayer(string layerName)
        {
            int layerIndex = LayerMask.NameToLayer(layerName);
            if (layerIndex != -1)
            {
                playerLayerMask = 1 << layerIndex;
                if (showDebug) Debug.Log($"Player layer set to: {layerName} (index {layerIndex})");
            }
            else
            {
                Debug.LogError($"Layer '{layerName}' not found! Please create it in Layer settings.");
            }
        }

        /// <summary>
        /// Validate current layer and tag setup
        /// </summary>
        [ContextMenu("Validate Setup")]
        private void ValidateSetup()
        {
            Debug.Log("=== TAKAU HIT COLLIDER SETUP VALIDATION ===");

            // Check TakauAI reference
            if (takauAI == null)
            {
                Debug.LogError("❌ TakauAI reference missing! This script should be child of Takau.");
            }
            else
            {
                Debug.Log("✅ TakauAI reference found");
            }

            // Check Hit Collider
            if (hitCollider == null)
            {
                Debug.LogError("❌ Hit Collider missing! Add BoxCollider/SphereCollider and set IsTrigger = true");
            }
            else
            {
                Debug.Log($"✅ Hit Collider found: {hitCollider.GetType().Name}");
                if (!hitCollider.isTrigger)
                {
                    Debug.LogWarning("⚠️ Hit Collider should be set as Trigger!");
                }
                else
                {
                    Debug.Log("✅ Hit Collider is properly set as Trigger");
                }
            }

            // Check Player Layer
            string[] layerNames = new string[32];
            for (int i = 0; i < 32; i++)
            {
                string layerName = LayerMask.LayerToName(i);
                if (!string.IsNullOrEmpty(layerName))
                {
                    layerNames[i] = layerName;
                }
            }

            bool foundPlayerLayer = false;
            for (int i = 0; i < 32; i++)
            {
                if ((playerLayerMask & (1 << i)) != 0)
                {
                    string layerName = LayerMask.LayerToName(i);
                    Debug.Log($"✅ Detecting layer {i}: '{layerName}'");
                    foundPlayerLayer = true;

                    if (layerName.ToLower().Contains("player"))
                    {
                        Debug.Log($"✅ Player layer properly configured: '{layerName}'");
                    }
                    else
                    {
                        Debug.LogWarning($"⚠️ Layer '{layerName}' doesn't seem to be a player layer");
                    }
                }
            }

            if (!foundPlayerLayer)
            {
                Debug.LogError("❌ No player layer selected in LayerMask!");
            }

            // Check Player Tag
            if (string.IsNullOrEmpty(playerTag))
            {
                Debug.LogWarning("⚠️ Player tag is empty");
            }
            else
            {
                Debug.Log($"✅ Player tag set to: '{playerTag}'");
            }

            Debug.Log("=== SETUP INSTRUCTIONS ===");
            Debug.Log("1. Create layer 'Player' in Project Settings → Tags and Layers");
            Debug.Log("2. Set player GameObject layer to 'Player'");
            Debug.Log("3. Set player GameObject tag to 'Player'");
            Debug.Log("4. Attach PlayerDeathHandler to player GameObject");
            Debug.Log("5. Place this hit collider as child of Takau's hand bone");
        }

        /// <summary>
        /// Auto-fix common setup issues
        /// </summary>
        [ContextMenu("Auto Fix Setup")]
        private void AutoFixSetup()
        {
            Debug.Log("=== AUTO FIXING SETUP ===");

            // Fix hit collider reference
            if (hitCollider == null)
            {
                hitCollider = GetComponent<Collider>();
                if (hitCollider != null)
                {
                    Debug.Log("✅ Auto-assigned Hit Collider");
                }
                else
                {
                    Debug.LogWarning("❌ No collider found - please add BoxCollider manually");
                }
            }

            // Fix trigger setting
            if (hitCollider != null && !hitCollider.isTrigger)
            {
                hitCollider.isTrigger = true;
                Debug.Log("✅ Auto-set collider as Trigger");
            }

            // Fix TakauAI reference
            if (takauAI == null)
            {
                takauAI = GetComponentInParent<KuntiAI>();
                if (takauAI != null)
                {
                    Debug.Log("✅ Auto-assigned TakauAI reference");
                }
                else
                {
                    Debug.LogWarning("❌ No TakauAI found in parent - make sure this is child of Takau");
                }
            }

            // Fix player layer to "Player" layer
            int playerLayerIndex = LayerMask.NameToLayer("Player");
            if (playerLayerIndex != -1)
            {
                playerLayerMask = 1 << playerLayerIndex;
                Debug.Log($"✅ Auto-set Player Layer Mask to 'Player' (layer {playerLayerIndex})");
            }
            else
            {
                Debug.LogWarning("❌ 'Player' layer not found - please create it in Project Settings");
            }

            Debug.Log("=== AUTO FIX COMPLETED ===");
            Debug.Log("Check inspector - components should now be properly assigned!");
        }

#if UNITY_EDITOR
        // Visual debugging
        private void OnDrawGizmos()
        {
            if (!showGizmosWhenActive || !isCurrentlyActive) return;
            if (hitCollider == null) return;

            // Draw hit collider bounds when active
            Gizmos.color = hasHitPlayer ? Color.red : Color.yellow;
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale);

            if (hitCollider is BoxCollider box)
            {
                Gizmos.DrawWireCube(box.center, box.size);
            }
            else if (hitCollider is SphereCollider sphere)
            {
                Gizmos.DrawWireSphere(sphere.center, sphere.radius);
            }
            else if (hitCollider is CapsuleCollider capsule)
            {
                // Approximate capsule as sphere for simplicity
                Gizmos.DrawWireSphere(capsule.center, capsule.radius);
            }

            Gizmos.matrix = Matrix4x4.identity;
        }

        // Debug GUI
        private void OnGUI()
        {
            if (!showDebug) return;

            GUILayout.BeginArea(new Rect(Screen.width - 320, 10, 300, 200));
            GUILayout.Label("=== TAKAU HIT COLLIDER DEBUG ===");
            GUILayout.Label($"Is Active: {isCurrentlyActive}");
            GUILayout.Label($"Has Hit Player: {hasHitPlayer}");

            if (takauAI != null)
            {
                GUILayout.Label($"Takau Mode: {takauAI.moveMode}");
            }

            if (isCurrentlyActive)
            {
                float timeActive = Time.time - activationTime;
                float timeRemaining = activeDuration - timeActive;
                GUILayout.Label($"Active Time: {timeActive:F1}s");
                GUILayout.Label($"Time Remaining: {timeRemaining:F1}s");

                bool playerInRange = IsPlayerInRange();
                GUILayout.Label($"Player In Range: {playerInRange}");
            }

            if (GUILayout.Button("Manual Enable"))
            {
                ActivateHitCollider();
            }

            if (GUILayout.Button("Manual Disable"))
            {
                DeactivateHitCollider();
            }

            if (GUILayout.Button("Reset Hit Status"))
            {
                ResetHitStatus();
            }

            GUILayout.EndArea();
        }
#endif
    }
}
