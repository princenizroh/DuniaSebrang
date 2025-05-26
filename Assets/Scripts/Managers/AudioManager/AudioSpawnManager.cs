using System.Collections.Generic;
using UnityEngine;

namespace DS
{
    public class AudioSpawnManager : MonoBehaviour
    {
        public static AudioSpawnManager Instance { get; private set; }

        [Header("Spawn Settings")]
        [SerializeField] private List<Transform> audioSpawnPoints;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public Transform GetSpawnPointByIndex(int index)
        {
            if (index >= 0 && index < audioSpawnPoints.Count)
            {
                return audioSpawnPoints[index];
            }
            else
            {
                Debug.LogWarning("[AudioSpawnManager] Index spawn point tidak valid: " + index);
                return null;
            }
        }
    }
}
