using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace DS
{
    [System.Serializable]
    public class ExtractionRequirement
    {
        [Header("Extraction Object Info")]
        public string objectName = "Pasak";
        public InteractionObject interactionObject;
        public bool isExtracted = false;
        
        [Header("Visual Feedback")]
        public GameObject extractedIndicator; // Visual indicator saat sudah di-extract
        public ParticleSystem extractedParticle; // Particle effect saat object di-extract
    }

    public class ExtractionManager : MonoBehaviour
    {
        [Header("=== EXTRACTION REQUIREMENTS ===")]
        [SerializeField] private List<ExtractionRequirement> extractionRequirements = new List<ExtractionRequirement>();
        [SerializeField] private int totalRequiredExtractions = 5;
        
        [Header("=== TARGET AI ===")]
        [SerializeField] private TakauAI targetAI; // AI yang akan mati
        [SerializeField] private bool autoFindKuntiAI = true; // Otomatis cari KuntiAI di scene
        
        [Header("=== COMPLETION EFFECTS ===")]
        [SerializeField] private ParticleSystem completionParticleEffect;
        [SerializeField] private AudioSource completionAudioSource;
        [SerializeField] private float completionDelay = 1f; // Delay sebelum AI mati
        
        [Header("=== PROGRESS TRACKING ===")]
        [SerializeField] private bool showDebugLogs = true;
        [SerializeField] private bool showProgressUI = true;
        
        // Runtime tracking
        private int currentExtractedCount = 0;
        private bool isCompleted = false;
        private bool isProcessingCompletion = false;
        
        // Events
        public System.Action<int, int> OnProgressChanged; // (current, total)
        public System.Action OnExtractionCompleted;
        public System.Action<string> OnObjectExtracted; // object name
        
        // Singleton pattern (opsional)
        public static ExtractionManager Instance { get; private set; }
        
        private void Awake()
        {
            // Singleton setup
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogWarning("Multiple ExtractionManager found! Destroying duplicate.");
                Destroy(gameObject);
                return;
            }
            
            // Auto setup total requirements
            if (extractionRequirements.Count > 0)
            {
                totalRequiredExtractions = extractionRequirements.Count;
            }
        }
        
        private void Start()
        {
            InitializeExtractionSystem();
        }
        
        private void InitializeExtractionSystem()
        {
            // Auto find KuntiAI if not assigned
            if (autoFindKuntiAI && targetAI == null)
            {
                targetAI = FindFirstObjectByType<TakauAI>();
                if (targetAI != null)
                {
                    if (showDebugLogs) Debug.Log($"ExtractionManager: Auto-found KuntiAI - {targetAI.name}");
                }
                else
                {
                    Debug.LogError("ExtractionManager: KuntiAI not found in scene!");
                }
            }
            
            // Setup extraction requirements
            SetupExtractionRequirements();
            
            // Initial progress update
            UpdateProgress();
            
            if (showDebugLogs) 
            {
                Debug.Log($"ExtractionManager: Initialized with {totalRequiredExtractions} required extractions");
                Debug.Log($"ExtractionManager: Found {extractionRequirements.Count} extraction objects");
            }
        }
        
        private void SetupExtractionRequirements()
        {
            for (int i = 0; i < extractionRequirements.Count; i++)
            {
                var requirement = extractionRequirements[i];
                
                if (requirement.interactionObject != null)
                {
                    // Pastikan object ini adalah extractable type
                    if (requirement.interactionObject.interactionType != InteractionType.ExtractableObject)
                    {
                        Debug.LogWarning($"ExtractionManager: {requirement.objectName} is not ExtractableObject type!");
                    }
                    
                    // Hide extracted indicator initially
                    if (requirement.extractedIndicator != null)
                    {
                        requirement.extractedIndicator.SetActive(false);
                    }
                }
                else
                {
                    Debug.LogError($"ExtractionManager: InteractionObject not assigned for {requirement.objectName}!");
                }
            }
        }
        
        // Method ini dipanggil oleh InteractionObject saat extraction selesai
        public void RegisterExtraction(InteractionObject extractedObject)
        {
            if (isCompleted || isProcessingCompletion) return;
            
            // Cari requirement yang sesuai
            ExtractionRequirement foundRequirement = null;
            for (int i = 0; i < extractionRequirements.Count; i++)
            {
                if (extractionRequirements[i].interactionObject == extractedObject)
                {
                    foundRequirement = extractionRequirements[i];
                    break;
                }
            }
            
            if (foundRequirement != null && !foundRequirement.isExtracted)
            {
                // Mark as extracted
                foundRequirement.isExtracted = true;
                currentExtractedCount++;
                
                // Visual feedback
                HandleExtractionVisualFeedback(foundRequirement);
                
                // Update progress
                UpdateProgress();
                
                // Trigger events
                OnObjectExtracted?.Invoke(foundRequirement.objectName);
                
                if (showDebugLogs)
                {
                    Debug.Log($"ExtractionManager: {foundRequirement.objectName} extracted! Progress: {currentExtractedCount}/{totalRequiredExtractions}");
                }
                
                // Check completion
                CheckCompletion();
            }
            else
            {
                Debug.LogWarning($"ExtractionManager: Object {extractedObject.name} not found in requirements or already extracted!");
            }
        }
        
        private void HandleExtractionVisualFeedback(ExtractionRequirement requirement)
        {
            // Show extracted indicator
            if (requirement.extractedIndicator != null)
            {
                requirement.extractedIndicator.SetActive(true);
            }
            
            // Play extracted particle effect
            if (requirement.extractedParticle != null)
            {
                var main = requirement.extractedParticle.main;
                main.loop = false;
                requirement.extractedParticle.Play();
            }
        }
        
        private void UpdateProgress()
        {
            OnProgressChanged?.Invoke(currentExtractedCount, totalRequiredExtractions);
            
            // Debug progress
            if (showDebugLogs)
            {
                Debug.Log($"ExtractionManager: Progress Updated - {currentExtractedCount}/{totalRequiredExtractions}");
            }
        }
        
        private void CheckCompletion()
        {
            if (currentExtractedCount >= totalRequiredExtractions && !isCompleted)
            {
                StartCoroutine(HandleCompletion());
            }
        }
        
        private IEnumerator HandleCompletion()
        {
            isProcessingCompletion = true;
            
            if (showDebugLogs)
            {
                Debug.Log("ExtractionManager: All extractions completed! Killing KuntiAI...");
            }
            
            // Play completion effects
            if (completionParticleEffect != null)
            {
                completionParticleEffect.Play();
            }
            
            if (completionAudioSource != null)
            {
                completionAudioSource.Play();
            }
            
            // Trigger completion event
            OnExtractionCompleted?.Invoke();
            
            // Wait for completion delay
            yield return new WaitForSeconds(completionDelay);
            
            // Kill the AI
            if (targetAI != null)
            {
                targetAI.Dying();
                
                if (showDebugLogs)
                {
                    Debug.Log("ExtractionManager: KuntiAI has been killed!");
                }
            }
            else
            {
                Debug.LogError("ExtractionManager: Cannot kill KuntiAI - not assigned!");
            }
            
            isCompleted = true;
            isProcessingCompletion = false;
        }
        
        // Public methods untuk UI atau debugging
        public int GetCurrentProgress() => currentExtractedCount;
        public int GetTotalRequired() => totalRequiredExtractions;
        public float GetProgressPercentage() => (float)currentExtractedCount / totalRequiredExtractions;
        public bool IsCompleted() => isCompleted;
        
        public List<string> GetExtractedObjectNames()
        {
            List<string> extractedNames = new List<string>();
            foreach (var requirement in extractionRequirements)
            {
                if (requirement.isExtracted)
                {
                    extractedNames.Add(requirement.objectName);
                }
            }
            return extractedNames;
        }
        
        public List<string> GetRemainingObjectNames()
        {
            List<string> remainingNames = new List<string>();
            foreach (var requirement in extractionRequirements)
            {
                if (!requirement.isExtracted)
                {
                    remainingNames.Add(requirement.objectName);
                }
            }
            return remainingNames;
        }
        
        // Debug method
        [ContextMenu("Debug Force Complete")]
        public void DebugForceComplete()
        {
            if (Application.isPlaying)
            {
                currentExtractedCount = totalRequiredExtractions;
                CheckCompletion();
            }
        }
        
        [ContextMenu("Debug Reset Progress")]
        public void DebugResetProgress()
        {
            if (Application.isPlaying)
            {
                currentExtractedCount = 0;
                isCompleted = false;
                isProcessingCompletion = false;
                
                foreach (var requirement in extractionRequirements)
                {
                    requirement.isExtracted = false;
                    if (requirement.extractedIndicator != null)
                    {
                        requirement.extractedIndicator.SetActive(false);
                    }
                }
                
                UpdateProgress();
            }
        }
        
        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}