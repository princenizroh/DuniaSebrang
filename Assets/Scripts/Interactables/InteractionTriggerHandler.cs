using UnityEngine;

namespace DS
{
    public class InteractionTriggerHandler : MonoBehaviour
    {
        private PlayerInteractionHandler playerHandler;
        
        public void Initialize(PlayerInteractionHandler handler)
        {
            playerHandler = handler;
        }
        
        private void OnTriggerEnter(Collider other)
        {
            InteractionObject interactionObject = other.GetComponent<InteractionObject>();
            if (interactionObject != null)
            {
                playerHandler.OnInteractionObjectEnter(interactionObject);
            }
        }
        
        private void OnTriggerExit(Collider other)
        {
            InteractionObject interactionObject = other.GetComponent<InteractionObject>();
            if (interactionObject != null)
            {
                playerHandler.OnInteractionObjectExit(interactionObject);
            }
        }
    }
}