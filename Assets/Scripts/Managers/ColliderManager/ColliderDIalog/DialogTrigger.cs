using UnityEngine;

namespace DS
{
    public class DialogTrigger : MonoBehaviour
    {
        public DialogData dialogData;
        public int lineIndex; // Baris dialog yang akan dimainkan
        private bool hasPlayed = false;

        private void OnTriggerEnter(Collider other)
        {
            if (hasPlayed) return;

            if (other.CompareTag("Player"))
            {
                DialogManager.Instance.PlaySpecificLine(dialogData, lineIndex);
                hasPlayed = true;
            }
        }
    }
}
