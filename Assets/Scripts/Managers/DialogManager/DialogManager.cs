using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DS
{
    public class DialogManager : MonoBehaviour
    {
        public static DialogManager Instance { get; private set; }

        public TextMeshProUGUI dialogText;

        public Canvas dialogUI;

        public bool dialogUIActive;
        
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip dialogSound;

        private void Awake()
        {
            if (Instance != null & Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }
        }

        public void OpenDialogUI()
        {
            dialogUI.gameObject.SetActive(true);
            dialogUIActive = true;
        }

        public void CloseDialogUI()
        {
            dialogUI.gameObject.SetActive(false);
            dialogUIActive = false;
        }

        public void SetDialogText(string text)
        {
            dialogText.text = text;
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
