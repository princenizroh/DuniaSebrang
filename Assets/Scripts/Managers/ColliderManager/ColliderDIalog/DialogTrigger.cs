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
        private bool playerInTrigger = false;
        private GameObject currentPlayer = null;
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                playerInTrigger = true;
                currentPlayer = other.gameObject;
                
                // Jika sudah pernah dimainkan, tidak perlu play lagi
                if (hasPlayed) return;
                
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
                
                // Request dialog ke manager dengan reference ke trigger ini
                // JANGAN set hasPlayed = true di sini! Biarkan DialogManager yang mengaturnya
                DialogManager.Instance.RequestDialog(dialogData, idx, this);
            }
        }
        
        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player") && other.gameObject == currentPlayer)
            {
                playerInTrigger = false;
                currentPlayer = null;
            }
        }
        
        // Method untuk mengecek apakah player masih di dalam trigger
        public bool IsPlayerInTrigger()
        {
            return playerInTrigger && currentPlayer != null;
        }
        
        // Method untuk DialogManager memanggil ketika dialog benar-benar dimainkan
        public void OnDialogPlayed()
        {
            hasPlayed = true;
        }
        
        // Method untuk mengecek apakah dialog sudah dimainkan
        public bool HasBeenPlayed()
        {
            return hasPlayed;
        }
        
        // Method untuk reset trigger (jika diperlukan)
        public void ResetTrigger()
        {
            hasPlayed = false;
        }
        
        // Method untuk force trigger (jika diperlukan)
        public void ForceTrigger()
        {
            if (playerInTrigger && dialogData != null)
            {
                int idx;
                if (randomLine && dialogData.dialogLines != null && dialogData.dialogLines.Count > 0)
                {
                    idx = Random.Range(0, dialogData.dialogLines.Count);
                }
                else
                {
                    idx = lineIndex;
                }
                
                DialogManager.Instance.RequestDialog(dialogData, idx, this);
            }
        }
        
        // Debugging info
        private void OnDrawGizmosSelected()
        {
            if (playerInTrigger)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(transform.position, GetComponent<Collider>().bounds.size);
            }
        }
    }
}