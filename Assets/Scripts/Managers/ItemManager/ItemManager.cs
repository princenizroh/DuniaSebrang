using UnityEngine;
using System.Collections.Generic;

namespace DS
{
    public class ItemManager : MonoBehaviour
    {
        public static ItemManager Instance { get; private set; }
        private CollectableItemData currentHeldItemData;
        private PlayerVisualItemHandler visualItemHandler;
        public bool IsHoldingItem() => currentHeldItemData != null;
        public CollectableItemData CurrentHeldItemData => currentHeldItemData;

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

            PlayerVisualItemHandler.Instance.HoldItem(data.itemPrefab);

            Destroy(sourceObject);

            return true;
        }

        public CollectableItemData GetCurrentHeldItemData()
        {
            return currentHeldItemData;
        }

        public void DropCurrentItem()
        {
            if (currentHeldItemData == null) return;

            GameObject droppedItem = Instantiate(
                currentHeldItemData.itemPrefab,
                PlayerVisualItemHandler.Instance.holdPoint.position + Vector3.forward * 0.5f,
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

            PlayerVisualItemHandler.Instance.DropItem();

            currentHeldItemData = null;
        }

    }
}
