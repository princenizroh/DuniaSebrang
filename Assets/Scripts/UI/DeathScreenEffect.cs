using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DS.Data.Audio;

namespace DS
{
    /// <summary>
    /// Death screen effect inspired by Little Nightmares - smooth fade to black with atmospheric feeling
    /// </summary>
    public class DeathScreenEffect : MonoBehaviour
    {
        [Header("=== FADE EFFECT ===")]
        [Tooltip("Image component untuk fade overlay (biasanya full screen black image)")]
        [SerializeField] private Image fadeOverlay;

        [Tooltip("Durasi fade dari transparan ke hitam (detik)")]
        [SerializeField] private float fadeDuration = 3f;

        [Tooltip("Delay sebelum fade dimulai (detik)")]
        [SerializeField] private float fadeDelay = 0.2f;

        [Tooltip("Curve untuk fade animation (easing)")]
        [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("=== VIGNETTE EFFECT (Little Nightmares Style) ===")]
        [Tooltip("Enable vignette effect (darkness from edges to center)")]
        [SerializeField] private bool useVignetteEffect = true;

        [Tooltip("Vignette intensity (how strong the edge darkening is)")]
        [SerializeField] private float vignetteIntensity = 1.5f;

        [Tooltip("Vignette smoothness (how smooth the transition from edge to center)")]
        [SerializeField] private float vignetteSmoothness = 0f;

        [Tooltip("Center point of vignette (0.5,0.5 = screen center)")]
        [SerializeField] private Vector2 vignetteCenter = new Vector2(0.5f, 0.5f);

        [Tooltip("Vignette progression speed (how fast it closes in)")]
        [SerializeField] private float vignetteSpeed = 1.2f;

        [Header("=== ATMOSPHERIC EFFECTS ===")]
        [Tooltip("Reduce audio volume saat fade (atmosphere effect)")]
        [SerializeField] private bool reduceAudioOnFade = true;

        [Tooltip("Target volume saat fade selesai")]
        [SerializeField] private float targetAudioVolume = 0.1f;

        [Tooltip("Slow motion effect saat fade (optional)")]
        [SerializeField] private bool enableSlowMotion = true;

        [Tooltip("Target time scale saat fade")]
        [SerializeField] private float targetTimeScale = 0.5f;

        [Header("=== ADDITIONAL EFFECTS ===")]
        [Tooltip("Blur effect saat fade (optional)")]
        [SerializeField] private bool enableBlurEffect = true;

        [Tooltip("Vignette effect untuk atmosphere")]
        [SerializeField] private bool enableVignetteEffect = true;

        [Tooltip("Screen shake saat death trigger")]
        [SerializeField] private bool enableScreenShake = true;

        [Tooltip("Intensitas screen shake")]
        [SerializeField] private float shakeIntensity = 0.5f;

        [Tooltip("Durasi screen shake")]
        [SerializeField] private float shakeDuration = 0.8f;

        [Header("=== FADE COLORS ===")]
        [Tooltip("Warna fade overlay (biasanya hitam)")]
        [SerializeField] private Color fadeColor = Color.black;

        [Tooltip("Optional: Fade ke warna lain dulu sebelum hitam (e.g. red for blood effect)")]
        [SerializeField] private bool useTwoStageFade = false;

        [Tooltip("Warna pertama untuk two-stage fade")]
        [SerializeField] private Color firstStageColor = new Color(0.8f, 0.2f, 0.2f, 0.5f); // Dark red

        [Tooltip("Durasi first stage fade")]
        [SerializeField] private float firstStageDuration = 0.8f;

        [Header("=== AMBIENT AUDIO ===")]
        [Tooltip("Ambient audio to play during death fade (e.g. SFX_Ambient_Dead)")]
        [SerializeField] private AudioData ambientAudio;

        [Tooltip("Enable ambient audio during death fade")]
        [SerializeField] private bool enableAmbientAudio = true;

