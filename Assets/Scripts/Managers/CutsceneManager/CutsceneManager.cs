using UnityEngine;
using DS.Data.Cutscene;
using System.Collections;

namespace DS
{
    public class CutsceneManager : MonoBehaviour
    {
        public static CutsceneManager Instance;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void PlayCutscene(CutsceneData cutsceneData)
        {
            StartCoroutine(PlaySteps(cutsceneData));
        }

        private IEnumerator PlaySteps(CutsceneData cutsceneData)
        {
            foreach (var step in cutsceneData.steps)
            {
                if (step.delayBeforeStep > 0)
                    yield return new WaitForSeconds(step.delayBeforeStep);

                ExecuteStep(step);

                // Untuk demo ini, kita tunggu sebentar tiap step
                yield return new WaitForSeconds(1f);
            }

            Debug.Log("Cutscene Selesai!");
        }

        private void ExecuteStep(CutsceneStep step)
        {
            switch (step.actionType)
            {
                case CutsceneActionType.MoveCamera:
                    Debug.Log($"[Cutscene] Move Camera to {step.targetObject.name}");
                    // Tambahkan logika move camera
                    break;
                case CutsceneActionType.PlayAnimation:
                    if (step.targetObject.TryGetComponent(out Animator animator))
                    {
                        animator.SetTrigger("Play"); // Asumsi ada Trigger 'Play'
                    }
                    break;
                case CutsceneActionType.ShowDialog:
                    Debug.Log($"[Cutscene] Show dialog: {step.description}");
                    // Tambahkan logika dialog
                    break;
                case CutsceneActionType.SpawnEnemy:
                    Debug.Log($"[Cutscene] Spawn Enemy at {step.targetObject.name}");
                    // Bisa instantiate enemy disini
                    break;
                case CutsceneActionType.None:
                default:
                    break;
            }
        }
    }
}
