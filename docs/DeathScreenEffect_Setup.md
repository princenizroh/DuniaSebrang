# Death Screen Effect Setup Guide
## Little Nightmares Style Fade to Black

### 🎯 **OVERVIEW**
Sistem death screen effect yang terinspirasi Little Nightmares dengan fade to black yang smooth dan atmospheric. Includes screen shake, audio effects, dan slow motion.

---

### 📁 **FILES CREATED/MODIFIED**

1. **`DeathScreenEffect.cs`** - Main death screen effect component
2. **`PlayerDeathHandler.cs`** - Updated to integrate with death effect

---

### 🛠️ **UNITY SETUP STEPS**

#### **Step 1: Create Death Screen UI**

1. **Create Canvas (if not exists):**
   - Right-click Hierarchy → UI → Canvas
   - Name: "DeathScreenCanvas"
   - Canvas Scaler → UI Scale Mode: "Scale With Screen Size"
   - Reference Resolution: 1920x1080

2. **Create Fade Overlay:**
   - Right-click Canvas → UI → Image
   - Name: "FadeOverlay"
   - Anchor: Stretch to full screen (Alt + Shift + Click stretch icon)
   - Color: Black (RGB: 0,0,0, Alpha: 0)
   - Remove Source Image (set to None)

3. **Add DeathScreenEffect Component:**
   - Select FadeOverlay GameObject
   - Add Component → Scripts → DeathScreenEffect
   - The script will auto-assign the Image component

#### **Step 2: Configure DeathScreenEffect**

**Inspector Settings:**
```
=== FADE EFFECT ===
✓ Fade Overlay: Auto-assigned (FadeOverlay Image)
• Fade Duration: 2.5s
• Fade Delay: 0.5s
• Fade Curve: EaseInOut curve

=== ATMOSPHERIC EFFECTS ===
✓ Reduce Audio On Fade: true
• Target Audio Volume: 0.1
✓ Enable Slow Motion: true
• Target Time Scale: 0.3

=== ADDITIONAL EFFECTS ===
✓ Enable Screen Shake: true
• Shake Intensity: 0.5
• Shake Duration: 0.8s

=== FADE COLORS ===
• Fade Color: Black (0,0,0,1)
• Use Two Stage Fade: false (optional blood effect)

=== REFERENCES ===
✓ Main Camera: Auto-found
✓ Canvas Group: Auto-assigned
```

#### **Step 3: Setup Player Integration**

1. **Find Player GameObject:**
   - Select your Player GameObject
   - Ensure it has `PlayerDeathHandler` component

2. **Configure PlayerDeathHandler:**
   - In Inspector, find "Death Screen Effect" field
   - Drag the FadeOverlay GameObject (with DeathScreenEffect) here
   - OR leave empty - it will auto-find the component

#### **Step 4: Audio Setup (Optional)**

1. **Add AudioSource to FadeOverlay:**
   - Add Component → Audio → Audio Source
   - Uncheck "Play On Awake"

2. **Add Death Sound Effects:**
   - Import death sound effect (atmospheric/scary sound)
   - Import heartbeat sound (optional)
   - Assign to DeathScreenEffect component

---

### 🎮 **TESTING**

#### **In-Game Testing:**
1. Play the scene
2. Let Takau attack and kill the player
3. Watch the smooth fade to black effect

#### **Manual Testing:**
1. **Test Death Effect:**
   - Right-click DeathScreenEffect → "Test Death Fade"
   - OR use Debug GUI button "Test Death Fade"

2. **Test Reset:**
   - Right-click DeathScreenEffect → "Reset Effect"
   - OR use Debug GUI button "Reset Effect"

#### **Debug Features:**
- **Debug GUI:** Shows fade progress, timing, audio volume
- **Console Logs:** Detailed debugging messages
- **Visual Gizmos:** None needed for screen effects

---

### ⚙️ **CUSTOMIZATION OPTIONS**

#### **Fade Timing:**
```csharp
• fadeDuration: How long the fade takes
• fadeDelay: Delay before fade starts
• fadeCurve: Animation curve for smooth easing
```

#### **Atmospheric Effects:**
```csharp
• reduceAudioOnFade: Gradually reduce game audio
• enableSlowMotion: Slow down time during fade
• enableScreenShake: Camera shake on death impact
```

#### **Two-Stage Fade (Blood Effect):**
```csharp
• useTwoStageFade: true
• firstStageColor: Dark red color
• firstStageDuration: Duration of blood effect
```

#### **Advanced Effects (Future):**
- Blur effect during fade
- Vignette darkening
- Color grading changes
- Particle effects

---

### 🎨 **VISUAL STYLE CUSTOMIZATION**

#### **Little Nightmares Style:**
```
• Fade Duration: 2.5s (slow, ominous)
• Screen Shake: Medium intensity
• Audio: Fade to almost silent
• Color: Pure black fade
• Slow Motion: Slight time dilation
```

#### **Horror Game Style:**
```
• Two Stage Fade: Blood red → Black
• Heartbeat Sound: Fading heartbeat
• Longer Duration: 3-4 seconds
• More Screen Shake: Higher intensity
```

#### **Action Game Style:**
```
• Quick Fade: 1-1.5 seconds
• No Slow Motion: Keep normal time
• Sound: Impact/crash sound
• Less Atmospheric: More direct
```

---

### 🐛 **TROUBLESHOOTING**

#### **Effect Doesn't Trigger:**
- Check PlayerDeathHandler has DeathScreenEffect reference
- Ensure Canvas is in scene and active
- Check Console for error messages

#### **Fade Overlay Not Visible:**
- Verify Canvas Render Mode (Screen Space - Overlay)
- Check FadeOverlay is child of Canvas
- Ensure Canvas Group alpha is being controlled

#### **Audio Not Working:**
- Add AudioSource component to FadeOverlay
- Assign audio clips in DeathScreenEffect
- Check AudioListener.volume is not 0

#### **Screen Shake Not Working:**
- Verify Main Camera reference is assigned
- Check Camera is not constrained by other scripts
- Test with higher shake intensity

---

### 🎯 **INTEGRATION COMPLETE**

The death screen effect is now fully integrated with:
- ✅ **TakauAI** - Boss attacks trigger player death
- ✅ **PlayerDeathHandler** - Handles death logic and animations  
- ✅ **DeathScreenEffect** - Creates atmospheric fade to black
- ✅ **Debug Tools** - Easy testing and validation

**Result:** When Takau attacks the player, you'll get a smooth, atmospheric fade to black exactly like Little Nightmares! 🌙✨
