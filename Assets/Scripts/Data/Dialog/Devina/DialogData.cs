using UnityEngine;
using System.Collections.Generic;
using Game.Core;

namespace DS
{
    [CreateAssetMenu(menuName = "Game Data/Dialog/Dialog Data", fileName = "New Dialog")]
    public class DialogData : BaseDataObject
    {
        public List<DialogLine> dialogLines = new();
        public bool oneTimePlay = true;
    }
}