        [Tooltip("Fade in ambient audio duration")]
        [SerializeField] private float ambientFadeInDuration = 2f;

        [Tooltip("Play ambient audio immediately or wait for fade to start")]
        [SerializeField] private bool playAmbientImmediately = false;

        [Header("=== REFERENCES ===")]
        [Tooltip("Main camera untuk screen shake effect")]
        [SerializeField] private Camera mainCamera;

        [Tooltip("Canvas group untuk fade control")]
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("=== RESPAWN FADE ===")]
        [Tooltip("Duration for fade out after respawn (fade from black to clear)")]
        [SerializeField] private float fadeOutDuration = 10f;

        [Tooltip("Delay after death fade complete before triggering respawn")]
        [SerializeField] private float respawnDelay = 0f;

        [Tooltip("Curve for fade out animation")]
        [SerializeField] private AnimationCurve fadeOutCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("=== DEBUG ===")]
        [Tooltip("Show debug messages")]
        [SerializeField] private bool showDebug = true;

        [Tooltip("Show fade progress in GUI")]
        [SerializeField] private bool showProgressGUI = true;

        // Runtime variables
        private bool isFading = false;
        private bool fadeComplete = false;
        private bool isFadingOut = false;
        private float fadeStartTime;
        private Vector3 originalCameraPosition;
        private float originalAudioVolume;
        private float originalTimeScale;

        // Respawn integration
        private System.Action onRespawnTrigger;

        // Vignette effect variables
        private Material vignetteMaterial;
        private float currentVignetteProgress = 0f;

        // Ambient audio variables
        private AudioSource ambientAudioSource;
        private bool ambientAudioPlaying = false;
        private Coroutine ambientFadeCoroutine;

        // Coroutine references
        private Coroutine fadeCoroutine;
        private Coroutine shakeCoroutine;

        // Properties
        public bool IsFading => isFading;
        public bool FadeComplete => fadeComplete;
        public bool IsFadingOut => isFadingOut;
        public float FadeProgress { get; private set; }
        public float RespawnDelay => respawnDelay;

        private void Awake()
        {
            // Auto-assign components
            if (fadeOverlay == null)
                fadeOverlay = GetComponent<Image>();

            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();

            if (mainCamera == null)
                mainCamera = Camera.main;

            // Create vignette material if using vignette effect
            if (useVignetteEffect)
                CreateVignetteMaterial();

            // Setup ambient audio source
            SetupAmbientAudio();

            // Store original values
            originalTimeScale = Time.timeScale;
            originalAudioVolume = AudioListener.volume;

            if (mainCamera != null)
                originalCameraPosition = mainCamera.transform.localPosition;
        }

        private void Start()
        {
            // Initialize fade overlay as transparent
            InitializeFadeOverlay();

            if (showDebug)
            {
                Debug.Log("DeathScreenEffect initialized");
                ValidateSetup();
            }
        }

        private void InitializeFadeOverlay()
        {
            if (fadeOverlay != null)
            {
                if (useVignetteEffect)
                {
                    // For vignette effect, start with white color and 0 alpha
                    Color initialColor = Color.white;
                    initialColor.a = 0f;
                    fadeOverlay.color = initialColor;
                }
                else
                {
                    // For standard fade, start with fade color and 0 alpha
                    Color initialColor = fadeColor;
                    initialColor.a = 0f;
                    fadeOverlay.color = initialColor;

                    // Remove any sprite for standard fade
                    fadeOverlay.sprite = null;
                }

                fadeOverlay.gameObject.SetActive(true);
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
        }

        /// <summary>
        /// Main method to trigger death fade effect with respawn callback
        /// </summary>
        public void TriggerDeathFade(System.Action respawnCallback = null)
        {
            if (isFading || isFadingOut)
            {
                if (showDebug) Debug.LogWarning("DeathScreenEffect: Fade already in progress!");
                return;
            }

            // Store respawn callback
            onRespawnTrigger = respawnCallback;

            if (showDebug)
            {
                Debug.Log("★★★ DEATH FADE EFFECT TRIGGERED! ★★★");
                Debug.Log("Starting Little Nightmares style fade to black...");
            }

            // Start fade sequence
            fadeStartTime = Time.unscaledTime; // Use unscaled time in case we slow down time
            isFading = true;
            fadeComplete = false;
            isFadingOut = false;
            FadeProgress = 0f;

            // Start ambient audio if enabled and set to play immediately
            if (enableAmbientAudio && playAmbientImmediately)
                StartAmbientAudio();

            // Play death sound
            PlayDeathSound();

            // Start screen shake
            if (enableScreenShake)
                StartScreenShake();

            // Start fade coroutine
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);

            fadeCoroutine = StartCoroutine(FadeToBlackCoroutine());
        }

        private IEnumerator FadeToBlackCoroutine()
        {
            if (showDebug) Debug.Log("Starting fade to black coroutine...");

            // Initial delay
            if (fadeDelay > 0)
            {
                if (showDebug) Debug.Log($"Fade delay: {fadeDelay}s");
                yield return new WaitForSecondsRealtime(fadeDelay);
            }

            // Two-stage fade (optional blood effect first)
            if (useTwoStageFade)
            {
                yield return StartCoroutine(FirstStageFade());
            }

            // Main fade to black
            yield return StartCoroutine(MainFadeToBlack());

            // Fade complete
            OnFadeComplete();
        }

        private IEnumerator FirstStageFade()
        {
            if (showDebug) Debug.Log("Starting first stage fade (blood effect)...");

            float elapsed = 0f;
            Color startColor = fadeColor;
            startColor.a = 0f;

            while (elapsed < firstStageDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = elapsed / firstStageDuration;
                float curveValue = fadeCurve.Evaluate(progress);

                // Fade to first stage color
                Color currentColor = Color.Lerp(startColor, firstStageColor, curveValue);
                ApplyFadeColor(currentColor);

                // Update progress
                FadeProgress = progress * 0.3f; // First stage is 30% of total progress

                yield return null;
            }

            // Hold first stage briefly
            yield return new WaitForSecondsRealtime(0.3f);
        }

        private IEnumerator MainFadeToBlack()
        {
            if (showDebug) Debug.Log("Starting main fade to black...");

            // Start ambient audio if not set to play immediately
            if (enableAmbientAudio && !playAmbientImmediately)
                StartAmbientAudio();

            float elapsed = 0f;
            Color startColor = useTwoStageFade ? firstStageColor : fadeColor;
            if (!useTwoStageFade) startColor.a = 0f;

            Color targetColor = fadeColor;
            targetColor.a = 1f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = elapsed / fadeDuration;
                float curveValue = fadeCurve.Evaluate(progress);

                // Fade color
                Color currentColor = Color.Lerp(startColor, targetColor, curveValue);
                ApplyFadeColor(currentColor);

                // Atmospheric effects
                ApplyAtmosphericEffects(curveValue);

                // Update progress
                float totalProgress = useTwoStageFade ? 0.3f + (progress * 0.7f) : progress;
                FadeProgress = totalProgress;

                yield return null;
            }

            // Ensure final state
            ApplyFadeColor(targetColor);
            ApplyAtmosphericEffects(1f);
            FadeProgress = 1f;
        }

        private void ApplyFadeColor(Color color)
        {
            if (useVignetteEffect)
            {
                // Use vignette effect instead of simple fade
                ApplyVignetteEffect(color.a);
            }
            else
            {
                // Standard fade to black
                if (fadeOverlay != null)
                    fadeOverlay.color = color;

                if (canvasGroup != null)
                    canvasGroup.alpha = color.a;
            }
        }

        private void ApplyAtmosphericEffects(float progress)
        {
            // Audio volume reduction
            if (reduceAudioOnFade)
            {
                float currentVolume = Mathf.Lerp(originalAudioVolume, targetAudioVolume, progress);
                AudioListener.volume = currentVolume;
            }

            // Slow motion effect
            if (enableSlowMotion)
            {
                float currentTimeScale = Mathf.Lerp(originalTimeScale, targetTimeScale, progress);
                Time.timeScale = currentTimeScale;
            }

            // Additional atmosphere effects can be added here
            // - Blur effect
            // - Color grading
            // - Vignette
        }

        private void PlayDeathSound()
        {
            // Audio effects removed - focusing only on visual screen effects
            if (showDebug) Debug.Log("Visual-only death effect - no audio");
        }

        // Heartbeat sequence removed - visual effects only

        private void StartScreenShake()
        {
            if (mainCamera == null) return;

            if (shakeCoroutine != null)
                StopCoroutine(shakeCoroutine);

            shakeCoroutine = StartCoroutine(ScreenShakeCoroutine());
        }

        private IEnumerator ScreenShakeCoroutine()
        {
            if (showDebug) Debug.Log("Starting screen shake effect...");

            float elapsed = 0f;

            while (elapsed < shakeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = elapsed / shakeDuration;

                // Reduce shake intensity over time
                float currentIntensity = shakeIntensity * (1f - progress);

                // Random shake offset
                Vector3 randomOffset = Random.insideUnitSphere * currentIntensity;
                randomOffset.z = 0f; // Keep camera on same Z plane

                mainCamera.transform.localPosition = originalCameraPosition + randomOffset;

                yield return null;
            }

            // Restore original position
            mainCamera.transform.localPosition = originalCameraPosition;

            if (showDebug) Debug.Log("Screen shake completed");
        }

        private void OnFadeComplete()
        {
            isFading = false;
            fadeComplete = true;

            if (canvasGroup != null)
            {
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }

            if (showDebug)
            {
                Debug.Log("★★★ DEATH FADE COMPLETE! ★★★");
                Debug.Log("Player death screen effect finished - triggering respawn...");
            }

            // Wait for respawn delay, then trigger respawn
            StartCoroutine(TriggerRespawnAfterDelay());
        }

        /// <summary>
        /// Wait for respawn delay, then trigger the respawn callback
        /// </summary>
        private IEnumerator TriggerRespawnAfterDelay()
        {
            if (showDebug) Debug.Log($"Waiting {respawnDelay}s before triggering respawn...");

            yield return new WaitForSecondsRealtime(respawnDelay);

            if (showDebug) Debug.Log("★★★ TRIGGERING RESPAWN CALLBACK ★★★");

            // Call the respawn callback if provided
            onRespawnTrigger?.Invoke();

            // Notify that fade is complete (for external systems)
            OnDeathFadeComplete();
        }

        /// <summary>
        /// Event when fade is complete - override or subscribe to this
        /// </summary>
        protected virtual void OnDeathFadeComplete()
        {
            // This can be overridden or events can be subscribed to
            if (showDebug) Debug.Log("=== DEATH FADE COMPLETE - WAITING FOR RESPAWN CALLBACK ===");

            // DO NOT automatically load main menu or restart scene
            // The respawn callback should handle the actual respawn logic
        }

        /// <summary>
        /// Reset death screen effect to initial state
        /// </summary>
        public void ResetDeathEffect()
        {
            if (showDebug) Debug.Log("Resetting death screen effect...");

            // Stop all coroutines
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);
            if (shakeCoroutine != null)
                StopCoroutine(shakeCoroutine);

            // Stop ambient audio
            StopAmbientAudio();

            // Reset state
            isFading = false;
            fadeComplete = false;
            FadeProgress = 0f;

            // Reset visual
            InitializeFadeOverlay();

            // Reset camera position
            if (mainCamera != null)
                mainCamera.transform.localPosition = originalCameraPosition;

            // Reset audio and time
            AudioListener.volume = originalAudioVolume;
            Time.timeScale = originalTimeScale;

            if (showDebug) Debug.Log("Death effect reset completed");
        }

        /// <summary>
        /// Quick fade for testing
        /// </summary>
        [ContextMenu("Test Death Fade")]
        private void TestDeathFade()
        {
            TriggerDeathFade();
        }

        /// <summary>
        /// Reset for testing
        /// </summary>
        [ContextMenu("Reset Effect")]
        private void TestReset()
        {
            ResetDeathEffect();
        }

        /// <summary>
        /// Test vignette effect creation
        /// </summary>
        [ContextMenu("Recreate Vignette Texture")]
        private void TestRecreateVignette()
        {
            if (useVignetteEffect)
            {
                CreateVignetteTexture();
                Debug.Log("Vignette texture recreated for testing");
            }
            else
            {
                Debug.Log("Vignette effect is disabled. Enable 'Use Vignette Effect' first.");
            }
        }

        private void ValidateSetup()
        {
            if (fadeOverlay == null)
                Debug.LogWarning("DeathScreenEffect: No fade overlay Image assigned!");

            if (mainCamera == null)
                Debug.LogWarning("DeathScreenEffect: No main camera found for shake effect!");

            // Audio validation removed - visual effects only
        }

        /// <summary>
        /// Create vignette material for edge-to-center fade effect
        /// </summary>
        private void CreateVignetteMaterial()
        {
            if (fadeOverlay == null) return;

            // Use simple Image-based vignette effect (no custom shader needed)
            // We'll simulate the Little Nightmares vignette using alpha transitions
            CreateSimpleVignetteEffect();
        }

        /// <summary>
        /// Alternative vignette effect using multiple UI elements
        /// </summary>
        private void CreateSimpleVignetteEffect()
        {
            if (fadeOverlay == null) return;

            // Create a procedural vignette texture
            CreateVignetteTexture();
            currentVignetteProgress = 0f;

            if (showDebug) Debug.Log("Vignette effect initialized with procedural texture");
        }

        /// <summary>
        /// Create a vignette texture procedurally
        /// </summary>
        private void CreateVignetteTexture()
        {
            int textureSize = 512;
            Texture2D vignetteTexture = new Texture2D(textureSize, textureSize);

            Vector2 center = new Vector2(textureSize * vignetteCenter.x, textureSize * vignetteCenter.y);
            float maxDistance = Vector2.Distance(center, Vector2.zero);

            for (int x = 0; x < textureSize; x++)
            {
                for (int y = 0; y < textureSize; y++)
                {
                    Vector2 pixelPos = new Vector2(x, y);
                    float distance = Vector2.Distance(pixelPos, center);

                    // Normalize distance
                    float normalizedDistance = distance / maxDistance;

                    // Create vignette gradient (black at edges, less transparent at center)
                    float vignetteValue = Mathf.Clamp01(normalizedDistance * vignetteIntensity);
                    vignetteValue = Mathf.Pow(vignetteValue, vignetteSmoothness);

                    // FORCE MINIMUM DARKNESS - even center should be somewhat dark for total coverage
                    float minimumDarkness = 0.3f; // Base darkness even at center
                    vignetteValue = Mathf.Lerp(minimumDarkness, 1.0f, vignetteValue);

                    Color pixelColor = new Color(0f, 0f, 0f, vignetteValue);
                    vignetteTexture.SetPixel(x, y, pixelColor);
                }
            }

            vignetteTexture.Apply();

            // Apply texture to the image
            if (fadeOverlay != null)
            {
                Sprite vignetteSprite = Sprite.Create(vignetteTexture,
                    new Rect(0, 0, textureSize, textureSize),
                    new Vector2(0.5f, 0.5f));
                fadeOverlay.sprite = vignetteSprite;
            }

            if (showDebug) Debug.Log("Vignette texture created and applied with minimum darkness coverage");
        }

        /// <summary>
        /// Apply vignette effect - darkness spreading from edges to center
        /// </summary>
        private void ApplyVignetteEffect(float progress)
        {
            if (!useVignetteEffect || fadeOverlay == null) return;

            currentVignetteProgress = progress;

            // Calculate vignette alpha with easing curve
            float easedProgress = fadeCurve.Evaluate(progress);
            float vignetteAlpha = Mathf.Pow(easedProgress, 1f / vignetteSpeed);

            // Apply alpha to the vignette texture
            Color vignetteColor = Color.white; // White color so the texture shows properly
            vignetteColor.a = vignetteAlpha;
            fadeOverlay.color = vignetteColor;

            // Also apply to canvas group for additional control
            if (canvasGroup != null)
            {
                canvasGroup.alpha = vignetteAlpha;
            }

            if (showDebug && Time.frameCount % 30 == 0) // Log every 30 frames to avoid spam
            {
                Debug.Log($"Vignette Effect: Progress={progress:F2}, Alpha={vignetteAlpha:F2}");
            }
        }

        // Debug GUI with scroll support
        private Vector2 debugScrollPosition = Vector2.zero;

        /// <summary>
        /// Start playing ambient audio with fade-in effect
        /// </summary>
        private void StartAmbientAudio()
        {
            if (!enableAmbientAudio || ambientAudio == null || ambientAudioSource == null)
            {
                if (showDebug) Debug.Log("Ambient audio disabled or not configured");
                return;
            }

            if (ambientAudioPlaying)
            {
                if (showDebug) Debug.LogWarning("Ambient audio already playing");
                return;
            }

            // Start playing
            ambientAudioSource.Play();
            ambientAudioPlaying = true;

            // Start fade-in coroutine
            if (ambientFadeCoroutine != null)
                StopCoroutine(ambientFadeCoroutine);

            ambientFadeCoroutine = StartCoroutine(FadeInAmbientAudio());

            if (showDebug) Debug.Log($"Started ambient audio: {ambientAudio.AudioName}");
        }

        /// <summary>
        /// Stop ambient audio with fade-out effect
        /// </summary>
        private void StopAmbientAudio()
        {
            if (!ambientAudioPlaying || ambientAudioSource == null)
                return;

            // Stop fade-in if running
            if (ambientFadeCoroutine != null)
                StopCoroutine(ambientFadeCoroutine);

            // Start fade-out
            ambientFadeCoroutine = StartCoroutine(FadeOutAmbientAudio());
        }

        /// <summary>
        /// Fade in ambient audio over time
        /// </summary>
        private IEnumerator FadeInAmbientAudio()
        {
            if (ambientAudioSource == null) yield break;

            float elapsed = 0f;
            float targetVolume = ambientAudio.volume;
            ambientAudioSource.volume = 0f;

            while (elapsed < ambientFadeInDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = elapsed / ambientFadeInDuration;

                float currentVolume = Mathf.Lerp(0f, targetVolume, progress);
                ambientAudioSource.volume = currentVolume;

                yield return null;
            }

            ambientAudioSource.volume = targetVolume;

            if (showDebug) Debug.Log("Ambient audio fade-in completed");
        }

        /// <summary>
        /// Fade out and stop ambient audio
        /// </summary>
        private IEnumerator FadeOutAmbientAudio()
        {
            if (ambientAudioSource == null) yield break;

            float elapsed = 0f;
            float fadeDuration = 1f; // Quick fade out
            float startVolume = ambientAudioSource.volume;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = elapsed / fadeDuration;

                float currentVolume = Mathf.Lerp(startVolume, 0f, progress);
                ambientAudioSource.volume = currentVolume;

                yield return null;
            }

            // Stop playing and reset
            ambientAudioSource.Stop();
            ambientAudioSource.volume = 0f;
            ambientAudioPlaying = false;

            if (showDebug) Debug.Log("Ambient audio stopped and faded out");
        }

        /// <summary>
        /// Setup ambient audio source for death screen ambience
        /// </summary>
        private void SetupAmbientAudio()
        {
            if (!enableAmbientAudio || ambientAudio == null) return;

            // Get or create AudioSource component
            ambientAudioSource = GetComponent<AudioSource>();
            if (ambientAudioSource == null)
            {
                ambientAudioSource = gameObject.AddComponent<AudioSource>();
            }

            // Configure AudioSource with AudioData settings
            ConfigureAmbientAudioSource();

            if (showDebug) Debug.Log("Ambient audio source setup completed");
        }

        /// <summary>
        /// Configure the ambient audio source with AudioData properties
        /// </summary>
        private void ConfigureAmbientAudioSource()
        {
            if (ambientAudioSource == null || ambientAudio == null) return;

            ambientAudioSource.clip = ambientAudio.audioClip;
            ambientAudioSource.volume = 0f; // Start at 0, will fade in
            ambientAudioSource.loop = ambientAudio.loop;
            ambientAudioSource.playOnAwake = false;

            // 3D Audio settings
            ambientAudioSource.minDistance = ambientAudio.minDistance;
            ambientAudioSource.maxDistance = ambientAudio.maxDistance;

            // For ambient death audio, usually 2D is preferred
            ambientAudioSource.spatialBlend = 0f; // 2D audio

            if (showDebug) Debug.Log($"Ambient audio configured: {ambientAudio.AudioName}");
        }

        /// <summary>
        /// Trigger fade out after respawn (fade from black to clear)
        /// </summary>
        public void TriggerFadeOut()
        {
            if (isFadingOut)
            {
                if (showDebug) Debug.LogWarning("DeathScreenEffect: Fade out already in progress!");
                return;
            }

            if (!fadeComplete)
            {
                if (showDebug) Debug.LogWarning("DeathScreenEffect: Cannot fade out - fade to black not complete yet!");
                return;
            }

            if (showDebug)
            {
                Debug.Log("★★★ STARTING FADE OUT TO RESTORE GAMEPLAY ★★★");
            }

            isFadingOut = true;

            // Stop ambient audio
            StopAmbientAudio();

            // Start fade out coroutine
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);

            fadeCoroutine = StartCoroutine(FadeOutCoroutine());
        }

        /// <summary>
        /// Coroutine to handle fade out from black to clear
        /// </summary>
        private IEnumerator FadeOutCoroutine()
        {
            if (showDebug) Debug.Log("Starting fade out coroutine...");

            float elapsed = 0f;
            Color startColor = fadeColor;
            startColor.a = 1f; // Start fully opaque
            Color targetColor = fadeColor;
            targetColor.a = 0f; // End transparent

            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = elapsed / fadeOutDuration;

                // Apply fade out curve
                float curveProgress = fadeOutCurve.Evaluate(progress);

                // Update fade overlay
                if (fadeOverlay != null)
                {
                    Color currentColor = Color.Lerp(startColor, targetColor, curveProgress);
                    fadeOverlay.color = currentColor;
                }

                // Update canvas group alpha
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = Mathf.Lerp(1f, 0f, curveProgress);
                }

                // Restore audio volume gradually
                if (reduceAudioOnFade)
                {
                    float audioVolume = Mathf.Lerp(targetAudioVolume, originalAudioVolume, curveProgress);
                    AudioListener.volume = audioVolume;
                }

                // Restore time scale gradually
                if (enableSlowMotion)
                {
                    float timeScale = Mathf.Lerp(targetTimeScale, originalTimeScale, curveProgress);
                    Time.timeScale = timeScale;
                }

                // Update fade progress for external monitoring
                FadeProgress = 1f - curveProgress; // Inverted for fade out

                yield return null;
            }

            // Ensure final state
            OnFadeOutComplete();
        }

        /// <summary>
        /// Called when fade out is complete
        /// </summary>
        private void OnFadeOutComplete()
        {
            if (showDebug) Debug.Log("★★★ FADE OUT COMPLETE - GAMEPLAY RESTORED ★★★");

            // Reset all states
            isFading = false;
            fadeComplete = false;
            isFadingOut = false;
            FadeProgress = 0f;

            // Restore full transparency
            if (fadeOverlay != null)
            {
                Color clearColor = fadeColor;
                clearColor.a = 0f;
                fadeOverlay.color = clearColor;
            }

            // Restore canvas group
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }

            // Fully restore audio and time
            AudioListener.volume = originalAudioVolume;
            Time.timeScale = originalTimeScale;

            // Reset camera position (in case shake is still active)
            if (mainCamera != null)
                mainCamera.transform.localPosition = originalCameraPosition;

            if (showDebug) Debug.Log("Death screen effect fully reset - ready for normal gameplay");
        }

        /// <summary>
        /// Test fade out for testing
        /// </summary>
        [ContextMenu("Test Fade Out")]
        private void TestFadeOut()
        {
            TriggerFadeOut();
        }
        
