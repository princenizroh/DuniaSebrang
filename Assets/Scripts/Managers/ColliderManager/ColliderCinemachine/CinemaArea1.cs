using UnityEngine;
using UnityEngine.Playables;

namespace DS
{
    public class CinemaArea1 : MonoBehaviour
    {
        [SerializeField] private PlayableDirector cutsceneDircetor;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                Debug.Log("Player detected by CinemaArea1! Starting cutscene.");
                cutsceneDircetor.Play();
                GetComponent<Collider>().enabled = false; // Disable collider to prevent multiple triggers
            }
        }
    }
}
