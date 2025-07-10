using UnityEngine;
using DS.Data.Dialog;
using System.Collections;
using DS; // Tambahkan agar bisa akses ColliderActiveBlockPlayerCollision

namespace DS
{
    public enum InteractionType
    {
        SimpleInteraction,
        ExtractableObject
    }

    public class InteractionObject : MonoBehaviour
    {
        [Header("Interaction Settings")]
        public InteractionType interactionType = InteractionType.SimpleInteraction;
        public string objectName = "Object";
        public DialogData dialogData;
        
        [Header("Extractable Object Settings")]
        [SerializeField] private int extractionCount = 6;
        [SerializeField] private float extractionDelay = 0.3f; // Reduced delay for more responsive extraction
        
        [Header("Holdable Object Settings")]
        [SerializeField] private bool isHoldable = true;
        [SerializeField] private bool moveToHand = true; // Move this object to hand instead of creating clone
        [SerializeField] private float postDialogDelay = 2f; // Delay after dialog finishes
        
        [Header("Objective Settings")]
        [SerializeField] private bool willKillKunti = false; 
        [SerializeField] private bool willKillTakau = false; 
        
        [Header("VFX Settings")]
        [SerializeField] private ParticleSystem extractionParticleEffect;
        [SerializeField] private ParticleSystem extractionCompleteEffect;
        [SerializeField] private ColliderActiveBlockPlayerCollision blockHandler;
        
        [Header("Ending Settings")]
        public bool isEndingObject = false;
        
        private int currentExtractionCount = 0;
        private bool isBeingInteracted = false;
        private bool hasBeenInteracted = false;
        private bool isCancelling = false; // Flag to prevent new interactions during cancellation
        private bool isProcessingInteraction = false; // Flag to prevent rapid re-interaction
        private bool isExtracted = false; // Tambahan deklarasi field isExtracted
        
        // Store original transform data for cancellation
        private Transform originalParent;
        private Vector3 originalPosition;
        private Quaternion originalRotation;
        private bool originalTransformStored = false;
        
        // Coroutine references for cleanup
        private Coroutine currentInteractionCoroutine;
        
        // Modify CanInteract property to prevent interaction with completed objects
        public bool CanInteract
        {
            get
            {
                if (interactionType == InteractionType.ExtractableObject)
                {
                    return !isExtracted; // Can't interact if already extracted
                }
                return true;
            }
        }
        public bool IsExtracted
        {
            get { return isExtracted; }
        }


        [Header("Extraction Manager Integration")]
        [SerializeField] private bool isPartOfExtractionChain = false; // Apakah object ini bagian dari chain 5 pasak
        [SerializeField] private bool registerWithExtractionManager = true;
        private void Start()
        {
            // Setup particle effect jika ada
            SetupParticleEffect();
        }

        public void StartInteraction(PlayerInteractionHandler player)
        {
            if (!CanInteract) 
            {
                Debug.LogWarning($"Cannot start interaction with {objectName} - CanInteract: {CanInteract}, isBeingInteracted: {isBeingInteracted}, isCancelling: {isCancelling}, isProcessingInteraction: {isProcessingInteraction}");
                return;
            }
            
            // Set all interaction flags
            isBeingInteracted = true;
            isProcessingInteraction = true;
            isCancelling = false;
            
            // Stop any existing coroutines
            if (currentInteractionCoroutine != null)
            {
                StopCoroutine(currentInteractionCoroutine);
            }
            
            switch (interactionType)
            {
                case InteractionType.SimpleInteraction:
                    currentInteractionCoroutine = StartCoroutine(HandleSimpleInteraction(player));
                    break;
                case InteractionType.ExtractableObject:
                    currentInteractionCoroutine = StartCoroutine(HandleExtractableInteraction(player));
                    break;
            }
        }
        
