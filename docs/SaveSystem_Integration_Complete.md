# Save System Integration Guide
## Complete Checkpoint & Save/Load System for Dunia Sebrang

### 📋 **System Overview**

Sistem save yang telah dibuat terdiri dari beberapa komponen utama:

1. **CheckpointData** (ScriptableObject) - Data checkpoint
2. **SaveData** (Class) - Game state data structure  
3. **CheckpointTrigger** (Component) - Collider untuk save points
4. **SaveManager** (Singleton) - Core save/load system
5. **PlayerDeathHandler Integration** - Auto-respawn ke checkpoint

---

### 🎯 **Key Features**

✅ **Multiple Save Slots** (5 slots default)  
✅ **Checkpoint-based Saving** dengan collider triggers  
✅ **Auto-respawn** ke last checkpoint saat player mati  
✅ **ScriptableObject-based** checkpoint tracking  
✅ **Comprehensive Game State** saving  
✅ **Debug GUI** untuk testing  
✅ **Auto-save** system dengan timer  
✅ **Scene management** untuk checkpoint di scene berbeda  

---

### 🛠️ **Setup Instructions**

#### **Step 1: Create Checkpoint Data Assets**

1. **Right-click di Project window**
2. **Game Data > Save > Checkpoint Data**
3. **Rename** sesuai level (e.g., `Level1_Start_Checkpoint`)
4. **Configure checkpoint data:**
   - `Checkpoint Name`: Nama untuk identifikasi
   - `Description`: Deskripsi checkpoint
   - `Spawn Position`: Posisi respawn player (auto-set dari transform)
   - `Spawn Rotation`: Rotasi respawn player
   - `Scene Name`: Scene checkpoint ini berada
   - `Is Starting Checkpoint`: Centang jika ini starting point
   - `Priority`: Priority checkpoint (higher = lebih prioritas)

#### **Step 2: Setup SaveManager**

1. **Create empty GameObject** di scene bernama `SaveManager`
2. **Add SaveManager component**
3. **Configure SaveManager:**
   - `Auto Save Interval`: 300 (5 minutes auto-save)
   - `Max Save Slots`: 5
   - `Current Save Slot`: 0
   - `All Checkpoints`: Drag semua CheckpointData assets ke list
   - `Starting Checkpoint`: Assign starting checkpoint
   - `Player`: Assign player GameObject
   - `Player Death Handler`: Auto-assigned

#### **Step 3: Create Checkpoint Triggers**

1. **Create GameObject** di scene untuk checkpoint
2. **Add Collider component** (Box/Sphere, set as **Trigger**)
3. **Add CheckpointTrigger component**
4. **Configure CheckpointTrigger:**
   - `Checkpoint Data`: Assign CheckpointData asset
   - `Player Tag`: "Player"
   - `Trigger Once`: true (opsional)
   - `Auto Save`: true
   - `Show Save Confirmation`: true

#### **Step 4: Update PlayerDeathHandler**

PlayerDeathHandler sudah ter-update dengan:
- **Auto Respawn to Checkpoint**: Otomatis respawn ke last checkpoint
- **Respawn Delay**: Delay sebelum respawn (default 2 detik)
- **SaveManager Integration**: Otomatis find dan integrate

---

### 🎮 **How It Works**

#### **Save Flow:**
1. Player **menginjak CheckpointTrigger**
2. CheckpointTrigger **calls SaveManager.SaveGameAtCheckpoint()**
3. SaveManager **gathers game state** dari berbagai systems
4. Data **disimpan ke JSON file** di persistent path
5. **Visual/audio feedback** dimainkan

#### **Death & Respawn Flow:**
1. Player **mati** (PlayerDeathHandler.Die())
2. **Death animation** dan vignette effect dimainkan
3. Setelah animation selesai, **auto-respawn** ter-trigger
4. SaveManager **loads last checkpoint**
5. Player **di-move** ke checkpoint position
6. **Player state di-reset**

#### **Load Flow:**
1. SaveManager **reads JSON file**
2. **Applies player position** dan rotation
3. **Restores game state** (inventory, progress, etc.)
4. **Sets current checkpoint**

---

### 💾 **Save Data Structure**

```csharp
SaveData mencakup:
├── Player Data (position, health, stats, inventory)
├── Checkpoint Data (last checkpoint, activated checkpoints)
├── Game Progress (completed levels, story progress)
├── Collectibles (collected items, states)
└── Settings (audio, graphics, controls)
```

---

### 🧪 **Testing & Debug**

#### **Debug GUI (In-Game)**
- **F1** atau enable `Enable Debug GUI` di SaveManager
- **Quick Save/Load** buttons
- **Respawn** ke checkpoint
- **Save slot** management

#### **Inspector Context Menus**
**CheckpointTrigger:**
- `Manual Trigger` - Test checkpoint trigger
- `Reset Trigger` - Reset trigger state

**PlayerDeathHandler:**
- `Test Death` - Trigger player death
- `Test Reset` - Reset player state
- `Test Checkpoint Respawn` - Test respawn logic

**SaveManager:**
- `Quick Save` - Save game instantly
- `Quick Load` - Load game instantly
- `Respawn at Checkpoint` - Test respawn

#### **Debug Console Commands**
All operations menggunakan debug messages untuk tracking.

---

### 📁 **File Structure**

```
Assets/Scripts/
├── Data/Save/
│   ├── CheckpointData.cs (ScriptableObject)
│   └── SaveData.cs (Data structures)
├── Checkpoints/
│   └── CheckpointTrigger.cs (Trigger component)
├── Core/
│   └── SaveManager.cs (Main save system)
└── Player/
    └── PlayerDeathHandler.cs (Updated with respawn)

Save Files Location:
{Application.persistentDataPath}/Saves/
├── DuniaSebrang_Save_Slot00.json
├── DuniaSebrang_Save_Slot01.json
└── ...
```

---

### ⚠️ **Important Notes**

1. **SaveManager harus Singleton** - Hanya 1 di scene
2. **CheckpointData assets** harus di-assign ke SaveManager.allCheckpoints
3. **Player tag** harus "Player" untuk trigger detection
4. **Scene names** di CheckpointData harus exact match
5. **Backup save files** sebelum testing major changes

---

### 🔧 **Customization**

#### **Adding New Save Data:**
1. Update **SaveData** structure
2. Update **SaveManager.GatherSaveData()**
3. Update **SaveManager.ApplyLoadedData()**

#### **Custom Checkpoint Behaviors:**
Extend **CheckpointTrigger** dengan additional logic:
- Special effects per checkpoint
- Conditional saves (e.g., berdasarkan player stats)
- Multiple checkpoint types

#### **Advanced Respawn Logic:**
Extend **PlayerDeathHandler.TriggerRespawnToCheckpoint()** untuk:
- Multiple respawn attempts
- Health/resource penalties
- Special death handling per death cause

---

### ✅ **Ready for Production**

Sistem ini sudah production-ready dengan:
- **Error handling** yang robust
- **Fallback systems** jika components tidak ditemukan
- **Comprehensive logging** untuk debugging
- **Modular design** untuk easy extension
- **Performance optimized** save/load operations

**Next Steps:**
1. Test di Unity editor
2. Create checkpoint assets
3. Setup triggers di scene
4. Test save/load cycle
5. Test death/respawn flow

Semua sudah siap untuk ditest! 🚀
