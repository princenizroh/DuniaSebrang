// // AnimationDrivenInteractionObject.cs
// using UnityEngine;
// using DS.Data.Dialog;
// using System.Collections;

// namespace DS
// {
//     public class AnimationDrivenInteractionObject : MonoBehaviour
//     {
//         [Header("Animation Settings")]
//         [SerializeField] private Animator playerAnimator;
//         [SerializeField] private string extractionAnimationName = "ExtractingObject";
//         [SerializeField] private string idleAnimationName = "Idle";
//         [SerializeField] private string reverseAnimationName = "ReverseReaching";
        
//         [Header("Extraction Settings")]
//         [SerializeField] private int totalExtractionSteps = 6;
//         [SerializeField] private float stepTimeout = 3f; // Timeout untuk menunggu input berikutnya
        
//         [Header("Object Settings")]
//         public string objectName = "Object";
//         public DialogData dialogData;
//         [SerializeField] private bool isHoldable = true;
//         [SerializeField] private bool moveToHand = true;
        
//         // State management
//         private int currentStep = 0;
//         private bool isExtracting = false;
//         private bool hasBeenExtracted = false;
//         private bool waitingForInput = false;
//         private PlayerInteractionHandler currentPlayer;
        
//         // Animation state tracking
//         private bool animationPaused = false;
//         private float currentAnimationTime = 0f;
//         private float[] animationStepPoints; // Timeline points untuk setiap step
        
//         // Original transform data
//         private Transform originalParent;
//         private Vector3 originalPosition;
//         private Quaternion originalRotation;
        
//         // Coroutine references
//         private Coroutine extractionCoroutine;
//         private Coroutine timeoutCoroutine;
        
//         public bool CanInteract => !isExtracting && !hasBeenExtracted && gameObject != null;
        
//         private void Awake()
//         {
//             // Setup animation step points (normalized time 0-1)
//             // Ini akan disesuaikan dengan animasi yang Anda buat
//             animationStepPoints = new float[totalExtractionSteps];
//             for (int i = 0; i < totalExtractionSteps; i++)
//             {
//                 animationStepPoints[i] = (float)(i + 1) / totalExtractionSteps;
//             }
//         }
        
//         public void StartInteraction(PlayerInteractionHandler player)
//         {
//             if (!CanInteract) return;
            
//             currentPlayer = player;
//             isExtracting = true;
//             currentStep = 0;
            
//             // Store original transform
//             if (isHoldable && moveToHand)
//             {
//                 originalParent = transform.parent;
//                 originalPosition = transform.position;
//                 originalRotation = transform.rotation;
//             }
            
//             // Start extraction process
//             extractionCoroutine = StartCoroutine(HandleAnimationDrivenExtraction());
//         }
        
//         private IEnumerator HandleAnimationDrivenExtraction()
//         {
//             // Play dialog first if available
//             if (dialogData != null && dialogData.dialogLines.Count > 0)
//             {
//                 yield return StartCoroutine(PlayAllDialogLines());
//             }
            
//             // Move object to hand
//             if (isHoldable && moveToHand)
//             {
//                 currentPlayer.MoveObjectToHand(gameObject);
//             }
            
//             // Start extraction animation
//             if (playerAnimator != null)
//             {
//                 playerAnimator.Play(extractionAnimationName);
//                 // Pause animation immediately at the start
//                 playerAnimator.speed = 0f;
//             }
            
//             Debug.Log($"Starting extraction of {objectName}. Press E to continue the animation!");
            
//             // Wait for first input to start
//             waitingForInput = true;
//             yield return new WaitUntil(() => !waitingForInput || !isExtracting);
            
//             // Process each step
//             for (int step = 0; step < totalExtractionSteps && isExtracting; step++)
//             {
//                 currentStep = step;
                
//                 // Play animation segment
//                 yield return StartCoroutine(PlayAnimationSegment(step));
                
//                 // If not the last step, wait for next input
//                 if (step < totalExtractionSteps - 1 && isExtracting)
//                 {
//                     Debug.Log($"Step {step + 1}/{totalExtractionSteps} complete. Press E to continue!");
//                     waitingForInput = true;
                    
//                     // Start timeout coroutine
//                     timeoutCoroutine = StartCoroutine(HandleStepTimeout());
                    
//                     // Wait for input or timeout
//                     yield return new WaitUntil(() => !waitingForInput || !isExtracting);
                    
//                     // Stop timeout coroutine
//                     if (timeoutCoroutine != null)
//                     {
//                         StopCoroutine(timeoutCoroutine);
//                         timeoutCoroutine = null;
//                     }
//                 }
//             }
            
//             // Complete extraction if all steps done
//             if (isExtracting && currentStep >= totalExtractionSteps - 1)
//             {
//                 yield return StartCoroutine(CompleteExtraction());
//             }
//         }
        
//         private IEnumerator PlayAnimationSegment(int stepIndex)
//         {
//             if (playerAnimator == null) yield break;
            
