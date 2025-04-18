using UnityEngine;

namespace DS
{
    public class ShaderScript : MonoBehaviour
    {
        private BoxCollider boxCollider;

        void Start()
        {
            boxCollider = GetComponent<BoxCollider>();
        }

        void Update()
        {
            if (boxCollider != null)
            {
                Shader.SetGlobalVector("_Player", transform.position + Vector3.up * (boxCollider.size.y / 2f));
            }
        }
    }
}
