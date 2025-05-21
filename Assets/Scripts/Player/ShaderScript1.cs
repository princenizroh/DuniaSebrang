using UnityEngine;

namespace DS
{
    public class ShaderScript1 : MonoBehaviour
    {
        private BoxCollider box;

        void Start()
        {
            box = GetComponent<BoxCollider>();
        }

        void Update()
        {
            if (box != null)
            {
                // Menggunakan half extents Y (tinggi/2) sebagai offset ke atas
                Shader.SetGlobalVector("_Player", transform.position + Vector3.up * box.size.y * 0.5f);
            }
        }
    }
}
