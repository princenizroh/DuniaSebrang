using UnityEngine;

namespace DS
{
    public class DeactivateOnPlayerTrigger : MonoBehaviour
    {
        [Header("Tag player (default: Player)")]
        [SerializeField] private string playerTag = "Player";
        [SerializeField] private GameObject objectDeactivate;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(playerTag))
            {
                // Nonaktifkan GameObject ini jika player masuk trigger
                objectDeactivate.SetActive(false);
            }
        }
    }
}