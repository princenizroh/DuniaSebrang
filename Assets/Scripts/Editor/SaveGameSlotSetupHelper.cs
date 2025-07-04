// using UnityEngine;
// using UnityEngine.UI;
// using TMPro;
// using DS.UI;

// namespace DS.Editor
// {
//     /// <summary>
//     /// Helper script untuk auto-setup existing SaveGame GameObjects
//     /// </summary>
//     public class SaveGameSlotSetupHelper : MonoBehaviour
//     {
//         [Header("=== AUTO SETUP ===")]
//         [Tooltip("Target SaveGame GameObjects to setup")]
//         [SerializeField] private GameObject[] saveGameObjects;
        
//         [Tooltip("Auto-create missing UI elements")]
//         [SerializeField] private bool autoCreateMissingElements = true;
        
//         [Header("=== UI SETTINGS ===")]
//         [SerializeField] private Vector2 slotSize = new Vector2(400, 80);
//         [SerializeField] private Color emptySlotColor = Color.gray;
//         [SerializeField] private Color filledSlotColor = Color.white;
//         [SerializeField] private Color selectedSlotColor = Color.yellow;
        
//         [ContextMenu("Auto Setup SaveGame Slots")]
//         public void AutoSetupSaveGameSlots()
//         {
//             if (saveGameObjects == null || saveGameObjects.Length == 0)
//             {
//                 Debug.LogError("No SaveGame objects assigned!");
//                 return;
//             }
            
//             foreach (GameObject slotObj in saveGameObjects)
//             {
//                 if (slotObj == null) continue;
                
//                 SetupSaveGameSlot(slotObj);
//             }
            
//             Debug.Log($"Auto-setup completed for {saveGameObjects.Length} SaveGame slots!");
//         }
        
//         private void SetupSaveGameSlot(GameObject slotObj)
//         {
//             Debug.Log($"Setting up: {slotObj.name}");
            
//             // Ensure RectTransform
//             RectTransform rectTransform = slotObj.GetComponent<RectTransform>();
//             if (rectTransform == null)
//             {
//                 rectTransform = slotObj.AddComponent<RectTransform>();
//             }
//             rectTransform.sizeDelta = slotSize;
            
//             // Setup Button component
//             Button slotButton = SetupSlotButton(slotObj);
            
//             // Setup Background Image
//             Image backgroundImage = SetupBackgroundImage(slotObj);
            
//             // Setup UI Structure
//             var uiElements = SetupUIStructure(slotObj);
            
//             // Setup SaveSlotUI component
//             SetupSaveSlotUIComponent(slotObj, slotButton, backgroundImage, uiElements);
            
//             Debug.Log($"✅ Setup completed for: {slotObj.name}");
//         }
        
//         private Button SetupSlotButton(GameObject slotObj)
//         {
//             Button button = slotObj.GetComponent<Button>();
//             if (button == null)
//             {
//                 button = slotObj.AddComponent<Button>();
//                 button.targetGraphic = slotObj.GetComponent<Image>();
//                 Debug.Log($"Added Button component to {slotObj.name}");
//             }
//             return button;
//         }
        
//         private Image SetupBackgroundImage(GameObject slotObj)
//         {
//             Image image = slotObj.GetComponent<Image>();
//             if (image == null)
//             {
//                 image = slotObj.AddComponent<Image>();
//                 image.color = emptySlotColor;
//                 Debug.Log($"Added Image component to {slotObj.name}");
//             }
//             return image;
//         }
        
//         private UIElements SetupUIStructure(GameObject slotObj)
//         {
//             UIElements elements = new UIElements();
            
//             // Find or create SlotInfoPanel
//             Transform slotInfoPanel = slotObj.transform.Find("SlotInfoPanel");
//             if (slotInfoPanel == null && autoCreateMissingElements)
//             {
//                 GameObject panel = new GameObject("SlotInfoPanel");
//                 panel.transform.SetParent(slotObj.transform, false);
                
