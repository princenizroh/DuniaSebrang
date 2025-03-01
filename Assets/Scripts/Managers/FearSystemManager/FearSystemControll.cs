using UnityEngine;

namespace DS
{
    public class FearSystemControll : MonoBehaviour
    {
        public Light pointLight; // Assign Light di Inspector
        public float fearLevel = 0f; // Nilai ketakutan (0 - 1)
        public float minRadius = 2f;
        public float maxRadius = 10f;

        void Update()
        {
            float radius = Mathf.Lerp(minRadius, maxRadius, fearLevel);
            pointLight.range = radius;
        }
    }
}
