# 🎮 Cara Menggunakan Save Slot System di MainMenu

## 📋 Skenario Penggunaan

### **1. Setup di Inspector**
- **MainMenu.cs**: 
  - Drag `SaveSlotManager` ke field "Save Slot Manager"
  - Pastikan semua panel UI sudah di-assign
  
### **2. Flow Permainan**

#### **🎯 Mulai Permainan (New Game)**
```
Player Klik "Mulai Permainan"
    ↓
MainMenu.OnMulaiPermainanClicked()
    ↓ 
Set SaveSlotManager mode = NewGame
    ↓
Buka panel dataSaveGame
    ↓
Player pilih slot:
    ├── Slot KOSONG → Langsung buat game baru ✅
    └── Slot ADA DATA → Dialog pilihan:
        ├── Continue Existing ✅
        ├── Start Fresh (hapus data lama) ⚠️
        └── Cancel ❌
```

#### **🎯 Lanjutkan (Continue)**
```
Player Klik "Lanjutkan" 
    ↓
MainMenu.OnLanjutkanClicked()
    ↓
Set SaveSlotManager mode = Continue
    ↓ 
Buka panel dataSaveGame
    ↓
Player pilih slot:
    ├── Slot ADA DATA → Langsung load game ✅
    └── Slot KOSONG → Show error message ❌
```

## 🔧 Method yang Tersedia

### **MainMenu.cs Methods:**
- `OnMulaiPermainanClicked()` - Handler tombol "Mulai Permainan"
- `OnLanjutkanClicked()` - Handler tombol "Lanjutkan" 
- `HasAnySaveData()` - Cek apakah ada save data
- `UpdateContinueButtonState()` - Update status tombol continue
- `OnBackToMainMenuFromSlots()` - Kembali ke main menu
- `OnGameLoadedFromSlot(int slot)` - Handler game berhasil di-load
- `OnNewGameStartedInSlot(int slot)` - Handler game baru dimulai

### **SaveSlotManager.cs Methods:**
- `SetMode(SlotSelectionMode.NewGame)` - Set mode New Game
- `SetMode(SlotSelectionMode.Continue)` - Set mode Continue
- `RefreshSaveSlots()` - Refresh tampilan slot
- `GetEnhancedSaveSlotInfo(int slot)` - Get info detail slot

## 🎨 UI Setup

### **Panel Structure:**
```
MainMenuPanel
├── Button: Lanjutkan → OnLanjutkanClicked()
├── Button: Mulai Permainan → OnMulaiPermainanClicked()
├── Button: Pengaturan → OpenSettings()
├── Button: Tentang Kami → OpenAbout()
└── Button: Keluar → OpenKeluar()

DataSaveGamePanel  
├── SaveSlotManager component
├── Array of SaveSlotUI components
└── Button: Back → OnBackToMainMenuFromSlots()
```

## 💡 Tips Implementation

### **1. Update Continue Button**
Tambahkan di `Update()` atau event trigger:
```csharp
void Start() 
{
    UpdateContinueButtonState();
}
```

### **2. Visual Feedback**
Bisa tambahkan di `UpdateContinueButtonState()`:
```csharp
public void UpdateContinueButtonState()
{
    bool hasData = HasAnySaveData();
    
    // Update button appearance
    if (continueButton != null)
    {
        continueButton.interactable = hasData;
        
        var buttonText = continueButton.GetComponentInChildren<Text>();
        if (buttonText != null)
        {
            buttonText.text = hasData ? "Lanjutkan" : "Lanjutkan (Tidak Ada Data)";
            buttonText.color = hasData ? Color.white : Color.gray;
        }
    }
}
```

### **3. Back Button di Save Slot Panel**
Link back button di save slot panel ke:
```csharp
MainMenu.Instance.OnBackToMainMenuFromSlots();
```

## 🛡️ Safety Features

- ✅ **Auto-connect SaveSlotManager** di Awake()
- ✅ **Event cleanup** di OnDestroy()
- ✅ **Mode isolation** - NewGame vs Continue terpisah
- ✅ **Null checks** di semua method critical
- ✅ **Debug logging** untuk troubleshooting

## 🎯 Expected Behavior

### **Scenario 1: First Time Player**
1. Klik "Mulai Permainan" → Semua slot kosong
2. Pilih slot manapun → Game langsung dimulai

### **Scenario 2: Returning Player** 
1. Klik "Lanjutkan" → Slot menampilkan data existing
2. Pilih slot dengan data → Game langsung di-load

### **Scenario 3: New Game on Used Slot**
1. Klik "Mulai Permainan" → Pilih slot yang ada data
2. Dialog muncul dengan pilihan (Continue/Fresh/Cancel)
3. Player pilih sesuai keinginan

### **Scenario 4: Continue on Empty Slot**
1. Klik "Lanjutkan" → Pilih slot kosong  
2. Error message muncul
3. Player diminta pilih slot lain atau mode New Game

---

**Status**: ✅ Ready to Use  
**Integration**: MainMenu.cs sudah terintegrasi dengan SaveSlotManager  
**Safety**: All critical scenarios handled  
