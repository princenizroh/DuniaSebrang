using UnityEngine;
using UnityEngine.SceneManagement;

namespace DS
{
    /// <summary>
    /// Debug GUI untuk testing Game Ending Monitor
    /// Memungkinkan testing semua fitur tanpa harus main game dari awal
    /// </summary>
    public class DebugGUIEndingMonitor : MonoBehaviour
    {
        [Header("=== REFERENCES ===")]
        [SerializeField] private GameEndingMonitor endingMonitor;
        [SerializeField] private TakauAI takauAI;
        [SerializeField] private GameObject suratGameObject;
        [SerializeField] private GameObject sleepingCharacter;
        [SerializeField] private DeathScreenEffect deathScreenEffect;
        [SerializeField] private InteractionObject suratInteractionObject;
        
        [Header("=== DEBUG SETTINGS ===")]
        [SerializeField] private bool showDebugGUI = true;
        [SerializeField] private bool enableKeyboardShortcuts = true;
        [SerializeField] private KeyCode toggleGUIKey = KeyCode.F1;
        [SerializeField] private KeyCode killTakauKey = KeyCode.F2;
        [SerializeField] private KeyCode forceEndingKey = KeyCode.F3;
        [SerializeField] private KeyCode resetKey = KeyCode.F4;
        
        [Header("=== GUI STYLE ===")]
        [SerializeField] private int guiWidth = 350;
        [SerializeField] private int guiHeight = 600;
        [SerializeField] private int fontSize = 12;
        
        // GUI variables
        private bool showGUI = true;
        private Vector2 scrollPosition = Vector2.zero;
        private GUIStyle headerStyle;
        private GUIStyle buttonStyle;
        private GUIStyle labelStyle;
        private GUIStyle boxStyle;
        private bool stylesInitialized = false;
        
        // Status tracking
        private string currentStatus = "Monitoring...";
        private float lastUpdateTime = 0f;
        private int frameCount = 0;
        
        private void Start()
        {
            // Auto-find components jika tidak di-assign
            if (endingMonitor == null)
                endingMonitor = FindObjectOfType<GameEndingMonitor>();
            
            if (takauAI == null)
                takauAI = FindObjectOfType<TakauAI>();
            
            if (deathScreenEffect == null)
                deathScreenEffect = FindObjectOfType<DeathScreenEffect>();
            
            if (suratGameObject != null && suratInteractionObject == null)
                suratInteractionObject = suratGameObject.GetComponent<InteractionObject>();
            
            // Subscribe to events
            if (endingMonitor != null)
            {
                endingMonitor.OnTakauActivated += () => UpdateStatus("Takau Diaktifkan!");
                endingMonitor.OnTakauDied += () => UpdateStatus("Takau Mati! Surat Diaktifkan!");
                endingMonitor.OnSuratActivated += () => UpdateStatus("Surat Siap untuk Interaksi!");
                endingMonitor.OnSuratInteractionCompleted += () => UpdateStatus("Surat Dibaca! Mulai Ending...");
                endingMonitor.OnEndingSequenceStarted += () => UpdateStatus("Ending Sequence Dimulai!");
                endingMonitor.OnEndingSequenceCompleted += () => UpdateStatus("Ending Selesai!");
            }
            
            Debug.Log("DebugGUI: Initialized - Tekan F1 untuk toggle GUI");
        }
        
        private void Update()
        {
            // Handle keyboard shortcuts
            if (enableKeyboardShortcuts)
            {
                if (Input.GetKeyDown(toggleGUIKey))
                {
                    showGUI = !showGUI;
                    Debug.Log($"DebugGUI: GUI {(showGUI ? "Ditampilkan" : "Disembunyikan")}");
                }
                
                if (Input.GetKeyDown(killTakauKey))
                {
                    ForceKillTakau();
                }
                
                if (Input.GetKeyDown(forceEndingKey))
                {
                    ForceEndingSequence();
                }
                
                if (Input.GetKeyDown(resetKey))
                {
                    ResetAllSystems();
                }
            }
            
            // Update frame counter
            frameCount++;
            if (Time.time - lastUpdateTime > 1f)
            {
                lastUpdateTime = Time.time;
                frameCount = 0;
            }
        }
        
        private void OnGUI()
        {
            if (!showDebugGUI || !showGUI) return;
            
            // Initialize styles
            if (!stylesInitialized)
            {
                InitializeGUIStyles();
                stylesInitialized = true;
            }
            
            // Main debug window
            GUILayout.BeginArea(new Rect(10, 10, guiWidth, guiHeight), boxStyle);
            
            // Header
            GUILayout.Label("🎮 DEBUG GAME ENDING MONITOR", headerStyle);
            GUILayout.Space(10);
            
            // Status
            GUILayout.Label($"Status: {currentStatus}", labelStyle);
            GUILayout.Label($"Scene: {SceneManager.GetActiveScene().name}", labelStyle);
            GUILayout.Label($"Time: {Time.time:F1}s", labelStyle);
            GUILayout.Space(10);
            
            // Scroll view untuk konten
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(450));
            
