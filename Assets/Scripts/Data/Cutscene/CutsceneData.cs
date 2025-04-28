using System.Collections.Generic;
using UnityEngine;
using Game.Core;

namespace DS.Data.Cutscene
{
    [System.Serializable]
    public class CutsceneStep
    {
        public string stepName; 
        public bool hasPlayed; 
    }
    
    [CreateAssetMenu(fileName = "NewCutsceneData", menuName = "Game Data/Cutscene/Cutscene Data")]
    public class CutsceneData : BaseDataObject
    {
        [Header("State")]
        public bool hasPlayed; 
        [Header("Cutscene Steps")]  
        public List<CutsceneStep> steps = new();
    }
}
