# Little Nightmares Vignette Death Effect Setup

## 🌙 **TRUE LITTLE NIGHTMARES STYLE**
Sekarang death effect menggunakan **vignette technique** yang benar - gelap dari pinggir-pinggir layar yang perlahan masuk ke tengah sampai seluruh layar gelap, persis seperti di Little Nightmares!

---

## 🎭 **PERBEDAAN DENGAN VERSI SEBELUMNYA**

### **❌ Versi Lama (Salah):**
```
┌─────────────────────┐
│                     │
│      ┌─────┐        │  ← Image kecil fade di tengah
│      │FADE │        │
│      └─────┘        │
│                     │
└─────────────────────┘
```

### **✅ Versi Baru (Benar - Little Nightmares Style):**
```
┌─────────────────────┐
│██░░░░░░░░░░░░░░░░░██│  ← Gelap dari pinggir
│█░░░░░░░░░░░░░░░░░░░█│  
│░░░░░░░░   ░░░░░░░░░░│  ← Terang di tengah
│░░░░░░░░░░░░░░░░░░░░│  ← Perlahan tutup ke tengah
│█░░░░░░░░░░░░░░░░░░░█│
│██░░░░░░░░░░░░░░░░░██│
└─────────────────────┘
```

---

## 🛠️ **SETUP UNITY - UPDATED**

### **Step 1: Create Canvas & Image (sama seperti sebelumnya)**
1. **Create Canvas:** `DeathScreenCanvas`
2. **Create Image:** `DeathScreenUI` (child of Canvas)
3. **Anchor:** Alt + Shift + Stretch (full screen)
4. **Color:** White `(1,1,1,0)` ← **PENTING: White, bukan Black!**
5. **Source Image:** None (akan dibuat procedural)

### **Step 2: Add DeathScreenEffect Component**
1. **Select DeathScreenUI**
2. **Add Component** → **DeathScreenEffect**

### **Step 3: Configure Vignette Settings**
```
=== FADE EFFECT ===
✓ Fade Overlay: Auto-assigned
• Fade Duration: 3.5s (lebih lama untuk atmosfer)
• Fade Delay: 0.5s

=== VIGNETTE EFFECT (Little Nightmares Style) ===
✓ Use Vignette Effect: TRUE ← **ENABLE INI!**
• Vignette Intensity: 1.5 (seberapa gelap pinggir)
• Vignette Smoothness: 0.8 (seberapa halus transisi)
• Vignette Center: (0.5, 0.5) (center layar)
• Vignette Speed: 1.2 (kecepatan closing in)

=== ATMOSPHERIC EFFECTS ===
✓ Reduce Audio On Fade: true
✓ Enable Slow Motion: true
✓ Enable Screen Shake: true
```

---

## 🎮 **CARA KERJA VIGNETTE EFFECT**

### **1. Texture Generation:**
Script otomatis membuat texture vignette 512x512:
- **Center:** Transparan (bisa lihat game)
- **Edges:** Hitam (gelap)
- **Gradient:** Smooth transition dari center ke edge

### **2. Animation Process:**
```
Progress 0.0: ░░░░░░░░░░ (texture invisible - alpha 0)
Progress 0.3: ██░░░░░░██ (edges mulai gelap)
Progress 0.6: ███░░░░███ (gelap masuk ke tengah)
Progress 1.0: ██████████ (seluruh layar gelap)
```

### **3. Easing & Speed:**
- **Fade Curve:** Smooth easing transition
- **Vignette Speed:** Kontrol seberapa cepat gelap "closing in"
- **Vignette Intensity:** Seberapa kuat kontras edge vs center

---

## 🎯 **TESTING & DEBUGGING**

### **Testing Methods:**
1. **Context Menu:**
   - Right-click Component → "Test Death Fade"
   - Right-click Component → "Recreate Vignette Texture"

2. **Debug GUI:** (kanan atas saat play)
   - Shows vignette progress
   - "Test Death Fade" button
   - "Recreate Vignette" button

3. **Console Logs:**
   - Vignette creation messages
   - Progress updates every 30 frames

### **Visual Debugging:**
- **Debug GUI** menampilkan:
  - Effect Type: "Vignette (Little Nightmares)"
  - Vignette Progress: 0.00 - 1.00
  - Vignette Intensity & Speed values

---

## ⚙️ **CUSTOMIZATION UNTUK DIFFERENT STYLES**

### **🌙 Little Nightmares (Current):**
```
• Vignette Intensity: 1.5
• Vignette Smoothness: 0.8
• Vignette Speed: 1.2
• Fade Duration: 3.5s
• Slow & atmospheric
```

### **👻 Horror Game (Faster):**
```
• Vignette Intensity: 2.0
• Vignette Smoothness: 0.6
• Vignette Speed: 1.5
• Fade Duration: 2.0s
• More aggressive closing
```

### **🎮 Action Game (Quick):**
```
• Use Vignette Effect: FALSE
• Standard fade to black
• Fade Duration: 1.0s
• No atmospheric effects
```

---

## 🎨 **TECHNICAL DETAILS**

### **Procedural Texture Generation:**
- **Size:** 512x512 texture dibuat runtime
- **Algorithm:** Distance dari center → alpha value
- **Performance:** Texture dibuat sekali saat Awake()
- **Memory:** ~1MB texture (acceptable untuk death effect)

### **Alpha Blending:**
```csharp
// Vignette formula:
distance = Vector2.Distance(pixelPos, center)
normalizedDistance = distance / maxDistance
vignetteValue = Clamp01(normalizedDistance * intensity)
alpha = Pow(vignetteValue, smoothness)
```

### **Animation Control:**
- **Time.unscaledDeltaTime:** Works even with Time.timeScale changes
- **AnimationCurve:** Smooth easing untuk natural feel
- **Coroutine:** Non-blocking animation system

---

## ✨ **HASIL AKHIR**

Saat Takau menyerang player:
1. 📳 **Screen shake** sekilas (impact feel)
2. ⏱️ **Slow motion** + audio fade (atmospheric)
3. 🌑 **Vignette closing:** Gelap dari pinggir masuk ke tengah
4. 🖤 **Full darkness:** Layar sepenuhnya gelap
5. 🔄 **Ready for restart** logic

**Perfect Little Nightmares atmosphere!** 🌙✨

**Setup:** Create UI → Add Component → Enable Vignette → Test!
**Result:** Authentic edge-to-center darkness effect! 🎯
