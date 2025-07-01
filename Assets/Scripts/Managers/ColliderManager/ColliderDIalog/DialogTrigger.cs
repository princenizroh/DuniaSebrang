using UnityEngine;
using DS.Data.Dialog;

namespace DS
{
    public class DialogTrigger : MonoBehaviour
    {
        public DialogData dialogData;
        public int lineIndex; 
        public bool randomLine = false;
        private bool hasPlayed = false;

        private void OnTriggerEnter(Collider other)
        {
            if (hasPlayed) return;

            if (other.CompareTag("Player"))
            {
                int idx;
                if (randomLine && dialogData != null && dialogData.dialogLines != null && dialogData.dialogLines.Count > 0)
                {
                    idx = Random.Range(0, dialogData.dialogLines.Count);
                }
                else
                {
                    // Pakai lineIndex dari Inspector
                    idx = lineIndex;
                }
                DialogManager.Instance.PlaySpecificLine(dialogData, idx);
                hasPlayed = true;
            }
        }
    }
}
