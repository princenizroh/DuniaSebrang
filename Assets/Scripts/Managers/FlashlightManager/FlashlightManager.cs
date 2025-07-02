using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace DS
{
    public class FlashlightManager : MonoBehaviour
    {
        [SerializeField] TwoBoneIKConstraint TwoBoneIKConstraint;
        [SerializeField] Transform flashlightTransform;
        [SerializeField] Transform targetPositionOn; // Posisi target saat flashlight menyala
        [SerializeField] Transform targetPositionOff; // Posisi target saat flashlight mati
        [SerializeField] float transitionSpeed = 2f; // Kecepatan transisi weight
        [SerializeField] private Transform aimTarget;
        [SerializeField] private float aimSpeed = 0.5f; // Turunkan speed agar tidak terlalu sensitif
        [SerializeField] private Vector2 xRange = new Vector2(-1f, 1f);
        [SerializeField] private Vector2 yRange = new Vector2(-0.5f, 1f);
        [SerializeField] private float fixedZ = 1f;
        [SerializeField] private bool useWorldSpaceAiming = true; // Gunakan koordinat dunia untuk aiming
        private Vector3 aimOffset = new Vector3(0f, 0f, 0f);
        private Vector3 targetOffset = new Vector3(0f, 0f, 0f);
        private bool isFlashlightOn = false; // Status flashlight
        private void Awake()
        {
            if (TwoBoneIKConstraint == null)
            {
                TwoBoneIKConstraint = GetComponent<TwoBoneIKConstraint>();
            }
            if (flashlightTransform != null)
                flashlightTransform.gameObject.SetActive(false);
        }
        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                ToggleFlashlight();
            }
            UpdateAimDirection();

            UpdateWeight();
            UpdateFlashlightTransform();
        }

        private void UpdateAimDirection()
        {
            float xInput = 0f;
            float yInput = 0f;

            // Coba gunakan Input Manager terlebih dahulu
            try
            {
                xInput = Input.GetAxisRaw("AimHorizontal");
                yInput = Input.GetAxisRaw("AimVertical");
            }
            catch (System.Exception)
            {
                // Input Manager tidak ditemukan, gunakan arrow keys langsung
            }

            // Fallback: gunakan arrow keys jika tidak ada input dari Input Manager
            if (Mathf.Abs(xInput) < 0.01f && Mathf.Abs(yInput) < 0.01f)
            {
                if (Input.GetKey(KeyCode.RightArrow)) xInput = 1f;
                else if (Input.GetKey(KeyCode.LeftArrow)) xInput = -1f;
                
                if (Input.GetKey(KeyCode.UpArrow)) yInput = 1f;
                else if (Input.GetKey(KeyCode.DownArrow)) yInput = -1f;
            }

            Vector2 input = new Vector2(xInput, yInput);
            
            // Debug input untuk memastikan input terdeteksi
            // if (input.magnitude > 0.01f)
            // {
            //     Debug.Log($"Flashlight Input: {input}, Player Forward: {transform.forward}");
            // }
            
            // Sistem dengan deteksi arah hadap player
            if (input.magnitude > 0.01f)
            {
                // Perbaikan untuk hadap kamera: deteksi arah hadap player
                float adjustedXInput = input.x;
                Vector3 playerForward = transform.forward;
                
                // Jika player menghadap ke arah kamera (forward.z < 0), balik input horizontal
                if (playerForward.z < -0.5f) // Threshold untuk mendeteksi hadap kamera
                {
                    adjustedXInput = -input.x; // Balik input horizontal untuk hadap kamera
                    // Debug.Log($"Player facing camera - Input X flipped: {input.x} -> {adjustedXInput}");
                }
                
                // Langsung gunakan input yang sudah disesuaikan untuk update targetOffset
                targetOffset += new Vector3(adjustedXInput, input.y, 0f) * aimSpeed * Time.deltaTime;
            }
            else
            {
                // Tidak ada input - kembali ke center
                targetOffset = Vector3.Lerp(targetOffset, Vector3.zero, Time.deltaTime * 5f);
            }

            // Clamp target
            targetOffset.x = Mathf.Clamp(targetOffset.x, xRange.x, xRange.y);
            targetOffset.y = Mathf.Clamp(targetOffset.y, yRange.x, yRange.y);

            // Lerp menuju targetOffset
            aimOffset = Vector3.Lerp(aimOffset, targetOffset, Time.deltaTime * 10f);

            // Debug targetOffset untuk troubleshooting
            // if (Mathf.Abs(targetOffset.x) > 0.01f || Mathf.Abs(targetOffset.y) > 0.01f)
            // {
            //     Debug.Log($"Flashlight TargetOffset: {targetOffset}, AimOffset: {aimOffset}");
            // }

            // Tetap di depan karakter
            aimTarget.localPosition = new Vector3(aimOffset.x, aimOffset.y + 1f, fixedZ);
        }

        private void UpdateWeight()
        {
            if (TwoBoneIKConstraint != null)
            {
                float targetWeight = isFlashlightOn ? 1f : 0f;
                TwoBoneIKConstraint.weight = Mathf.MoveTowards(TwoBoneIKConstraint.weight, targetWeight, transitionSpeed * Time.deltaTime);
            }
        }

        private void UpdateFlashlightTransform()
        {
            if (flashlightTransform != null)
            {
                Transform targetPosition = isFlashlightOn ? targetPositionOn : targetPositionOff;

                float smoothedWeight = Mathf.SmoothStep(0f, 1f, TwoBoneIKConstraint.weight);
                float adjustedSpeed = transitionSpeed * smoothedWeight;

                // Move position
                flashlightTransform.position = Vector3.MoveTowards(flashlightTransform.position, targetPosition.position, adjustedSpeed * Time.deltaTime);

                // Move rotation
                Quaternion targetRotation = isFlashlightOn ? targetPositionOn.rotation : targetPositionOff.rotation;
                flashlightTransform.rotation = Quaternion.RotateTowards(flashlightTransform.rotation, targetRotation, adjustedSpeed * 100f * Time.deltaTime);
            }
        }

        public void ToggleFlashlight()
        {
            isFlashlightOn = !isFlashlightOn;

            // Toggle flashlight visibility only when turning it on
            if (isFlashlightOn)
            {
                flashlightTransform.gameObject.SetActive(true);
            }
            else
            {
                // Delay turning off the flashlight until the weight reaches 0
                StartCoroutine(WaitForWeightToReachZero());
            }
        }

        private System.Collections.IEnumerator WaitForWeightToReachZero()
        {
            while (TwoBoneIKConstraint.weight > 0)
            {
                yield return null; // Wait for the next frame
            }

            flashlightTransform.gameObject.SetActive(false);
        }
        
        public Vector3 GetAimOffset()
        {
            return aimOffset;
        }

        public bool IsFlashlightOn()
        {
            return isFlashlightOn;
        }

        public float GetFlashlightWeight()
        {
            return TwoBoneIKConstraint != null ? TwoBoneIKConstraint.weight : 0f;
        }

        public void SetUseWorldSpaceAiming(bool useWorldSpace)
        {
            useWorldSpaceAiming = useWorldSpace;
        }

        public bool GetUseWorldSpaceAiming()
        {
            return useWorldSpaceAiming;
        }
    }
}