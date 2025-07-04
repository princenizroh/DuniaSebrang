using UnityEngine;
using UnityEngine.SceneManagement;

namespace DS
{
    /// <summary>
    /// Helper component to manage SaveManager initialization and state
    /// Add this to SaveManager GameObject for better control
    /// </summary>
    [RequireComponent(typeof(SaveManager))]
    public class SaveManagerController : MonoBehaviour
    {
        [Header("=== SCENE CONTROL ===")]
        [Tooltip("Scenes where SaveManager should be inactive (main menu, etc.)")]
        [SerializeField] private string[] inactiveInScenes = { "MainMenu", "Menu", "StartMenu" };
        
        [Tooltip("Auto-disable SaveManager in menu scenes")]
        [SerializeField] private bool autoDisableInMenus = true;
        
        [Header("=== DEBUG ===")]
        [SerializeField] private bool showDebug = true;
        
        private SaveManager saveManager;
        private bool wasDisabledByController = false;
        
        private void Awake()
        {
            saveManager = GetComponent<SaveManager>();
            
            if (autoDisableInMenus)
            {
                CheckAndControlSaveManager();
            }
        }
        
        private void Start()
        {
            // Register for scene change events
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        
        private void OnDestroy()
        {
            // Unregister from scene change events
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (autoDisableInMenus)
            {
                CheckAndControlSaveManager();
            }
        }
        
        /// <summary>
        /// Check current scene and enable/disable SaveManager accordingly
        /// </summary>
        private void CheckAndControlSaveManager()
        {
            if (saveManager == null) return;
            
            string currentScene = SceneManager.GetActiveScene().name;
            bool shouldBeInactive = ShouldBeInactiveInCurrentScene(currentScene);
            
            if (shouldBeInactive && saveManager.enabled)
            {
                if (showDebug) Debug.Log($"★ SaveManagerController: Disabling SaveManager in scene '{currentScene}'");
                saveManager.enabled = false;
                wasDisabledByController = true;
            }
            else if (!shouldBeInactive && !saveManager.enabled && wasDisabledByController)
            {
                if (showDebug) Debug.Log($"★ SaveManagerController: Re-enabling SaveManager in scene '{currentScene}'");
                saveManager.enabled = true;
                wasDisabledByController = false;
            }
        }
        
        /// <summary>
        /// Check if SaveManager should be inactive in current scene
        /// </summary>
        private bool ShouldBeInactiveInCurrentScene(string sceneName)
        {
            foreach (string inactiveScene in inactiveInScenes)
            {
                if (sceneName.Contains(inactiveScene))
                {
                    return true;
                }
            }
            return false;
        }
        
        /// <summary>
        /// Manually enable SaveManager (for game scenes)
        /// </summary>
        public void EnableSaveManager()
        {
            if (saveManager != null && !saveManager.enabled)
            {
                if (showDebug) Debug.Log("★ SaveManagerController: Manually enabling SaveManager");
                saveManager.enabled = true;
                wasDisabledByController = false;
            }
        }
        
        /// <summary>
        /// Manually disable SaveManager (for menu scenes)
        /// </summary>
        public void DisableSaveManager()
        {
            if (saveManager != null && saveManager.enabled)
            {
                if (showDebug) Debug.Log("★ SaveManagerController: Manually disabling SaveManager");
                saveManager.enabled = false;
                wasDisabledByController = true;
            }
        }
        
        /// <summary>
        /// Force SaveManager to load data explicitly (call when entering game)
        /// </summary>
        public bool ForceLoadSaveData()
        {
            if (saveManager == null)
            {
                Debug.LogError("SaveManagerController: No SaveManager found!");
                return false;
            }
            
            if (!saveManager.enabled)
            {
                if (showDebug) Debug.Log("★ SaveManagerController: Enabling SaveManager for explicit load");
                EnableSaveManager();
            }
            
            // Try to call ExplicitLoadGame method
            var explicitLoadMethod = saveManager.GetType().GetMethod("ExplicitLoadGame");
            if (explicitLoadMethod != null)
            {
                if (showDebug) Debug.Log("★ SaveManagerController: Calling ExplicitLoadGame");
                return (bool)explicitLoadMethod.Invoke(saveManager, null);
            }
            else
            {
                Debug.LogWarning("SaveManagerController: ExplicitLoadGame method not found");
                return false;
            }
        }
    }
}
