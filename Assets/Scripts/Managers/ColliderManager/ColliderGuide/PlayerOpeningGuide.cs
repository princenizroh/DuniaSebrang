using System;
using UnityEngine;

namespace DS
{
    public class PlayerOpeningGuide : MonoBehaviour
    {
        public event Action OnPlayerDetected;

        [SerializeField] private GameObject followText; // Referensi ke teks UI

        private void Awake()
        {
            if (followText != null)
            {
                followText.SetActive(false); // Pastikan teks tidak aktif di awal
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // Jika yang masuk adalah Player, panggil event dan aktifkan teks
            if (other.CompareTag("Player"))
            {
                Debug.Log("Player detected by PlayerOpening! Switching GuideAI to patrol.");
                followText?.SetActive(true); // Aktifkan teks "Ikuti jejak kaki ini"
                OnPlayerDetected?.Invoke();
            }
        }
    }
}