using UnityEngine;
using UnityEngine.AI;

namespace DS
{
    public class EnemyAI : MonoBehaviour
    {
        private NavMeshAgent agent;
        private Transform player;
        private bool isChasing = false;

        void Start()
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            agent = GetComponent<NavMeshAgent>();
        }

        void Update()
        {
            if (isChasing && player != null)
            {
                agent.SetDestination(player.position);
            }
            }
        void OnTriggerEnter(Collider other)
        {
            // Jika menyentuh FearShield (bukan langsung Player), mulai mengejar
            if (other.CompareTag("Light"))
            {
                Debug.Log("Enemy mendeteksi FearShield! Mulai mengejar.");
                isChasing = true;
            }
        }

        void OnTriggerExit(Collider other)
        {
            // Jika FearShield keluar dari area deteksi, berhenti mengejar
            if (other.CompareTag("Light"))
            {
                Debug.Log("FearShield keluar dari jangkauan! Enemy berhenti mengejar.");
                isChasing = false;
                agent.ResetPath();
            }
        }
    }
}
