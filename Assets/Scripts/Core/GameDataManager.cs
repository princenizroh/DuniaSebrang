using UnityEngine;
using DS.Data.Audio;

namespace Game.Core
{
    public class GameDataManager : MonoBehaviour
    {
        public static GameDataManager Instance { get; private set; }

        // public MusicLibrary area1MusicLibrary;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
            {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);

            // Contoh load manual jika belum assign via Inspector
            // if (area1MusicLibrary == null)
            // {
            //     area1MusicLibrary = Resources.Load<MusicLibrary>("Game Data/Music/Gameplay/Area 1/Area1_MusicLibrary");
            // }
        }
    }
}
