# Life Screen Effect - Setup dan Penggunaan Lengkap

## 🚀 Quick Setup (Metode Otomatis)

### 1. **Menggunakan LifeEffectSetupHelper**

Cara tercepat untuk setup:

1. **Buat GameObject baru** (atau gunakan existing GameObject)
2. **Attach script `LifeEffectSetupHelper`**
3. **Klik "Auto Setup Life Effect"** di context menu atau inspector
4. **Done!** Semua komponen akan dibuat otomatis

```csharp
// Di Inspector:
Auto Setup On Start: ✅ (centang ini untuk setup otomatis saat Start)
Canvas Name: "LifeEffectCanvas"
Overlay Name: "LifeOverlay" 
Canvas Sort Order: 999
```

### 2. **Verifikasi Setup**
Setelah auto setup, klik **"Validate Setup"** untuk memastikan semua komponen benar.

## 🔧 Manual Setup (Metode Manual)

Jika ingin setup manual atau memahami struktur:

### Step 1: Buat Canvas
```csharp
GameObject canvasGO = new GameObject("LifeEffectCanvas");
Canvas canvas = canvasGO.AddComponent<Canvas>();
canvas.renderMode = RenderMode.ScreenSpaceOverlay;
canvas.sortingOrder = 999; // Tinggi agar di atas UI lain
canvasGO.AddComponent<CanvasScaler>();
canvasGO.AddComponent<GraphicRaycaster>();
```

### Step 2: Buat Overlay Image
```csharp
GameObject overlayGO = new GameObject("LifeOverlay");
overlayGO.transform.SetParent(canvas.transform, false);
Image overlay = overlayGO.AddComponent<Image>();

// Setup RectTransform untuk full screen
RectTransform rect = overlay.rectTransform;
rect.anchorMin = Vector2.zero;
rect.anchorMax = Vector2.one;
rect.offsetMin = Vector2.zero;
rect.offsetMax = Vector2.zero;

// Setup Image
overlay.color = new Color(0, 0, 0, 0); // Transparan
overlay.raycastTarget = true;
```

### Step 3: Tambah CanvasGroup (Optional)
```csharp
CanvasGroup canvasGroup = canvas.gameObject.AddComponent<CanvasGroup>();
canvasGroup.alpha = 0f;
canvasGroup.interactable = false;
canvasGroup.blocksRaycasts = false;
```

### Step 4: Setup LifeScreenEffect Component
```csharp
// Attach ke GameObject manapun (biasanya GameManager)
LifeScreenEffect lifeEffect = gameObject.AddComponent<LifeScreenEffect>();

// Connect components (via Inspector atau code)
// lifeOverlay = overlay;
// canvasGroup = canvasGroup;
// mainCamera = Camera.main;
```

## ⚙️ Konfigurasi Settings

### Basic Settings
```csharp
[Header("Life Effect Timing")]
lifeFadeDuration = 2.5f;      // Total durasi effect
lifeDelay = 0.3f;             // Delay sebelum mulai
useTwoStageLife = false;      // Multi-stage effect

[Header("Visual Settings")]
useReverseVignetteEffect = true;  // Center-to-edge light spreading
lightIntensity = 1.8f;            // Intensitas cahaya
lightSpeed = 1.2f;                // Kecepatan spreading
lightCenter = Vector2(0.5f, 0.5f); // Pusat cahaya (screen center)

[Header("Debug")]
showDebug = true;             // Console logging
showProgressGUI = true;       // Debug GUI
```

### Advanced Settings
```csharp
[Header("Advanced Visual")]
lightSmoothness = 2.0f;           // Kelembutan gradien
firstStageLifeDuration = 1.0f;    // Durasi warm glow stage
firstStageLifeColor = Color(0.2f, 0.1f, 0.05f, 1f); // Warna warm

[Header("Audio")]
enableAmbientAudio = true;        // Enable audio
playAmbientImmediately = false;   // Play audio langsung atau delay

[Header("Screen Effects")]
enableScreenPulse = true;         // Gentle camera pulse
pulseDuration = 3.0f;             // Durasi pulse
pulseIntensity = 0.02f;           // Intensitas pulse
```

## 🎮 Cara Penggunaan

### 1. **Basic Usage - Trigger Life Effect**
```csharp
// Dapatkan reference
LifeScreenEffect lifeEffect = FindObjectOfType<LifeScreenEffect>();

// Trigger effect (hanya jika death effect sudah complete)
lifeEffect.TriggerLifeFade();
```

### 2. **Integration dengan Save System**
```csharp
// Di SaveManager.cs, saat respawn:
public void RespawnAtCheckpoint()
{
    // ... load position logic ...
    
    // Trigger life effect
    var lifeEffect = FindObjectOfType<LifeScreenEffect>();
    if (lifeEffect != null)
    {
        lifeEffect.TriggerLifeFade();
    }
}
```

### 3. **Integration dengan Death System**
```csharp
// Di PlayerDeathHandler.cs:
private void OnDeathSequenceComplete()
{
    // Trigger respawn via SaveManager
    saveManager.RespawnAtCheckpoint();
    
    // Life effect akan otomatis dipanggil oleh SaveManager
}
```

