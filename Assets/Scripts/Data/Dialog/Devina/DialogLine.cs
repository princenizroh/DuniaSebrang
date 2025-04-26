using UnityEngine;

namespace DS
{
    [System.Serializable]
    public class DialogLine 
    {
        public string speakerName;
        [TextArea(3, 10)] public string text;
        public AudioClip voiceClip;
        public float duration = 3f;
    }
}
