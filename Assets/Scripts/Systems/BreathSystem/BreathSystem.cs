using UnityEngine;
using DS.Data.Audio;

namespace DS
{
    public class BreathSystem : MonoBehaviour
    {
        [Header("Audio Data Settings")]
        [SerializeField] private AudioData[] breathAudioData;
        
        [Header("Debug Info")]
        [SerializeField] private int lastPlayedIndex = -1; // Info terakhir dimainkan
        
        private AudioSource audioSource;

        private void Awake()
        {
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
                
            // Pastikan AudioSource dalam mode 3D untuk distance berfungsi
            if (audioSource != null)
            {
                audioSource.spatialBlend = 1f; // 1 = full 3D, 0 = full 2D
                audioSource.rolloffMode = AudioRolloffMode.Linear;
            }
        }

        // Mainkan napas biasa (index 0)
        public void PlayBreath()
        {
            PlayBreathByIndex(0);
        }

        // Fungsi untuk animation event - bisa dipanggil dengan index tertentu
        public void PlayBreathByIndex(int index)
        {
            PlayBreathByIndex(index, 1f);
        }

        // Fungsi untuk animation event - dengan pitch custom
        public void PlayBreathWithPitch(float pitch)
        {
            PlayBreathByIndex(0, pitch);
        }

        // Fungsi untuk animation event - index dan pitch
        public void PlayBreathIndexPitch(int index)
        {
            PlayBreathByIndex(index, 1f);
        }

        // Fungsi utama untuk memutar audio berdasarkan index
        private void PlayBreathByIndex(int index, float pitch)
        {
            // Validasi array dan index
            if (breathAudioData == null || breathAudioData.Length <= index || index < 0)
            {
                Debug.LogWarning($"BreathSystem: Invalid index {index} or breathAudioData is null/empty");
                return;
            }

            // Validasi AudioData
            AudioData audioData = breathAudioData[index];
            if (audioData == null || audioData.audioClip == null)
            {
                Debug.LogWarning($"BreathSystem: AudioData at index {index} is null or has no audio clip");
                return;
            }

            // Update debug info
            lastPlayedIndex = index;

            // Stop audio sebelumnya
            audioSource.Stop();

            // Terapkan settings dari AudioData
            audioSource.volume = audioData.volume;
            audioSource.loop = audioData.loop;
            audioSource.pitch = pitch;

            // 3D Audio Settings - PENTING untuk distance
            audioSource.spatialBlend = 1f; // Pastikan full 3D
            audioSource.minDistance = audioData.minDistance;
            audioSource.maxDistance = audioData.maxDistance;
            audioSource.rolloffMode = AudioRolloffMode.Linear;

            // Play audio
            audioSource.PlayOneShot(audioData.audioClip);

            // Reset pitch setelah play
            audioSource.pitch = 1f;
        }

        // Fungsi tambahan untuk keperluan debugging
        [ContextMenu("Test Play Breath Index 0")]
        private void TestPlayBreath0()
        {
            PlayBreathByIndex(0);
        }

        [ContextMenu("Test Play Breath Index 1")]
        private void TestPlayBreath1()
        {
            PlayBreathByIndex(1);
        }

        // Getter untuk mendapatkan jumlah audio data
        public int GetBreathAudioCount()
        {
            return breathAudioData != null ? breathAudioData.Length : 0;
        }

        // Fungsi untuk mendapatkan nama audio berdasarkan index
        public string GetBreathAudioName(int index)
        {
            if (breathAudioData != null && index >= 0 && index < breathAudioData.Length && breathAudioData[index] != null)
            {
                return breathAudioData[index].AudioName;
            }
            return "Unknown";
        }

        // GIZMO untuk visualisasi 3D Audio Distance
        private void OnDrawGizmosSelected()
        {
            if (breathAudioData == null || breathAudioData.Length == 0) return;

            // Ambil AudioData yang sedang aktif (lastPlayedIndex atau index 0)
            int gizmoIndex = lastPlayedIndex >= 0 ? lastPlayedIndex : 0;
            if (gizmoIndex >= breathAudioData.Length) gizmoIndex = 0;

            AudioData audioData = breathAudioData[gizmoIndex];
            if (audioData == null) return;

            Vector3 position = transform.position;

            // Gizmo untuk Min Distance (warna hijau)
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(position, audioData.minDistance);
            
            // Gizmo untuk Max Distance (warna merah)
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(position, audioData.maxDistance);

            // Label untuk info
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(position + Vector3.up * (audioData.maxDistance + 0.5f), 
                $"Audio: {audioData.AudioName}\nMin: {audioData.minDistance}m\nMax: {audioData.maxDistance}m");
            #endif
        }

        // Gizmo yang selalu terlihat (tidak perlu select)
        private void OnDrawGizmos()
        {
            if (breathAudioData == null || breathAudioData.Length == 0) return;

            // Hanya tampilkan gizmo tipis jika tidak diselect
            int gizmoIndex = lastPlayedIndex >= 0 ? lastPlayedIndex : 0;
            if (gizmoIndex >= breathAudioData.Length) gizmoIndex = 0;

            AudioData audioData = breathAudioData[gizmoIndex];
            if (audioData == null) return;

            Vector3 position = transform.position;

            // Gizmo tipis untuk referensi
            Gizmos.color = new Color(0, 1, 0, 0.1f); // Hijau transparan
            Gizmos.DrawWireSphere(position, audioData.minDistance);
            
            Gizmos.color = new Color(1, 0, 0, 0.1f); // Merah transparan
            Gizmos.DrawWireSphere(position, audioData.maxDistance);
        }
    }
}