using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace DS
{
    /// <summary>
    /// Monitors game events and handles the ending sequence
    /// Tracks: Player trigger -> Takau activation -> Takau death -> Surat activation -> Ending
    /// </summary>
    public class GameEndingMonitor : MonoBehaviour
    {
        [Header("=== GAME OBJECTS TO MONITOR ===")]
        [SerializeField] private TakauAI takauAI;
        [SerializeField] private GameObject suratGameObject;
        [SerializeField] private GameObject sleepingCharacter;
        [SerializeField] private InteractionObject suratInteractionObject;
        [SerializeField] private DeathScreenEffect deathScreenEffect;
        
        [Header("=== MAIN MENU SETTINGS ===")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";
        [SerializeField] private float delayBeforeMainMenu = 5f;
        
        [Header("=== SURAT OBJECT DETECTION ===")]
        [SerializeField] private bool checkSuratByName = true;
        [SerializeField] private string suratObjectName = "Surat";
        [SerializeField] private bool checkSuratByTag = false;
        [SerializeField] private string suratObjectTag = "EndingObject";
        [SerializeField] private bool useSuratEndingFlag = true;
        
        [Header("=== INTERACTION MONITORING ===")]
        [SerializeField] private float interactionCheckDelay = 1f; // Delay sebelum mulai monitoring interaction
        [SerializeField] private int maxMissingObjectChecks = 3; // Berapa kali boleh object tidak ditemukan sebelum dianggap selesai
        [SerializeField] private float missingObjectCheckInterval = 0.5f; // Interval check object yang hilang
        
        [Header("=== ALTERNATIVE ENDING TRIGGERS ===")]
        [SerializeField] private bool useAlternativeEndingTrigger = false;
        [SerializeField] private KeyCode manualEndingKey = KeyCode.F9;
        [SerializeField] private bool enableEndingSequence = true;
        [SerializeField] private float delayAfterSuratInteraction = 2f;
        [SerializeField] private bool showEndingDebug = true;
        
        [Header("=== MONITORING STATUS (READ ONLY) ===")]
        [SerializeField] private bool takauIsActive = false;
        [SerializeField] private bool takauIsDead = false;
        [SerializeField] private bool suratIsActive = false;
        [SerializeField] private bool sleepingCharacterDeactivated = false;
        [SerializeField] private bool suratInteractionCompleted = false;
        [SerializeField] private bool endingSequenceStarted = false;
        [SerializeField] private bool endingSequenceCompleted = false;
        [SerializeField] private bool isMonitoringInteraction = false; // NEW: Flag untuk monitoring state
        
        // Tracking variables untuk missing object detection
        private int missingObjectCount = 0;
        private bool wasInteractionObjectFound = false;
        private InteractionObject trackedSuratObject = null;
        
        // Events for other scripts to subscribe to
        public System.Action OnTakauActivated;
        public System.Action OnTakauDied;
        public System.Action OnSuratActivated;
        public System.Action OnSuratInteractionCompleted;
        public System.Action OnEndingSequenceStarted;
        public System.Action OnEndingSequenceCompleted;
        
        private void Start()
        {
            // Auto-find components if not assigned
            if (takauAI == null)
                takauAI = FindObjectOfType<TakauAI>();
            
            if (suratInteractionObject == null && suratGameObject != null)
                suratInteractionObject = suratGameObject.GetComponent<InteractionObject>();
            
            if (deathScreenEffect == null)
                deathScreenEffect = FindObjectOfType<DeathScreenEffect>();
            
            // Validate setup
            ValidateSetup();
            
            if (showEndingDebug)
                Debug.Log("GameEndingMonitor: Initialized and ready to monitor game ending sequence");
        }
        
        private void Update()
        {
            // Manual testing key
            if (useAlternativeEndingTrigger && Input.GetKeyDown(manualEndingKey))
            {
                Debug.Log("GameEndingMonitor: Manual ending triggered!");
                ForceEndingSequence();
                return;
            }
            
            // Monitor Takau activation
            if (!takauIsActive && takauAI != null && takauAI.gameObject.activeInHierarchy)
            {
                OnTakauActivation();
            }
            
            // Monitor Takau death
            if (takauIsActive && !takauIsDead && takauAI != null && takauAI.moveMode == MoveMode.dying)
            {
                OnTakauDeath();
            }
            
            // Monitor Surat interaction completion - ONLY if monitoring is active
            if (isMonitoringInteraction && suratIsActive && !suratInteractionCompleted)
            {
                CheckSuratInteractionStatus();
            }
        }
        
        private void CheckSuratInteractionStatus()
        {
            InteractionObject currentSuratObject = GetSuratInteractionObject();
            
            if (currentSuratObject != null)
            {
                // Object found, reset missing count
                missingObjectCount = 0;
                wasInteractionObjectFound = true;
                trackedSuratObject = currentSuratObject;
                
                // Check if interaction is completed
                if (HasSuratInteractionCompleted_ByObject(currentSuratObject))
                {
                    if (showEndingDebug)
                        Debug.Log($"GameEndingMonitor: Surat interaction completed via object check!");
                    
                    OnSuratInteractionComplete();
                }
            }
            else
            {
                // Object not found
                if (wasInteractionObjectFound)
                {
                    // We previously found the object but now it's missing
                    missingObjectCount++;
                    
                    if (showEndingDebug)
                        Debug.Log($"GameEndingMonitor: Surat object missing (count: {missingObjectCount}/{maxMissingObjectChecks})");
                    
                    // Only consider interaction complete if object was found before and now consistently missing
                    if (missingObjectCount >= maxMissingObjectChecks)
                    {
                        if (showEndingDebug)
                            Debug.Log($"GameEndingMonitor: Surat object consistently missing - assuming interaction completed!");
                        
                        OnSuratInteractionComplete();
                    }
                }
                else
                {
                    // Never found the object yet, keep looking
                    if (showEndingDebug && Time.frameCount % 120 == 0) // Debug every 2 seconds
                        Debug.Log("GameEndingMonitor: Still looking for Surat interaction object...");
                }
            }
        }
        
        private InteractionObject GetSuratInteractionObject()
        {
            // Priority 1: Check assigned reference
            if (suratInteractionObject != null && suratInteractionObject.gameObject != null)
            {
                return suratInteractionObject;
            }
            
            // Priority 2: Check by ending flag
            if (useSuratEndingFlag)
            {
                InteractionObject[] allInteractionObjects = FindObjectsOfType<InteractionObject>();
                foreach (InteractionObject obj in allInteractionObjects)
                {
                    if (obj.isEndingObject)
                    {
                        return obj;
                    }
                }
            }
            
            // Priority 3: Check by name
            if (checkSuratByName)
            {
                GameObject suratByName = GameObject.Find(suratObjectName);
                if (suratByName != null)
                {
                    InteractionObject obj = suratByName.GetComponent<InteractionObject>();
                    if (obj != null)
                    {
                        return obj;
                    }
                }
            }
            
            // Priority 4: Check by tag
            if (checkSuratByTag)
            {
                GameObject suratByTag = GameObject.FindGameObjectWithTag(suratObjectTag);
                if (suratByTag != null)
                {
                    InteractionObject obj = suratByTag.GetComponent<InteractionObject>();
                    if (obj != null)
                    {
                        return obj;
                    }
                }
            }
            
            return null;
        }
        
        private void OnTakauActivation()
        {
            takauIsActive = true;
            OnTakauActivated?.Invoke();
            
            if (showEndingDebug)
                Debug.Log("GameEndingMonitor: Takau has been activated!");
        }
        
        private void OnTakauDeath()
        {
            takauIsDead = true;
            OnTakauDied?.Invoke();
            
            // Activate Surat GameObject
            if (suratGameObject != null)
            {
                suratGameObject.SetActive(true);
                suratIsActive = true;
                
                if (showEndingDebug)
                    Debug.Log("GameEndingMonitor: Takau died - Surat activated!");
            }
            
            // Deactivate sleeping character
            if (sleepingCharacter != null)
            {
                sleepingCharacter.SetActive(false);
                sleepingCharacterDeactivated = true;
                
                if (showEndingDebug)
                    Debug.Log("GameEndingMonitor: Sleeping character deactivated!");
            }
            
            OnSuratActivated?.Invoke();
            
            // START monitoring interaction after delay
            StartCoroutine(StartInteractionMonitoring());
        }
        
        private IEnumerator StartInteractionMonitoring()
        {
            if (showEndingDebug)
                Debug.Log($"GameEndingMonitor: Will start monitoring Surat interaction in {interactionCheckDelay} seconds...");
            
            yield return new WaitForSeconds(interactionCheckDelay);
            
            // Reset monitoring variables
            missingObjectCount = 0;
            wasInteractionObjectFound = false;
            trackedSuratObject = null;
            isMonitoringInteraction = true;
            
            if (showEndingDebug)
                Debug.Log("GameEndingMonitor: Started monitoring Surat interaction!");
        }
        
        private void OnSuratInteractionComplete()
        {
            if (suratInteractionCompleted) return; // Prevent double execution
            
            suratInteractionCompleted = true;
            isMonitoringInteraction = false; // Stop monitoring
            OnSuratInteractionCompleted?.Invoke();
            
            if (showEndingDebug)
                Debug.Log("GameEndingMonitor: Surat interaction completed - Starting ending sequence!");
            
            // Start ending sequence
            if (enableEndingSequence)
            {
                StartCoroutine(StartEndingSequence());
            }
        }
        
        private IEnumerator StartEndingSequence()
        {
            if (endingSequenceStarted)
                yield break;
            
            endingSequenceStarted = true;
            OnEndingSequenceStarted?.Invoke();
            
            if (showEndingDebug)
                Debug.Log("GameEndingMonitor: Ending sequence started!");
            
            // Wait for interaction to fully complete
            yield return new WaitForSeconds(delayAfterSuratInteraction);
            
            // Trigger death screen effect (fade to black)
            if (deathScreenEffect != null)
            {
                if (showEndingDebug)
                    Debug.Log("GameEndingMonitor: Triggering death screen effect for ending");
                
                TriggerEndingFade();
                
                // Wait for fade to complete
                yield return new WaitForSeconds(deathScreenEffect.RespawnDelay + 3f);
            }
            else
            {
                if (showEndingDebug)
                    Debug.LogWarning("GameEndingMonitor: DeathScreenEffect not found - using default delay");
                
                yield return new WaitForSeconds(3f);
            }
            
            // Additional delay before going to main menu
            yield return new WaitForSeconds(delayBeforeMainMenu);
            
            // Load main menu
            LoadMainMenu();
        }
        
        private void TriggerEndingFade()
        {
            if (showEndingDebug)
                Debug.Log("GameEndingMonitor: Triggering ending fade effect");
            
            // Implement your fade logic here
            // You might need to add a method to DeathScreenEffect or use a different approach
        }
        
    public void LoadMainMenu()
        {
            if (showEndingDebug)
                Debug.Log($"GameEndingMonitor: Loading main menu scene: {mainMenuSceneName}");
            
            try
            {
                SceneManager.LoadScene(mainMenuSceneName);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"GameEndingMonitor: Failed to load main menu scene '{mainMenuSceneName}': {e.Message}");
                SceneManager.LoadScene(0);
            }
            
            endingSequenceCompleted = true;
            OnEndingSequenceCompleted?.Invoke();
        }
        
        private bool HasSuratInteractionCompleted_ByObject(InteractionObject obj)
        {
            if (obj == null)
                return false;
            
            if (obj.interactionType == InteractionType.SimpleInteraction)
            {
                // Untuk SimpleInteraction, object akan di-destroy setelah interaction selesai
                // Tapi kita perlu memastikan bahwa object memang sudah di-interact, bukan hanya tidak aktif
                return obj.gameObject == null || !obj.gameObject.activeInHierarchy;
            }
            else if (obj.interactionType == InteractionType.ExtractableObject)
            {
                // Untuk ExtractableObject, gunakan property IsExtracted
                return obj.IsExtracted;
            }
            
            return false;
        }
        
        private void ValidateSetup()
        {
            bool hasErrors = false;
            
            if (takauAI == null)
            {
                Debug.LogError("GameEndingMonitor: TakauAI reference is missing!");
                hasErrors = true;
            }
            
            if (suratGameObject == null)
            {
                Debug.LogError("GameEndingMonitor: Surat GameObject reference is missing!");
                hasErrors = true;
            }
            
            if (sleepingCharacter == null)
            {
                Debug.LogWarning("GameEndingMonitor: Sleeping character reference is missing!");
            }
            
            if (deathScreenEffect == null)
            {
                Debug.LogError("GameEndingMonitor: DeathScreenEffect reference is missing!");
                hasErrors = true;
            }
            
            if (string.IsNullOrEmpty(mainMenuSceneName))
            {
                Debug.LogError("GameEndingMonitor: Main menu scene name is empty!");
                hasErrors = true;
            }
            
            if (hasErrors)
            {
                Debug.LogError("GameEndingMonitor: Setup validation failed! Please fix the errors above.");
            }
            else if (showEndingDebug)
            {
                Debug.Log("GameEndingMonitor: Setup validation passed - all references are valid!");
            }
        }
        
        // Public methods for manual control
        public void ForceEndingSequence()
        {
            if (showEndingDebug)
                Debug.Log("GameEndingMonitor: Force ending sequence called!");
            
            StartCoroutine(StartEndingSequence());
        }
        
        public void ResetMonitor()
        {
            if (showEndingDebug)
                Debug.Log("GameEndingMonitor: Resetting monitor state");
            
            takauIsActive = false;
            takauIsDead = false;
            suratIsActive = false;
            sleepingCharacterDeactivated = false;
            suratInteractionCompleted = false;
            endingSequenceStarted = false;
            endingSequenceCompleted = false;
            isMonitoringInteraction = false;
            
            // Reset tracking variables
            missingObjectCount = 0;
            wasInteractionObjectFound = false;
            trackedSuratObject = null;
        }
        
        // NEW: Method to manually trigger interaction monitoring (for testing)
        public void StartMonitoringInteraction()
        {
            if (showEndingDebug)
                Debug.Log("GameEndingMonitor: Manually starting interaction monitoring");
            
            StartCoroutine(StartInteractionMonitoring());
        }
        
        // Debug methods
        [ContextMenu("Debug - Force Takau Death")]
        private void DebugForceTakauDeath()
        {
            if (takauAI != null)
            {
                takauAI.Dying();
                Debug.Log("GameEndingMonitor: Debug - Forced Takau death");
            }
        }
        
        [ContextMenu("Debug - Force Ending Sequence")]
        private void DebugForceEndingSequence()
        {
            ForceEndingSequence();
        }
        
        [ContextMenu("Debug - Show Current State")]
        private void DebugShowCurrentState()
        {
            Debug.Log($"=== GAME ENDING MONITOR STATE ===");
            Debug.Log($"Takau Active: {takauIsActive}");
            Debug.Log($"Takau Dead: {takauIsDead}");
            Debug.Log($"Surat Active: {suratIsActive}");
            Debug.Log($"Sleeping Character Deactivated: {sleepingCharacterDeactivated}");
            Debug.Log($"Surat Interaction Completed: {suratInteractionCompleted}");
            Debug.Log($"Ending Sequence Started: {endingSequenceStarted}");
            Debug.Log($"Ending Sequence Completed: {endingSequenceCompleted}");
            Debug.Log($"Is Monitoring Interaction: {isMonitoringInteraction}");
            Debug.Log($"Missing Object Count: {missingObjectCount}");
            Debug.Log($"Was Interaction Object Found: {wasInteractionObjectFound}");
        }
        
        [ContextMenu("Debug - Check Surat Objects")]
        private void DebugCheckSuratObjects()
        {
            Debug.Log($"=== CHECKING SURAT OBJECTS ===");
            
            InteractionObject currentSurat = GetSuratInteractionObject();
            if (currentSurat != null)
            {
                Debug.Log($"Found Surat Object: {currentSurat.objectName}");
                Debug.Log($"- Active: {currentSurat.gameObject.activeInHierarchy}");
                Debug.Log($"- Type: {currentSurat.interactionType}");
                Debug.Log($"- Is Ending Object: {currentSurat.isEndingObject}");
                Debug.Log($"- Can Interact: {currentSurat.CanInteract}");
                if (currentSurat.interactionType == InteractionType.ExtractableObject)
                {
                    Debug.Log($"- Is Extracted: {currentSurat.IsExtracted}");
                }
            }
            else
            {
                Debug.Log("No Surat object found using current detection methods");
            }
            
            // Check all ending objects
            InteractionObject[] allInteractionObjects = FindObjectsOfType<InteractionObject>();
            Debug.Log($"Found {allInteractionObjects.Length} InteractionObjects in scene");
            
            foreach (InteractionObject obj in allInteractionObjects)
            {
                if (obj.isEndingObject)
                {
                    Debug.Log($"- Ending Object: {obj.objectName} ({obj.gameObject.name})");
                    Debug.Log($"  - Active: {obj.gameObject.activeInHierarchy}");
                    Debug.Log($"  - Type: {obj.interactionType}");
                    Debug.Log($"  - Can Interact: {obj.CanInteract}");
                    if (obj.interactionType == InteractionType.ExtractableObject)
                    {
                        Debug.Log($"  - Is Extracted: {obj.IsExtracted}");
                    }
                }
            }
        }
        
        [ContextMenu("Debug - Skip To Surat")]
        private void DebugSkipToSurat()
        {
            Debug.Log("GameEndingMonitor: Debug - Skipping to Surat phase");
            
            // Force set states
            takauIsActive = true;
            takauIsDead = true;
            suratIsActive = true;
            sleepingCharacterDeactivated = true;
            
            // Activate Surat GameObject
            if (suratGameObject != null)
            {
                suratGameObject.SetActive(true);
                Debug.Log("Surat GameObject activated");
            }
            
            // Deactivate sleeping character
            if (sleepingCharacter != null)
            {
                sleepingCharacter.SetActive(false);
                Debug.Log("Sleeping character deactivated");
            }
            
            // Start monitoring interaction
            StartCoroutine(StartInteractionMonitoring());
            
            Debug.Log("Now interact with Surat object to trigger ending sequence");
        }
        
        [ContextMenu("Debug - Start Interaction Monitoring")]
        private void DebugStartInteractionMonitoring()
        {
            StartMonitoringInteraction();
        }
    }
}