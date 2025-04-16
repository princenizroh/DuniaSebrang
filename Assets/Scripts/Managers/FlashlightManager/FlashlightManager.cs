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
        private bool isFlashlightOn = true; // Status flashlight

        private void Awake()
        {
            if (TwoBoneIKConstraint == null)
            {
                TwoBoneIKConstraint = GetComponent<TwoBoneIKConstraint>();
            }
        }

private void Update()
{
    if (Input.GetKeyDown(KeyCode.F))
    {
        ToggleFlashlight();
    }

    // Smoothly transition the weight
    if (TwoBoneIKConstraint != null)
    {
        float targetWeight = isFlashlightOn ? 1f : 0f;
        TwoBoneIKConstraint.weight = Mathf.MoveTowards(TwoBoneIKConstraint.weight, targetWeight, transitionSpeed * Time.deltaTime);
    }

    // Smoothly move flashlight to the target position and rotation
    if (flashlightTransform != null)
    {
        Transform targetPosition = isFlashlightOn ? targetPositionOn : targetPositionOff;

        // Adjust speed based on weight using SmoothStep
        float smoothedWeight = Mathf.SmoothStep(0f, 1f, TwoBoneIKConstraint.weight);
        float adjustedSpeed = transitionSpeed * smoothedWeight;

        // Move position
        flashlightTransform.position = Vector3.MoveTowards(flashlightTransform.position, targetPosition.position, adjustedSpeed * Time.deltaTime);

        // Move rotation
        Quaternion targetRotation = isFlashlightOn ? targetPositionOn.rotation : targetPositionOff.rotation;
        flashlightTransform.rotation = Quaternion.RotateTowards(flashlightTransform.rotation, targetRotation, adjustedSpeed * 100f * Time.deltaTime);
    }
}

        private void ToggleFlashlight()
        {
            isFlashlightOn = !isFlashlightOn;

            // Toggle flashlight visibility only when turning it on
            if (isFlashlightOn)
            {
                // flashlightTransform.gameObject.SetActive(true);
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

            // flashlightTransform.gameObject.SetActive(false);
        }
    }
}