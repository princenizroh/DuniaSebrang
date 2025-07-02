using UnityEngine;
using Unity.Cinemachine;

namespace DS
{
    public class FlashlightCameraFollow : MonoBehaviour
    {
        // Komponen yang diperlukan
        [SerializeField] private FlashlightManager flashlightManager;
        [SerializeField] private CinemachineCamera[] cinemachineCameras; // Array untuk multiple cameras
        [SerializeField] private Transform playerTransform; // Transform player untuk mengetahui arah hadap
        [SerializeField] private float offsetStrength = 3f; // Nilai lebih kecil untuk responsivitas lebih baik
        [SerializeField] private float smoothSpeed = 8f; // Lebih cepat
        [SerializeField] private Vector2 maxCameraOffset = new Vector2(15f, 10f); // Batas lebih kecil
        [SerializeField] private bool onlyWhenFlashlightOn = false; // Selalu aktif untuk testing
        [SerializeField] private float fadeSpeed = 5f; // Fade lebih cepat
        [SerializeField] private bool invertVertical = true; // Untuk membalik kontrol vertikal
        [SerializeField] private bool returnToCenter = true; // Kembali ke center ketika input dilepas
        [SerializeField] private float returnSpeed = 3f; // Kecepatan kembali ke center

        private float currentEffectStrength = 0f; // Untuk fade in/out effect
        private Vector2 defaultPanTilt = Vector2.zero; // Posisi default pan/tilt
        private CinemachineCamera currentActiveCamera = null; // Cache untuk camera yang sedang aktif

        private void Start()
        {
            // Initialize default pan/tilt values untuk semua cameras
            InitializeAllCameras();

            // Auto-assign player transform jika tidak di-set manual
            if (playerTransform == null && flashlightManager != null)
            {
                playerTransform = flashlightManager.transform;
            }
        }

        private void InitializeAllCameras()
        {
            if (cinemachineCameras == null || cinemachineCameras.Length == 0)
            {
                Debug.LogWarning("No Cinemachine Cameras assigned to FlashlightCameraFollow!");
                return;
            }

            foreach (var camera in cinemachineCameras)
            {
                if (camera != null)
                {
                    var panTilt = camera.GetComponent<CinemachinePanTilt>();
                    if (panTilt == null)
                    {
                        Debug.LogWarning($"No Pan Tilt component found on camera: {camera.name}. Please set Rotation Control to 'Pan Tilt' in Cinemachine Camera.");
                    }
                }
            }
        }

        private void LateUpdate()
        {
            if (flashlightManager == null) return;

            // Dapatkan camera yang sedang aktif
            CinemachineCamera activeCamera = GetActiveCamera();
            if (activeCamera == null) return;

            // Update currentActiveCamera dan default pan/tilt jika berubah
            if (currentActiveCamera != activeCamera)
            {
                currentActiveCamera = activeCamera;
                UpdateDefaultPanTilt(activeCamera);
            }

            // Tentukan strength berdasarkan status flashlight
            float targetStrength = 1f;
            if (onlyWhenFlashlightOn)
            {
                targetStrength = IsFlashlightActive() ? 1f : 0f;
            }

            // Smooth fade in/out effect
            currentEffectStrength = Mathf.MoveTowards(currentEffectStrength, targetStrength, fadeSpeed * Time.deltaTime);

            Vector3 aimOffset = flashlightManager.GetAimOffset();
            
            // Debug untuk melihat nilai aimOffset dan currentEffectStrength
            // if (Mathf.Abs(aimOffset.x) > 0.01f || Mathf.Abs(aimOffset.y) > 0.01f)
            // {
            //     Debug.Log($"Camera - AimOffset: {aimOffset}, EffectStrength: {currentEffectStrength}");
            // }
            
            // Jika FlashlightManager menggunakan world space aiming, tidak perlu konversi lagi
            Vector3 cameraRelativeOffset = aimOffset;
            
            // Gunakan Pan Tilt component dari active camera
            var panTilt = activeCamera.GetComponent<CinemachinePanTilt>();
            if (panTilt != null)
            {
                ApplyPanTiltOffset(panTilt, cameraRelativeOffset);
            }
        }

        /// <summary>
        /// Mendapatkan camera yang sedang aktif dari array cameras
        /// </summary>
        private CinemachineCamera GetActiveCamera()
        {
            if (cinemachineCameras == null || cinemachineCameras.Length == 0)
                return null;

            // Cari camera yang sedang aktif (enabled dan priority tertinggi)
            CinemachineCamera activeCamera = null;
            int highestPriority = int.MinValue;

            foreach (var camera in cinemachineCameras)
            {
                if (camera != null && camera.gameObject.activeInHierarchy && camera.enabled)
                {
                    if (camera.Priority > highestPriority)
                    {
                        highestPriority = camera.Priority;
                        activeCamera = camera;
                    }
                }
            }

            return activeCamera;
        }

