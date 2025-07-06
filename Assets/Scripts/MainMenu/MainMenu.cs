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

            // Setup SaveSlotManager event handlers
            SetupSaveSlotEvents();
        }

        /// <summary>
        /// Setup event handlers for SaveSlotManager
        /// </summary>
        private void SetupSaveSlotEvents()
        {
            if (saveSlotManager != null)
            {
                // Subscribe to slot selection events
                saveSlotManager.OnNewGameSlotSelected += OnNewGameStartedInSlot;
                saveSlotManager.OnLoadGameSlotSelected += OnGameLoadedFromSlot;

                Debug.Log("★ SaveSlotManager events connected to MainMenu");
            }
            else
            {
                Debug.LogWarning("★ SaveSlotManager not found, events not connected");
            }
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
            Debug.Log("★ LANJUTKAN clicked!");

            // Open save slot selection in Continue mode
            if (saveSlotManager != null)
            {
                Debug.Log("★ SaveSlotManager found, setting to Continue mode");
                saveSlotManager.SetMode(SaveSlotManager.SlotSelectionMode.Continue);
                saveSlotManager.RefreshSaveSlots();
            }
            else
            {
                Debug.LogError("★ SaveSlotManager NOT FOUND!");
            }

            // Open data save game panel
            OpenDataSaveGame();
            Debug.Log("★ Data save game panel opened in Continue mode");
        }

        /// <summary>
        /// Check if there's any save data available for continue option
        /// </summary>
        public bool HasAnySaveData()
        {
            if (saveSlotManager == null) return false;

            // Check all slots for data (assuming 5 slots max)
            for (int i = 0; i < 5; i++)
            {
                var slotInfo = saveSlotManager.GetEnhancedSaveSlotInfo(i);
                if (!slotInfo.isEmpty)
                {
                    Debug.Log($"★ Found save data in slot {i}: {slotInfo.areaName}");
                    return true;
                }
            }

            Debug.Log("★ No save data found in any slot");
            return false;
        }

        /// <summary>
        /// Update continue button availability based on save data
        /// Call this method to enable/disable continue button
        /// </summary>
        public void UpdateContinueButtonState()
        {
            // This method can be called from UI to update button state
            bool hasData = HasAnySaveData();
            Debug.Log($"★ Continue button should be {(hasData ? "ENABLED" : "DISABLED")}");

            // You can add UI update logic here if needed
            // For example: continueButton.interactable = hasData;
        }

        /// <summary>
        /// Close data save game panel and return to main menu
        /// </summary>
        public void OnBackToMainMenuFromSlots()
        {
            Debug.Log("★ BACK TO MAIN MENU from save slots");

            // Force close any open confirmation dialogs
            if (saveSlotManager != null)
            {
                // Try to find and close confirmation dialog
                var confirmationDialog = FindFirstObjectByType<SaveSlotConfirmationDialog>();
                if (confirmationDialog != null)
                {
                    confirmationDialog.ForceCloseDialog();
                    Debug.Log("★ Forced close confirmation dialog");
                }
            }

            // Close data save game panel
            CloseDataSaveGame();

            // Show main menu panel
            OpenMainMenuPanel();
        }

        /// <summary>
        /// Handle when a game is successfully loaded from slot
        /// </summary>
        public void OnGameLoadedFromSlot(int slotIndex)
        {
            Debug.Log($"★ Game loaded successfully from slot {slotIndex}");

            // Close all menu panels since game is starting
            HideAllPanels();
            CloseDataSaveGame();
            CloseMainMenuPanel();
        }

        /// <summary>
        /// Handle when a new game is started in slot
        /// </summary>
        public void OnNewGameStartedInSlot(int slotIndex)
        {
            Debug.Log($"★ New game started successfully in slot {slotIndex}");

            // Close all menu panels since game is starting
            HideAllPanels();
            CloseDataSaveGame();
            CloseMainMenuPanel();
        }

        private void OnDestroy()
        {
            // Cleanup event subscriptions
            if (saveSlotManager != null)
            {
                saveSlotManager.OnNewGameSlotSelected -= OnNewGameStartedInSlot;
                saveSlotManager.OnLoadGameSlotSelected -= OnGameLoadedFromSlot;
            }
        }
        public void QuitGame()
        {
            Debug.Log("Keluar dari game...");

            // Jika dijalankan di editor Unity
    #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
    #else
            // Jika dijalankan sebagai build (PC, Android, dll)
            Application.Quit();
    #endif
        }

    }
}
