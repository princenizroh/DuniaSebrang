using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace DS
{
    public class ExtractionUIManager : MonoBehaviour
    {
        [Header("=== UI COMPONENTS ===")]
        [SerializeField] private GameObject progressPanel;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private TextMeshProUGUI objectiveText;
        [SerializeField] private Slider progressBar;
        [SerializeField] private Image progressFillImage;
        
        [Header("=== PROGRESS INDICATORS ===")]
        [SerializeField] private Transform progressIndicatorsParent;
        [SerializeField] private GameObject progressIndicatorPrefab; // Prefab untuk setiap pasak indicator
        [SerializeField] private Color completedColor = Color.green;
        [SerializeField] private Color incompleteColor = Color.gray;
        
        [Header("=== COMPLETION EFFECTS ===")]
        [SerializeField] private GameObject completionPanel;
        [SerializeField] private TextMeshProUGUI completionText;
        [SerializeField] private float completionDisplayTime = 3f;
        
        [Header("=== SETTINGS ===")]
        [SerializeField] private bool showProgressPanel = true;
        [SerializeField] private bool autoHideWhenComplete = true;
        [SerializeField] private float autoHideDelay = 2f;
        
        private ExtractionManager extractionManager;
        private Image[] progressIndicators;
        
        private void Start()
        {
            InitializeUI();
            SetupExtractionManager();
        }
        
        private void InitializeUI()
        {
            // Hide completion panel initially
            if (completionPanel != null)
            {
                completionPanel.SetActive(false);
            }
            
            // Set initial objective text
            if (objectiveText != null)
            {
                objectiveText.text = "Find and extract all 5 ritual stakes to banish the entity";
            }
            
            // Show or hide progress panel
            if (progressPanel != null)
            {
                progressPanel.SetActive(showProgressPanel);
            }
        }
        
        private void SetupExtractionManager()
        {
            // Find ExtractionManager
            extractionManager = ExtractionManager.Instance;
            if (extractionManager == null)
            {
                extractionManager = FindFirstObjectByType<ExtractionManager>();
            }
            
            if (extractionManager != null)
            {
                // Subscribe to events
                extractionManager.OnProgressChanged += UpdateProgressUI;
                extractionManager.OnExtractionCompleted += HandleExtractionComplete;
                extractionManager.OnObjectExtracted += HandleObjectExtracted;
                
                // Setup progress indicators
                SetupProgressIndicators();
                
                // Initial UI update
                UpdateProgressUI(extractionManager.GetCurrentProgress(), extractionManager.GetTotalRequired());
                
                Debug.Log("ExtractionUIManager: Successfully connected to ExtractionManager");
            }
            else
            {
                Debug.LogError("ExtractionUIManager: ExtractionManager not found!");
            }
        }
        
        private void SetupProgressIndicators()
        {
            if (progressIndicatorsParent == null || progressIndicatorPrefab == null) return;
            
            int totalRequired = extractionManager.GetTotalRequired();
            progressIndicators = new Image[totalRequired];
            
            // Clear existing indicators
            foreach (Transform child in progressIndicatorsParent)
            {
                Destroy(child.gameObject);
            }
            
            // Create new indicators
            for (int i = 0; i < totalRequired; i++)
            {
                GameObject indicator = Instantiate(progressIndicatorPrefab, progressIndicatorsParent);
                Image indicatorImage = indicator.GetComponent<Image>();
                
                if (indicatorImage != null)
                {
                    progressIndicators[i] = indicatorImage;
                    indicatorImage.color = incompleteColor;
                }
            }
        }
        
        private void UpdateProgressUI(int current, int total)
        {
            // Update progress text
            if (progressText != null)
            {
                progressText.text = $"Stakes Extracted: {current}/{total}";
            }
            
            // Update progress bar
            if (progressBar != null)
            {
                float progress = (float)current / total;
                progressBar.value = progress;
                
                // Update fill color
                if (progressFillImage != null)
                {
                    progressFillImage.color = Color.Lerp(Color.red, Color.green, progress);
                }
            }
            
            // Update individual indicators
            UpdateProgressIndicators(current);
        }
        
        private void UpdateProgressIndicators(int completedCount)
        {
            if (progressIndicators == null) return;
            
            for (int i = 0; i < progressIndicators.Length; i++)
            {
                if (progressIndicators[i] != null)
                {
                    progressIndicators[i].color = i < completedCount ? completedColor : incompleteColor;
                }
            }
        }
        
        private void HandleObjectExtracted(string objectName)
        {
            // Optional: Show notification for individual extraction
            Debug.Log($"ExtractionUIManager: {objectName} extracted!");
            
            // You can add notification popup here
            StartCoroutine(ShowExtractionNotification(objectName));
        }
        
        private IEnumerator ShowExtractionNotification(string objectName)
        {
            // Simple notification example
            if (objectiveText != null)
            {
                string originalText = objectiveText.text;
                objectiveText.text = $"✓ {objectName} extracted!";
                objectiveText.color = completedColor;
                
                yield return new WaitForSeconds(1.5f);
                
                objectiveText.text = originalText;
                objectiveText.color = Color.white;
            }
        }
        
        private void HandleExtractionComplete()
        {
            StartCoroutine(ShowCompletionSequence());
        }
        
        private IEnumerator ShowCompletionSequence()
        {
            // Show completion panel
            if (completionPanel != null)
            {
                completionPanel.SetActive(true);
                
                if (completionText != null)
                {
                    completionText.text = "ALL STAKES EXTRACTED!\nThe entity has been banished!";
                }
            }
            
            // Update objective text
            if (objectiveText != null)
            {
                objectiveText.text = "✓ RITUAL COMPLETE - Entity Banished!";
                objectiveText.color = completedColor;
            }
            
            // Wait for completion display time
            yield return new WaitForSeconds(completionDisplayTime);
            
            // Hide completion panel
            if (completionPanel != null)
            {
                completionPanel.SetActive(false);
            }
            
            // Auto hide progress panel if enabled
            if (autoHideWhenComplete)
            {
                yield return new WaitForSeconds(autoHideDelay);
                
                if (progressPanel != null)
                {
                    progressPanel.SetActive(false);
                }
            }
        }
        
        // Public methods for external control
        public void ShowProgressPanel()
        {
            if (progressPanel != null)
            {
                progressPanel.SetActive(true);
            }
        }
        
        public void HideProgressPanel()
        {
            if (progressPanel != null)
            {
                progressPanel.SetActive(false);
            }
        }
        
        public void ToggleProgressPanel()
        {
            if (progressPanel != null)
            {
                progressPanel.SetActive(!progressPanel.activeSelf);
            }
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            if (extractionManager != null)
            {
                extractionManager.OnProgressChanged -= UpdateProgressUI;
                extractionManager.OnExtractionCompleted -= HandleExtractionComplete;
                extractionManager.OnObjectExtracted -= HandleObjectExtracted;
            }
        }
    }
}