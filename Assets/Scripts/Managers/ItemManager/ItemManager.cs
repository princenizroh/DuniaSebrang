using UnityEngine;
using DS.Data.Item;

namespace DS
{
    public class ItemManager : MonoBehaviour
    {
        public static ItemManager Instance { get; private set; }
        private CollectableItemData currentHeldItemData;
        private PlayerVisualItemHandler visualItemHandler;
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            visualItemHandler = FindFirstObjectByType<PlayerVisualItemHandler>();
            if (visualItemHandler == null)
            {
                Debug.LogError("PlayerVisualItemHandler tidak ditemukan di scene.");
            }
        }

        public bool Collect(CollectableItemData data, GameObject sourceObject)
        {
            if (data == null) return false;
            if (currentHeldItemData != null)
            {
                Debug.Log("Sudah memegang item lain, buang dulu.");
                DropCurrentItem();
            }
            currentHeldItemData = data;
            visualItemHandler.HoldItem(data.itemPrefab);
            Destroy(sourceObject);
            return true;
        }
        public bool IsHoldingItem() => currentHeldItemData != null;
        public CollectableItemData GetCurrentHeldItemData() => currentHeldItemData;
        public void DropCurrentItem()
        {
            if (currentHeldItemData == null) return;

            GameObject droppedItem = Instantiate(
                currentHeldItemData.itemPrefab,
                visualItemHandler.holdPoint.position + Vector3.forward * 0.5f,
                Quaternion.identity
            );

            if (droppedItem.TryGetComponent(out CollectableItem collectable))
            {
                collectable.itemData = currentHeldItemData;
            }
            else
            {
                Debug.LogWarning("Prefab item tidak memiliki komponen CollectableItem!");
            }

            visualItemHandler.DropItem();

            currentHeldItemData = null;
        }

    }
}
