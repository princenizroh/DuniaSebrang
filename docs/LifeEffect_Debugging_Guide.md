# Life Screen Effect - Debugging dan Troubleshooting Guide

## 📋 Daftar Masalah Umum dan Solusi

### 1. **GameObject Menjadi Tidak Aktif Setelah Effect**

**Masalah**: Overlay GameObject menjadi tidak aktif setelah life effect selesai.

**Solusi**:
- Pastikan method `OnLifeFadeComplete()` TIDAK memanggil `SetActive(false)`
- Effect harus berakhir dengan `alpha = 0` (transparan) bukan `SetActive(false)`
- GameObject harus tetap aktif untuk digunakan kembali

```csharp
// ❌ SALAH - jangan lakukan ini
lifeOverlay.gameObject.SetActive(false);

// ✅ BENAR - lakukan ini
Color transparentColor = Color.black;
transparentColor.a = 0f;
lifeOverlay.color = transparentColor;
```

### 2. **Effect Tidak Terlihat**

**Gejala**: Effect dipanggil tapi tidak ada perubahan visual.

**Penyebab dan Solusi**:

#### A. Canvas Setup
```csharp
// Pastikan Canvas menggunakan:
- Render Mode: Screen Space - Overlay
- Sort Order: 999 (tinggi untuk berada di atas)
- Canvas Group Alpha: 1.0
```

#### B. Image Component
```csharp
// Pastikan Image menggunakan:
- Color: Black (0,0,0,1)
- RectTransform: Anchors = Full Screen
- Raycast Target: Enabled (untuk blocking input)
```

#### C. Hierarchy Check
```
Canvas (Screen Space - Overlay)
├── LifeEffectPanel
    ├── LifeOverlay (Image Component)
    └── CanvasGroup (optional)
```

### 3. **Reverse Vignette Tidak Bekerja**

**Masalah**: Mode reverse vignette tidak menampilkan efek center-to-edge.

**Solusi**:
1. Enable "Use Reverse Vignette Effect" di inspector
2. Pastikan `lightIntensity` dan `lightSpeed` diatur dengan benar
3. Test dengan `lightCenter` di (0.5, 0.5) untuk center screen

```csharp
// Setting yang direkomendasikan:
useReverseVignetteEffect = true;
lightIntensity = 2.0f;
lightSpeed = 1.5f;
lightCenter = new Vector2(0.5f, 0.5f);
lightSmoothness = 2.0f;
```

### 4. **Effect Terlalu Cepat atau Lambat**

**Masalah**: Timing effect tidak sesuai.

**Solusi - Sesuaikan Duration**:
```csharp
// Untuk effect yang lebih dramatis
lifeFadeDuration = 3.0f;      // Total durasi
lifeDelay = 0.5f;             // Delay sebelum mulai
firstStageLifeDuration = 1.0f; // Durasi warm glow (jika enabled)
```

### 5. **Audio Tidak Berfungsi**

**Masalah**: Ambient audio atau sound effect tidak terdengar.

**Solusi**:
```csharp
// Setup audio component:
enableAmbientAudio = true;
playAmbientImmediately = true;
// Assign AudioSource component
// Assign AudioClip untuk ambientLifeAudio
```

## 🔧 Testing Tools

### 1. **Built-in Debug GUI**
- Enable `showProgressGUI = true` di inspector
- GUI akan muncul di kiri atas dengan semua kontrol testing

### 2. **Context Menu Testing**
Klik kanan pada component di inspector:
- "Test Life Fade" - Test normal
- "Force Test Life Fade" - Test tanpa cek prerequisites  
- "Reset Life Effect" - Reset ke state awal
- "Debug Component Setup" - Cek setup component

### 3. **LifeEffectTester Script**
Attach script `LifeEffectTester.cs` untuk testing keyboard:
- **L** = Test Life Effect
- **F** = Force Test
- **R** = Reset
- **GUI** = Kontrol visual di kanan atas

### 4. **Manual Testing Sequence**

