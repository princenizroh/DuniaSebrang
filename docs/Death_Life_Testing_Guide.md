# 🧪 Death-Life Testing Guide
## Proper Testing Sequence for Death and Life Effects

### 🎯 **Konsep yang Benar**

```
Normal Game → Death (Gelap Masuk) → Black Screen → Life (Terang Keluar dari Tengah) → Normal Game
```

**Little Nightmares 2 Style:**
- **Death**: Gelap dari pinggir masuk ke tengah
- **Life**: Terang/Game terlihat dari tengah keluar ke pinggir

---

## 🔧 **Testing Methods**

### **✅ Method 1: Individual Testing**

#### **Test Death Effect:**
```
1. Right-click DeathScreenEffect → "Test Death Fade"
2. Screen becomes dark from edges to center
3. Full black screen at end
4. LifeScreenEffect automatically notified
```

#### **Test Life Effect (after death):**
```
1. Right-click LifeScreenEffect → "Test Life Fade"
2. If death was completed: Life effect runs
3. If death not completed: Warning message shown
4. Game becomes visible from center outward
```

### **✅ Method 2: Force Testing**

#### **Force Life Test (skip death check):**
```
1. Right-click LifeScreenEffect → "Force Test Life Fade"
2. Bypasses death completion check
3. Directly runs life effect
4. Good for quick testing
```

### **✅ Method 3: Complete Sequence**

#### **Full Death-Life Sequence:**
```
1. Right-click LifeScreenEffect → "Test Death-Life Sequence"
2. Automatically runs death effect first
3. Waits for death completion
4. Automatically runs life effect
5. Complete cycle testing
```

---

## 🎮 **Debug GUI Controls**

### **LifeScreenEffect Debug Panel (Left Side):**
```
=== LIFE SCREEN EFFECT DEBUG ===
Effect Type: Reverse Vignette (Center to Edge)
Is Life Fading: [True/False]
Life Complete: [True/False]
Life Progress: [0-100%]

--- Status Info ---
Death Effect Complete: [True/False]
Ready for Life Test: [True/False]

--- Controls ---
[Test Life Fade]           - Normal test (requires death complete)
[Force Test Life Fade]     - Skip death check
[Test Death-Life Sequence] - Complete cycle
[Reset Life Effect]        - Reset to initial state
```

### **DeathScreenEffect Debug Panel (Right Side):**
```
=== DEATH SCREEN EFFECT DEBUG ===
Effect Type: Vignette (Little Nightmares)
Is Fading: [True/False]
Fade Complete: [True/False]
Fade Progress: [0-100%]

--- Controls ---
[Test Death Fade]     - Test death effect
[Reset Effect]        - Reset to initial state
```

---

## ⚠️ **Common Issues & Solutions**

### **❌ "Cannot test Life Fade - Death effect not completed!"**
**Problem:** Trying to test life effect without death completion
**Solution:** 
- Run "Test Death Fade" first, OR
- Use "Force Test Life Fade" to bypass check

### **❌ "Life effect shows white screen"**
**Problem:** Old implementation showing white overlay
**Solution:** Updated implementation now starts with black screen

### **❌ "No visual change during life effect"**
**Problem:** GameObject might be inactive or Canvas issues
**Solution:** 
- Ensure LifeScreenEffect GameObject is active
- Check Canvas is set to Screen Space - Overlay
- Verify Image component is assigned

### **❌ "Effect too fast/slow"**
**Problem:** Timing not matching game feel
**Solution:**
- Adjust `lifeFadeDuration` (default: 2.5s)
- Modify `lifeDelay` (default: 0.2s)
- Tune `lifeCurve` for different easing

---

## 🎨 **Visual Effect Verification**

### **Death Effect Should Show:**
1. **Start**: Normal game view
2. **Progress**: Dark vignette growing from edges
3. **End**: Complete black screen

### **Life Effect Should Show:**
1. **Start**: Complete black screen (from death)
2. **Progress**: Game becomes visible from center outward
3. **End**: Normal game view fully restored

### **Reverse Vignette Behavior:**
- **Center**: Game becomes visible first (transparent)
- **Edges**: Stay black longer (opaque)
- **Expansion**: Light/visibility spreads outward smoothly

---

## 🔄 **Integration with Save System**

### **Automatic Flow (In Game):**
```
1. Player dies
2. DeathScreenEffect automatically triggered
3. Death animation completes
4. SaveManager.RespawnAtLastCheckpoint() called
5. LifeScreenEffect automatically triggered
6. Player spawns at checkpoint
7. Life effect reveals game
```

### **Manual Testing Flow:**
```
1. Use context menu or debug GUI
2. Test effects individually or in sequence
3. Verify visual appearance
4. Adjust settings as needed
```

---

## 📝 **Setup Checklist**

### **Before Testing:**
- [ ] **DeathScreenEffect** GameObject active in scene
- [ ] **LifeScreenEffect** GameObject active in scene
- [ ] Both have **Canvas** components (Screen Space - Overlay)
- [ ] Both have **Image** components assigned
- [ ] **Debug GUI** enabled in both scripts

### **During Testing:**
- [ ] Use **right-click context menus** for quick tests
- [ ] Monitor **console messages** for debug info
- [ ] Watch **debug GUI panels** for real-time status
- [ ] Test **both individual and sequence** modes

### **After Testing:**
- [ ] **Adjust timing** settings if needed
- [ ] **Customize colors** to match game theme
- [ ] **Test integration** with actual save system
- [ ] **Verify mobile performance** if applicable

---

## 🎯 **Perfect Test Result**

**Successful Death-Life Cycle:**
1. **Death**: Screen gradually darkens from edges, ending in complete black
2. **Transition**: Brief moment of black screen (realistic transition)
3. **Life**: Game world gradually becomes visible from center outward
4. **Complete**: Normal game view restored, player at checkpoint

**Visual Quality Check:**
- ✅ Smooth transitions without jarring cuts
- ✅ Proper vignette/reverse vignette effects
- ✅ Appropriate timing (not too fast/slow)
- ✅ Clean start and end states
- ✅ No visual artifacts or glitches

The testing system is now complete and ready for use! 🎮✨
