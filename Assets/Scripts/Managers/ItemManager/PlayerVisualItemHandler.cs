using UnityEngine;

namespace DS
{
    public class PlayerVisualItemHandler : MonoBehaviour
    {
        public static PlayerVisualItemHandler Instance;

        [Header("Tempat Menempelkan Item")]
        public Transform holdPoint;

        private GameObject currentItem;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void HoldItem(GameObject itemPrefab)
        {
            if (currentItem != null)
                Destroy(currentItem);

            currentItem = Instantiate(itemPrefab, holdPoint.position, holdPoint.rotation, holdPoint);
        }

        public void DropItem()
        {
            if (currentItem != null)
            {
                Destroy(currentItem);
                currentItem = null;
            }
        }

    }
}
