using UnityEngine;
using DS.Data.Cutscene;

namespace DS
{
    public class CutsceneTrigger : MonoBehaviour
    {
        [SerializeField] private string areaName;
        [SerializeField] private int stepIndex;
        [SerializeField] private CutsceneData cutsceneData;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            CutsceneManager.Instance.PlayCutsceneStep(cutsceneData, areaName, stepIndex);
            GetComponent<Collider>().enabled = false;
        }
    }
}
