using UnityEngine;

namespace DS
{
    /// <summary>
    /// Ketika player masuk trigger, KuntiAI akan dipindahkan ke posisi spawnPoint.
    /// </summary>
    public class PlayerTriggerKuntiAISpawn : MonoBehaviour
    {
        [Header("Assign KuntiAI yang akan dipindahkan")]
        [SerializeField] private KuntiAI targetKuntiAI;
        [Header("Assign GameObject sebagai spawn point")]
        [SerializeField] private Transform spawnPoint;
        [Header("Tag player (default: Player)")]
        [SerializeField] private string playerTag = "Player";

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(playerTag))
            {
                if (targetKuntiAI != null && spawnPoint != null)
                {
                    targetKuntiAI.transform.position = spawnPoint.position;
                    targetKuntiAI.transform.rotation = spawnPoint.rotation;
                }
            }
        }

        public void DisableTrigger()
        {
            gameObject.SetActive(false);
        }
    }
}