            // === SYSTEM STATUS ===
            DrawSystemStatus();
            GUILayout.Space(10);
            
            // === MANUAL CONTROLS ===
            DrawManualControls();
            GUILayout.Space(10);
            
            // === TESTING BUTTONS ===
            DrawTestingButtons();
            GUILayout.Space(10);
            
            // === INFORMATION ===
            DrawInformation();
            
            GUILayout.EndScrollView();
            
            // Footer
            GUILayout.Space(5);
            GUILayout.Label("Tekan F1 untuk toggle GUI", labelStyle);
            
            GUILayout.EndArea();
        }
        
        private void DrawSystemStatus()
        {
            GUILayout.Label("📊 STATUS SISTEM", headerStyle);
            
            if (endingMonitor != null)
            {
                // Menggunakan reflection untuk membaca private fields
                var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
                var type = endingMonitor.GetType();
                
                bool takauActive = (bool)type.GetField("takauIsActive", flags)?.GetValue(endingMonitor);
                bool takauDead = (bool)type.GetField("takauIsDead", flags)?.GetValue(endingMonitor);
                bool suratActive = (bool)type.GetField("suratIsActive", flags)?.GetValue(endingMonitor);
                bool sleepingDeactivated = (bool)type.GetField("sleepingCharacterDeactivated", flags)?.GetValue(endingMonitor);
                bool suratCompleted = (bool)type.GetField("suratInteractionCompleted", flags)?.GetValue(endingMonitor);
                bool endingStarted = (bool)type.GetField("endingSequenceStarted", flags)?.GetValue(endingMonitor);
                
                DrawStatusLine("Takau Aktif", takauActive);
                DrawStatusLine("Takau Mati", takauDead);
                DrawStatusLine("Surat Aktif", suratActive);
                DrawStatusLine("Karakter Tidur Nonaktif", sleepingDeactivated);
                DrawStatusLine("Surat Interaction Selesai", suratCompleted);
                DrawStatusLine("Ending Sequence Dimulai", endingStarted);
            }
            else
            {
                GUILayout.Label("❌ GameEndingMonitor tidak ditemukan!", labelStyle);
            }
            
            // Takau AI Status
            if (takauAI != null)
            {
                GUILayout.Label($"Takau Mode: {takauAI.moveMode}", labelStyle);
                GUILayout.Label($"Takau GameObject: {(takauAI.gameObject.activeInHierarchy ? "Aktif" : "Tidak Aktif")}", labelStyle);
            }
            else
            {
                GUILayout.Label("❌ TakauAI tidak ditemukan!", labelStyle);
            }
        }
        
        private void DrawManualControls()
        {
            GUILayout.Label("🎛️ KONTROL MANUAL", headerStyle);
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Aktifkan Takau", buttonStyle))
            {
                ActivateTakau();
            }
            if (GUILayout.Button("Bunuh Takau", buttonStyle))
            {
                ForceKillTakau();
            }
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Aktifkan Surat", buttonStyle))
            {
                ActivateSurat();
            }
            if (GUILayout.Button("Nonaktifkan Tidur", buttonStyle))
            {
                DeactivateSleepingCharacter();
            }
            GUILayout.EndHorizontal();
            
            if (GUILayout.Button("Force Ending Sequence", buttonStyle))
            {
                ForceEndingSequence();
            }
            
            if (GUILayout.Button("Reset Semua", buttonStyle))
            {
                ResetAllSystems();
            }
        }
        
        private void DrawTestingButtons()
        {
            GUILayout.Label("🧪 TESTING SCENARIOS", headerStyle);
            
            if (GUILayout.Button("Test: Skenario Lengkap", buttonStyle))
            {
                StartCoroutine(TestFullScenario());
            }
            
            if (GUILayout.Button("Test: Langsung ke Ending", buttonStyle))
            {
                StartCoroutine(TestDirectEnding());
            }
            
            if (GUILayout.Button("Test: Fade Effect", buttonStyle))
            {
                TestFadeEffect();
            }
            
            if (GUILayout.Button("Test: Load Main Menu", buttonStyle))
            {
                TestLoadMainMenu();
            }
        }
        
        private void DrawInformation()
        {
            GUILayout.Label("ℹ️ INFORMASI", headerStyle);
            
            GUILayout.Label("Keyboard Shortcuts:", labelStyle);
            GUILayout.Label("F1 = Toggle GUI", labelStyle);
            GUILayout.Label("F2 = Kill Takau", labelStyle);
            GUILayout.Label("F3 = Force Ending", labelStyle);
            GUILayout.Label("F4 = Reset All", labelStyle);
            
            GUILayout.Space(5);
            GUILayout.Label("Skenario Game:", labelStyle);
            GUILayout.Label("1. Player injak trigger", labelStyle);
            GUILayout.Label("2. Takau & Pasak aktif", labelStyle);
            GUILayout.Label("3. Player ekstrak Pasak", labelStyle);
            GUILayout.Label("4. Takau mati", labelStyle);
            GUILayout.Label("5. Surat aktif, tidur nonaktif", labelStyle);
            GUILayout.Label("6. Player baca surat", labelStyle);
            GUILayout.Label("7. Ending sequence", labelStyle);
            GUILayout.Label("8. Fade to main menu", labelStyle);
        }
        
