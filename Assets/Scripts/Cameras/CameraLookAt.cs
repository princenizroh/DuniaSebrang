using UnityEngine;

namespace DS 
{
    public class CameraLookAt : MonoBehaviour
    {
        public Transform target; // Objek yang akan diikuti oleh kamera
        public float rotationSpeed = 5f; // Kecepatan rotasi kamera

        private void Update()
        {
            if (target == null)
            {
                Debug.LogWarning("Target belum diatur untuk CameraLookAt!");
                return;
            }

            // Rotasi kamera agar selalu menghadap ke target
            Vector3 direction = target.position - transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            // Smooth rotasi menggunakan Lerp
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}