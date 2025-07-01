using UnityEngine;
using Unity.Cinemachine;

namespace DS
{
    public class FlashlightCameraFollow : MonoBehaviour
    {
        // Komponen yang diperlukan
        [SerializeField] private FlashlightManager flashlightManager;
        [SerializeField] private CinemachineCamera cinemachineCamera;
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

        private void Start()
        {
            if (cinemachineCamera != null)
            {
                var panTilt = cinemachineCamera.GetComponent<CinemachinePanTilt>();
                if (panTilt != null)
                {
                    Debug.Log("Using Pan Tilt for camera control");
                    // Simpan posisi default pan/tilt
                    defaultPanTilt = new Vector2(panTilt.PanAxis.Value, panTilt.TiltAxis.Value);
                }
                else
                {
                    Debug.LogWarning("No Pan Tilt component found. Please set Rotation Control to 'Pan Tilt' in Cinemachine Camera.");
                }
            }

            // Auto-assign player transform jika tidak di-set manual
            if (playerTransform == null && flashlightManager != null)
            {
                playerTransform = flashlightManager.transform;
            }
        }

        private void LateUpdate()
        {
            if (flashlightManager == null || cinemachineCamera == null) return;

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
            if (Mathf.Abs(aimOffset.x) > 0.01f || Mathf.Abs(aimOffset.y) > 0.01f)
            {
                Debug.Log($"Camera - AimOffset: {aimOffset}, EffectStrength: {currentEffectStrength}");
            }
            
            // Jika FlashlightManager menggunakan world space aiming, tidak perlu konversi lagi
            Vector3 cameraRelativeOffset = aimOffset;
            
            // Gunakan Pan Tilt component
            var panTilt = cinemachineCamera.GetComponent<CinemachinePanTilt>();
            if (panTilt != null)
            {
                ApplyPanTiltOffset(panTilt, cameraRelativeOffset);
            }
        }

        private Vector3 ConvertToCameraSpace(Vector3 aimOffset)
        {
            if (playerTransform == null || cinemachineCamera == null)
                return aimOffset;

            // Cara sederhana: gunakan transform direction
            // Konversi dari player local space ke world space, lalu ke camera space
            Vector3 worldOffset = playerTransform.TransformDirection(aimOffset);
            Vector3 cameraLocalOffset = cinemachineCamera.transform.InverseTransformDirection(worldOffset);
            
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
            if (cinemachineCamera != null)
            {
                var panTilt = cinemachineCamera.GetComponent<CinemachinePanTilt>();
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
    }
}
