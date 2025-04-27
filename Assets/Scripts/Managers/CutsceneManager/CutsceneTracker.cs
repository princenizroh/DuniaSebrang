using UnityEngine;
using System.Collections.Generic;
using DS.Data.Cutscene;

namespace DS
{
    public class CutsceneTracker : MonoBehaviour
    {
        public static CutsceneTracker Instance { get; private set; }

        private HashSet<string> playedCutscenes = new HashSet<string>();

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

        public bool HasPlayed(CutsceneData cutsceneData)
        {
            return playedCutscenes.Contains(cutsceneData.Id) || cutsceneData.hasPlayed;
        }

        public void MarkAsPlayed(CutsceneData cutsceneData)
        {
            if (!playedCutscenes.Contains(cutsceneData.Id))
            {
                playedCutscenes.Add(cutsceneData.Id);
                cutsceneData.hasPlayed = true;
            }
        }
    }
}
