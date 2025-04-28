using UnityEngine;
using UnityEngine.Playables;
using DS.Data.Cutscene;

namespace DS
{
    public class CutsceneManager : MonoBehaviour
    {
        public static CutsceneManager Instance { get; private set; }
        private CutsceneGroupManager cutsceneGroupManager;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            cutsceneGroupManager = FindFirstObjectByType<CutsceneGroupManager>();
            if (cutsceneGroupManager == null)
                Debug.LogError("[CutsceneManager] Tidak menemukan CutsceneGroupManager di Scene!");
        }
        public void PlayCutsceneStep(CutsceneData cutsceneData, string areaName, int stepIndex)
        {
            if (CutsceneTracker.Instance.HasPlayedStep(cutsceneData, stepIndex))
            {
                Debug.Log($"[CutsceneManager] Step {cutsceneData.steps[stepIndex].stepName} sudah pernah dimainkan, skip.");
                return;
            }

            var director = cutsceneGroupManager.GetDirector(areaName, stepIndex);
            if (director == null)
            {
                Debug.LogError($"[CutsceneManager] Tidak menemukan PlayableDirector di Area {areaName} untuk Step {stepIndex}!");
                return;
            }

            director.Play();
            cutsceneData.steps[stepIndex].hasPlayed = true;
            CutsceneTracker.Instance.MarkStepAsPlayed(cutsceneData, stepIndex);
        }

    }
}
