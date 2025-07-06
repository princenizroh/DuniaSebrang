using UnityEngine;

namespace DS
{
    public class ActivateOnPlayerTrigger : MonoBehaviour
    {
        [SerializeField] private string playerTag = "Player";
        [SerializeField] private GameObject takauObject;
        [SerializeField] private GameObject pasakObject;

        private bool hasActivated = false;

        private void OnTriggerEnter(Collider other)
        {
            if (!hasActivated && other.CompareTag(playerTag))
            {
                if (takauObject != null) takauObject.SetActive(true);
                if (pasakObject != null) pasakObject.SetActive(true);
                hasActivated = true;
            }
        }
    }
}