using UnityEngine;

namespace DS.Data.Cutscene
{

    [System.Serializable]
    public class CutsceneStep
    {
        public string description; // Penjelasan langkah ini
        public float delayBeforeStep; // Delay sebelum langkah ini mulai
        public GameObject targetObject; // Objek yang akan diinteraksi
        public CutsceneActionType actionType; // Jenis aksi di langkah ini
    }

    public enum CutsceneActionType
    {
        None,
        MoveCamera,
        PlayAnimation,
        ShowDialog,
        SpawnEnemy
        // bisa tambah lain nanti
    }
}
