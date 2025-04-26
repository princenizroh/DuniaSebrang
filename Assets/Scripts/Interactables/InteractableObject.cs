using UnityEngine;
using Game.Core;

namespace DS
{
    public class InteractableObject : MonoBehaviour
    {
        public InteractRequirementData requirementData;

        public void TryInteract()
        {
            if (requirementData == null)
            {
                Debug.Log("Tidak ada syarat, interaksi bebas dilakukan.");
                ExecuteInteraction();
                return;
            }

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
            // Lakukan aksi disini, misal membuka pintu, menghancurkan objek, dll
            Debug.Log("Interaksi berhasil dilakukan!");
            Destroy(gameObject); // contoh: objek dihancurkan
        }
    }
}
