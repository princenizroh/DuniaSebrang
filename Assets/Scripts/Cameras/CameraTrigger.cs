using UnityEngine;

public class CameraTrigger : MonoBehaviour
{
    public int cameraIndex; // Indeks kamera yang akan diaktifkan
    private CameraManager cameraManager;

    private void Start()
    {
        // Gunakan metode baru untuk mencari CameraManager
        cameraManager = FindFirstObjectByType<CameraManager>();

        if (cameraManager == null)
        {
            Debug.LogError("CameraManager tidak ditemukan!");
        }

        // Pastikan collider ini adalah trigger
        Collider col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            col.isTrigger = true;
            Debug.LogWarning("Collider tidak diset sebagai trigger. Sudah diubah otomatis.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && cameraManager != null)
        {
            if (cameraManager.CurrentCameraIndex != cameraIndex)
            {
                Debug.Log($"Player masuk ke trigger. Berpindah ke kamera indeks {cameraIndex}");
                cameraManager.SwitchCamera(cameraIndex);
            }
        }
    }
}
