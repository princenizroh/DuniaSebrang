using UnityEngine;
using System.Collections;

namespace DS
{
    public class PlayerInteractionHandler : MonoBehaviour
    {
        [Header("Interaction Settings")]
        [SerializeField] private LayerMask interactionLayer = 1;
        [SerializeField] private float interactionRange = 2f;
        [SerializeField] private KeyCode interactionKey = KeyCode.E;
        
        [Header("Animation")]
        [SerializeField] private Animator playerAnimator;
        [SerializeField] private string reachingAnimationName = "Reaching";
        [SerializeField] private string reverseReachingAnimationName = "Reverse Reaching";
        [SerializeField] private string idleAnimationName = "Idle";
        
        [Header("Player Control Settings")]
        [SerializeField] private bool disableMovementOnInteraction = true;
        [SerializeField] private bool disableInputOnInteraction = true;
        private Rigidbody playerRigidbody;
        
        // Animation override system
        private bool forceAnimationOverride = false;
        
        [Header("Holdable Object")]
        [SerializeField] private Transform handTransform; // Assign the hand bone/transform
        
        [Header("UI")]
        [SerializeField] private GameObject interactionPrompt;
        [SerializeField] private TMPro.TextMeshProUGUI interactionText;
        
        private InteractionObject currentInteractionObject;
        private bool isInteracting = false;
        private bool isMoving = false;
        private GameObject currentHeldObject;
        private MonoBehaviour[] playerScripts;
        
        // Box collider trigger for interaction detection
        private BoxCollider interactionTrigger;
        
        private void Awake()
        {
            playerRigidbody = GetComponent<Rigidbody>();
            playerScripts = GetComponents<MonoBehaviour>();
        }
        
        private void Start()
        {
            SetupInteractionTrigger();
            if (interactionPrompt != null)
                interactionPrompt.SetActive(false);
        }
        
        private void SetupInteractionTrigger()
        {
            // Create interaction trigger in front of player
            GameObject triggerObj = new GameObject("InteractionTrigger");
            triggerObj.transform.SetParent(transform);
            triggerObj.transform.localPosition = Vector3.forward * (interactionRange / 2);
            
            interactionTrigger = triggerObj.AddComponent<BoxCollider>();
            interactionTrigger.isTrigger = true;
            interactionTrigger.size = new Vector3(1f, 1f, interactionRange);
            
            // Add trigger handler
            InteractionTriggerHandler triggerHandler = triggerObj.AddComponent<InteractionTriggerHandler>();
            triggerHandler.Initialize(this);
        }
        
        private void Update()
        {
            // Only check movement if player controls are enabled
            if (!isInteracting)
            {
                CheckMovement();
            }
            else
            {
                // If we're in an extractable interaction, still check for movement to cancel
                if (currentInteractionObject != null && 
                    currentInteractionObject.interactionType == InteractionType.ExtractableObject)
                {
                    CheckMovement();
                }
            }
            
            HandleInteractionInput();
            
            // Handle animation override system
            HandleAnimationOverride();
        }
        
        private void HandleAnimationOverride()
        {
            // Force animation updates when movement is detected during interaction
            if (forceAnimationOverride && playerAnimator != null)
            {
                // If player is moving but animation is stuck, force transition
                if (isMoving && !isInteracting)
                {
                    playerAnimator.Play(idleAnimationName);
                    forceAnimationOverride = false;
                }
            }
        }
        
        private void CheckMovement()
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            
            bool wasMoving = isMoving;
            isMoving = Mathf.Abs(horizontal) > 0.1f || Mathf.Abs(vertical) > 0.1f;
            
            // For extractable objects, allow movement cancellation
            if (isMoving && !wasMoving && isInteracting && 
                currentInteractionObject != null && 
                currentInteractionObject.interactionType == InteractionType.ExtractableObject)
            {
                Debug.Log("Movement detected during extraction. Cancelling interaction...");
                CancelCurrentInteraction();
            }
        }
        
