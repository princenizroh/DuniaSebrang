using UnityEngine;
using UnityEngine.UI;
using Unity.Cinemachine; // Ganti ke UnityEngine.Cinemachine jika pakai versi umum

[System.Serializable]
public struct ButtonCameraLink
{
    public Button button;
    public int targetCameraIndex;
}

public class CameraButtonTrigger : MonoBehaviour
{
    [Header("Cinemachine Cameras")]
    public CinemachineCamera[] cameras;

    [Header("Button → Camera Index")]
    public ButtonCameraLink[] buttonTo;

    private int currentCameraIndex = 0;

    void Start()
    {
        // Hanya aktifkan kamera awal
        for (int i = 0; i < cameras.Length; i++)
        {
            cameras[i].Priority = (i == currentCameraIndex) ? 10 : 0;
        }

        // Atur semua tombol agar berpindah ke kamera target masing-masing
        foreach (var link in buttonTo)
        {
            int targetIndex = link.targetCameraIndex;
            link.button.onClick.AddListener(() => SwitchCamera(targetIndex));
        }
    }

    void SwitchCamera(int targetIndex)
    {
        if (targetIndex < 0 || targetIndex >= cameras.Length) return;

        // Nonaktifkan kamera saat ini
        cameras[currentCameraIndex].Priority = 0;

        // Aktifkan kamera tujuan
        cameras[targetIndex].Priority = 10;

        // Perbarui index kamera aktif
        currentCameraIndex = targetIndex;
    }
}
