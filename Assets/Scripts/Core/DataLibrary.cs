using UnityEngine;
using System.Collections.Generic;

namespace Game.Core
{
    public abstract class DataLibrary<T> : ScriptableObject where T : BaseDataObject
    {
        [SerializeField] private List<T> items = new();

        public IReadOnlyList<T> Items => items;

        private Dictionary<string, T> _lookup;

        private void OnEnable()
        {
            _lookup = new Dictionary<string, T>();
            foreach (var item in items)
            {
                if (item != null && !_lookup.ContainsKey(item.Id))
                {
                    _lookup.Add(item.Id, item);
                }
            }
        }

        public T GetById(string id)
        {
            if (_lookup != null && _lookup.TryGetValue(id, out var result))
                return result;

            Debug.LogWarning($"[DataLibrary] Data dengan ID {id} tidak ditemukan");
            return null;
        }
    }
}
