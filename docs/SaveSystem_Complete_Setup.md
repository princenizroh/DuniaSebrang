# Save System: Complete Integration Guide

## 🚀 Quick Setup for All Components

This guide helps you integrate all the save system components into your Unity project.

## 📁 File Overview

### Core Scripts (Already Implemented)
- `CheckpointData.cs` - Individual checkpoint ScriptableObjects
- `CheckpointLibrary.cs` - Collection of all checkpoints  
- `SaveData.cs` - Save file data structure
- `SaveManager.cs` - Core save/load functionality
- `CheckpointTrigger.cs` - Checkpoint activation
- `PlayerDeathHandler.cs` - Death/respawn system
- `DeathScreenEffect.cs` - Death screen transitions

### New UI Scripts (Phase 2)
- `ConfirmationDialog.cs` - Overwrite/Delete confirmations
- `SaveSlotUI.cs` - Individual save slot display
- `SaveSlotManager.cs` - Save slot selection management
- `SaveSlotActions.cs` - Load/Delete actions for continue mode
- `LoadingScreen.cs` - Smooth scene transitions
- `MainMenu.cs` - Main menu integration

## 🎨 UI Prefab Setup

### 1. ConfirmationDialog Prefab
```
ConfirmationDialog (GameObject)
├── Background (Image) - Semi-transparent overlay
├── DialogPanel (Image) - Main dialog box
    ├── TitleText (TextMeshPro) - Dialog title
    ├── MessageText (TextMeshPro) - Dialog message  
    ├── ButtonContainer (Horizontal Layout Group)
        ├── ConfirmButton (Button + TextMeshPro)
        └── CancelButton (Button + TextMeshPro)
```

**ConfirmationDialog Component Setup:**
- dialogPanel → DialogPanel GameObject
- titleText → TitleText component
- messageText → MessageText component
- confirmButton → ConfirmButton component
- cancelButton → CancelButton component
- backgroundBlocker → Background GameObject (with Button component)

### 2. SaveSlotUI Prefab
```
SaveSlotUI (GameObject + Button)
├── EmptySlotIndicator (GameObject)
│   └── EmptyText (TextMeshPro) - "Empty"
├── FilledSlotContent (GameObject)
    ├── AreaText (TextMeshPro) - Area name
    ├── TimeText (TextMeshPro) - Play time
    └── SlotBackground (Image)
```

**SaveSlotUI Component Setup:**
- slotButton → Root Button component
- areaText → AreaText component
- timeText → TimeText component
- emptySlotIndicator → EmptySlotIndicator GameObject
- filledSlotContent → FilledSlotContent GameObject

### 3. SaveSlotActions Prefab
```
SaveSlotActions (GameObject)
├── ActionsPanel (Image) - Main panel
    ├── SlotInfoText (TextMeshPro) - Slot details
    ├── ButtonContainer (Vertical Layout Group)
        ├── LoadButton (Button + TextMeshPro)
        ├── DeleteButton (Button + TextMeshPro)
        └── CloseButton (Button + TextMeshPro)
```

**SaveSlotActions Component Setup:**
- actionsPanel → ActionsPanel GameObject
- slotInfoText → SlotInfoText component
- loadButton → LoadButton component
- deleteButton → DeleteButton component
- closeButton → CloseButton component

### 4. SaveSlotManager Setup
```
SaveSlotManager (GameObject)
├── SaveSlotContainer (Vertical Layout Group)
    ├── SaveSlot1 (SaveSlotUI Prefab)
    ├── SaveSlot2 (SaveSlotUI Prefab)
    ├── SaveSlot3 (SaveSlotUI Prefab)
    ├── SaveSlot4 (SaveSlotUI Prefab)
    └── SaveSlot5 (SaveSlotUI Prefab)
```

**SaveSlotManager Component Setup:**
- saveSlots → Array of 5 SaveSlotUI components
- saveManager → SaveManager in scene (auto-found if null)
- newGameScene → "GameScene" (your main game scene)

### 5. LoadingScreen Prefab
```
LoadingScreen (GameObject - DontDestroyOnLoad)
├── LoadingPanel (Canvas)
    ├── BackgroundImage (Image) - Full screen
    ├── ContentContainer (GameObject)
        ├── LoadingText (TextMeshPro)
        ├── ProgressBar (Slider)
        ├── TipText (TextMeshPro)
        └── LoadingIcon (Image) - Optional spinner
```

