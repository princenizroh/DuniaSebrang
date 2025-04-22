using UnityEngine;

namespace Game.Core
{
    public abstract class BaseDataObject : ScriptableObject
    {
        [SerializeField] private string id;

        public string Id => id;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(id))
            {
                id = name.ToLower().Replace(" ", "_");
                Debug.Log($"[BaseDataObject] ID otomatis di-set: {id}");
            }
        }
#endif
    }
}
