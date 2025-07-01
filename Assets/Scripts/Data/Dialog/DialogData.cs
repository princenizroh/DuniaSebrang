using UnityEngine;
using System.Collections.Generic;
using Game.Core;

namespace DS.Data.Dialog
{
    [System.Serializable]
    public class DialogLine 
    {
        public int id; // Unique identifier for the dialog line
        public string speakerName;
        [TextArea(3, 10)] public string text;
        public AudioClip voiceClip;
        public float duration = 3f;
    }
    [CreateAssetMenu(menuName = "Game Data/Dialog/Dialog Data", fileName = "New Dialog")]
    public class DialogData : BaseDataObject
    {
        public List<DialogLine> dialogLines = new();
        public bool oneTimePlay = true;
#if UNITY_EDITOR
        private void OnValidate()
        {
            // Beri id unik dan berurutan untuk setiap dialog line
            for (int i = 0; i < dialogLines.Count; i++)
            {
                dialogLines[i].id = i;
            }
        }
#endif
    }
}
