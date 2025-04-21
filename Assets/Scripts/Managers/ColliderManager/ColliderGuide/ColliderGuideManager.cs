using UnityEngine;

namespace DS
{
    public class ColliderGuideManager : MonoBehaviour
    {
        [field: SerializeField] public GameObject[] colliders { get; private set; }

        private void Awake()
        {
            colliders = new GameObject[transform.childCount];
            for (int i = 0; i < transform.childCount; i++)
            {
                colliders[i] = transform.GetChild(i).gameObject;
            }
        }

        private void Update()
        {

        }
    }
}
