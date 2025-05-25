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
        [SerializeField] private float aimSpeed = 1f;
        [SerializeField] private Vector2 xRange = new Vector2(-1f, 1f);
        [SerializeField] private Vector2 yRange = new Vector2(-0.5f, 1f);
        [SerializeField] private float fixedZ = 1f;

        private Vector3 aimOffset = Vector3.zero;
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
            float xInput = Input.GetAxisRaw("AimHorizontal");
            float yInput = Input.GetAxisRaw("AimVertical");

            Vector2 input = new Vector2(xInput, yInput).normalized;
            aimOffset += new Vector3(input.x, input.y, 0f) * aimSpeed * Time.deltaTime;

            // Clamp supaya aim tidak terlalu jauh
            aimOffset.x = Mathf.Clamp(aimOffset.x, xRange.x, xRange.y);
            aimOffset.y = Mathf.Clamp(aimOffset.y, yRange.x, yRange.y);

            // Tetap di depan karakter (z tetap)
            aimTarget.localPosition = new Vector3(aimOffset.x, aimOffset.y, fixedZ);
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
    }
}