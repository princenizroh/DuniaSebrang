using UnityEngine;
using Game.Core;

namespace Game.Data.Audio
{
    [CreateAssetMenu(fileName = "NewMusicData", menuName = "Game Data/Music")]
    public class MusicData : BaseDataObject
    {
        public AudioClip audioClip;
        public float volume = 1.0f;
        public bool loop = true;
    }
}
