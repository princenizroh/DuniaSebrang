using UnityEngine;
using Unity.Cinemachine;

public class CameraManager : MonoBehaviour
{
    public CinemachineCamera[] cameras;
    public CinemachineCamera startCamera;
    private CinemachineCamera currentCam;
    private int currentCamIndex;

    public int CurrentCameraIndex => currentCamIndex;

    private void Start()
    {
        currentCam = startCamera;
        currentCamIndex = System.Array.IndexOf(cameras, currentCam);

        for (int i = 0; i < cameras.Length; i++)
        {
            cameras[i].enabled = (cameras[i] == currentCam);
        }
    }

    public void SwitchCamera(int cameraIndex)
    {
        if (cameraIndex < 0 || cameraIndex >= cameras.Length)
        {
            Debug.LogError($"Kamera dengan indeks {cameraIndex} tidak valid!");
            return;
        }

        if (cameraIndex == currentCamIndex)
        {
            return;
        }

        currentCam.enabled = false;

        currentCam = cameras[cameraIndex];
        currentCam.enabled = true;
        currentCamIndex = cameraIndex;
    }
}
