using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

namespace DS.UI
{
    /// <summary>
    /// Loading screen for smooth scene transitions in the save system
    /// </summary>
    public class LoadingScreen : MonoBehaviour
    {
        [Header("=== UI REFERENCES ===")]
        [Tooltip("Loading screen panel")]
        [SerializeField] private GameObject loadingPanel;
        
        [Tooltip("Loading progress bar")]
        [SerializeField] private Slider progressBar;
        
        [Tooltip("Loading text")]
        [SerializeField] private TextMeshProUGUI loadingText;
        
        [Tooltip("Tip text (optional)")]
        [SerializeField] private TextMeshProUGUI tipText;
        
        [Tooltip("Background image")]
        [SerializeField] private Image backgroundImage;
        
        [Header("=== LOADING SETTINGS ===")]
        [Tooltip("Minimum loading time (for smooth UX)")]
        [SerializeField] private float minimumLoadingTime = 2f;
        
        [Tooltip("Fade duration")]
        [SerializeField] private float fadeDuration = 0.5f;
        
        [Tooltip("Loading text variations")]
        [SerializeField] private string[] loadingTexts = {
            "Loading...",
            "Preparing game world...",
            "Loading player data...",
            "Initializing systems..."
        };
        
        [Tooltip("Loading tips")]
        [SerializeField] private string[] loadingTips = {
            "Listen carefully for audio cues",
            "Collect items to unlock new areas",
            "Save at checkpoints to preserve progress",
            "Explore every corner for hidden secrets"
        };
        
        [Header("=== DEBUG ===")]
        [Tooltip("Show debug messages")]
        [SerializeField] private bool showDebug = true;
        
        // Static instance for easy access
        public static LoadingScreen Instance { get; private set; }
        
        // Current loading operation
        private AsyncOperation currentLoadOperation;
        private Coroutine loadingCoroutine;
        
        private void Awake()
        {
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
            
            // Start hidden
            Hide();
        }
        
        /// <summary>
        /// Show loading screen and load scene
        /// </summary>
        public void LoadScene(string sceneName, string customLoadingText = null, string customTip = null)
        {
            if (loadingCoroutine != null)
            {
                if (showDebug) Debug.LogWarning("LoadingScreen: Already loading a scene!");
                return;
            }
            
            if (showDebug) Debug.Log($"★★★ LOADING SCENE: {sceneName} ★★★");
            
            loadingCoroutine = StartCoroutine(LoadSceneCoroutine(sceneName, customLoadingText, customTip));
        }
        
        /// <summary>
        /// Load scene coroutine with progress tracking
        /// </summary>
        private IEnumerator LoadSceneCoroutine(string sceneName, string customLoadingText, string customTip)
        {
            // Show loading screen
            Show();
            
            // Set loading text and tip
            SetLoadingText(customLoadingText);
            SetTip(customTip);
            
            // Reset progress
            if (progressBar != null)
                progressBar.value = 0f;
            
            // Start async load operation
            currentLoadOperation = SceneManager.LoadSceneAsync(sceneName);
            currentLoadOperation.allowSceneActivation = false;
            
            float startTime = Time.unscaledTime;
            float progress = 0f;
            
            // Wait for loading to complete and minimum time
            while (!currentLoadOperation.isDone || (Time.unscaledTime - startTime) < minimumLoadingTime)
            {
                // Calculate progress (0.9f is when Unity finishes loading, 0.1f for activation)
                float loadProgress = Mathf.Clamp01(currentLoadOperation.progress / 0.9f);
                float timeProgress = Mathf.Clamp01((Time.unscaledTime - startTime) / minimumLoadingTime);
                
                // Use the slower of the two for smooth experience
                progress = Mathf.Min(loadProgress, timeProgress);
                
                // Update progress bar
                if (progressBar != null)
                    progressBar.value = progress;
                
                // Update loading text randomly
                if (Time.unscaledTime % 1f < 0.1f) // Every ~1 second
                {
                    SetRandomLoadingText();
                }
                
                yield return null;
            }
            
            // Ensure full progress
            if (progressBar != null)
                progressBar.value = 1f;
            
            // Final loading text
            if (loadingText != null)
                loadingText.text = "Ready!";
            
            // Small delay for UX
            yield return new WaitForSecondsRealtime(0.5f);
            
            // Activate the scene
            currentLoadOperation.allowSceneActivation = true;
            
            // Wait for scene to actually load
            while (!currentLoadOperation.isDone)
            {
                yield return null;
            }
            
            // Hide loading screen
            Hide();
            
            // Clear loading operation
            currentLoadOperation = null;
            loadingCoroutine = null;
            
            if (showDebug) Debug.Log($"★★★ SCENE LOADED: {sceneName} ★★★");
        }
        
        /// <summary>
        /// Show loading screen
        /// </summary>
        public void Show()
        {
            if (loadingPanel != null)
                loadingPanel.SetActive(true);
                
            if (showDebug) Debug.Log("Loading screen shown");
        }
        
        /// <summary>
        /// Hide loading screen
        /// </summary>
        public void Hide()
        {
            if (loadingPanel != null)
                loadingPanel.SetActive(false);
                
            if (showDebug) Debug.Log("Loading screen hidden");
        }
        
        /// <summary>
        /// Set loading text
        /// </summary>
        private void SetLoadingText(string customText = null)
        {
            if (loadingText == null) return;
            
            if (!string.IsNullOrEmpty(customText))
            {
                loadingText.text = customText;
            }
            else if (loadingTexts.Length > 0)
            {
                loadingText.text = loadingTexts[0];
            }
        }
        
        /// <summary>
        /// Set random loading text
        /// </summary>
        private void SetRandomLoadingText()
        {
            if (loadingText == null || loadingTexts.Length == 0) return;
            
            string randomText = loadingTexts[Random.Range(0, loadingTexts.Length)];
            loadingText.text = randomText;
        }
        
        /// <summary>
        /// Set tip text
        /// </summary>
        private void SetTip(string customTip = null)
        {
            if (tipText == null) return;
            
            if (!string.IsNullOrEmpty(customTip))
            {
                tipText.text = customTip;
            }
            else if (loadingTips.Length > 0)
            {
                string randomTip = loadingTips[Random.Range(0, loadingTips.Length)];
                tipText.text = randomTip;
            }
        }
        
        /// <summary>
        /// Static method to load scene with loading screen
        /// </summary>
        public static void LoadSceneWithLoading(string sceneName, string loadingText = null, string tip = null)
        {
            if (Instance != null)
            {
                Instance.LoadScene(sceneName, loadingText, tip);
            }
            else
            {
                // Fallback: direct scene load
                Debug.LogWarning("LoadingScreen: No instance found! Loading scene directly.");
                SceneManager.LoadScene(sceneName);
            }
        }
        
        #if UNITY_EDITOR
        /// <summary>
        /// Test loading screen (Editor only)
        /// </summary>
        [ContextMenu("Test Loading Screen")]
        private void TestLoadingScreen()
        {
            LoadScene("TestScene", "Testing loading screen...", "This is a test tip!");
        }
        
        /// <summary>
        /// Test show/hide (Editor only)
        /// </summary>
        [ContextMenu("Test Show")]
        private void TestShow()
        {
            Show();
            SetLoadingText("Test loading text");
            SetTip("Test tip text");
        }
        
        /// <summary>
        /// Test hide (Editor only)
        /// </summary>
        [ContextMenu("Test Hide")]
        private void TestHide()
        {
            Hide();
        }
        #endif
    }
}
