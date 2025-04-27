using System.Collections.Generic;
using UnityEngine;
using Game.Core;

namespace DS.Data.Cutscene
{
    [CreateAssetMenu(fileName = "NewCutsceneData", menuName = "Game Data/Cutscene/Cutscene Data")]
    public class CutsceneData : BaseDataObject
    {
        [Header("State")]
        public bool hasPlayed; 
        [Header("Steps")]  
        public List<CutsceneStep> steps = new();
    }
}
