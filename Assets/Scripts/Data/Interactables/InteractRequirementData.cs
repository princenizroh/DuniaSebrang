using UnityEngine;
using DS;
namespace Game.Core
{
    [CreateAssetMenu(menuName = "Game Data/Interactable/Interact Requirement", fileName = "New Interact Requirement")]
    public class InteractRequirementData : BaseDataObject
    {
        public CollectableItemData requiredItem;
    }
}