        private IEnumerator HandleSimpleInteraction(PlayerInteractionHandler player)
        {
            try
            {
                // Play reaching animation
                player.PlayReachingAnimation();
                
                // Wait for reaching animation to start
                yield return new WaitForSeconds(0.5f);
                
                // Check if cancelled during wait
                if (isCancelling) yield break;
                
                // Move object to hand if available
                Transform originalParent = null;
                Vector3 originalPosition = Vector3.zero;
                Quaternion originalRotation = Quaternion.identity;
                
                if (isHoldable && moveToHand)
                {
                    // Store original transform data
                    originalParent = transform.parent;
                    originalPosition = transform.position;
                    originalRotation = transform.rotation;
                    
                    // Move to hand
                    player.MoveObjectToHand(gameObject);
                }
                
                // Check if cancelled after moving to hand
                if (isCancelling) yield break;
                
                // Play all dialog lines if available
                if (dialogData != null && dialogData.dialogLines.Count > 0)
                {
                    yield return StartCoroutine(PlayAllDialogLines());
                }
                
                // Check if cancelled during dialog
                if (isCancelling) yield break;
                
                // Post-dialog delay
                yield return new WaitForSeconds(postDialogDelay);
                
                // Check if cancelled during delay
                if (isCancelling) yield break;
                
                // Play reverse reaching animation
                player.PlayReverseReachingAnimation();
                
                // Wait for reverse reaching animation to complete
                yield return new WaitForSeconds(2f); // Give more time for reverse reaching
                
                // Check if cancelled during animation
                if (isCancelling) yield break;
                
                // Return object to original position or destroy it
                if (isHoldable && moveToHand)
                {
                    // Option 1: Return to original position
                    // transform.SetParent(originalParent);
                    // transform.position = originalPosition;
                    // transform.rotation = originalRotation;
                    
                    // Option 2: Destroy the object after interaction
                    Destroy(gameObject);
                }
                
                // Play idle animation
                player.PlayIdleAnimation();
                
                hasBeenInteracted = true;
                
                // Notify player that interaction is complete
                player.OnInteractionComplete();
            }
            finally
            {
                // Always cleanup state
                CleanupInteractionState();
            }
        }
        
        private IEnumerator HandleExtractableInteraction(PlayerInteractionHandler player)
        {
            try
            {
                // Play reaching animation and hold it
                player.PlayReachingAnimation();
                
                // Wait for reaching animation to start
                yield return new WaitForSeconds(0.5f);
                
                // Check if cancelled during wait
                if (isCancelling) yield break;
                
                // Store original transform data for potential cancellation
                if (isHoldable && moveToHand)
                {
                    originalParent = transform.parent;
                    originalPosition = transform.position;
                    originalRotation = transform.rotation;
                    originalTransformStored = true;
                }
                
                // Check if cancelled after moving to hand
                if (isCancelling) yield break;
                
                // Play all dialog lines if available (tidak menunggu dialog selesai)
                if (dialogData != null && dialogData.dialogLines.Count > 0 && currentExtractionCount == 0)
                {
                    StartCoroutine(PlayAllDialogLines()); // Tidak menunggu dengan yield
                }
                
                // Check if cancelled before starting extraction
                if (isCancelling) yield break;
                
                // Langsung mulai extraction process tanpa post-dialog delay
                Debug.Log($"Starting extraction process for {objectName}. Press E repeatedly to extract!");
                
                // Wait for extraction process - check both conditions properly
                while (currentExtractionCount < extractionCount && isBeingInteracted && !isCancelling)
                {
                    yield return new WaitForSeconds(extractionDelay);
                    
                    // Additional safety check - if object is destroyed or player moved away
                    if (gameObject == null || !isBeingInteracted || isCancelling)
                    {
                        break;
                    }
                }
                
                // Check if cancelled during extraction
                if (isCancelling) yield break;
                
                // Only handle completion if still being interacted and not cancelled
                if (isBeingInteracted && !isCancelling && currentExtractionCount >= extractionCount && gameObject != null)
                {
                    // Object extracted successfully
                    Debug.Log($"{objectName} extraction complete!");
                    // TAMBAHAN: Play extraction complete effect
                    PlayExtractionCompleteEffect();

                    // Pindahkan objek ke tangan setelah extraction selesai
                    if (isHoldable && moveToHand)
                    {
                        player.MoveObjectToHand(gameObject);
                    }
                    if (isPartOfExtractionChain && registerWithExtractionManager)
                        {
                            // Register extraction dengan ExtractionManager
                            if (ExtractionManager.Instance != null)
                            {
                                ExtractionManager.Instance.RegisterExtraction(this);
                                Debug.Log($"Registered {objectName} extraction with ExtractionManager");
                            }
                            else
                            {
                                Debug.LogWarning($"ExtractionManager not found! Cannot register {objectName} extraction.");
                                
                                // Fallback: Handle individual killing if willKillKunti is true
                                if (willKillTakau)
                                {
                                    var kuntiAI = UnityEngine.Object.FindFirstObjectByType<DS.TakauAI>();
                                    if (kuntiAI != null)
                                    {
                                        kuntiAI.Dying();
                                    }
                                }
                            }
                        }
                        else
                        {
                            // Handle individual extraction (tidak bagian dari chain)
                            HandleIndividualExtraction();
                        }

                    // Play reverse reaching animation
                    player.PlayReverseReachingAnimation();

                    // Wait for reverse reaching animation to complete
                    yield return new WaitForSeconds(2f);

                    // Check if cancelled during animation
                    if (isCancelling) yield break;

                    // Play idle animation
                    player.PlayIdleAnimation();

                    // Destroy the object after successful extraction
                    if (isHoldable && moveToHand)
                    {
                        Destroy(gameObject);
                    }

                    // Object is now extracted
                    hasBeenInteracted = true;

                    // Notify player that interaction is complete
                    player.OnInteractionComplete();
                }
                else
                {
                    Debug.Log($"Extraction process ended - Cancelled: {isCancelling}, InteractionState: {isBeingInteracted}, Count: {currentExtractionCount}/{extractionCount}");
                }
            }
            finally
            {
                // Always cleanup state
                CleanupInteractionState();
                
            }
        }
        
