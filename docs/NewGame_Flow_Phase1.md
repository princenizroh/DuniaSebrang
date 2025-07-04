# 🎮 New Game Flow - Phase 1 Implementation

## ✅ **What We've Implemented:**

### **📊 Data Structure Enhancement:**
1. **CheckpointData.cs** - Added `areaName` field for save slot display
2. **SaveData.cs** - Added `totalPlayTime` and `lastCheckpointTime` tracking

### **🎯 UI Components:**
1. **SaveSlotUI.cs** - Component untuk menampilkan individual save slot:
   - Display area name (e.g., "Prison") 
   - Display play time format `HH:MM:SS`
   - Handle empty vs filled slot states
   - Button click events

2. **SaveSlotManager.cs** - Manager untuk save slot selection:
   - Support New Game dan Continue modes
   - Auto-refresh save slot displays
   - Handle slot clicks dengan mode-specific logic
   - Start new game in selected slot

### **🔗 Main Menu Integration:**
1. **MainMenu.cs** - Enhanced dengan save system:
   - `OnMulaiPermainanClicked()` - Set New Game mode
   - `OnLanjutkanClicked()` - Set Continue mode  
   - Auto-find SaveSlotManager reference

### **⏱️ Play Time Tracking:**
1. **SaveManager.cs** - Added time tracking:
   - `StartTimeTracking()` - Mulai tracking waktu
   - `UpdatePlayTime()` - Update total play time
   - `FormatPlayTime()` - Format ke HH:MM:SS
   - Auto-start tracking saat game dimulai

---

## 🎯 **Current New Game Flow:**

```
Main Menu → "Mulai Permainan" Button 
    ↓
SaveSlotManager.SetMode(NewGame)
    ↓
dataSaveGame Panel Opens (with save slots)
    ↓
User Clicks Empty Slot → StartNewGameInSlot()
    ↓
Create New Save → Load Game Scene
```

---

## 🔧 **Setup Instructions:**

### **1. UI Setup (in Unity):**
- **dataSaveGame Panel**: Assign SaveSlotUI components untuk setiap slot
- **SaveSlotManager**: Assign save slot array dan new game scene name
- **MainMenu**: Assign SaveSlotManager reference

### **2. Button Events:**
- **"Mulai Permainan" Button** → `MainMenu.OnMulaiPermainanClicked()`
- **"Lanjutkan" Button** → `MainMenu.OnLanjutkanClicked()`

### **3. SaveSlotUI Setup:**
- **Slot Button**: Button component untuk click events
- **Area Text**: TextMeshProUGUI untuk area name
- **Time Text**: TextMeshProUGUI untuk play time  
- **Empty/Filled Content**: GameObjects untuk state visual

---

## 🎮 **Test Scenario:**
1. ✅ Click "Mulai Permainan" → Save slot selection opens in New Game mode
2. ✅ Click empty slot → New game starts and loads game scene
3. ✅ Play time tracking starts automatically
4. ✅ Save slots show area name dan formatted time

---

## 🚀 **Next Steps (Future):**
- Override confirmation untuk filled slots
- Continue mode functionality  
- Delete save confirmation
- Loading screen integration
- Load/Delete buttons untuk continue mode

**Current Status: New Game flow READY untuk basic testing!** 🎯
