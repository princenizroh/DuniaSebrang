using UnityEngine;
using Game.Core;
namespace DS.Data.Audio
{
    public enum AudioCategory
    {        
        Music,
        SFX,
        Ambience
    }

    [CreateAssetMenu(fileName = "NewAudioData", menuName = "Game Data/Audio/Audio Data")]
    public class AudioData : BaseDataObject
    {
        public string AudioName;
        public AudioClip audioClip;
        public AudioCategory type;
        [Range(0f, 1f)] 
        public float volume = 1f;
        public bool loop = false;
        [Header("3D Audio Settings")]
        public float minDistance = 1f;
        public float maxDistance = 15f;
        public bool playOnAwake = false;
    }
}