        private void HandleIndividualExtraction()
        {
            // Jika ada ColliderActiveBlockPlayerCollision di scene, nonaktifkan block (pintu)
            if (blockHandler != null)
            {
                blockHandler.DisableBlock();
            }

            // Jika extraction ini memang untuk membunuh KuntiAI (individual)
            if (willKillKunti)
            {
                var kuntiAI = UnityEngine.Object.FindFirstObjectByType<DS.KuntiAI>();
                if (kuntiAI != null)
                {
                    kuntiAI.SetDyingMode();
                }
            }
        }
        private IEnumerator PlayAllDialogLines()
        {
            if (dialogData == null || dialogData.dialogLines.Count == 0) yield break;

            for (int i = 0; i < dialogData.dialogLines.Count; i++)
            {
                DS.DialogManager.Instance?.RequestDialog(dialogData, i);
                yield return new WaitForSeconds(dialogData.dialogLines[i].duration);
            }
        }
        public bool IsExtractionComplete()
        {
            if (interactionType == InteractionType.ExtractableObject)
            {
                return currentExtractionCount >= extractionCount;
            }
            return false;
        }

        
        public void ProcessExtraction()
        {
            if (interactionType != InteractionType.ExtractableObject) return;
            
            // FIX: Check if already complete
            if (IsExtractionComplete())
            {
                Debug.Log($"{objectName} extraction already complete!");
                return;
            }

            currentExtractionCount++;
            Debug.Log($"Extraction progress: {currentExtractionCount}/{extractionCount}");

            // Play particle effect setiap kali E dipencet
            PlayExtractionParticleEffect();

            // Update UI or visual feedback
            OnExtractionProgress?.Invoke(currentExtractionCount, extractionCount);

            // Check if extraction is complete
            if (currentExtractionCount >= extractionCount)
            {
                CompleteExtraction();
            }
        }
        private void CompleteExtraction()
        {
            // FIX: Add completion guard
            if (isExtracted)
            {
                Debug.Log($"{objectName} already extracted!");
                return;
            }

            Debug.Log($"{objectName} extraction complete!");
            isExtracted = true;
            
            // Move object to hand
            if (currentInteractionHandler != null)
            {
                currentInteractionHandler.MoveObjectToHand(gameObject);
            }

            // Trigger completion event
            OnExtractionComplete?.Invoke();
            
            // Notify extraction manager if registered
            if (isPartOfExtractionChain && extractionManager != null)
            {
                extractionManager.OnObjectExtracted(objectName);
            }

            // Complete the interaction
            if (currentInteractionHandler != null)
            {
                currentInteractionHandler.OnInteractionComplete();
            }
        }
        private void PlayExtractionParticleEffect()
        {
            if (extractionParticleEffect != null)
            {
                // Pastikan particle tidak looping
                var main = extractionParticleEffect.main;
                main.loop = false;

                // Stop particle terlebih dahulu jika masih playing (untuk reset)
                if (extractionParticleEffect.isPlaying)
                {
                    extractionParticleEffect.Stop();
                }

                // Play particle effect
                extractionParticleEffect.Play();

                Debug.Log($"Played extraction particle effect for {objectName}");
            }
            else
            {
                Debug.LogWarning($"No particle effect assigned to {objectName}!");
            }
        }
        private void SetupParticleEffect()
        {
            if (extractionParticleEffect != null)
            {
                // Pastikan particle tidak loop
                extractionParticleEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                var main = extractionParticleEffect.main;
                main.loop = false;
                main.playOnAwake = false; // Jangan auto play saat start
                main.duration = 0.5f; // 0.5 detik
                main.startLifetime = 1f; // 1 detik
                var emission = extractionParticleEffect.emission;
                emission.rateOverTime = 50f; // 50 particle per detik
                Debug.Log($"Particle effect setup complete for {objectName}");
            }
        }
        private void PlayExtractionCompleteEffect()
        {
            if (extractionCompleteEffect != null)
            {
                var main = extractionCompleteEffect.main;
                main.loop = false;
                
                if (extractionCompleteEffect.isPlaying)
                {
                    extractionCompleteEffect.Stop();
                }
                
                extractionCompleteEffect.Play();
                Debug.Log($"Played extraction complete effect for {objectName}");
            }
        }
        
        
        public void CancelInteraction(PlayerInteractionHandler player)
        {
            if (isBeingInteracted && !isCancelling)
            {
                Debug.Log($"Cancelling interaction with {objectName}. Progress lost!");

                // Set cancellation flag immediately
                isCancelling = true;

                // Stop any running coroutines
                if (currentInteractionCoroutine != null)
                {
                    StopCoroutine(currentInteractionCoroutine);
                    currentInteractionCoroutine = null;
                }

                // Reset extraction progress completely
                currentExtractionCount = 0;

                // Return object to original position if it was moved to hand
                if (isHoldable && moveToHand && originalTransformStored)
                {
                    // Re-enable collider if it was disabled
                    Collider objCollider = GetComponent<Collider>();
                    if (objCollider != null)
                    {
                        objCollider.enabled = true;
                    }

                    // Return to original position
                    transform.SetParent(originalParent);
                    transform.position = originalPosition;
                    transform.rotation = originalRotation;

                    // Clear stored transform data
                    originalTransformStored = false;
                    originalParent = null;
                }

                // Clear player's held object reference
                player.ClearHeldObject();

                // Immediately play reverse reaching animation
                player.PlayReverseReachingAnimation();

                // Start coroutine to handle animation transition
                StartCoroutine(HandleCancellationAnimation(player));
            }
        }
        
