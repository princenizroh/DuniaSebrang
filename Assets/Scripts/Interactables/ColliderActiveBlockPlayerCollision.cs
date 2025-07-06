using UnityEngine;

namespace DS
{
    /// <summary>
    /// Collider yang mengatur aktif/matinya GameObject (misal: pintu/BlockPlayerCollision) berdasarkan trigger player.
    /// - Saat player masuk (OnTriggerEnter), pintu tetap terbuka (gameObject target tetap nonaktif).
    /// - Saat player keluar (OnTriggerExit), pintu/BlockPlayerCollision diaktifkan (menutup ruangan).
    /// - Bisa dipanggil method DisableBlock() untuk menonaktifkan pintu dari script lain (misal setelah extraction selesai).
    /// </summary>
    public class ColliderActiveBlockPlayerCollision : MonoBehaviour
    {
        [Header("Target yang akan diaktifkan/nonaktifkan (misal: pintu)")]
        [SerializeField] private GameObject[] blockPlayerCollisions;
        [Header("Tag player (default: Player)")]
        [SerializeField] private string playerTag = "Player";

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(playerTag))
            {
                // Saat player masuk, pastikan block tetap nonaktif (pintu terbuka)
                foreach (var blockPlayerCollision in blockPlayerCollisions)
                {
                    if (blockPlayerCollision != null)
                        blockPlayerCollision.SetActive(false);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(playerTag))
            {
                // Saat player keluar, aktifkan block (pintu menutup)
                foreach (var blockPlayerCollision in blockPlayerCollisions)
                {
                    if (blockPlayerCollision != null)
                        blockPlayerCollision.SetActive(true);
                }
            }
        }

        /// <summary>
        /// Nonaktifkan block (misal: dipanggil setelah extraction selesai)
        /// </summary>
        public void DisableBlock()
        {
                foreach (var blockPlayerCollision in blockPlayerCollisions)
                {
                    if (blockPlayerCollision != null)
                        blockPlayerCollision.SetActive(false);
                }
        }
    }
}
