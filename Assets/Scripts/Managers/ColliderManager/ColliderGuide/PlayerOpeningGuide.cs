using System;
using UnityEngine;

namespace DS
{
    public class PlayerOpeningGuide : MonoBehaviour
    {
        public event Action OnPlayerDetected;

        [SerializeField] private GameObject followText; // Referensi ke teks UI
        [SerializeField] private GameObject followText2; // Referensi ke teks UI kedua
        [SerializeField] private GameObject followText3; // Referensi ke teks UI ketiga
        [SerializeField] private bool area1Trigger = false; // Apakah ini adalah trigger area 1
        [SerializeField] private bool area2Trigger = false; // Apakah ini adalah trigger area 2
        [SerializeField] private bool area3Trigger = false; // Apakah ini adalah trigger area 2
        [SerializeField] private bool turnOffTrigger = false; 
        [SerializeField] private bool isPlayerInside = false;

        private void Awake()
        {

        }

        private void Update()
        {
            if (isPlayerInside && turnOffTrigger && Input.GetKeyDown(KeyCode.B))
            {
                Debug.Log("Player pressed B to turn off the trigger");
                followText?.SetActive(false); // Nonaktifkan teks "Ikuti jejak kaki ini"
                followText2?.SetActive(true);
                followText3?.SetActive(true); // Nonaktifkan teks "Ikuti jejak kaki ini"
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
                isPlayerInside = true;
            }

            if (other.CompareTag("Player") && area1Trigger == true)
            {
                followText?.SetActive(false); // Nonaktifkan teks "Ikuti jejak kaki ini"
            }

            if (other.CompareTag("Player") && area2Trigger == true)
            {
                followText?.SetActive(true); // Nonaktifkan teks "Ikuti jejak kaki ini"
            }

            if (other.CompareTag("Player") && area3Trigger == true)
            {
                followText?.SetActive(false); // Nonaktifkan teks "Ikuti jejak kaki ini"
            }
        }

    }
}