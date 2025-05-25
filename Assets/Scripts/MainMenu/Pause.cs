using UnityEngine;
using UnityEngine.SceneManagement;

namespace Lilu
{
    public class Pause : MonoBehaviour
    {
        [SerializeField] private GameObject pauseMenu;
        [SerializeField] private GameObject settingsMenu;
        [SerializeField] private GameObject brightnessMenu;
        [SerializeField] private GameObject audioMenu;
        [SerializeField] private GameObject pauseButton;
        [SerializeField] private GameObject ControlMenu;

        // public SaveLoadManager saveLoadManager;
        private bool isPaused = false;
        private bool isSetting = false;
        private bool isBrightness = false;
        private bool isAudio = false;
        private bool isControl = false;


        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (isSetting || isAudio || isControl)
                {
                    ResumeGame();
                }
                else if (isBrightness)
                {
                    CloseBrightness();
                }
                else if (isPaused)
                {
                    ResumeGame();
                }
                else
                {
                    PauseGame();
                }
            }
        }

        public void PauseGame()
        {
            isPaused = true;
            pauseMenu.SetActive(true);
            pauseButton.SetActive(false); // Hide pause button
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            // PickUpManager.Instance.DisableSelection();
            // PickUpManager.Instance.GetComponent<PickUpManager>().enabled = false;
        }

        // public void SaveGame()
        // {
        //     saveLoadManager.SaveGame();
        //     Debug.Log("Game Saved");
        // }

        public void ResumeGame()
        {
            isPaused = false;
            isSetting = false;
            isBrightness = false;
            isAudio = false;
            isControl = false;

            pauseMenu.SetActive(false);
            settingsMenu.SetActive(false);
            brightnessMenu.SetActive(false);
            audioMenu.SetActive(false);
            pauseButton.SetActive(true); // Show pause button
            ControlMenu.SetActive(false);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Time.timeScale = 1f;
            // PickUpManager.Instance.EnableSelection();
            // PickUpManager.Instance.GetComponent<PickUpManager>().enabled = true;
        }

        public void MainMenu()
        {
            Time.timeScale = 1f; 
            SceneManager.LoadScene("MainMenu");
        }

        public void OpenDisplaySettings()
        {
            isSetting = true;
            isBrightness = false;
            isAudio = false;
            isControl = false;

            settingsMenu.SetActive(true);
            brightnessMenu.SetActive(false);
            audioMenu.SetActive(false);
            ControlMenu.SetActive(false);

        }

        public void OpenBrightness()
       {
            isPaused = false;
            isSetting = false;
            isBrightness = true;
            isAudio = false;
            isControl = false;

            settingsMenu.SetActive(false);
            brightnessMenu.SetActive(true);
            audioMenu.SetActive(false);
            ControlMenu.SetActive(false);
        }

        public void OpenAudio()
        {
            isSetting = false;
            isBrightness = false;
            isAudio = true;
            isControl = false;

            settingsMenu.SetActive(false);
            brightnessMenu.SetActive(false);
            audioMenu.SetActive(true);
            ControlMenu.SetActive(false);
        }
        public void OpenControl()
        {
            isSetting = false;
            isBrightness = false;
            isAudio = false;
            isControl = true;

            settingsMenu.SetActive(false);
            brightnessMenu.SetActive(false);
            audioMenu.SetActive(false);
            ControlMenu.SetActive(true);
        }

        public void CloseBrightness()
        {
            isBrightness = false;
            brightnessMenu.SetActive(false);
        }
    }
}