        /// <summary>
        /// Update default pan/tilt values ketika camera aktif berubah
        /// </summary>
        private void UpdateDefaultPanTilt(CinemachineCamera camera)
        {
            if (camera != null)
            {
                var panTilt = camera.GetComponent<CinemachinePanTilt>();
                if (panTilt != null)
                {
                    defaultPanTilt = new Vector2(panTilt.PanAxis.Value, panTilt.TiltAxis.Value);
                    // Debug.Log($"Updated default pan/tilt for camera: {camera.name} - Pan: {defaultPanTilt.x}, Tilt: {defaultPanTilt.y}");
                }
            }
        }

        private Vector3 ConvertToCameraSpace(Vector3 aimOffset)
        {
            if (playerTransform == null)
                return aimOffset;

            CinemachineCamera activeCamera = GetActiveCamera();
            if (activeCamera == null)
                return aimOffset;

            // Cara sederhana: gunakan transform direction
            // Konversi dari player local space ke world space, lalu ke camera space
            Vector3 worldOffset = playerTransform.TransformDirection(aimOffset);
            Vector3 cameraLocalOffset = activeCamera.transform.InverseTransformDirection(worldOffset);
            
            // Untuk pan/tilt, kita hanya butuh X (pan) dan Y (tilt)
            return new Vector3(cameraLocalOffset.x, cameraLocalOffset.y, 0f);
        }

        private void ApplyPanTiltOffset(CinemachinePanTilt panTilt, Vector3 aimOffset)
        {
            if (returnToCenter)
            {
                // Cek apakah ada input aktif dari flashlight aim
                bool hasInput = Mathf.Abs(aimOffset.x) > 0.01f || Mathf.Abs(aimOffset.y) > 0.01f;
                
                if (hasInput)
                {
                    // Ada input - hitung target berdasarkan aim offset
                    float targetPan = aimOffset.x * offsetStrength * currentEffectStrength;
                    float targetTilt = aimOffset.y * offsetStrength * currentEffectStrength;
                    
                    // Perbaikan untuk hadap kamera: deteksi arah hadap player
                    if (playerTransform != null)
                    {
                        // Dapatkan arah hadap player dalam world space
                        Vector3 playerForward = playerTransform.forward;
                        
                        // Jika player menghadap ke arah kamera (forward.z < 0), balik pan
                        if (playerForward.z < -0.5f) // Threshold untuk mendeteksi hadap kamera
                        {
                            targetPan = -targetPan; // Balik pan untuk hadap kamera
                        }
                    }
                    
                    // Balik arah vertikal jika perlu
                    if (invertVertical)
                    {
                        targetTilt = -targetTilt;
                    }

                    // Clamp values
                    targetPan = Mathf.Clamp(targetPan, -maxCameraOffset.x, maxCameraOffset.x);
                    targetTilt = Mathf.Clamp(targetTilt, -maxCameraOffset.y, maxCameraOffset.y);
                    
                    // Target adalah posisi default + offset
                    Vector2 targetPanTilt = defaultPanTilt + new Vector2(targetPan, targetTilt);
                    
                    // Gunakan smooth speed normal untuk input aktif
                    Vector2 currentPanTilt = new Vector2(panTilt.PanAxis.Value, panTilt.TiltAxis.Value);
                    Vector2 smoothedPanTilt = Vector2.Lerp(currentPanTilt, targetPanTilt, smoothSpeed * Time.deltaTime);
                    
                    panTilt.PanAxis.Value = smoothedPanTilt.x;
                    panTilt.TiltAxis.Value = smoothedPanTilt.y;
                }
                else
                {
                    // Tidak ada input - kembali ke posisi default
                    Vector2 currentPanTilt = new Vector2(panTilt.PanAxis.Value, panTilt.TiltAxis.Value);
                    Vector2 smoothedPanTilt = Vector2.Lerp(currentPanTilt, defaultPanTilt, returnSpeed * Time.deltaTime);
                    
                    panTilt.PanAxis.Value = smoothedPanTilt.x;
                    panTilt.TiltAxis.Value = smoothedPanTilt.y;
                }
            }
            else
            {
                // Mode lama - tanpa return to center
                float targetPan = aimOffset.x * offsetStrength * currentEffectStrength;
                float targetTilt = aimOffset.y * offsetStrength * currentEffectStrength;
                
                if (invertVertical)
                {
                    targetTilt = -targetTilt;
                }

                targetPan = Mathf.Clamp(targetPan, -maxCameraOffset.x, maxCameraOffset.x);
                targetTilt = Mathf.Clamp(targetTilt, -maxCameraOffset.y, maxCameraOffset.y);

                Vector2 currentPanTilt = new Vector2(panTilt.PanAxis.Value, panTilt.TiltAxis.Value);
                Vector2 targetPanTilt = defaultPanTilt + new Vector2(targetPan, targetTilt);
                Vector2 smoothedPanTilt = Vector2.Lerp(currentPanTilt, targetPanTilt, smoothSpeed * Time.deltaTime);

                panTilt.PanAxis.Value = smoothedPanTilt.x;
                panTilt.TiltAxis.Value = smoothedPanTilt.y;
            }
        }

