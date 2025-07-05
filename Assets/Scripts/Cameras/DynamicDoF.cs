using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DS
{
    public class DynamicDoF : MonoBehaviour
    {
        Ray raycast;
        RaycastHit hit;
        bool isHit;
        float hitDistance;
        public Volume volume;
        DepthOfField depthOfField;

        [Range(1, 10)]
        public float focusSpeed = 5f;
        public float maxFocusDistance = 20f;
        public GameObject focusObject;

        private void Start()
        {
            if (volume != null)
            {
                if (volume.profile.TryGet(out DepthOfField dof))
                {
                    depthOfField = dof;
                }
                else
                {
                    Debug.LogWarning("Depth of Field tidak ditemukan pada Volume.");
                }
            }
            else
            {
                Debug.LogWarning("Volume tidak diassign.");
            }
        }

        private void Update()
        {
            if (depthOfField == null) return;

            raycast = new Ray(transform.position, transform.forward);
            isHit = false;

            if (focusObject != null)
            {
                hitDistance = Vector3.Distance(transform.position, focusObject.transform.position);
            }
            else
            {
                if (Physics.Raycast(raycast, out hit, maxFocusDistance))
                {
                    isHit = true;
                    hitDistance = Vector3.Distance(transform.position, hit.point);
                    Debug.Log("Hit: " + hit.point);
                }
                else
                {
                    hitDistance = Mathf.Lerp(hitDistance, maxFocusDistance, Time.deltaTime * focusSpeed);
                }
            }

            SetFocus();
        }

        void SetFocus()
        {
            depthOfField.focusDistance.value = Mathf.Lerp(depthOfField.focusDistance.value, hitDistance, Time.deltaTime * focusSpeed);
        }

        private void OnDrawGizmos()
        {
            if (isHit)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(hit.point, 0.1f);
                Debug.DrawRay(transform.position, transform.forward * hitDistance, Color.red);
            }
            else
            {
                Debug.DrawRay(transform.position, transform.forward * maxFocusDistance, Color.green);
            }
        }
    }
}