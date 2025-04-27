using UnityEngine;
using DS.Data.Item;

namespace DS
{
    public class CollectableItem : MonoBehaviour
    {
         public CollectableItemData itemData;

         public void Collect()
         {
            bool collected = ItemManager.Instance.Collect(itemData, gameObject);

            if (collected)
            {
                // Ini akan dihancurkan dari dalam ItemManager
                Debug.Log("Item berhasil dikoleksi");
            }
            else
            {
                Debug.Log("Gagal mengkoleksi item");
            }
         }
    }
}