        private void HandleInteractionInput()
        {
            if (Input.GetKeyDown(interactionKey))
            {
                if (currentInteractionObject != null && !isInteracting)
                {
                    // Start new interaction
                    StartInteraction();
                }
                else if (isInteracting && currentInteractionObject != null)
                {
                    // Continue extractable interaction
                    if (currentInteractionObject.interactionType == InteractionType.ExtractableObject)
                    {
                        currentInteractionObject.ProcessExtraction();
                    }
                }
            }
        }
        
        private void StartInteraction()
        {
            if (currentInteractionObject != null && currentInteractionObject.CanInteract && !isInteracting)
            {
                Debug.Log($"Starting interaction with {currentInteractionObject.objectName}");
                
                isInteracting = true;
                
                // Disable player controls based on interaction type
                if (currentInteractionObject.interactionType == InteractionType.SimpleInteraction)
                {
                    DisablePlayerControls();
                }
                // For ExtractableObject, don't disable controls to allow cancellation by movement
                
                currentInteractionObject.StartInteraction(this);
                UpdateInteractionPrompt();
            }
            else
            {
                Debug.Log($"Cannot start interaction - Object: {currentInteractionObject?.objectName}, isInteracting: {isInteracting}, CanInteract: {currentInteractionObject?.CanInteract}");
            }
        }
        
        private void CancelCurrentInteraction()
        {
            if (currentInteractionObject != null && isInteracting)
            {
                Debug.Log($"Movement detected - cancelling interaction with {currentInteractionObject.objectName}");
                
                // Force animation override before cancelling
                forceAnimationOverride = true;
                
                // Cancel the interaction object first
                currentInteractionObject.CancelInteraction(this);
                
                // Set our state
                isInteracting = false;
                
                // Re-enable player controls
                EnablePlayerControls();
                
                // Update UI
                UpdateInteractionPrompt();
                
                // Force immediate animation transition
                StartCoroutine(ForceAnimationTransition());
            }
        }
        
        private IEnumerator ForceAnimationTransition()
        {
            // Wait a tiny bit for the cancel to process
            yield return new WaitForSeconds(0.1f);
            
            // Force reverse reaching animation
            PlayReverseReachingAnimation();
            
            // Wait for reverse reaching to complete
            yield return new WaitForSeconds(2f);
            
            // Force idle animation
            PlayIdleAnimation();
            
            // Reset override flag
            forceAnimationOverride = false;
        }
        
        public void OnInteractionComplete()
        {
            Debug.Log("Interaction completed");
            
            // Called when interaction is finished
            isInteracting = false;
            EnablePlayerControls();
            UpdateInteractionPrompt();
            
            // Clear held object reference (but don't destroy - let the interaction object handle that)
            if (currentHeldObject != null)
            {
                Debug.Log($"Clearing held object reference: {currentHeldObject.name}");
                currentHeldObject = null;
            }
        }
        
        public void PlayReachingAnimation()
        {
            if (playerAnimator != null)
            {
                forceAnimationOverride = false; // Reset override flag
                playerAnimator.Play(reachingAnimationName);
            }
        }
        
        public void PlayReverseReachingAnimation()
        {
            if (playerAnimator != null)
            {
                forceAnimationOverride = true; // Set override flag
                playerAnimator.Play(reverseReachingAnimationName);
            }
        }
        
        public void PlayIdleAnimation()
        {
            if (playerAnimator != null)
            {
                forceAnimationOverride = true; // Set override flag
                playerAnimator.Play(idleAnimationName);
            }
        }
        
        public GameObject CreateHoldableObject(GameObject prefab)
        {
            if (prefab != null && handTransform != null)
            {
                // Destroy existing held object if any
                if (currentHeldObject != null)
                {
                    Destroy(currentHeldObject);
                }
                
                // Create new held object
                currentHeldObject = Instantiate(prefab, handTransform);
                currentHeldObject.transform.localPosition = Vector3.zero;
                currentHeldObject.transform.localRotation = Quaternion.identity;
                
                return currentHeldObject;
            }
            return null;
        }
        
        public void MoveObjectToHand(GameObject obj)
        {
            if (obj != null && handTransform != null)
            {
                // Clear any existing held object first
                ClearHeldObject();
                
                // Store as current held object
                currentHeldObject = obj;
                
                // Move to hand
                obj.transform.SetParent(handTransform);
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localRotation = Quaternion.identity;
                
                // Disable collider to prevent interference
                Collider objCollider = obj.GetComponent<Collider>();
                if (objCollider != null)
                {
                    objCollider.enabled = false;
                }
                
                Debug.Log($"Moved {obj.name} to hand");
            }
        }
        
