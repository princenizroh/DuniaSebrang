using UnityEngine;
using DS.Data.Cutscene;

namespace DS
{
    public class CutsceneTrigger : MonoBehaviour
    {
        public CutsceneData cutsceneData;
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && cutsceneData != null)
            {
                if (!cutsceneData.hasPlayed && !CutsceneTracker.Instance.HasPlayed(cutsceneData))
                {
                    CutsceneManager.Instance.PlayCutscene(cutsceneData);
                    cutsceneData.hasPlayed = true;
                }
            }
        }
    }
}
