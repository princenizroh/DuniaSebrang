using UnityEngine;

public class CameraTrigger : MonoBehaviour
{
    public int cameraIndex; // Indeks kamera yang akan diaktifkan
    private CameraManager cameraManager;

    private void Start()
    {
        cameraManager = FindObjectOfType<CameraManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Debug.Log($"Player masuk ke trigger dengan kamera indeks {cameraIndex}");
            cameraManager.SwitchCamera(cameraIndex);
        }
    }
}
