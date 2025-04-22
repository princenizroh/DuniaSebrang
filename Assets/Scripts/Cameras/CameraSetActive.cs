using System;
using UnityEngine;

namespace DS 
{
    public class CameraSetActive : MonoBehaviour
    {
        public GameObject[] cameras;
        public GameObject startCamera;
        private GameObject currentCam;

        private void Start()
        {
            // Set kamera awal
            currentCam = startCamera;

            for (int i = 0; i < cameras.Length; i++)
            {
                cameras[i].SetActive(cameras[i] == currentCam);
            }
        }

        public void SwitchCamera(int cameraIndex)
        {
            if (cameraIndex < 0 || cameraIndex >= cameras.Length)
            {
                Debug.LogError($"Kamera dengan indeks {cameraIndex} tidak valid!");
                return;
            }

            // Matikan kamera saat ini
            currentCam.SetActive(false);

            // Aktifkan kamera baru
            currentCam = cameras[cameraIndex];
            currentCam.SetActive(true);
        }
    }
}