        private IEnumerator HandleCancellationAnimation(PlayerInteractionHandler player)
        {
            try
            {
                // Wait for reverse reaching animation to complete
                yield return new WaitForSeconds(2f);
                
                // Play idle animation
                player.PlayIdleAnimation();
                
                // Notify player that interaction is complete (cancelled)
                player.OnInteractionComplete();
            }
            finally
            {
                // Always cleanup state after cancellation
                CleanupInteractionState();
            }
        }
        
        private void CleanupInteractionState()
        {
            // Reset all flags
            isBeingInteracted = false;
            isCancelling = false;
            
            // Clear coroutine reference
            currentInteractionCoroutine = null;
            
            // Add a small delay before allowing new interactions to prevent rapid re-interaction
            StartCoroutine(ResetProcessingFlag());
        }
        
        private IEnumerator ResetProcessingFlag()
        {
            // Wait a bit to prevent rapid re-interaction
            yield return new WaitForSeconds(0.5f);
            isProcessingInteraction = false;
        }
        
        private IEnumerator PlayIdleAfterCancellation(PlayerInteractionHandler player, float delay)
        {
            yield return new WaitForSeconds(delay);
            player.PlayIdleAnimation();
            
            // Notify player that interaction is complete (cancelled)
            player.OnInteractionComplete();
        }
        
        private IEnumerator PlayIdleAfterDelay(PlayerInteractionHandler player, float delay)
        {
            yield return new WaitForSeconds(delay);
            player.PlayIdleAnimation();
        }

        // Tambahan deklarasi agar error hilang
        public event System.Action<int, int> OnExtractionProgress;
        public event System.Action OnExtractionComplete;
        private PlayerInteractionHandler currentInteractionHandler;
        private ExtractionManager extractionManager;
    }
}