#### Step 1: Basic Test
```csharp
1. Reset effect
2. Force test life fade
3. Amati: screen harus mulai hitam → transparan
4. Durasi sesuai lifeFadeDuration
```

#### Step 2: Death-Life Sequence
```csharp
1. Reset effect
2. Test death-life sequence
3. Amati: death effect → life effect
4. Transisi harus smooth
```

#### Step 3: Reverse Vignette Test
```csharp
1. Enable "Use Reverse Vignette Effect"
2. Reset dan test
3. Amati: light spreading dari center ke edge
```

## 🚨 Common Debug Messages

### Normal Flow
```
★★★ LIFE FADE EFFECT TRIGGERED! ★★★
Starting rebirth fade from center to edges...
Life overlay prepared - starting from full darkness
Starting life fade coroutine...
Starting main life fade (center to edge)...
★★★ LIFE FADE COMPLETE! ★★★
```

### Error Messages
```
❌ "LifeScreenEffect: No life overlay Image assigned!"
   → Assign Image component ke lifeOverlay field

❌ "Life effect not ready - death effect must complete first"
   → Gunakan ForceTestLifeFade() atau complete death effect dulu

❌ "No Canvas found in parent!"
   → Pastikan Image ada dalam Canvas
```

## 📐 Recommended Settings

### Basic Setup
```csharp
// Timing
lifeFadeDuration = 2.5f;
lifeDelay = 0.3f;

// Visual
useReverseVignetteEffect = true;
lightIntensity = 1.8f;
lightSpeed = 1.2f;
lightCenter = Vector2(0.5f, 0.5f);

// Debug
showDebug = true;
showProgressGUI = true;
```

### Little Nightmares Style
```csharp
// Slower, more atmospheric
lifeFadeDuration = 4.0f;
lifeDelay = 0.8f;
useTwoStageLife = true;
firstStageLifeDuration = 1.5f;
firstStageLifeColor = new Color(0.2f, 0.1f, 0.05f, 1f); // Dark warm
```

### Fast/Arcade Style  
```csharp
// Quick respawn
lifeFadeDuration = 1.0f;
lifeDelay = 0.1f;
useTwoStageLife = false;
```

## 🔍 Debugging Checklist

### Pre-Test Checks
- [ ] Canvas setup (Screen Space Overlay, High Sort Order)
- [ ] Image assigned to lifeOverlay field
- [ ] RectTransform covers full screen
- [ ] Initial color is transparent black (0,0,0,0)

### During Test
- [ ] Effect starts immediately when triggered
- [ ] Screen starts black (if was transparent before)
- [ ] Light/transparency spreads from center
- [ ] Duration matches lifeFadeDuration setting
- [ ] Audio plays (if enabled)
- [ ] No console errors

### Post-Test
- [ ] Screen fully transparent when complete
- [ ] GameObject remains active
- [ ] Can trigger effect again
- [ ] No lingering visual artifacts

## 🛠️ Advanced Debugging

### Enable Detailed Logging
```csharp
// Di LifeScreenEffect inspector
showDebug = true;
```

### Frame-by-Frame Analysis
```csharp
// Temporary add ini ke ApplyLifeColor() untuk detailed tracking
if (Time.frameCount % 10 == 0) // Every 10 frames
    Debug.Log($"Frame {Time.frameCount}: Alpha={color.a:F3}, Progress={lifeProgress:F3}");
```

### Component State Inspection
```csharp
// Use context menu "Debug Component Setup"
// atau manual check di Update():
void Update() {
    if (Input.GetKeyDown(KeyCode.I)) {
        Debug.Log($"Overlay: Active={lifeOverlay.gameObject.activeSelf}, Color={lifeOverlay.color}");
        Debug.Log($"Canvas: Alpha={canvasGroup?.alpha}");
        Debug.Log($"Effect: Fading={isLifeFading}, Progress={LifeProgress}");
    }
}
```

Dengan panduan ini, Anda seharusnya dapat mengidentifikasi dan memperbaiki masalah apapun dengan Life Screen Effect!
