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

        public bool HasPlayedStep(CutsceneData cutsceneData, int stepIndex)
        {
            if (cutsceneData == null || stepIndex < 0 || stepIndex >= cutsceneData.steps.Count)
                return true; // Kalau error data, dianggap sudah main

            return cutsceneData.steps[stepIndex].hasPlayed;
        }

        public void MarkStepAsPlayed(CutsceneData cutsceneData, int stepIndex)
        {
            if (cutsceneData != null && stepIndex >= 0 && stepIndex < cutsceneData.steps.Count)
            {
                cutsceneData.steps[stepIndex].hasPlayed = true;
            }
        }
    }
}