//                 RectTransform panelRect = panel.AddComponent<RectTransform>();
//                 panelRect.anchorMin = new Vector2(0, 0);
//                 panelRect.anchorMax = new Vector2(0.7f, 1);
//                 panelRect.offsetMin = Vector2.zero;
//                 panelRect.offsetMax = Vector2.zero;
                
//                 slotInfoPanel = panel.transform;
//                 Debug.Log($"Created SlotInfoPanel for {slotObj.name}");
//             }
            
//             if (slotInfoPanel != null)
//             {
//                 // Setup SlotText
//                 elements.slotText = SetupTextElement(slotInfoPanel, "SlotText", "Empty", 16);
                
//                 // Setup DateTimeText  
//                 elements.dateTimeText = SetupTextElement(slotInfoPanel, "DateTimeText", "", 12);
//             }
            
//             // Find or create ActionButtonsPanel
//             Transform actionPanel = slotObj.transform.Find("ActionButtonsPanel");
//             if (actionPanel == null && autoCreateMissingElements)
//             {
//                 GameObject panel = new GameObject("ActionButtonsPanel");
//                 panel.transform.SetParent(slotObj.transform, false);
//                 panel.SetActive(false); // Initially hidden
                
//                 RectTransform panelRect = panel.AddComponent<RectTransform>();
//                 panelRect.anchorMin = new Vector2(0.7f, 0);
//                 panelRect.anchorMax = new Vector2(1, 1);
//                 panelRect.offsetMin = Vector2.zero;
//                 panelRect.offsetMax = Vector2.zero;
                
//                 // Add Layout Group
//                 HorizontalLayoutGroup layoutGroup = panel.AddComponent<HorizontalLayoutGroup>();
//                 layoutGroup.spacing = 10;
//                 layoutGroup.padding = new RectOffset(10, 10, 10, 10);
                
//                 actionPanel = panel.transform;
//                 Debug.Log($"Created ActionButtonsPanel for {slotObj.name}");
//             }
            
//             if (actionPanel != null)
//             {
//                 elements.actionButtonsPanel = actionPanel.gameObject;
                
//                 // Setup Load Button
//                 elements.loadButton = SetupButtonElement(actionPanel, "LoadButton", "Load");
                
//                 // Setup Delete Button
//                 elements.deleteButton = SetupButtonElement(actionPanel, "DeleteButton", "Delete");
//             }
            
//             return elements;
//         }
        
//         private TextMeshProUGUI SetupTextElement(Transform parent, string name, string text, int fontSize)
//         {
//             Transform existing = parent.Find(name);
//             if (existing != null)
//             {
//                 return existing.GetComponent<TextMeshProUGUI>();
//             }
            
//             if (!autoCreateMissingElements) return null;
            
//             GameObject textObj = new GameObject(name);
//             textObj.transform.SetParent(parent, false);
            
//             TextMeshProUGUI textComp = textObj.AddComponent<TextMeshProUGUI>();
//             textComp.text = text;
//             textComp.fontSize = fontSize;
//             textComp.color = Color.white;
            
//             RectTransform rectTransform = textObj.GetComponent<RectTransform>();
//             rectTransform.anchorMin = Vector2.zero;
//             rectTransform.anchorMax = Vector2.one;
//             rectTransform.offsetMin = Vector2.zero;
//             rectTransform.offsetMax = Vector2.zero;
            
//             Debug.Log($"Created {name} for {parent.name}");
//             return textComp;
//         }
        
//         private Button SetupButtonElement(Transform parent, string name, string text)
//         {
//             Transform existing = parent.Find(name);
//             if (existing != null)
//             {
//                 return existing.GetComponent<Button>();
//             }
            
//             if (!autoCreateMissingElements) return null;
            
//             GameObject buttonObj = new GameObject(name);
//             buttonObj.transform.SetParent(parent, false);
            
//             // Add Image for button background
//             Image buttonImage = buttonObj.AddComponent<Image>();
//             buttonImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            
//             // Add Button component
//             Button button = buttonObj.AddComponent<Button>();
//             button.targetGraphic = buttonImage;
            
