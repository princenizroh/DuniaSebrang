using UnityEngine;
using TMPro;

namespace DS {
    public class DialogUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI dialogText;

        private void Awake()
        {
            if (dialogText == null)
                dialogText = GetComponentInChildren<TextMeshProUGUI>();
        }

        public void ShowDialog(string text)
        {
            dialogText.text = text;
            gameObject.SetActive(true);
        }

        public void HideDialog()
        {
            gameObject.SetActive(false);
        }
    }
}

