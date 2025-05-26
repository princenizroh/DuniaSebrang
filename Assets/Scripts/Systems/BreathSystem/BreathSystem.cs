using UnityEngine;

namespace DS
{
    public class BreathSystem : MonoBehaviour
    {
        [SerializeField] private AudioClip[] breathClips;
        private AudioSource audioSource;

        private void Awake()
        {
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
        }

        // Mainkan napas biasa (index 0)
        public void PlayBreath()
        {
            PlayBreathByIndex(0, 1f);
        }

        // Mainkan napas cepat (index 1)
        public void PlayBreathFast()
        {
            PlayBreathByIndex(1, 1.5f);
        }

        // Fungsi umum berdasarkan index dan pitch
        private void PlayBreathByIndex(int index, float pitch)
        {
            if (breathClips == null || breathClips.Length <= index || breathClips[index] == null) return;
            audioSource.Stop();            
            audioSource.pitch = pitch;
            audioSource.PlayOneShot(breathClips[index]);
            audioSource.pitch = 1f;
        }
    }
}
