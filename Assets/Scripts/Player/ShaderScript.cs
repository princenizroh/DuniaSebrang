using UnityEngine;

namespace DS
{
    public class ShaderScript : MonoBehaviour
    {
        private CapsuleCollider capCollider;

        void Start()
        {
            capCollider = GetComponent<CapsuleCollider>();
        }

        void Update()
        {
            if (capCollider != null)
            {
                Shader.SetGlobalVector("_Player", transform.position + Vector3.up * capCollider.radius);
            }
        }
    }
}
