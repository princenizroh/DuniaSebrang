using UnityEngine;
using UnityEngine.SceneManagement;
using DS.UI;

namespace DS
{
    public class MainMenu : MonoBehaviour
    {
        public static MainMenu Instance { get; private set; }

        [Header("UI Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject settingsMenu;
        [SerializeField] private GameObject dataSaveGame;
        [SerializeField] private GameObject kotrolMenu;
        [SerializeField] private GameObject aboutUs;
        [SerializeField] private GameObject keluar;

        [Header("Save System")]
        [SerializeField] private SaveSlotManager saveSlotManager;

        private bool isMainMenuPanel = false;
        private bool isSettingsOpen = false;
        private bool isdataSaveGame = false;
        private bool isKontrolOpen = false;
        private bool isAboutOpen = false;
        private bool isKeluarOpen = false;

        public bool IsMenuOpen => isMainMenuPanel || isSettingsOpen || isdataSaveGame || isKontrolOpen || isAboutOpen || isKeluarOpen;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Auto-find SaveSlotManager if not assigned
            if (saveSlotManager == null)
                saveSlotManager = FindFirstObjectByType<SaveSlotManager>();
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
            else if (isMainMenuPanel)
            {
                CloseMainMenuPanel();
            }
            else if (isdataSaveGame)
            {
                CloseDataSaveGame();
            }
            else if (isKontrolOpen)
            {
                CloseKonrol();
            }
            else if (isAboutOpen)
            {
                CloseAbout();
            }
            else if (isKeluarOpen)
            {
                CloseKeluar();
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
        public void OpenMainMenuPanel()
        {
            isMainMenuPanel = true;

            mainMenuPanel.SetActive(true);
        }
        public void OpenDataSaveGame()
        {
            isdataSaveGame = true;

            dataSaveGame.SetActive(true);
        }
        public void OpenKontrol()
        {
            isSettingsOpen = false;
            isKontrolOpen = true;

            settingsMenu.SetActive(false);
            kotrolMenu.SetActive(true);
        }

        public void OpenAbout()
        {
            isSettingsOpen = false;
            isAboutOpen = true;

            settingsMenu.SetActive(false);
            aboutUs.SetActive(true);
        }
        public void OpenKeluar()
        {
            isKeluarOpen = true;
            isMainMenuPanel = false;

            keluar.SetActive(true);
            mainMenuPanel.SetActive(false);
        }

        public void CloseSettings()
        {
            isSettingsOpen = false;
            settingsMenu.SetActive(false);
        }
        public void CloseMainMenuPanel()
        {
            isMainMenuPanel = false;
            mainMenuPanel.SetActive(false);
        }
        public void CloseDataSaveGame()
        {
            isdataSaveGame = false;
            dataSaveGame.SetActive(false);
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

        public void CloseKeluar()
        {
            isKeluarOpen = false;
            keluar.SetActive(false);

            isMainMenuPanel = true;
            mainMenuPanel.SetActive(true);
        }

        public void HideAllPanels()
        {
            isSettingsOpen = false;
            isAboutOpen = false;

            settingsMenu?.SetActive(false);
            aboutUs?.SetActive(false);
        }

        /// <summary>
        /// Called when "Mulai Permainan" button is clicked
        /// </summary>
        public void OnMulaiPermainanClicked()
        {
            Debug.Log("★ MULAI PERMAINAN clicked!");
            
            // Open save slot selection in New Game mode
            if (saveSlotManager != null)
            {
                Debug.Log("★ SaveSlotManager found, setting to NewGame mode");
                saveSlotManager.SetMode(SaveSlotManager.SlotSelectionMode.NewGame);
                saveSlotManager.RefreshSaveSlots();
            }
            else
            {
                Debug.LogError("★ SaveSlotManager NOT FOUND!");
            }
            
            // Open data save game panel
            OpenDataSaveGame();
            Debug.Log("★ Data save game panel opened");
        }
        
        /// <summary>
        /// Called when "Lanjutkan" button is clicked
        /// </summary>
        public void OnLanjutkanClicked()
        {
            // Open save slot selection in Continue mode
            if (saveSlotManager != null)
            {
                saveSlotManager.SetMode(SaveSlotManager.SlotSelectionMode.Continue);
                saveSlotManager.RefreshSaveSlots();
            }
            
            // Open data save game panel
            OpenDataSaveGame();
        }
    }
}
