# Save Slot System Documentation

## Overview
This is a robust, production-ready save slot system for Unity games. Each save slot operates independently with no cross-contamination, ensuring data safety and reliability.

## Key Features
✅ **Independent Save Slots**: Each slot saves/loads its own data with zero cross-contamination  
✅ **Persistent Slot Tracking**: Slot selection survives scene changes and SaveManager recreation  
✅ **Safety Validation**: Comprehensive checks before all critical operations  
✅ **User-Friendly Conflict Resolution**: Handles slot conflicts gracefully  
✅ **Debug & Development Tools**: Extensive logging and testing methods  
✅ **Production Ready**: Conditional compilation for dev tools, optimized for release builds  

## Core Components

### SaveManager.cs
- **Purpose**: Core save system handling all save/load operations
- **Key Feature**: `persistentCurrentSlot` - Static slot tracking that survives object recreation
- **Location**: `Assets/Scripts/Core/SaveManager.cs`

### SaveSlotManager.cs  
- **Purpose**: UI manager for save slot selection and conflict resolution
- **Key Features**: New Game/Continue flows, slot validation, user choice handling
- **Location**: `Assets/Scripts/UI/SaveSlotManager.cs`

### SaveSlotUI.cs
- **Purpose**: Individual slot UI components displaying slot information
- **Location**: `Assets/Scripts/UI/SaveSlotUI.cs`

### SaveSlotConfirmationDialog.cs
- **Purpose**: Dialog system for handling slot conflicts and user choices
- **Location**: `Assets/Scripts/UI/SaveSlotConfirmationDialog.cs`

## How It Works

### Slot Independence
Each save slot has its own file (`GameSave_Slot0.json`, `GameSave_Slot1.json`, etc.) and operates completely independently. No slot can accidentally overwrite or load from another slot.

### Persistent Slot Tracking
The system uses `static int persistentCurrentSlot` to track the active slot across scene changes and SaveManager recreation, preventing the "always saving to slot 0" bug.

### Safety Checks
Before any critical operation, the system performs:
- Slot index validation
- SaveManager state verification  
- File system access checks
- Data integrity validation

### Conflict Resolution
When conflicts arise (e.g., starting new game on occupied slot), the system:
1. **With UI Dialog**: Shows user-friendly options (Continue/Start Fresh/Cancel)
2. **Fallback Mode**: Uses debug logs with safe default behavior

## Usage

### Setting Up UI Dialog (Recommended)
1. Create UI elements for the confirmation dialog
2. Assign `SaveSlotConfirmationDialog` component
3. Uncomment dialog code in `SaveSlotManager.cs`:
   ```csharp
   [SerializeField] private SaveSlotConfirmationDialog confirmationDialog;
   ```
4. Link dialog reference in inspector

### Basic Usage
```csharp
// New Game flow
saveSlotManager.StartNewGameWithValidation(slotIndex);

// Continue Game flow  
saveSlotManager.LoadGameWithValidation(slotIndex);

// Force new game (bypasses conflict resolution)
saveSlotManager.ForceStartNewGameInSlot(slotIndex);
```

### Safety Methods
```csharp
// Perform safety check before operations
bool safe = saveSlotManager.PerformSafetyCheck(slotIndex, "Save Game");

// Validate slot isolation (no cross-contamination)
bool isolated = saveSlotManager.ValidateSlotIsolation();

// Check specific slot integrity
bool valid = saveSlotManager.ValidateSlotIntegrity(slotIndex);
```

## Development vs Production

### Development Mode
- Full debug logging enabled
- Developer testing methods available
- Fallback to log-based conflict resolution
- Safety warnings and detailed diagnostics

### Production Mode  
- Minimal logging for performance
- Developer methods excluded via conditional compilation
- UI-based conflict resolution
- Streamlined user experience

### Conditional Compilation
Developer methods are wrapped in:
```csharp
#if DEVELOPMENT_BUILD || UNITY_EDITOR
// Developer-only code here
#endif
```

## Configuration

### SaveSlotManager Inspector Settings
- **Save Slots**: Array of SaveSlotUI components
- **Save Manager**: Reference to SaveManager instance
- **Confirmation Dialog**: Reference to SaveSlotConfirmationDialog (optional)
- **New Game Scene**: Scene to load for new games
- **Show Debug**: Enable/disable debug logging
- **Force Refresh Key**: Hotkey to refresh all slots (F5)
- **Debug Save Files Key**: Hotkey to debug save files (F12)

### SaveManager Inspector Settings
- **Auto Save Interval**: Seconds between auto-saves (0 = disabled)
- **Max Save Slots**: Maximum number of save slots
- **Current Save Slot**: Default active slot
- **Save File Prefix**: Prefix for save file names
- **Show Debug**: Enable/disable debug logging

## Troubleshooting

### Common Issues

**Problem**: Save always goes to slot 0  
**Solution**: Ensure `persistentCurrentSlot` is properly updated in all slot-changing methods

**Problem**: Slots showing wrong information  
**Solution**: Call `RefreshAllSlots()` or use F5 hotkey

**Problem**: Cross-slot contamination  
**Solution**: Run `ValidateSlotIsolation()` to check for issues

**Problem**: Dialog not showing  
**Solution**: Ensure `confirmationDialog` is assigned and dialog code is uncommented

### Debug Methods (Development Only)
```csharp
// Test different scenarios
DEV_TestNewGameOnEmptySlot(slotIndex);
DEV_TestNewGameOnFullSlot(slotIndex);  
DEV_TestContinueOnEmptySlot(slotIndex);
DEV_TestContinueOnFullSlot(slotIndex);

// Check slot status
DEV_ShowSlotStatus(slotIndex);
```

### Hotkeys (Development)
- **F5**: Force refresh all slots
- **F12**: Debug save files and paths

## File Structure
```
Assets/Scripts/
├── Core/
│   └── SaveManager.cs          # Core save system
├── UI/
│   ├── SaveSlotManager.cs      # Slot selection manager
│   ├── SaveSlotUI.cs           # Individual slot UI
│   └── SaveSlotConfirmationDialog.cs  # Conflict resolution dialog
└── Data/Save/
    └── SaveData.cs             # Save data structures
```

## Best Practices

1. **Always use validation methods** before critical operations
2. **Test slot isolation** regularly during development  
3. **Assign confirmation dialog** for production builds
4. **Remove developer methods** in final release builds
5. **Monitor debug logs** during development for slot tracking
6. **Use safety wrappers** (`StartNewGameWithValidation`, `LoadGameWithValidation`)

## Migration from Previous Systems

If upgrading from an older save system:
1. Backup existing save files
2. Update all save/load calls to use new validation methods
3. Replace direct SaveManager calls with SaveSlotManager methods
4. Test slot independence thoroughly
5. Implement confirmation dialog UI

## Security Considerations

- Save files are stored in `Application.persistentDataPath`
- No automatic file cleanup (preserves user data)
- Slot validation prevents accidental overwrites
- Safe default behaviors prevent data loss

## Performance Notes

- File I/O is performed on-demand (not cached)
- Debug logging can be disabled for production
- Conditional compilation removes dev tools from release builds
- Slot UI updates are manual (call `RefreshAllSlots()` when needed)

---

**Created**: July 2025  
**Version**: 2.0  
**Status**: Production Ready ✅
