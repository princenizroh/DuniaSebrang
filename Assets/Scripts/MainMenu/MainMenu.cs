using UnityEngine;
using UnityEngine.SceneManagement;

namespace Lilu
{
    public class MainMenu : MonoBehaviour
    {
        public static MainMenu Instance { get; private set; }

        [Header("UI Panels")]
        [SerializeField] private GameObject settingsMenu;
        [SerializeField] private GameObject kotrolMenu;
        [SerializeField] private GameObject aboutUs;

        private bool isSettingsOpen = false;
        private bool isKontrolOpen = false;
        private bool isAboutOpen = false;

        public bool IsMenuOpen => isSettingsOpen || isKontrolOpen || isAboutOpen;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                HandleEscape();
            }
        }

        private void HandleEscape()
        {
            if (isSettingsOpen)
            {
                CloseSettings();
            }
            else if (isKontrolOpen)
            {
                CloseKonrol();
            }
            else if (isAboutOpen)
            {
                CloseAbout();
            }
            else
            {
                HideAllPanels();
            }
        }

        public void OpenSettings()
        {
            isSettingsOpen = true;
            isAboutOpen = false;

            settingsMenu.SetActive(true);
            aboutUs.SetActive(false);
        }
        public void OpenKontrol()
        {
            isSettingsOpen = false;
            isKontrolOpen = true;
            isAboutOpen = false;

            settingsMenu.SetActive(false);
            kotrolMenu.SetActive(true);
            aboutUs.SetActive(false); 
        }

        public void OpenAbout()
        {
            isSettingsOpen = false;
            isAboutOpen = true;

            settingsMenu.SetActive(false);
            aboutUs.SetActive(true);
        }

        public void CloseSettings()
        {
            isSettingsOpen = false;
            settingsMenu.SetActive(false);
        }
        public void CloseKonrol()
        {
            isKontrolOpen = false;
            kotrolMenu.SetActive(false);
            
            isSettingsOpen = true;
            settingsMenu.SetActive(true);
        }


        public void CloseAbout()
        {
            isAboutOpen = false;
            aboutUs.SetActive(false);
        }

        public void HideAllPanels()
        {
            isSettingsOpen = false;
            isAboutOpen = false;

            settingsMenu?.SetActive(false);
            aboutUs?.SetActive(false);
        }
    }
}
