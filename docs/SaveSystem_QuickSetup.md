# 🚀 Save System Quick Setup Checklist

## ✅ **Step-by-Step Setup**

### **1. Create CheckpointData Assets**
- [ ] Right-click → **Game Data > Save > Checkpoint Data**
- [ ] Create minimal 1 starting checkpoint: `Level1_Start_Checkpoint`
- [ ] Set **Is Starting Checkpoint** = true
- [ ] Configure spawn position dan rotation

### **2. Setup SaveManager**
- [ ] Create GameObject bernama `SaveManager`
- [ ] Add **SaveManager** component
- [ ] Assign **Starting Checkpoint** 
- [ ] Add checkpoint assets ke **All Checkpoints** list
- [ ] Set **Player** reference
- [ ] Enable **Enable Debug GUI** untuk testing

### **3. Create Checkpoint Trigger**
- [ ] Create GameObject di scene untuk save point
- [ ] Add **Collider** component (set **Is Trigger** = true)
- [ ] Add **CheckpointTrigger** component
- [ ] Assign **Checkpoint Data** asset
- [ ] Set **Player Tag** = "Player"

### **4. Player Setup**
- [ ] Ensure Player GameObject memiliki tag **"Player"**
- [ ] **PlayerDeathHandler** sudah ter-update otomatis
- [ ] Set **Auto Respawn To Checkpoint** = true
- [ ] Set **Respawn Delay** = 2 seconds

### **5. Test Everything**
- [ ] Play scene dan trigger checkpoint
- [ ] Check console untuk "Game saved at checkpoint" message
- [ ] Test player death (via debug button atau script lain)
- [ ] Verify respawn ke checkpoint position
- [ ] Test save/load via debug GUI

---

## 🎮 **Quick Test Sequence**

1. **Start Play Mode**
2. **Walk into checkpoint trigger** → Should see save message
3. **Press F1** untuk debug GUI
4. **Click "Test Death"** di PlayerDeathHandler debug
5. **Wait for death animation + respawn delay**
6. **Player should respawn** di checkpoint position

---

## 🔧 **Debug Features Available**

### **In-Game Debug GUI (F1 key):**
- Quick Save/Load buttons
- Respawn to checkpoint
- Save slot management
- Current save info

### **Inspector Context Menus:**
- **CheckpointTrigger**: Manual Trigger, Reset Trigger
- **PlayerDeathHandler**: Test Death, Test Reset, Test Respawn
- **SaveManager**: Quick Save, Quick Load, Respawn

### **Console Messages:**
- All save/load operations logged
- Death and respawn events tracked
- Error messages for troubleshooting

---

## ⚠️ **Common Issues & Solutions**

### **"No SaveManager found"**
- Pastikan SaveManager GameObject ada di scene
- Component SaveManager ter-attach

### **"No checkpoint available"**
- Assign Starting Checkpoint di SaveManager
- Create minimal 1 CheckpointData asset

### **Player tidak respawn**
- Check Auto Respawn To Checkpoint enabled
- Verify SaveManager.Player reference assigned
- Check console untuk error messages

### **Checkpoint tidak trigger**
- Pastikan Player tag = "Player"
- Collider Is Trigger = true
- CheckpointData ter-assign

---

## 📂 **Expected File Structure**

```
Assets/
├── Scripts/
│   ├── Data/Save/
│   │   ├── CheckpointData.cs ✅
│   │   └── SaveData.cs ✅
│   ├── Checkpoints/
│   │   └── CheckpointTrigger.cs ✅
│   ├── Core/
│   │   └── SaveManager.cs ✅
│   └── Player/
│       └── PlayerDeathHandler.cs ✅ (Updated)
│
└── ScriptableObjects/Save/ (Create these)
    ├── Level1_Start_Checkpoint.asset
    └── ... (more checkpoints)
```

---

## 🎯 **Success Indicators**

- [ ] Console shows: "★ Game saved successfully at [checkpoint name]"
- [ ] Player death triggers: "★★★ TRIGGERING RESPAWN TO CHECKPOINT ★★★"
- [ ] Debug GUI shows SaveManager Found: True
- [ ] JSON save files created di: `%USERPROFILE%/AppData/LocalLow/[Company]/Dunia Sebrang/Saves/`
- [ ] Player respawns di exact checkpoint position

---

## 🚀 **Ready to Go!**

Semua code sudah siap dan compile tanpa error. Tinggal setup di Unity editor dan test!

Jika ada masalah, check console messages dan pastikan semua references ter-assign dengan benar.

**Happy Saving! 💾**