        public void ClearHeldObject()
        {
            if (currentHeldObject != null)
            {
                Debug.Log($"Clearing held object: {currentHeldObject.name}");
                currentHeldObject = null;
            }
        }
        
        public void DestroyHeldObject()
        {
            if (currentHeldObject != null)
            {
                Debug.Log($"Destroying held object: {currentHeldObject.name}");
                Destroy(currentHeldObject);
                currentHeldObject = null;
            }
        }
        
        public void OnInteractionObjectEnter(InteractionObject interactionObject)
        {
            if (currentInteractionObject == null)
            {
                currentInteractionObject = interactionObject;
                UpdateInteractionPrompt();
            }
        }
        
        public void OnInteractionObjectExit(InteractionObject interactionObject)
        {
            if (currentInteractionObject == interactionObject)
            {
                if (isInteracting)
                {
                    CancelCurrentInteraction();
                }
                
                currentInteractionObject = null;
                UpdateInteractionPrompt();
            }
        }
        
        private void UpdateInteractionPrompt()
        {
            if (interactionPrompt == null) return;
            
            if (currentInteractionObject != null && currentInteractionObject.CanInteract)
            {
                interactionPrompt.SetActive(true);
                
                if (interactionText != null)
                {
                    string promptText = "";
                    
                    if (currentInteractionObject.interactionType == InteractionType.SimpleInteraction)
                    {
                        promptText = $"Press E to interact with {currentInteractionObject.objectName}";
                    }
                    else if (currentInteractionObject.interactionType == InteractionType.ExtractableObject)
                    {
                        if (isInteracting)
                        {
                            promptText = $"Keep pressing E to extract {currentInteractionObject.objectName}";
                        }
                        else
                        {
                            promptText = $"Press E to extract {currentInteractionObject.objectName}";
                        }
                    }
                    
                    interactionText.text = promptText;
                }
            }
            else
            {
                interactionPrompt.SetActive(false);
            }
        }
        
        private void DisablePlayerControls()
        {
            if (disableMovementOnInteraction)
            {
                // Stop player movement
                if (playerRigidbody != null)
                {
                    playerRigidbody.linearVelocity = Vector3.zero;
                    playerRigidbody.angularVelocity = Vector3.zero;
                    playerRigidbody.isKinematic = true; // Prevent physics movement
                }
            }
            
            if (disableInputOnInteraction)
            {
                // Disable specific player control scripts
                DisablePlayerScripts();
            }
        }
        
        private void EnablePlayerControls()
        {
            if (disableMovementOnInteraction)
            {
                // Re-enable physics movement
                if (playerRigidbody != null)
                {
                    playerRigidbody.isKinematic = false;
                }
            }
            
            if (disableInputOnInteraction)
            {
                // Re-enable player control scripts
                EnablePlayerScripts();
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
                "MouseLook",
                "CameraController",
                "PlayerLook",
                "FPSController",
                "CharacterController",
                "PlayerCameraController"
            };
            
            foreach (string scriptName in scriptsToDisable)
            {
                // Cari di self, parent, dan root
                Component script = GetComponent(scriptName) ??
                                  (transform.parent ? transform.parent.GetComponent(scriptName) : null) ??
                                  transform.root.GetComponent(scriptName);
                if (script != null && script is MonoBehaviour && script != this)
                {
                    ((MonoBehaviour)script).enabled = false;
                }
            }
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
                "MouseLook",
                "CameraController",
                "PlayerLook",
                "FPSController",
                "CharacterController",
                "PlayerCameraController"
            };
            foreach (string scriptName in scriptsToEnable)
            {
                // Cari di self, parent, dan root
                Component script = GetComponent(scriptName) ??
                                  (transform.parent ? transform.parent.GetComponent(scriptName) : null) ??
                                  transform.root.GetComponent(scriptName);
                if (script != null && script is MonoBehaviour && script != this)
                {
                    ((MonoBehaviour)script).enabled = true;
                }
            }
        }
    }
}