using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace Lilu
{
    public class MainMenu : MonoBehaviour
    {
        public static MainMenu Instance { get; set; } 
        
        [SerializeField] private GameObject settingsMenu;
        [SerializeField] private GameObject brightnessMenu;
        [SerializeField] private GameObject audioMenu;
        [SerializeField] private GameObject guideMenu;
        [SerializeField] private GameObject aboutMenu;
        [SerializeField] private GameObject newGameMenu;
        // public DataManager dataManager;

        private bool isSetting = false;
        private bool isBrightness = false;
        private bool isAudio = false;
        private bool isGuide = false;
        private bool isAbout = false;

        public bool isMenuOpen;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (isSetting)
                {
                    CloseSettings();
                }
                else if (isBrightness)
                {
                    CloseBrightness();
                }
                else if (isAudio)
                {
                    CloseAudio();
                }
                else if (isGuide)
                {
                    CloseGuide();
                }
                else if (isAbout)
                {
                    CloseAbout();
                }
                else
                {
                    BackToMainMenu();
                }
            }
        }

        public void OpenSettings()
        {
            isSetting = true;
            isBrightness = false;
            isAudio = false;
            isGuide = false;
            isAbout = false;

            settingsMenu.SetActive(true);
            brightnessMenu.SetActive(false);
            audioMenu.SetActive(false);
            guideMenu.SetActive(false);
            aboutMenu.SetActive(false);
        }

        public void OpenBrightness()
        {
            isSetting = false;
            isBrightness = true;
            isAudio = false;

            settingsMenu.SetActive(false);
            brightnessMenu.SetActive(true);
            audioMenu.SetActive(false);
        }

        public void OpenAudio()
        {
            isSetting = false;
            isBrightness = false;
            isAudio = true;

            settingsMenu.SetActive(false);
            brightnessMenu.SetActive(false);
            audioMenu.SetActive(true);
        }

        public void OpenGuide()
        {
            isSetting = false;
            isBrightness = false;
            isAudio = false;
            isGuide = true;

            settingsMenu.SetActive(false);
            brightnessMenu.SetActive(false);
            audioMenu.SetActive(false);
            guideMenu.SetActive(true);
        }

        public void OpenAbout()
        {
            isSetting = false;
            isBrightness = false;
            isAudio = false;
            isGuide = false;
            isAbout = true;

            settingsMenu.SetActive(false);
            brightnessMenu.SetActive(false);
            audioMenu.SetActive(false);
            guideMenu.SetActive(false);
            aboutMenu.SetActive(true);
        }

        public void CloseSettings()
        {
            isSetting = false;
            settingsMenu.SetActive(false);
        }

        public void CloseBrightness()
        {
            isBrightness = false;
            brightnessMenu.SetActive(false);
        }

        public void CloseAudio()
        {
            isAudio = false;
            audioMenu.SetActive(false);
        }

        public void CloseGuide()
        {
            isGuide = false;
            guideMenu.SetActive(false);
        }

        public void CloseAbout()
        {
            isAbout = false;
            aboutMenu.SetActive(false);
        }

        public void NewGame()
        {
            // Implementation for starting a new game
        }

        // public void ContinueGame()
        // {
        //     StartCoroutine(LoadGameCoroutine());
        // }

        // private IEnumerator LoadGameCoroutine()
        // {
        //   // Load the game data
            
        //     // Wait until the scene has loaded
        //     AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("EastVillage");
        //     while (!asyncLoad.isDone)
        //     {
        //         yield return null;
        //     }
        //     if (dataManager == null)
        //     {
        //         Debug.LogError("DataManager is null!");
        //         yield break;
        //     }
        //     // Load the game data
        //     dataManager.DataLoad();

        //     // Set the player position after the scene has loaded
        //     GameObject player = GameObject.FindWithTag("Player");
        //     if (player != null)
        //     {
        //         player.transform.position = new Vector3(dataManager.datas.playerPosX, dataManager.datas.playerPosY, player.transform.position.z);
        //     }
        // }
        public void BackToMainMenu()
        {
            isSetting = false;
            isBrightness = false;
            isAudio = false;
            isGuide = false;
            isAbout = false;

            settingsMenu.SetActive(false);
            brightnessMenu.SetActive(false);
            audioMenu.SetActive(false);
            guideMenu.SetActive(false);
            aboutMenu.SetActive(false);
        }


    }
}