**LoadingScreen Component Setup:**
- loadingPanel → LoadingPanel GameObject
- progressBar → ProgressBar Slider component
- loadingText → LoadingText component
- tipText → TipText component
- backgroundImage → BackgroundImage component

## 📋 Scene Integration Steps

### Main Menu Scene
1. **Add MainMenu GameObject** with MainMenu.cs component
2. **Add SaveSlotManager** as child with SaveSlotUI array
3. **Add ConfirmationDialog** prefab to scene
4. **Add SaveSlotActions** prefab to scene
5. **Add LoadingScreen** prefab (will persist across scenes)

### Game Scene
1. **Add SaveManager** to persistent GameObject
2. **Add CheckpointLibrary** ScriptableObject to SaveManager
3. **Set up CheckpointTrigger** components at checkpoints
4. **Add PlayerDeathHandler** to Player GameObject
5. **Add DeathScreenEffect** prefab to scene

## 🔧 Component Connections

### SaveManager Setup
```csharp
[Header("SAVE SETTINGS")]
maxSaveSlots = 5
currentSaveSlot = 0
saveFilePrefix = "DuniaSebrang_Save"

[Header("CHECKPOINTS")]
checkpointLibrary = YourCheckpointLibrary ScriptableObject

[Header("REFERENCES")]
player = Player GameObject
playerDeathHandler = PlayerDeathHandler component
```

### MainMenu Setup
```csharp
[Header("UI REFERENCES")]
startGameButton = "Mulai Permainan" Button
continueButton = "Lanjutkan" Button

[Header("REFERENCES")]
saveSlotManager = SaveSlotManager component (auto-found)
```

## 🎮 Testing Workflow

### 1. Test New Game Flow
1. Click "Mulai Permainan"
2. Select empty slot → Should start new game immediately
3. Select filled slot → Should show overwrite confirmation
4. Confirm overwrite → Should start new game with loading screen

### 2. Test Continue Flow  
1. Click "Lanjutkan"
2. Select filled slot → Should show Load/Delete actions
3. Click Load → Should load game with loading screen
4. Click Delete → Should show delete confirmation
5. Confirm delete → Should delete and refresh slot

### 3. Test Loading Screen
1. All scene transitions should use loading screen
2. Progress bar should animate smoothly
3. Loading texts should change randomly
4. Minimum loading time should prevent flashing

## 🎨 Styling Guidelines

### Color Scheme (Suggested)
- **Primary UI:** Dark grays (#2C2C2C, #3C3C3C)
- **Text:** White (#FFFFFF) or light gray (#E0E0E0)
- **Accent:** Orange/amber (#FF8C00) for highlights
- **Confirmation:** Red (#FF4444) for destructive actions
- **Success:** Green (#44FF44) for confirmations

### Typography
- **Headers:** Bold, 18-24pt
- **Body Text:** Regular, 14-16pt  
- **UI Labels:** Medium, 12-14pt
- **Buttons:** Bold, 14-16pt

### Layout
- **8px grid system** for consistent spacing
- **Rounded corners** (4-8px radius) for modern look
- **Drop shadows** for depth and hierarchy
- **Smooth animations** (0.2-0.5s duration)

## 🚨 Common Issues & Solutions

### Issue: ConfirmationDialog not found
**Solution:** Ensure ConfirmationDialog prefab is in the scene and has the component attached.

### Issue: LoadingScreen doesn't show
**Solution:** Check that LoadingScreen has DontDestroyOnLoad and is set up as singleton.

### Issue: Save slots not refreshing
**Solution:** Verify SaveSlotManager.RefreshSaveSlots() is called after slot changes.

### Issue: Reflection errors
**Solution:** Ensure all referenced method names match exactly (case-sensitive).

## ✅ Final Checklist

- [ ] All prefabs created and configured
- [ ] Scene integration complete
- [ ] Component references assigned
- [ ] Test flows validated
- [ ] UI styling applied
- [ ] Error handling tested
- [ ] Performance verified

This completes the full save system integration with all advanced features!
