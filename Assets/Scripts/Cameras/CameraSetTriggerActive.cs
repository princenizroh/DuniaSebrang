using UnityEngine;

namespace DS
{
    public class CameraSetTriggerActive : MonoBehaviour
    {
        public int cameraIndex; // Indeks kamera yang akan diaktifkan
        private CameraSetActive cameraManager;

        [System.Obsolete]
        private void Start()
        {
            cameraManager = FindObjectOfType<CameraSetActive>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                Debug.Log($"Player masuk ke trigger dengan kamera indeks {cameraIndex}");
                cameraManager.SwitchCamera(cameraIndex);
            }
        }
    }
}