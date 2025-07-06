using UnityEngine;

namespace DS
{
    public class ManyAtivateOnPlayerTrigger : MonoBehaviour
    {
        [Header("Tag player (default: Player)")]
        [SerializeField] private string playerTag = "Player";
        [SerializeField] private GameObject[] objectActivates;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(playerTag))
            {
                // Aktifkan semua object di array jika player masuk trigger
                if (objectActivates != null)
                {
                    foreach (var obj in objectActivates)
                    {
                        if (obj != null)
                            obj.SetActive(true);
                    }
                }
            }
        }
    }
}