#if UNITY_EDITOR
        private void OnGUI()
        {
            if (!showProgressGUI) return;
            
            // Fixed area for debug GUI
            Rect debugArea = new Rect(Screen.width - 420, 10, 400, Mathf.Min(Screen.height - 50, 500));
            GUILayout.BeginArea(debugArea);
            
            // Begin scroll view to ensure all content is accessible
            debugScrollPosition = GUILayout.BeginScrollView(debugScrollPosition, false, true, 
                GUILayout.Width(380), GUILayout.ExpandHeight(true));
            
            // Use larger font and better styling
            GUIStyle headerStyle = new GUIStyle(GUI.skin.label);
            headerStyle.fontSize = 14;
            headerStyle.fontStyle = FontStyle.Bold;
            
            GUIStyle normalStyle = new GUIStyle(GUI.skin.label);
            normalStyle.fontSize = 12;
            
            GUILayout.Label("=== DEATH SCREEN EFFECT DEBUG ===", headerStyle);
            GUILayout.Space(5);
            
            GUILayout.Label($"Effect Type: {(useVignetteEffect ? "Vignette (Little Nightmares)" : "Standard Fade")}", normalStyle);
            GUILayout.Label($"Is Fading In: {isFading}", normalStyle);
            GUILayout.Label($"Is Fading Out: {isFadingOut}", normalStyle);
            GUILayout.Label($"Fade Complete: {fadeComplete}", normalStyle);
            GUILayout.Label($"Fade Progress: {FadeProgress:P1}", normalStyle);
            
            if (useVignetteEffect)
            {
                GUILayout.Space(2);
                GUILayout.Label("--- Vignette Settings ---", headerStyle);
                GUILayout.Label($"Progress: {currentVignetteProgress:F2} | Intensity: {vignetteIntensity:F1} | Speed: {vignetteSpeed:F1}", normalStyle);
                GUILayout.Label($"Center: ({vignetteCenter.x:F1}, {vignetteCenter.y:F1})", normalStyle);
            }
            
            if (isFading)
            {
                GUILayout.Space(2);
                GUILayout.Label("--- Real-time Info ---", headerStyle);
                float elapsed = Time.unscaledTime - fadeStartTime;
                GUILayout.Label($"Time: {elapsed:F1}s | Vol: {AudioListener.volume:F2} | Scale: {Time.timeScale:F2}", normalStyle);
            }
            
            GUILayout.Space(8);
            GUILayout.Label("--- Controls ---", headerStyle);
            
            if (GUILayout.Button("Test Death Fade", GUILayout.Height(30)))
            {
                TriggerDeathFade();
            }
            
            if (GUILayout.Button("Test Fade Out", GUILayout.Height(30)))
            {
                TriggerFadeOut();
            }
            
            if (GUILayout.Button("Reset Effect", GUILayout.Height(30)))
            {
                ResetDeathEffect();
            }
            
            // Vignette-specific button (only show when vignette is enabled)
            if (useVignetteEffect)
            {
                if (GUILayout.Button("Recreate Vignette", GUILayout.Height(30)))
                {
                    CreateVignetteTexture();
                }
            }
            
            // End scroll view and area
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }
#endif
    }
}