### 4. **Manual Testing**
```csharp
// Force test (skip prerequisites)
lifeEffect.ForceTestLifeFade();

// Test dengan death sequence
lifeEffect.TestDeathLifeSequence();

// Reset to initial state
lifeEffect.ResetLifeEffect();
```

## 🎯 Testing dan Debugging

### 1. **Keyboard Testing dengan LifeEffectTester**
Attach `LifeEffectTester.cs` ke GameObject:
- **L** = Test Life Effect
- **F** = Force Test (skip prerequisites)
- **R** = Reset Effect
- **GUI panel** muncul di kanan atas

### 2. **Built-in Debug GUI**
Enable `showProgressGUI = true`:
- Debug panel muncul di kiri atas
- Shows real-time progress, states, controls
- Buttons untuk semua test scenarios

### 3. **Context Menu Testing**
Right-click pada LifeScreenEffect component:
- "Test Life Fade"
- "Force Test Life Fade"  
- "Reset Life Effect"
- "Debug Component Setup"

### 4. **Console Debugging**
Enable `showDebug = true` untuk detailed logging:
```
★★★ LIFE FADE EFFECT TRIGGERED! ★★★
Starting rebirth fade from center to edges...
Life overlay prepared - starting from full darkness
Starting life fade coroutine...
Starting main life fade (center to edge)...
★★★ LIFE FADE COMPLETE! ★★★
```

## 🔄 Integration Patterns

### Pattern 1: GameManager Integration
```csharp
public class GameManager : MonoBehaviour
{
    [SerializeField] private LifeScreenEffect lifeEffect;
    [SerializeField] private DeathScreenEffect deathEffect;
    [SerializeField] private SaveManager saveManager;
    
    public void HandlePlayerDeath()
    {
        // 1. Trigger death effect
        deathEffect.TriggerDeathFade();
        
        // 2. Wait for death complete, then respawn
        StartCoroutine(WaitForDeathThenRespawn());
    }
    
    private IEnumerator WaitForDeathThenRespawn()
    {
        yield return new WaitUntil(() => deathEffect.IsDeathComplete);
        
        // 3. Load checkpoint
        saveManager.RespawnAtCheckpoint();
        
        // 4. Trigger life effect
        lifeEffect.TriggerLifeFade();
    }
}
```

### Pattern 2: Event-Based Integration
```csharp
public class DeathLifeController : MonoBehaviour
{
    [SerializeField] private LifeScreenEffect lifeEffect;
    
    private void OnEnable()
    {
        PlayerHealth.OnPlayerDied += HandlePlayerDeath;
        SaveManager.OnPlayerRespawned += HandlePlayerRespawn;
    }
    
    private void HandlePlayerRespawn()
    {
        lifeEffect.TriggerLifeFade();
    }
}
```

### Pattern 3: State Machine Integration
```csharp
public enum GameState { Playing, Dying, Dead, Respawning }

public class GameStateMachine : MonoBehaviour
{
    private GameState currentState;
    
    public void TransitionToRespawning()
    {
        currentState = GameState.Respawning;
        lifeEffect.TriggerLifeFade();
        
        // Wait for life effect complete
        StartCoroutine(WaitForRespawnComplete());
    }
    
    private IEnumerator WaitForRespawnComplete()
    {
        yield return new WaitUntil(() => lifeEffect.LifeFadeComplete);
        currentState = GameState.Playing;
    }
}
```

## 📱 Platform Considerations

### Mobile Optimization
```csharp
// Reduce effect complexity for mobile
lifeFadeDuration = 1.5f;     // Shorter duration
useReverseVignetteEffect = false; // Use simple fade
enableScreenPulse = false;   // Disable camera effects
```

### High-End Setup
```csharp
// More dramatic effects for powerful devices
lifeFadeDuration = 4.0f;
useTwoStageLife = true;
enableScreenPulse = true;
lightIntensity = 2.5f;
```

## 🚨 Troubleshooting Checklist

Jika effect tidak bekerja, cek:

1. **Canvas Setup**
   - [ ] Render Mode = Screen Space Overlay
   - [ ] Sort Order tinggi (999+)
   - [ ] CanvasGroup Alpha = 0 (start) → 1 (during effect) → 0 (end)

2. **Image Setup**
   - [ ] RectTransform covers full screen
   - [ ] Initial color = transparent black (0,0,0,0)
   - [ ] Raycast Target = enabled

3. **Component Connections**
   - [ ] lifeOverlay field assigned
   - [ ] canvasGroup field assigned (optional)
   - [ ] mainCamera assigned

4. **Prerequisites**
   - [ ] Death effect completed (atau gunakan ForceTest)
   - [ ] No other life effect running

5. **Common Issues**
   - [ ] GameObject stays active after effect
   - [ ] Only alpha changes, tidak SetActive(false)
   - [ ] Effect visible dari start sampai end

Dengan setup ini, Life Screen Effect siap untuk digunakan dalam production game! 🎉
