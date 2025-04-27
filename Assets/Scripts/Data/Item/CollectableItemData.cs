using UnityEngine;
using Game.Core;

namespace DS.Data.Item
{
    [CreateAssetMenu(menuName = "Game Data/Item/Collectable Item", fileName = "New Collectable Item")]
    public class CollectableItemData : BaseDataObject
    {
        [TextArea(2, 5)] public string description;
        public GameObject itemPrefab;
    }
}
