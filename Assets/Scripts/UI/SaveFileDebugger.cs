using UnityEngine;
using System.IO;

namespace DS.UI
{
    /// <summary>
    /// Debug helper untuk troubleshoot save file paths
    /// </summary>
    public class SaveFileDebugger : MonoBehaviour
    {
        [Header("=== DEBUG CONTROLS ===")]
        [SerializeField] private KeyCode debugKey = KeyCode.F12;
        [SerializeField] private SaveManager saveManager;
        
        private void Update()
        {
            if (Input.GetKeyDown(debugKey))
            {
                DebugSaveFiles();
            }
        }
        
        [ContextMenu("Debug Save Files")]
        public void DebugSaveFiles()
        {
            Debug.Log("=== SAVE FILE DEBUGGER ===");
            
            // Check SaveManager
            if (saveManager == null)
                saveManager = FindObjectOfType<SaveManager>();
            
            bool saveManagerFound = saveManager != null;
            Debug.Log($"✓ SaveManager Found: {saveManagerFound}");
            
            // Check paths
            string persistentPath = Application.persistentDataPath;
            string savesPath = Path.Combine(persistentPath, "Saves");
            
            Debug.Log($"✓ Persistent Data Path: {persistentPath}");
            Debug.Log($"✓ Expected Saves Path: {savesPath}");
            
            if (saveManager != null)
            {
                string managerSaveDir = saveManager.SaveDirectory;
                Debug.Log($"✓ SaveManager Save Directory: {managerSaveDir}");
                Debug.Log($"✓ Paths Match: {savesPath == managerSaveDir}");
            }
            
            // Check directory existence
            bool saveDirExists = Directory.Exists(savesPath);
            Debug.Log($"✓ Save Directory Exists: {saveDirExists}");
            
            if (saveDirExists)
            {
                // List all files
                string[] allFiles = Directory.GetFiles(savesPath);
                string[] jsonFiles = Directory.GetFiles(savesPath, "*.json");
                
                Debug.Log($"✓ Total Files: {allFiles.Length}");
                Debug.Log($"✓ JSON Files: {jsonFiles.Length}");
                
                foreach (string file in jsonFiles)
                {
                    string fileName = Path.GetFileName(file);
                    long fileSize = new FileInfo(file).Length;
                    Debug.Log($"  - {fileName} ({fileSize} bytes)");
                    
                    // Try to read first few lines
                    try
                    {
                        string[] lines = File.ReadAllLines(file);
                        Debug.Log($"    First line: {(lines.Length > 0 ? lines[0] : "EMPTY")}");
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"    Error reading file: {e.Message}");
                    }
                }
            }
            else
            {
                Debug.LogWarning("Save directory does not exist!");
            }
            
            // Test SaveSlotManager compatibility
            SaveSlotManager slotManager = FindObjectOfType<SaveSlotManager>();
            if (slotManager != null)
            {
                Debug.Log("✓ SaveSlotManager Found - Testing slot info...");
                
                for (int i = 0; i < 3; i++)
                {
                    var slotInfo = slotManager.GetEnhancedSaveSlotInfo(i);
                    Debug.Log($"  Slot {i}: isEmpty={slotInfo.isEmpty}, area='{slotInfo.areaName}'");
                }
            }
            
            Debug.Log("=== END DEBUG ===");
        }
    }
}
