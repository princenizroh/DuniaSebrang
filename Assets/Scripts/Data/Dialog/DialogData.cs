using UnityEngine;
using System.Collections.Generic;
using Game.Core;

namespace DS.Data.Dialog
{
    [System.Serializable]
    public class DialogLine 
    {
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
    }
}
