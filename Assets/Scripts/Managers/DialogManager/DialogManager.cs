using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using DS.Data.Dialog;


namespace DS
{
    public class DialogManager : MonoBehaviour
    {
        public static DialogManager Instance { get; private set; }
        public DialogUI ui;
        private HashSet<string> triggeredDialogs = new();
        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            ui = FindFirstObjectByType<DialogUI>();
            ui.HideDialog();
        }


        public void PlaySpecificLine(DialogData data, int index)
        {
            if (data.oneTimePlay && triggeredDialogs.Contains($"{data.Id}_{index}")) return;
            if (index < 0 || index >= data.dialogLines.Count) return;

            StartCoroutine(PlayCoroutine(data.dialogLines[index]));

            if (data.oneTimePlay)
                triggeredDialogs.Add($"{data.Id}_{index}");
        }
        private IEnumerator PlayCoroutine(DialogLine line)
        {
            string speaker = string.IsNullOrEmpty(line.speakerName) ? "" : line.speakerName + ": ";
            ui.ShowDialog(speaker + line.text);

            if (line.voiceClip)
                AudioSource.PlayClipAtPoint(line.voiceClip, Vector3.zero);

            yield return new WaitForSeconds(line.duration);
            ui.HideDialog();
        }

    }
}