        private bool IsFlashlightActive()
        {
            // Cek status flashlight dari FlashlightManager
            return flashlightManager.IsFlashlightOn() && flashlightManager.GetFlashlightWeight() > 0.1f;
        }

        private bool GetFlashlightActiveStatus()
        {
            // Method helper untuk mendapatkan status flashlight
            return flashlightManager.IsFlashlightOn();
        }

        // Method untuk mengatur strength secara manual dari script lain
        public void SetOffsetStrength(float newStrength)
        {
            offsetStrength = newStrength;
        }

        // Method untuk mengatur apakah efek hanya aktif saat flashlight menyala
        public void SetOnlyWhenFlashlightOn(bool value)
        {
            onlyWhenFlashlightOn = value;
        }

        // Method untuk mengatur inversi vertikal
        public void SetInvertVertical(bool value)
        {
            invertVertical = value;
        }

        // Method untuk mengatur return to center
        public void SetReturnToCenter(bool value)
        {
            returnToCenter = value;
        }

        // Method untuk mengatur kecepatan return to center
        public void SetReturnSpeed(float speed)
        {
            returnSpeed = speed;
        }

        // Method untuk reset posisi kamera ke default secara manual
        public void ResetCameraToDefault()
        {
            CinemachineCamera activeCamera = GetActiveCamera();
            if (activeCamera != null)
            {
                var panTilt = activeCamera.GetComponent<CinemachinePanTilt>();
                if (panTilt != null)
                {
                    panTilt.PanAxis.Value = defaultPanTilt.x;
                    panTilt.TiltAxis.Value = defaultPanTilt.y;
                }
            }
        }

        // Method untuk set player transform secara manual
        public void SetPlayerTransform(Transform player)
        {
            playerTransform = player;
        }

        // Method untuk menambah camera ke array
        public void AddCamera(CinemachineCamera camera)
        {
            if (camera == null) return;

            // Cek apakah camera sudah ada dalam array
            if (cinemachineCameras != null)
            {
                foreach (var existingCamera in cinemachineCameras)
                {
                    if (existingCamera == camera) return; // Sudah ada
                }
            }

            // Tambahkan camera ke array
            if (cinemachineCameras == null)
            {
                cinemachineCameras = new CinemachineCamera[] { camera };
            }
            else
            {
                CinemachineCamera[] newArray = new CinemachineCamera[cinemachineCameras.Length + 1];
                cinemachineCameras.CopyTo(newArray, 0);
                newArray[cinemachineCameras.Length] = camera;
                cinemachineCameras = newArray;
            }

        }

        // Method untuk menghapus camera dari array
        public void RemoveCamera(CinemachineCamera camera)
        {
            if (camera == null || cinemachineCameras == null) return;

            // Cari index camera
            int indexToRemove = -1;
            for (int i = 0; i < cinemachineCameras.Length; i++)
            {
                if (cinemachineCameras[i] == camera)
                {
                    indexToRemove = i;
                    break;
                }
            }

            if (indexToRemove == -1) return; // Camera tidak ditemukan

            // Buat array baru tanpa camera tersebut
            CinemachineCamera[] newArray = new CinemachineCamera[cinemachineCameras.Length - 1];
            int newIndex = 0;
            for (int i = 0; i < cinemachineCameras.Length; i++)
            {
                if (i != indexToRemove)
                {
                    newArray[newIndex] = cinemachineCameras[i];
                    newIndex++;
                }
            }

            cinemachineCameras = newArray;
        }

        // Method untuk mendapatkan camera yang sedang aktif (public)
        public CinemachineCamera GetCurrentActiveCamera()
        {
            return GetActiveCamera();
        }

        // Method untuk set cameras array secara langsung
        public void SetCameras(CinemachineCamera[] cameras)
        {
            cinemachineCameras = cameras;
            InitializeAllCameras();
        }

        // Method untuk reset semua cameras ke posisi default
        public void ResetAllCamerasToDefault()
        {
            if (cinemachineCameras == null) return;

            foreach (var camera in cinemachineCameras)
            {
                if (camera != null)
                {
                    var panTilt = camera.GetComponent<CinemachinePanTilt>();
                    if (panTilt != null)
                    {
                        panTilt.PanAxis.Value = 0f;
                        panTilt.TiltAxis.Value = 0f;
                    }
                }
            }
        }
    }
}
