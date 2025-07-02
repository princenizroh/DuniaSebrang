using UnityEngine;
using UnityEngine.UI;
using Unity.Cinemachine;

public class CameraButtonTrigger : MonoBehaviour
{
    [Header("Cinemachine Cameras")]
    public CinemachineCamera[] cameras;

    [Header("Buttons")]
    public Button[] buttonTo;
    public Button[] buttonBack;

    private int currentCameraIndex = 0;
    private bool isAtNextCamera = false;

    void Start()
    {
        // Pastikan hanya kamera awal yang aktif
        for (int i = 0; i < cameras.Length; i++)
        {
            cameras[i].Priority = (i == currentCameraIndex) ? 10 : 0;
        }

        // Pasang listener untuk tombol-tombol
        for (int i = 0; i < buttonTo.Length; i++)
        {
            int index = i;
            buttonTo[i].onClick.AddListener(() => SwitchCamera(index + 1));
        }

        for (int i = 0; i < buttonBack.Length; i++)
        {
            int index = i;
            buttonBack[i].onClick.AddListener(() => SwitchCamera(index));
        }
    }

    /// <summary>
    /// Fungsi untuk berpindah kamera dengan mengatur Priority
    /// </summary>
    /// <param name="targetIndex"></param>
    void SwitchCamera(int targetIndex)
    {
        if (targetIndex < 0 || targetIndex >= cameras.Length) return;

        // Turunkan kamera saat ini
        cameras[currentCameraIndex].Priority = 0;

        // Naikkan kamera tujuan
        cameras[targetIndex].Priority = 10;

        currentCameraIndex = targetIndex;
    }
}