        private void DrawStatusLine(string label, bool status)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label + ":", labelStyle);
            GUILayout.FlexibleSpace();
            GUILayout.Label(status ? "✅" : "❌", labelStyle);
            GUILayout.EndHorizontal();
        }
        
        private void InitializeGUIStyles()
        {
            headerStyle = new GUIStyle(GUI.skin.label);
            headerStyle.fontSize = fontSize + 2;
            headerStyle.fontStyle = FontStyle.Bold;
            headerStyle.normal.textColor = Color.yellow;
            
            buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = fontSize;
            
            labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontSize = fontSize;
            labelStyle.normal.textColor = Color.white;
            
            boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.normal.background = MakeTexture(2, 2, new Color(0, 0, 0, 0.8f));
        }
        
        private Texture2D MakeTexture(int width, int height, Color color)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++)
                pix[i] = color;
            
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }
        
        private void UpdateStatus(string newStatus)
        {
            currentStatus = newStatus;
            Debug.Log($"DebugGUI: {newStatus}");
        }
        
        // === TESTING METHODS ===
        
        private void ActivateTakau()
        {
            if (takauAI != null)
            {
                takauAI.gameObject.SetActive(true);
                UpdateStatus("Takau diaktifkan secara manual");
            }
        }
        
        private void ForceKillTakau()
        {
            if (takauAI != null)
            {
                takauAI.Dying();
                UpdateStatus("Takau dipaksa mati");
            }
            else
            {
                UpdateStatus("Takau tidak ditemukan!");
            }
        }
        
        private void ActivateSurat()
        {
            if (suratGameObject != null)
            {
                suratGameObject.SetActive(true);
                UpdateStatus("Surat diaktifkan secara manual");
            }
        }
        
        private void DeactivateSleepingCharacter()
        {
            if (sleepingCharacter != null)
            {
                sleepingCharacter.SetActive(false);
                UpdateStatus("Karakter tidur dinonaktifkan");
            }
        }
        
        private void ForceEndingSequence()
        {
            if (endingMonitor != null)
            {
                endingMonitor.ForceEndingSequence();
                UpdateStatus("Ending sequence dipaksa dimulai");
            }
        }
        
        private void ResetAllSystems()
        {
            if (endingMonitor != null)
            {
                endingMonitor.ResetMonitor();
            }
            
            if (takauAI != null)
            {
                takauAI.gameObject.SetActive(false);
            }
            
            if (suratGameObject != null)
            {
                suratGameObject.SetActive(false);
            }
            
            if (sleepingCharacter != null)
            {
                sleepingCharacter.SetActive(true);
            }
            
            UpdateStatus("Semua sistem di-reset");
        }
        
        private System.Collections.IEnumerator TestFullScenario()
        {
            UpdateStatus("Memulai test skenario lengkap...");
            
            // Step 1: Activate Takau
            ActivateTakau();
            yield return new WaitForSeconds(2f);
            
            // Step 2: Kill Takau
            ForceKillTakau();
            yield return new WaitForSeconds(2f);
            
            // Step 3: Simulate Surat interaction completion
            if (endingMonitor != null)
            {
                // Simulate surat interaction
                var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
                var type = endingMonitor.GetType();
                var method = type.GetMethod("OnSuratInteractionComplete", flags);
                method?.Invoke(endingMonitor, null);
            }
            
            UpdateStatus("Test skenario lengkap selesai!");
        }
        
        private System.Collections.IEnumerator TestDirectEnding()
        {
            UpdateStatus("Test langsung ke ending...");
            
            // Set all prerequisites
            ActivateTakau();
            yield return new WaitForSeconds(0.5f);
            
            ForceKillTakau();
            yield return new WaitForSeconds(0.5f);
            
            ForceEndingSequence();
            
            UpdateStatus("Test direct ending dimulai!");
        }
        
        private void TestFadeEffect()
        {
            if (deathScreenEffect != null)
            {
                // Trigger fade effect
                UpdateStatus("Test fade effect...");
                // You might need to call the appropriate method
            }
            else
            {
                UpdateStatus("DeathScreenEffect tidak ditemukan!");
            }
        }
        
        private void TestLoadMainMenu()
        {
            UpdateStatus("Test load main menu...");
            
            // Show confirmation dialog
            if (Application.isEditor)
            {
                Debug.Log("Dalam editor - tidak akan load scene sebenarnya");
            }
            else
            {
                SceneManager.LoadScene("MainMenu");
            }
        }
    }
}