using UnityEngine;
using Unity.Cinemachine;

public class CameraManager : MonoBehaviour
{
    public CinemachineCamera[] cameras;
    public CinemachineCamera startCamera;
    private CinemachineCamera currentCam;

    private void Start()
    {
        // Set kamera awal
        currentCam = startCamera;

        for (int i = 0; i < cameras.Length; i++)
        {
            cameras[i].enabled = (cameras[i] == currentCam);
        }
    }

    public void SwitchCamera(int cameraIndex)
    {
        // Validasi indeks kamera
        if (cameraIndex < 0 || cameraIndex >= cameras.Length)
        {
            Debug.LogError($"Kamera dengan indeks {cameraIndex} tidak valid!");
            return;
        }

        // Matikan kamera saat ini
        currentCam.enabled = false;

        // Aktifkan kamera baru
        currentCam = cameras[cameraIndex];
        currentCam.enabled = true;
    }
}
