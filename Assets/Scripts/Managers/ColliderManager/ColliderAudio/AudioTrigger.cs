using UnityEngine;
using DS.Data.Audio;

namespace DS
{
    [RequireComponent(typeof(Collider))]
    public class AudioTrigger : MonoBehaviour
    {
        [Header("Referensi Data Musik")]
        [SerializeField] AudioData audioData;
        
        [SerializeField] private bool playOnce = true;
        private bool hasPlayed = false;


        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            if (audioData == null)
            {
                Debug.LogWarning($"[AudioTrigger] AudioData kosong di {gameObject.name}");
                return;
            }
            if (playOnce && hasPlayed) return;
            switch (audioData.type)
            {
                case AudioCategory.Music:
                    AudioManager.Instance.PlayMusic(audioData, transform.position);
                    break;
                case AudioCategory.SFX:
                    AudioManager.Instance.PlaySFX(audioData, transform.position);
                    break;
                default:
                    Debug.LogWarning("[AudioTrigger] Jenis audio tidak dikenali");
                    break;
            }

            hasPlayed = true;
        }
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (audioData == null) return;

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, audioData.minDistance);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, audioData.maxDistance);
        }
#endif
    }
}
