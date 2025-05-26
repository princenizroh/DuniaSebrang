using UnityEngine;
using DS.Data.Audio;

namespace DS
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }
        [Header("Audio Sources")]  
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private GameObject sfxPrefab;
        [SerializeField] private Transform sfxContainer;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // -------------------- MUSIC --------------------

        public void PlayMusic(AudioData data, Vector3 position)
        {
            if (data == null || data.audioClip == null || data.type != AudioCategory.Music)
            {
                Debug.LogWarning("[AudioManager] Data musik tidak valid.");
                return;
            }
            audioSource.transform.position = position;
            audioSource.clip = data.audioClip;
            audioSource.volume = data.volume;
            audioSource.loop = data.loop;
            audioSource.spatialBlend = 1f;
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            audioSource.minDistance = data.minDistance;
            audioSource.maxDistance = data.maxDistance;
            audioSource.playOnAwake = data.playOnAwake;
            audioSource.Play();
            
        }

        public void StopMusic()
        {
            audioSource.Stop();
        }

        // -------------------- SFX --------------------

        public void PlaySFX(AudioData data, Vector3 position)
        {
            if (data == null || data.audioClip == null || data.type != AudioCategory.SFX) return;

            GameObject sfxObj = Instantiate(sfxPrefab, position, Quaternion.identity, sfxContainer);
            AudioSource source = sfxObj.GetComponent<AudioSource>();

            source.clip = data.audioClip;
            source.volume = data.volume;
            source.loop = data.loop;

            source.spatialBlend = 1f;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.minDistance = data.minDistance;
            source.maxDistance = data.maxDistance;
            audioSource.playOnAwake = data.playOnAwake;

            source.Play();

            if (!data.loop)
                Destroy(sfxObj, data.audioClip.length + 0.5f);
        }
    }
}
