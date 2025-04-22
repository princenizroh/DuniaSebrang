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
        _audioSource.minDistance = 1f;
        _audioSource.maxDistance = 15f; // Jarak dengar maksimum
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

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _audioSource.Stop();
            Debug.Log($"[ColliderAudio] Musik dihentikan: {musicData.name}");
        }
    }
}
}