//             // Calculate start and end times for this segment
//             float startTime = stepIndex == 0 ? 0f : animationStepPoints[stepIndex - 1];
//             float endTime = animationStepPoints[stepIndex];
            
//             // Set animation to start time
//             playerAnimator.Play(extractionAnimationName, 0, startTime);
//             playerAnimator.speed = 1f; // Resume normal speed
            
//             // Wait for animation to reach end time
//             while (playerAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < endTime && isExtracting)
//             {
//                 yield return null;
//             }
            
//             // Pause animation at end of segment
//             playerAnimator.speed = 0f;
//         }
        
//         private IEnumerator HandleStepTimeout()
//         {
//             yield return new WaitForSeconds(stepTimeout);
            
//             if (waitingForInput && isExtracting)
//             {
//                 Debug.Log($"Timeout! Extraction cancelled due to inactivity.");
//                 CancelExtraction();
//             }
//         }
        
//         private IEnumerator CompleteExtraction()
//         {
//             Debug.Log($"{objectName} extraction complete!");
            
//             // Let animation finish completely
//             if (playerAnimator != null)
//             {
//                 playerAnimator.speed = 1f;
//                 yield return new WaitForSeconds(0.5f); // Let it finish
//             }
            
//             // Clear player's held object
//             currentPlayer.ClearHeldObject();
            
//             // Play reverse animation
//             if (playerAnimator != null)
//             {
//                 playerAnimator.Play(reverseAnimationName);
//                 yield return new WaitForSeconds(2f);
//             }
            
//             // Play idle animation
//             if (playerAnimator != null)
//             {
//                 playerAnimator.Play(idleAnimationName);
//             }
            
//             // Destroy object
//             if (isHoldable && moveToHand)
//             {
//                 Destroy(gameObject);
//             }
            
//             hasBeenExtracted = true;
//             isExtracting = false;
            
//             // Notify player
//             currentPlayer.OnInteractionComplete();
//         }
        
//         public void ProcessExtractionInput()
//         {
//             if (isExtracting && waitingForInput)
//             {
//                 waitingForInput = false;
//                 Debug.Log($"Input received for step {currentStep + 1}!");
//             }
//         }
        
//         public void CancelExtraction()
//         {
//             if (isExtracting)
//             {
//                 Debug.Log($"Extraction of {objectName} cancelled!");
                
//                 // Stop all coroutines
//                 if (extractionCoroutine != null)
//                 {
//                     StopCoroutine(extractionCoroutine);
//                     extractionCoroutine = null;
//                 }
                
//                 if (timeoutCoroutine != null)
//                 {
//                     StopCoroutine(timeoutCoroutine);
//                     timeoutCoroutine = null;
//                 }
                
//                 // Reset state
//                 isExtracting = false;
//                 waitingForInput = false;
//                 currentStep = 0;
                
//                 // Return object to original position
//                 if (isHoldable && moveToHand)
//                 {
//                     Collider objCollider = GetComponent<Collider>();
//                     if (objCollider != null)
//                         objCollider.enabled = true;
                    
//                     transform.SetParent(originalParent);
//                     transform.position = originalPosition;
//                     transform.rotation = originalRotation;
//                 }
                
//                 // Reset animation
//                 if (playerAnimator != null)
//                 {
//                     playerAnimator.speed = 1f;
//                     playerAnimator.Play(reverseAnimationName);
//                 }
                
//                 // Clear player's held object
//                 currentPlayer.ClearHeldObject();
                
//                 // Start cancellation animation
//                 StartCoroutine(HandleCancellationAnimation());
//             }
//         }
        
//         private IEnumerator HandleCancellationAnimation()
//         {
//             // Wait for reverse animation
//             yield return new WaitForSeconds(2f);
            
//             // Play idle
//             if (playerAnimator != null)
//             {
//                 playerAnimator.Play(idleAnimationName);
//             }
            
//             // Notify player
//             currentPlayer.OnInteractionComplete();
//         }
        
//         private IEnumerator PlayAllDialogLines()
//         {
//             if (dialogData == null || dialogData.dialogLines.Count == 0) yield break;
            
//             for (int i = 0; i < dialogData.dialogLines.Count; i++)
//             {
//                 DS.DialogManager.Instance?.PlaySpecificLine(dialogData, i);
//                 yield return new WaitForSeconds(dialogData.dialogLines[i].duration);
//             }
//         }
        
//         // Animation Events - Panggil dari Animation Timeline
//         public void OnExtractionStepReached(int stepIndex)
//         {
//             Debug.Log($"Animation reached step {stepIndex + 1}");
//             // Bisa digunakan untuk efek visual tambahan, suara, dll
//         }
        
//         public void OnExtractionComplete()
//         {
//             Debug.Log("Animation signals extraction complete!");
//             // Dipanggil dari animation event di akhir animasi
//         }
//     }
// }
