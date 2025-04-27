using UnityEngine;
using DS.Data.Interactables;

namespace DS
{
    public class InteractableObject : MonoBehaviour
    {
        public InteractRequirementData requirementData;

        public void TryInteract()
        {

            if (ItemManager.Instance.GetCurrentHeldItemData() == requirementData.requiredItem)
            {
                Debug.Log("Syarat terpenuhi, interaksi berhasil.");
                ExecuteInteraction();
            }
            else
            {
                Debug.Log("Tidak memegang item yang diperlukan.");
            }
        }

        private void ExecuteInteraction()
        {
            Debug.Log("Interaksi berhasil dilakukan!");
            Destroy(gameObject); 
        }
    }
}
