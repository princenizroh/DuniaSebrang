using UnityEngine;

namespace DS
{
    public class DialogTrigger : MonoBehaviour
    {
        private DialogManager dialogManager;  
        [field: SerializeField] private string dialogText;
        [field: SerializeField] private AudioSource audioSource;
        [field: SerializeField] private AudioClip dialogSound;

        private void Start()
        {
            dialogManager = FindFirstObjectByType<DialogManager>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                dialogManager.OpenDialogUI();
                dialogManager.SetDialogText(dialogText);
                PlayDialogSound();
            }
        }

        private void OntTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                dialogManager.CloseDialogUI();
            }
        }


        private void PlayDialogSound()
        {
            if (audioSource != null && dialogSound != null)
            {
                audioSource.PlayOneShot(dialogSound);
            }
        }
        
    }
}
