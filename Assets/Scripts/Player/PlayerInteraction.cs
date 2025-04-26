using UnityEngine;

namespace DS
{
    public class PlayerInteraction : MonoBehaviour
    {
       private InteractableObject currentInteractable;
       private CollectableItem currentCollectableItem;

        private void Update()
        {
            HandleInteractionInput();
            HandleDropInput();
        }

        private void HandleInteractionInput()
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (currentInteractable != null)
                {
                    currentInteractable.TryInteract();
                }
                else if (currentCollectableItem != null)
                {
                    if (ItemManager.Instance.IsHoldingItem())
                    {
                        Debug.Log("Sudah memegang item. Tekan Q untuk membuang.");
                    }
                    else
                    {
                        currentCollectableItem.Collect();
                    }
                }
            }
        }

        private void HandleDropInput()
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                if (ItemManager.Instance.IsHoldingItem())
                {
                    ItemManager.Instance.DropCurrentItem();
                }
            }
        }
 
        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out InteractableObject interactable))
            {
                currentInteractable = interactable;
            }
            else if (other.TryGetComponent(out CollectableItem collactable))
            {
                currentCollectableItem = collactable;
            }
        }


        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out InteractableObject interactable))
            {
                if (interactable == currentInteractable)
                {
                    currentInteractable = null;
                }
            }

            if (other.TryGetComponent(out CollectableItem collectable))
            {
                if (collectable == currentCollectableItem)
                {
                    currentCollectableItem = null;
                }
            }
        }
    }
}