//             // Add text child
//             GameObject textObj = new GameObject("Text");
//             textObj.transform.SetParent(buttonObj.transform, false);
            
//             TextMeshProUGUI textComp = textObj.AddComponent<TextMeshProUGUI>();
//             textComp.text = text;
//             textComp.fontSize = 12;
//             textComp.color = Color.white;
//             textComp.alignment = TextAlignmentOptions.Center;
            
//             RectTransform textRect = textObj.GetComponent<RectTransform>();
//             textRect.anchorMin = Vector2.zero;
//             textRect.anchorMax = Vector2.one;
//             textRect.offsetMin = Vector2.zero;
//             textRect.offsetMax = Vector2.zero;
            
//             Debug.Log($"Created {name} button for {parent.name}");
//             return button;
//         }
        
//         private void SetupSaveSlotUIComponent(GameObject slotObj, Button slotButton, Image backgroundImage, UIElements elements)
//         {
//             SaveSlotUI saveSlotUI = slotObj.GetComponent<SaveSlotUI>();
//             if (saveSlotUI == null)
//             {
//                 saveSlotUI = slotObj.AddComponent<SaveSlotUI>();
//                 Debug.Log($"Added SaveSlotUI component to {slotObj.name}");
//             }
            
//             // Use reflection to set private fields (since they're SerializeField)
//             var fields = typeof(SaveSlotUI).GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
//             foreach (var field in fields)
//             {
//                 if (field.Name == "slotButton")
//                     field.SetValue(saveSlotUI, slotButton);
//                 else if (field.Name == "slotText")
//                     field.SetValue(saveSlotUI, elements.slotText);
//                 else if (field.Name == "dateTimeText")
//                     field.SetValue(saveSlotUI, elements.dateTimeText);
//                 else if (field.Name == "backgroundImage")
//                     field.SetValue(saveSlotUI, backgroundImage);
//                 else if (field.Name == "loadGameButton")
//                     field.SetValue(saveSlotUI, elements.loadButton);
//                 else if (field.Name == "deleteSaveButton")
//                     field.SetValue(saveSlotUI, elements.deleteButton);
//                 else if (field.Name == "actionButtonsPanel")
//                     field.SetValue(saveSlotUI, elements.actionButtonsPanel);
//                 else if (field.Name == "emptySlotColor")
//                     field.SetValue(saveSlotUI, emptySlotColor);
//                 else if (field.Name == "filledSlotColor")
//                     field.SetValue(saveSlotUI, filledSlotColor);
//                 else if (field.Name == "selectedSlotColor")
//                     field.SetValue(saveSlotUI, selectedSlotColor);
//             }
            
//             Debug.Log($"Configured SaveSlotUI component for {slotObj.name}");
//         }
        
//         private struct UIElements
//         {
//             public TextMeshProUGUI slotText;
//             public TextMeshProUGUI dateTimeText;
//             public Button loadButton;
//             public Button deleteButton;
//             public GameObject actionButtonsPanel;
//         }
//     }
// }

// #if UNITY_EDITOR
// using UnityEditor;

// [CustomEditor(typeof(SaveGameSlotSetupHelper))]
// public class SaveGameSlotSetupHelperEditor : Editor
// {
//     public override void OnInspectorGUI()
//     {
//         DrawDefaultInspector();
        
//         EditorGUILayout.Space();
        
//         SaveGameSlotSetupHelper helper = (SaveGameSlotSetupHelper)target;
        
//         if (GUILayout.Button("Auto Setup SaveGame Slots", GUILayout.Height(40)))
//         {
//             helper.AutoSetupSaveGameSlots();
//         }
        
//         EditorGUILayout.Space();
        
//         EditorGUILayout.HelpBox(
//             "Instructions:\n" +
//             "1. Assign your SaveGame GameObjects to the array\n" +
//             "2. Configure UI settings if needed\n" +
//             "3. Click 'Auto Setup SaveGame Slots'\n" +
//             "4. Script will automatically create UI structure and components",
//             MessageType.Info
//         );
//     }
// }
// #endif
