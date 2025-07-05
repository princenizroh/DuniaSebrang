using UnityEngine;

namespace DS
{
    public class Pause : MonoBehaviour
    {
        [Header("Main Panels")]
        [SerializeField] private GameObject pauseButton;
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject resumeButton;

        [Header("Sub Panels")]
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject controlPanel;

        private bool isPaused = false;

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (controlPanel.activeSelf)
                {
                    BackToSettings(); 
                }
                else if (settingsPanel.activeSelf)
                {
                    BackToPause();
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

        public void OnPauseButtonPressed()
        {
            if (!isPaused)
            {
                PauseGame();
            }
        }

        public void PauseGame()
        {
            isPaused = true;
            Time.timeScale = 0f;

            pausePanel.SetActive(true);
            resumeButton.SetActive(true);
            pauseButton.SetActive(false); // Sembunyikan tombol pause
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void ResumeGame()
        {
            isPaused = false;
            Time.timeScale = 1f;

            pausePanel.SetActive(false);
            settingsPanel.SetActive(false);
            controlPanel.SetActive(false);
            resumeButton.SetActive(true);
            pauseButton.SetActive(true); // Tampilkan tombol pause kembali
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void OpenSettings()
        {
            pausePanel.SetActive(false);
            settingsPanel.SetActive(true);
        }

        public void OpenControl()
        {
            settingsPanel.SetActive(false);
            controlPanel.SetActive(true);
        }

        public void BackToSettings()
        {
            controlPanel.SetActive(false);
            settingsPanel.SetActive(true);
        }

        public void BackToPause()
        {
            settingsPanel.SetActive(false);
            controlPanel.SetActive(false);
            pausePanel.SetActive(true);
        }
    }
}
