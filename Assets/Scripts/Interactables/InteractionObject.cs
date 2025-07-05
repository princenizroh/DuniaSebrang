using UnityEngine;
using DS.Data.Dialog;
using System.Collections;

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
        [SerializeField] private float extractionDelay = 0.5f;
        
        [Header("Holdable Object Settings")]
        [SerializeField] private bool isHoldable = true;
        [SerializeField] private bool moveToHand = true; // Move this object to hand instead of creating clone
        [SerializeField] private float postDialogDelay = 2f; // Delay after dialog finishes
        
        private int currentExtractionCount = 0;
        private bool isBeingInteracted = false;
        private bool hasBeenInteracted = false;
        
        public bool CanInteract => !isBeingInteracted && (!hasBeenInteracted || interactionType == InteractionType.ExtractableObject);
        
        public void StartInteraction(PlayerInteractionHandler player)
        {
            if (!CanInteract) return;
            
            isBeingInteracted = true;
            
            switch (interactionType)
            {
                case InteractionType.SimpleInteraction:
                    StartCoroutine(HandleSimpleInteraction(player));
                    break;
                case InteractionType.ExtractableObject:
                    StartCoroutine(HandleExtractableInteraction(player));
                    break;
            }
        }
        
        private IEnumerator HandleSimpleInteraction(PlayerInteractionHandler player)
        {
            // Play reaching animation
            player.PlayReachingAnimation();
            
            // Wait for reaching animation to start
            yield return new WaitForSeconds(0.5f);
            
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
            
            // Play all dialog lines if available
            if (dialogData != null && dialogData.dialogLines.Count > 0)
            {
                yield return StartCoroutine(PlayAllDialogLines());
            }
            
            // Post-dialog delay
            yield return new WaitForSeconds(postDialogDelay);
            
            // Play reverse reaching animation
            player.PlayReverseReachingAnimation();
            
            // Wait for reverse reaching animation to complete
            yield return new WaitForSeconds(2f); // Give more time for reverse reaching
            
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
            isBeingInteracted = false;
            
            // Notify player that interaction is complete
            player.OnInteractionComplete();
        }
        
        private IEnumerator HandleExtractableInteraction(PlayerInteractionHandler player)
        {
            // Play reaching animation and hold it
            player.PlayReachingAnimation();
            
            // Wait for reaching animation to start
            yield return new WaitForSeconds(0.5f);
            
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
            
            // Play all dialog lines if available
            if (dialogData != null && dialogData.dialogLines.Count > 0 && currentExtractionCount == 0)
            {
                yield return StartCoroutine(PlayAllDialogLines());
            }
            
            // Post-dialog delay
            yield return new WaitForSeconds(postDialogDelay);
            
            // Wait for extraction process
            while (currentExtractionCount < extractionCount)
            {
                yield return new WaitForSeconds(extractionDelay);
                // Wait for next E press will be handled by PlayerInteractionHandler
                yield return new WaitUntil(() => !isBeingInteracted || currentExtractionCount >= extractionCount);
            }
            
            // Object extracted successfully
            if (currentExtractionCount >= extractionCount)
            {
                // Play reverse reaching animation
                player.PlayReverseReachingAnimation();
                
                // Wait for reverse reaching animation to complete
                yield return new WaitForSeconds(2f); // Give more time for reverse reaching
                
                // Destroy the object after successful extraction
                if (isHoldable && moveToHand)
                {
                    Destroy(gameObject);
                }
                
                // Play idle animation
                player.PlayIdleAnimation();
                
                // Object is now extracted
                hasBeenInteracted = true;
                Debug.Log($"{objectName} has been extracted!");
                
                // Notify player that interaction is complete
                player.OnInteractionComplete();
            }
            
            isBeingInteracted = false;
        }
        
        private IEnumerator PlayAllDialogLines()
        {
            if (dialogData == null || dialogData.dialogLines.Count == 0) yield break;
            
            for (int i = 0; i < dialogData.dialogLines.Count; i++)
            {
                DS.DialogManager.Instance?.PlaySpecificLine(dialogData, i);
                yield return new WaitForSeconds(dialogData.dialogLines[i].duration);
            }
        }
        
        public void ProcessExtraction()
        {
            if (interactionType == InteractionType.ExtractableObject && isBeingInteracted)
            {
                currentExtractionCount++;
                Debug.Log($"Extraction progress: {currentExtractionCount}/{extractionCount}");
                
                if (currentExtractionCount >= extractionCount)
                {
                    Debug.Log($"{objectName} extraction complete!");
                }
            }
        }
        
        public void CancelInteraction(PlayerInteractionHandler player)
        {
            if (isBeingInteracted)
            {
                player.PlayReverseReachingAnimation();
                
                // Play idle after reverse reaching
                StartCoroutine(PlayIdleAfterDelay(player, 2f)); // Give more time for reverse reaching
                
                isBeingInteracted = false;
            }
        }
        
        private IEnumerator PlayIdleAfterDelay(PlayerInteractionHandler player, float delay)
        {
            yield return new WaitForSeconds(delay);
            player.PlayIdleAnimation();
        }
    }
}