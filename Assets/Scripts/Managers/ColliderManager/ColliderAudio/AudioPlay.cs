using UnityEngine;
using Game.Data.Audio;

namespace DS
{
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(AudioSource))]
    public class AudioPlay : MonoBehaviour
    {
        [Header("Referensi Data Musik")]
        public MusicData musicData;
        private AudioSource _audioSource;
        [SerializeField] private float minDistance = 1f;
        [SerializeField] private float maxDistance = 15f;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            SetupAudioSource();
        }

        private void SetupAudioSource()
        {
            if (musicData == null || musicData.audioClip == null)
            {
                Debug.LogWarning($"[ColliderAudio] MusicData belum diisi pada {gameObject.name}");
                return;
            }

            _audioSource.clip = musicData.audioClip;
            _audioSource.volume = musicData.volume;
            _audioSource.loop = musicData.loop;

            _audioSource.spatialBlend = 1f; // 3D audio
            _audioSource.rolloffMode = AudioRolloffMode.Linear;
            _audioSource.minDistance = minDistance;
            _audioSource.maxDistance = maxDistance; // Jarak dengar maksimum
            _audioSource.playOnAwake = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                _audioSource.Play();
                Debug.Log($"[ColliderAudio] Musik diputar: {musicData.name}");
            }
        }
        private void OnDrawGizmos()
        {
            if (_audioSource == null) return;

            // Warna untuk minDistance
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, _audioSource.minDistance);

            // Warna untuk maxDistance
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _audioSource.maxDistance);
        }
    }
}
