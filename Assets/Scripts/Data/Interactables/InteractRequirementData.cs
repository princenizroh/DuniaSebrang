using UnityEngine;
using Game.Core;
using DS.Data.Item;

namespace DS.Data.Interactables
{
    [CreateAssetMenu(menuName = "Game Data/Interactable/Interact Requirement", fileName = "New Interact Requirement")]
    public class InteractRequirementData : BaseDataObject
    {
        public CollectableItemData requiredItem;
    }
}
