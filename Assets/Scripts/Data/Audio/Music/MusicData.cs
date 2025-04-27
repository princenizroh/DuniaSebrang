using UnityEngine;
using Game.Core;

namespace DS.Data.Audio
{
    [CreateAssetMenu(fileName = "NewMusicData", menuName = "Game Data/Audio/Music")]
    public class MusicData : BaseDataObject
    {
        public AudioClip audioClip;
        public float volume = 1.0f;
        public bool loop = true;
    }
}
