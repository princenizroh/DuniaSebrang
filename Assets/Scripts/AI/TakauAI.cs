using UnityEngine;
using UnityEngine.AI;
namespace DS
{
    public class TakauAI : MonoBehaviour
    {
        [field: Header("=== AI MODE & ANIMATION ===")]
        [field: SerializeField] public MoveMode moveMode { get; private set; }
        private Animator animator;
        private Rigidbody rb;

        [field: Header("=== BASIC MOVEMENT ===")]
        [field: SerializeField] public float chaseSpeed { get; private set; } = 5f;
        [field: SerializeField] public float maxTimeChasing { get; private set; } = 5f;
        [field: SerializeField] public float maxTimeWaiting { get; private set; } = 3f;
        [Tooltip("Radius dimana player BENAR-BENAR MATI (death zone) - lebih kecil dari attack range")]
        [field: SerializeField] public float radiusHit { get; private set; } = 1.5f;

        [field: Header("=== ACCELERATION SETTINGS ===")]
        [Tooltip("Kecepatan awal saat mulai chase (akan naik ke chaseSpeed)")]
        [field: SerializeField] public float chaseStartSpeed { get; private set; } = 1f;
        [Tooltip("Waktu untuk mencapai kecepatan penuh saat chase")]
        [field: SerializeField] public float chaseAccelerationTime { get; private set; } = 2f;
        [Tooltip("Kecepatan awal saat mulai charge (akan naik ke chargeSpeed)")]
        [field: SerializeField] public float chargeStartSpeed { get; private set; } = 15f;
        [Tooltip("Waktu untuk mencapai kecepatan penuh saat charge")]
        [field: SerializeField] public float chargeAccelerationTime { get; private set; } = 0.8f;

        [field: Header("=== CHASE BEHAVIOR ===")]
        [field: SerializeField] public float loseTargetDistance { get; private set; } = 15f;
        [field: SerializeField] public float minChaseDistance { get; private set; } = 5f;
        [Tooltip("Minimum distance untuk chase detection - mencegah overlap dengan attack")]
        [field: SerializeField] public float minChaseDetectionDistance { get; private set; } = 6f;
        [field: SerializeField] public bool showChaseDebug { get; private set; } = true;

        [field: Header("=== CHARGE SYSTEM ===")]
        [field: SerializeField] public float chargeSpeed { get; private set; } = 50f;
        [field: SerializeField] public float chargeSearchTime { get; private set; } = 4f;
        [field: SerializeField] public float chargeCooldown { get; private set; } = 8f;
        [field: SerializeField] public float chargeMinDistance { get; private set; } = 15f;

        [field: Header("=== CHARGE VISION SETTINGS ===")]
        [field: SerializeField] public float chargeForwardVisionAngle { get; private set; } = 30f;
        [field: SerializeField] public float chargeForwardVisionBonus { get; private set; } = 18f;
        [Tooltip("Vision bonus saat mulai charge search (akan berkembang ke Max)")]
        [field: SerializeField] public float chargeSearchVisionBonusStart { get; private set; } = 18.5f;
        [Tooltip("Vision bonus maksimum setelah 4 detik charge search")]
        [field: SerializeField] public float chargeSearchVisionBonusMax { get; private set; } = 30f;
        [Tooltip("Runtime value - akan berubah selama charge search")]
        [field: SerializeField] private float currentChargeSearchVisionBonus;

        [field: Header("=== ATTACK SYSTEM ===")]
        [Tooltip("Range dimana Takau bisa mulai attack animation (TIDAK langsung membunuh)")]
        [field: SerializeField] public float attackRange { get; private set; } = 3f;
        [Tooltip("Angle untuk attack - player harus di depan Takau")]
        [field: SerializeField] public float attackAngle { get; private set; } = 45f;
        [Tooltip("Bonus range untuk attack jika player di forward vision")]
        [field: SerializeField] public float attackForwardBonus { get; private set; } = 2f;
        [Tooltip("Cooldown attack dalam detik")]
        [field: SerializeField] public float attackCooldown { get; private set; } = 2f;
        [Tooltip("Durasi attack animation")]
        [field: SerializeField] public float attackDuration { get; private set; } = 1.5f;

        [field: Header("=== ROTATION SETTINGS ===")]
        [field: SerializeField] public float rotationSpeed { get; private set; } = 120f;
        [field: SerializeField] public bool useCustomRotation { get; private set; } = true;
        [field: SerializeField] public float rotationSmoothness { get; private set; } = 0.1f;

        [field: Header("=== NAVMESH & TARGET ===")]
        [field: SerializeField] public NavMeshAgent agent { get; private set; }
        [field: SerializeField] public Transform currentTarget { get; private set; }

        [field: Header("=== FIELD OF VIEW ===")]
        [field: SerializeField] public float viewRadius { get; private set; } = 7f;
        [Tooltip("Bonus range untuk forward vision cone")]
        [field: SerializeField] public float forwardVisionBonus { get; private set; } = 7f;
        [field: SerializeField] public float forwardVisionAngle { get; private set; } = 60f;
        [field: SerializeField] public LayerMask TargetMask { get; private set; }
        [field: SerializeField] public LayerMask ObstacleMask { get; private set; }

        [field: Header("=== RUNTIME STATUS (READ ONLY) ===")]
        [field: SerializeField] public bool isDetectTarget { get; private set; }
        [field: SerializeField] private bool isHit;
        [field: SerializeField] private float currentTimeChasing, currentTimeWaiting;
        [field: SerializeField] private float currentChargeSearchTime, lastChargeTime;
        [field: SerializeField] private Vector3 chargeTargetPosition;
        [field: SerializeField] private bool isCharging;
        [field: SerializeField] private float currentChaseAccelTime;
        [field: SerializeField] private float currentChargeAccelTime;
        [field: SerializeField] private float currentEffectiveSpeed;

        // === ATTACK TRACKING ===
        [field: SerializeField] private float lastAttackTime;
        public float LastAttackTime => lastAttackTime;
        [field: SerializeField] private float currentAttackTime;
        [field: SerializeField] private bool isAttacking;

        private void Awake()
        {
            animator = GetComponentInChildren<Animator>();
            rb = GetComponent<Rigidbody>();
        }
        private void Start()
        {
            if (agent == null)
                agent = GetComponent<NavMeshAgent>();
            if (agent.stoppingDistance < 0.5f)
                agent.stoppingDistance = 0.5f;

            agent.acceleration = 15f; // Default acceleration untuk NavMeshAgent

            if (useCustomRotation)
            {
                agent.updateRotation = false; // Disable NavMesh rotation
                agent.angularSpeed = 0; // Disable NavMesh angular speed
            }
            else
            {
                agent.updateRotation = true;
                agent.angularSpeed = rotationSpeed; // Use NavMesh rotation with custom speed
            }

            // Initialize charge search vision with starting value
            currentChargeSearchVisionBonus = chargeSearchVisionBonusStart;

            // FORCE CORRECT VALUES - Override Inspector values if they're wrong
            if (chargeSearchVisionBonusMax < chargeForwardVisionBonus)
            {
                Debug.LogWarning($"ChargeSearchVisionBonusMax ({chargeSearchVisionBonusMax}) is less than ChargeForwardVisionBonus ({chargeForwardVisionBonus})! Fixing...");
                chargeSearchVisionBonusMax = 30f; // Force correct value
                Debug.Log($"ChargeSearchVisionBonusMax corrected to: {chargeSearchVisionBonusMax}");
            }

            // Initialize charge system
            lastChargeTime = -chargeCooldown; // Allow immediate charge

            moveMode = MoveMode.wait;

            // Debug initial values
            Debug.Log($"=== INITIAL PARAMETER VALUES ===");
            Debug.Log($"chargeForwardVisionBonus: {chargeForwardVisionBonus}");
            Debug.Log($"chargeSearchVisionBonusStart: {chargeSearchVisionBonusStart}");
            Debug.Log($"chargeSearchVisionBonusMax: {chargeSearchVisionBonusMax}");
            Debug.Log($"Expected ranges - Charge: {viewRadius + chargeForwardVisionBonus:F1}m, ChargeSearch: {viewRadius + chargeSearchVisionBonusStart:F1}m-{viewRadius + chargeSearchVisionBonusMax:F1}m");
        }

        private void Update()
        {
            if (moveMode == MoveMode.dying) 
            {
                animator.Play("Dying");
                return;
            }
            switch (moveMode)
            {
                case MoveMode.chase:
                    Chasing();
                    animator.Play("Run");
                    break;
                case MoveMode.wait:
                    Waiting();
                    animator.Play("Roaming");
                    break;
                case MoveMode.charge:
                    Charging();
                    animator.Play("Run");
                    break;
                case MoveMode.chargeSearch:
                    ChargeSearching();
                    animator.Play("Roaming");
                    break;
                case MoveMode.attack:
                    Attacking();
                    animator.Play("Swiping");
                    break;
                case MoveMode.dying:
                    animator.Play("Dying");
                    break;
            }

            FieldOfView();
            HandleRotation();
        }

        private void Chasing()
        {
            if (moveMode == MoveMode.dying) return;
            if (currentTarget == null) // Jika tidak ada target, kembali ke wait
            {
                if (showChaseDebug) Debug.Log("Takau: No target found, switching to wait");
                SwitchMoveMode(MoveMode.wait);
                return;
            }

            float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);

            // ACCELERATION SYSTEM untuk Chase menggunakan NavMeshAgent
            currentChaseAccelTime += Time.deltaTime;
            float accelProgress = Mathf.Clamp01(currentChaseAccelTime / chaseAccelerationTime);
            float targetSpeed = Mathf.Lerp(chaseStartSpeed, chaseSpeed, accelProgress);
            currentEffectiveSpeed = targetSpeed;

            // Set NavMeshAgent speed dan acceleration
            agent.speed = targetSpeed;
            agent.acceleration = Mathf.Lerp(8f, 15f, accelProgress); // Acceleration juga meningkat

            if (showChaseDebug)
            {
                Debug.Log($"Takau chasing: Distance = {distanceToTarget:F2}m, Speed = {agent.speed:F1}m/s, Accel = {agent.acceleration:F1}m/s² ({accelProgress * 100:F0}% progress)");
            }

            if (distanceToTarget > loseTargetDistance) // Jika target terlalu jauh, hentikan chase
            {
                if (showChaseDebug) Debug.Log("Takau: Target too far away, losing chase");
                SwitchMoveMode(MoveMode.wait);
                return;
            }

            if (distanceToTarget <= radiusHit) // Jika SANGAT dekat (dalam death zone)
            {
                if (!isHit)
                {
                    Debug.Log("Takau caught the player in DEATH ZONE - Game Over!");
                    isHit = true;
                    OnPlayerCaught(); // Immediate game over
                }
                return;
            }

            if (distanceToTarget <= attackRange) // Jika dalam attack range (tapi belum death zone)
            {
                if (showChaseDebug) Debug.Log($"Player in attack range ({distanceToTarget:F1}m <= {attackRange:F1}m) but outside death zone ({radiusHit:F1}m)");

                // Check if attack conditions are met
                if (CheckAttackConditions())
                {
                    // Switch to attack mode instead of immediate game over
                    float timeSinceLastAttack = Time.time - lastAttackTime;
                    if (timeSinceLastAttack >= attackCooldown)
                    {
                        SwitchMoveMode(MoveMode.attack);
                        return;
                    }
                    else
                    {
                        if (showChaseDebug) Debug.Log($"Attack on cooldown: {attackCooldown - timeSinceLastAttack:F1}s remaining");
                    }
                }
                else
                {
                    if (showChaseDebug) Debug.Log("Player in attack range but not meeting attack conditions (angle/LOS)");
                }
            }

            if (distanceToTarget <= minChaseDistance)  // Jika sudah dekat tapi belum hit, perlambat untuk lebih menakutkan
            {
                agent.speed = targetSpeed * 0.7f; // Perlambat 30% dari speed saat ini
                agent.acceleration = 8f; // Lebih lambat acceleration saat mendekati
                if (showChaseDebug) Debug.Log($"Takau: Close to target, slowing down to {agent.speed:F1}m/s");
            }

            agent.destination = currentTarget.position; // Set destination (NavMesh will handle pathfinding)
            if (currentTimeChasing > maxTimeChasing)  // Chase timeout logic - jika chase terlalu lama tanpa hasil
            {
                if (showChaseDebug) Debug.Log("Takau: Chase timeout, switching to wait");
                SwitchMoveMode(MoveMode.wait);
            }
            else if (!isDetectTarget)
            {
                currentTimeChasing += Time.deltaTime; // Masih chase tapi target tidak terdeteksi FOV (mungkin di belakang obstacle)
                if (showChaseDebug) Debug.Log($"Takau: Target not in FOV, chase time: {currentTimeChasing:F1}s");
            }
            else if (isDetectTarget)
            {
                currentTimeChasing = 0; // Reset timer jika target masih terlihat
            }
        }

        private void Waiting()
        {
            if (moveMode == MoveMode.dying) return;
            agent.destination = transform.position;
            agent.speed = 0;

            if (showChaseDebug)
            {
                Debug.Log($"Takau waiting... Time: {currentTimeWaiting:F1}s / {maxTimeWaiting:F1}s - Detect: {isDetectTarget}");
            }

            if (isDetectTarget && currentTarget != null) // PRIORITY: Jika detect target, langsung switch ke chase (jangan tunggu timer)
            {
                if (showChaseDebug) Debug.Log("Takau: Target detected during wait! Switching to chase immediately.");
                SwitchMoveMode(MoveMode.chase);
                return;
            }

            if (currentTimeWaiting > maxTimeWaiting)  // Timer logic hanya jalan jika tidak detect target
            {
                if (showChaseDebug) Debug.Log("Takau: Wait time finished, ready to hunt again");

                isHit = false;

                // Check if charge is available (cooldown finished)
                float timeSinceLastCharge = Time.time - lastChargeTime;
                bool chargeReady = timeSinceLastCharge >= chargeCooldown;

                if (chargeReady)
                {
                    if (showChaseDebug) Debug.Log("Takau: Charge available, entering charge search mode");
                    SwitchMoveMode(MoveMode.chargeSearch);
                }
                else
                {
                    if (showChaseDebug) Debug.Log($"Takau: Charge on cooldown ({chargeCooldown - timeSinceLastCharge:F1}s remaining)");
                    currentTimeWaiting = 0; // Reset wait timer
                }
            }
            else
            {
                currentTimeWaiting += Time.deltaTime;
            }
        }

        private void ChargeSearching()
        {
            // Gradually expand vision cone during charge search
            float searchProgress = currentChargeSearchTime / chargeSearchTime; // 0 to 1
            currentChargeSearchVisionBonus = Mathf.Lerp(chargeSearchVisionBonusStart, chargeSearchVisionBonusMax, searchProgress);

            // Rotate continuously to scan for targets during charge search
            float searchRotationSpeed = 90f; // 90 degrees per second
            transform.Rotate(0, searchRotationSpeed * Time.deltaTime, 0);

            agent.destination = transform.position;
            agent.speed = 0;

            if (showChaseDebug)
            {
                float expansionProgress = (currentChargeSearchTime / chargeSearchTime) * 100f;
                Debug.Log($"Takau charge searching... Time: {currentChargeSearchTime:F1}s / {chargeSearchTime:F1}s, Vision: {currentChargeSearchVisionBonus:F1}m ({expansionProgress:F0}% expanded)");
            }

            // NOTE: Charge detection is now handled in FieldOfView() method with extended range
            // FieldOfView() will automatically use currentChargeSearchVisionBonus during this mode

            // Timeout - return to normal behavior
            if (currentChargeSearchTime > chargeSearchTime)
            {
                if (showChaseDebug) Debug.Log("Takau: Charge search timeout, returning to wait");

                // Reset vision bonus when exiting search mode
                currentChargeSearchVisionBonus = chargeSearchVisionBonusStart;

                // If target is in normal vision during search timeout, switch to chase
                if (isDetectTarget && currentTarget != null)
                {
                    SwitchMoveMode(MoveMode.chase);
                }
                else
                {
                    SwitchMoveMode(MoveMode.wait);
                }
            }
            else
            {
                currentChargeSearchTime += Time.deltaTime;
            }
        }
        private void Attacking()
        {
            if (moveMode == MoveMode.dying) return;
            if (currentTarget == null)
            {
                if (showChaseDebug) Debug.Log("Takau: No target during attack, switching to wait");
                SwitchMoveMode(MoveMode.wait);
                return;
            }

            // Stop movement during attack
            agent.destination = transform.position;
            agent.speed = 0;

            currentAttackTime += Time.deltaTime;

            if (showChaseDebug)
            {
                Debug.Log($"Takau attacking! Time: {currentAttackTime:F1}s / {attackDuration:F1}s");
            }

            // Attack finished
            if (currentAttackTime >= attackDuration)
            {
                if (showChaseDebug) Debug.Log("Takau: Attack finished");

                // Execute attack effect
                OnPlayerAttacked();

                // Check if player is still close for another attack or if we should chase
                if (CheckAttackConditions())
                {
                    // Still in attack range, check cooldown
                    float timeSinceLastAttack = Time.time - lastAttackTime;
                    if (timeSinceLastAttack >= attackCooldown)
                    {
                        // Attack again
                        if (showChaseDebug) Debug.Log("Takau: Continuous attack - player still in range!");
                        lastAttackTime = Time.time;
                        currentAttackTime = 0;
                        return; // Stay in attack mode
                    }
                    else
                    {
                        // Wait for cooldown, but don't move away
                        if (showChaseDebug) Debug.Log($"Takau: Attack on cooldown ({attackCooldown - timeSinceLastAttack:F1}s remaining)");
                        return; // Stay in attack mode but don't attack yet
                    }
                }
                else
                {
                    // Player moved away, return to chase or wait
                    if (isDetectTarget && currentTarget != null)
                    {
                        if (showChaseDebug) Debug.Log("Takau: Player moved away from attack range, switching to chase");
                        SwitchMoveMode(MoveMode.chase);
                    }
                    else
                    {
                        if (showChaseDebug) Debug.Log("Takau: Lost target after attack, switching to wait");
                        SwitchMoveMode(MoveMode.wait);
                    }
                }
            }
        }
        private void Charging()
        {
            if (moveMode == MoveMode.dying) return;
            if (!isCharging) return;

            // ACCELERATION SYSTEM untuk Charge menggunakan NavMeshAgent
            currentChargeAccelTime += Time.deltaTime;
            float accelProgress = Mathf.Clamp01(currentChargeAccelTime / chargeAccelerationTime);
            float targetSpeed = Mathf.Lerp(chargeStartSpeed, chargeSpeed, accelProgress);
            currentEffectiveSpeed = targetSpeed;

            // Set NavMeshAgent speed dan acceleration untuk charge
            agent.speed = targetSpeed;
            agent.acceleration = Mathf.Lerp(20f, 50f, accelProgress); // High acceleration untuk charge yang explosive

            agent.destination = chargeTargetPosition;

            float distanceToChargeTarget = Vector3.Distance(transform.position, chargeTargetPosition);

            if (showChaseDebug)
            {
                Debug.Log($"Takau charging! Distance: {distanceToChargeTarget:F2}m, Speed: {agent.speed:F1}m/s, Accel: {agent.acceleration:F1}m/s² ({accelProgress * 100:F0}% progress)");
            }

            // Check if hit player during charge
            if (currentTarget != null)
            {
                float distanceToPlayer = Vector3.Distance(transform.position, currentTarget.position);
                if (distanceToPlayer <= radiusHit && !isHit)
                {
                    Debug.Log("Takau hit player during charge - Game Over!");
                    isHit = true;
                    OnPlayerCaught();
                    return;
                }
            }

            // Stop charge when reached target position or close enough
            if (distanceToChargeTarget <= 2f)
            {
                if (showChaseDebug) Debug.Log("Takau: Charge completed, entering cooldown wait");
                FinishCharge();
            }
        }

        public void Dying()
        {
            Debug.Log("Takau: Entering DYING mode - AI is now dead!");
                
                // Set mode dying terlebih dahulu
                moveMode = MoveMode.dying;
                
                // Stop semua movement dan navigation
                if (agent != null)
                {
                    agent.isStopped = true;
                    agent.speed = 0;
                    agent.destination = transform.position;
                    agent.enabled = false; // Disable NavMeshAgent sepenuhnya
                }
                
                // Reset semua status
                isDetectTarget = false;
                isAttacking = false;
                isCharging = false;
                isHit = false;
                
                // Reset timers
                currentTimeChasing = 0;
                currentTimeWaiting = 0;
                currentChargeSearchTime = 0;
                currentAttackTime = 0;
                currentTarget = null;
                
                // Reset speeds
                currentEffectiveSpeed = 0;

                Debug.Log("Takau: Successfully entered DYING mode - All systems stopped!");

        }

        private void OnPlayerCaught()
        {
            if (showChaseDebug) Debug.Log("Takau successfully caught the player!");
            agent.isStopped = true;
            isDetectTarget = false;

            // Try to kill player using PlayerDeathHandler
            if (currentTarget != null)
            {
                PlayerDeathHandler deathHandler = currentTarget.GetComponent<PlayerDeathHandler>();

                if (deathHandler == null)
                {
                    // Try in parent or children
                    deathHandler = currentTarget.GetComponentInParent<PlayerDeathHandler>();
                    if (deathHandler == null)
                        deathHandler = currentTarget.GetComponentInChildren<PlayerDeathHandler>();
                }

                if (deathHandler != null && deathHandler.CanDie())
                {
                    if (showChaseDebug) Debug.Log("★★★ KILLING PLAYER via PlayerDeathHandler (caught) ★★★");
                    deathHandler.Die("Caught by Takau");
                }
                else
                {
                    if (showChaseDebug) Debug.LogWarning("Player caught but no PlayerDeathHandler found or player cannot die!");
                }
            }
        }


        private bool CheckChargeTarget()
        {
            if (currentTarget == null) return false;

            Vector3 direction = (currentTarget.position - transform.position).normalized;
            float angleToTarget = Vector3.Angle(transform.forward, direction);
            float distance = Vector3.Distance(transform.position, currentTarget.position);

            // Check if target is in charge forward vision cone (normal range)
            bool inChargeVision = angleToTarget < chargeForwardVisionAngle / 2;
            bool inChargeRange = distance >= chargeMinDistance && distance <= (viewRadius + chargeForwardVisionBonus);
            bool lineOfSight = !Physics.Raycast(transform.position, direction, distance, ObstacleMask, QueryTriggerInteraction.Ignore);

            if (showChaseDebug && inChargeVision && inChargeRange)
            {
                Debug.Log($"Charge target check: Angle={angleToTarget:F1}°, Distance={distance:F1}m, LOS={lineOfSight}");
            }

            return inChargeVision && inChargeRange && lineOfSight;
        }

        private bool CheckChargeTargetDuringSearch()
        {
            if (currentTarget == null) return false;

            Vector3 direction = (currentTarget.position - transform.position).normalized;
            float angleToTarget = Vector3.Angle(transform.forward, direction);
            float distance = Vector3.Distance(transform.position, currentTarget.position);

            // Use EXTENDED vision range during charge search
            bool inChargeVision = angleToTarget < chargeForwardVisionAngle / 2;
            bool inChargeRange = distance >= chargeMinDistance && distance <= (viewRadius + chargeSearchVisionBonusMax); // Extended range
            bool lineOfSight = !Physics.Raycast(transform.position, direction, distance, ObstacleMask, QueryTriggerInteraction.Ignore);

            if (showChaseDebug && inChargeVision && inChargeRange)
            {
                Debug.Log($"Charge SEARCH target check: Angle={angleToTarget:F1}°, Distance={distance:F1}m (EXTENDED), LOS={lineOfSight}");
            }

            return inChargeVision && inChargeRange && lineOfSight;
        }

        private void InitiateCharge()
        {
            if (currentTarget == null) return;

            // Set charge target position to player's current position
            chargeTargetPosition = currentTarget.position;
            isCharging = true;
            lastChargeTime = Time.time;

            if (showChaseDebug)
            {
                Debug.Log($"Charge initiated to position: {chargeTargetPosition}");
            }

            SwitchMoveMode(MoveMode.charge);
        }

        private void FinishCharge()
        {
            isCharging = false;
            agent.isStopped = true;

            // Brief pause after charge before returning to normal behavior
            if (showChaseDebug) Debug.Log("Charge finished, entering extended cooldown");

            SwitchMoveMode(MoveMode.wait);
        }

        private void SwitchMoveMode(MoveMode _moveMode)
        {
            if (moveMode == MoveMode.dying && _moveMode != MoveMode.dying) return;
            if (moveMode == _moveMode) return; // Prevent unnecessary switches

            // Exit current mode
            switch (moveMode)
            {
                case MoveMode.chase:
                    if (showChaseDebug) Debug.Log("Takau: Exiting chase mode");
                    break;
                case MoveMode.wait:
                    if (showChaseDebug) Debug.Log("Takau: Exiting wait mode");
                    break;
                case MoveMode.chargeSearch:
                    if (showChaseDebug) Debug.Log("Takau: Exiting charge search mode");
                    break;
                case MoveMode.charge:
                    if (showChaseDebug) Debug.Log("Takau: Exiting charge mode");
                    break;
                case MoveMode.attack:
                    if (showChaseDebug) Debug.Log("Takau: Exiting attack mode");
                    isAttacking = false;
                    break;
            }

            // Enter new mode
            switch (_moveMode)
            {
                case MoveMode.chase:
                    currentTimeChasing = 0;
                    currentChaseAccelTime = 0; // Reset chase acceleration timer
                    currentEffectiveSpeed = chaseStartSpeed; // Start with chase start speed
                    agent.speed = chaseStartSpeed;
                    agent.acceleration = 8f; // Start with lower acceleration
                    agent.isStopped = false;
                    if (showChaseDebug) Debug.Log("Takau: Entering Chase Mode - HUNTING!");
                    break;
                case MoveMode.wait:
                    agent.destination = transform.position;
                    currentTimeWaiting = 0;
                    currentEffectiveSpeed = 0; // No movement during wait
                    agent.speed = 0;
                    agent.acceleration = 15f; // Default acceleration
                    agent.isStopped = false;
                    if (showChaseDebug) Debug.Log("Takau: Entering Wait Mode - Scanning...");
                    break;
                case MoveMode.chargeSearch:
                    currentChargeSearchTime = 0;
                    currentChargeSearchVisionBonus = chargeSearchVisionBonusStart; // Reset to starting value
                    currentEffectiveSpeed = 0; // No movement during search
                    agent.speed = 0;
                    agent.acceleration = 15f; // Default acceleration
                    agent.isStopped = false;
                    if (showChaseDebug) Debug.Log("Takau: Entering Charge Search Mode - Looking for charge target!");
                    break;
                case MoveMode.charge:
                    currentChargeAccelTime = 0; // Reset charge acceleration timer
                    currentEffectiveSpeed = chargeStartSpeed; // Start with charge start speed
                    agent.speed = chargeStartSpeed;
                    agent.acceleration = 20f; // Start with high acceleration for explosive feel
                    agent.isStopped = false;
                    if (showChaseDebug) Debug.Log("Takau: Entering Charge Mode - CHARGING!");
                    break;
                case MoveMode.attack:
                    agent.destination = transform.position;
                    currentAttackTime = 0; // Reset attack timer
                    currentEffectiveSpeed = 0; // No movement during attack
                    agent.speed = 0;
                    agent.acceleration = 15f; // Default acceleration
                    agent.isStopped = false;
                    isAttacking = true;
                    if (showChaseDebug) Debug.Log("Takau: Entering Attack Mode - ATTACKING PLAYER!");
                    break;
            }

            moveMode = _moveMode;
            Debug.Log($"Takau: Mode switched to {moveMode}");
        }



        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Light"))
            {
                // Hanya pindah ke chase jika AI sedang idle, wait, patrol, atau chargeSearch
                if (moveMode == MoveMode.wait || moveMode == MoveMode.patrol || moveMode == MoveMode.chargeSearch)
                {
                    Debug.Log("Enemy mendeteksi FearShield! Mulai mengejar.");
                    SwitchMoveMode(MoveMode.chase);
                }
                else
                {
                    Debug.Log($"Enemy trigger FearShield, tapi sedang mode {moveMode}, tidak pindah ke chase.");
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Light"))
            {
                Debug.Log("FearShield keluar dari jangkauan! Enemy berhenti mengejar.");
            }
        }

        private void FieldOfView()
        {
            if (moveMode == MoveMode.dying) 
            {
                isDetectTarget = false;
                return;
            }
            float extendedRadius = viewRadius + forwardVisionBonus;
            float chargeExtendedRadius = viewRadius + chargeForwardVisionBonus;
            float chargeSearchExtendedRadius = viewRadius + currentChargeSearchVisionBonus; // Even more extended during search

            // SAFETY CHECK - Force correct values if needed
            if (currentChargeSearchVisionBonus <= chargeForwardVisionBonus)
            {
                Debug.LogWarning("Detected wrong currentChargeSearchVisionBonus value! Using hardcoded correct value.");
                chargeSearchExtendedRadius = viewRadius + chargeSearchVisionBonusMax; // Force correct calculation
            }

            // Debug: Show current mode and ranges
            if (showChaseDebug && Time.frameCount % 60 == 0) // Every 60 frames to avoid spam
            {
                Debug.Log($"FieldOfView DEBUG - Mode: {moveMode}");
                Debug.Log($"  viewRadius: {viewRadius:F1}m");
                Debug.Log($"  forwardVisionBonus: {forwardVisionBonus:F1}m");
                Debug.Log($"  chargeForwardVisionBonus: {chargeForwardVisionBonus:F1}m");
                Debug.Log($"  currentChargeSearchVisionBonus: {currentChargeSearchVisionBonus:F1}m");
                Debug.Log($"  CALCULATED - Normal: {viewRadius:F1}m, Chase: {extendedRadius:F1}m, Charge: {chargeExtendedRadius:F1}m, ChargeSearch: {chargeSearchExtendedRadius:F1}m");

                // VALIDATION CHECK
                if (chargeSearchExtendedRadius <= chargeExtendedRadius)
                {
                    Debug.LogError($"ERROR: ChargeSearch range ({chargeSearchExtendedRadius:F1}m) should be > Charge range ({chargeExtendedRadius:F1}m)!");
                    Debug.LogError($"Check if currentChargeSearchVisionBonus ({currentChargeSearchVisionBonus:F1}) > chargeForwardVisionBonus ({chargeForwardVisionBonus:F1})");
                    Debug.LogError("Using hardcoded values to fix this!");
                    chargeSearchExtendedRadius = viewRadius + 30f; // Force fix
                }
            }

            // Use the largest radius to detect all possible targets
            float maxRadius = Mathf.Max(extendedRadius, chargeExtendedRadius, chargeSearchExtendedRadius);
            Collider[] range = Physics.OverlapSphere(transform.position, maxRadius, TargetMask, QueryTriggerInteraction.Ignore);

            if (range.Length > 0)
            {

                currentTarget = range[0].transform;

                Vector3 direction = (currentTarget.position - transform.position).normalized;
                float angleToTarget = Vector3.Angle(transform.forward, direction);
                float distance = Vector3.Distance(transform.position, currentTarget.position);

                // === ATTACK DETECTION (HIGHEST PRIORITY) ===
                // Check attack conditions first - if player is close and in front of Takau
                float timeSinceLastAttack = Time.time - lastAttackTime;
                bool attackReady = timeSinceLastAttack >= attackCooldown;

                if (moveMode != MoveMode.charge && moveMode != MoveMode.attack && attackReady)
                {
                    if (CheckAttackConditions())
                    {
                        if (showChaseDebug)
                        {
                            Debug.Log($"★★★ ATTACK CONDITIONS MET! Distance={distance:F1}m, Angle={angleToTarget:F1}° - ATTACKING PLAYER! ★★★");
                        }

                        // Initiate attack
                        lastAttackTime = Time.time;
                        currentAttackTime = 0;
                        isAttacking = true;
                        SwitchMoveMode(MoveMode.attack);
                        isDetectTarget = false; // Disable normal detection during attack
                        return;
                    }
                }

                // Check charge detection second (lower priority than attack)
                float timeSinceLastCharge = Time.time - lastChargeTime;
                bool chargeReady = timeSinceLastCharge >= chargeCooldown;

                // CHARGE DETECTION - Priority system based on mode
                if (moveMode != MoveMode.charge && chargeReady)
                {
                    bool inChargeVision = angleToTarget < chargeForwardVisionAngle / 2;
                    bool chargeLineOfSight = !Physics.Raycast(transform.position, direction, distance, ObstacleMask, QueryTriggerInteraction.Ignore);

                    // Use different charge ranges based on mode
                    bool inChargeRange = false;
                    string rangeType = "";

                    if (moveMode == MoveMode.chargeSearch)
                    {
                        // During charge search, use EXTENDED range (chargeSearchVisionBonus)
                        inChargeRange = distance >= chargeMinDistance && distance <= chargeSearchExtendedRadius;
                        rangeType = "EXTENDED SEARCH";

                        if (showChaseDebug)
                        {
                            Debug.Log($"★ CHARGE SEARCH ACTIVE: Using range {chargeMinDistance:F1}m - {chargeSearchExtendedRadius:F1}m");
                            Debug.Log($"★ CHARGE SEARCH CHECK: Angle={angleToTarget:F1}°/{chargeForwardVisionAngle / 2:F1}°, Distance={distance:F1}m, Range={chargeMinDistance:F1}m-{chargeSearchExtendedRadius:F1}m, InRange={inChargeRange}, LOS={chargeLineOfSight}");
                        }

                        if (inChargeVision && inChargeRange && chargeLineOfSight)
                        {
                            if (showChaseDebug)
                            {
                                Debug.Log($"★★★ CHARGE SEARCH SUCCESS: Target detected in {rangeType} range! Distance={distance:F1}m (Max: {chargeSearchExtendedRadius:F1}m) - INITIATING CHARGE! ★★★");
                            }

                            // Initiate charge from search mode
                            chargeTargetPosition = currentTarget.position;
                            isCharging = true;
                            lastChargeTime = Time.time;
                            SwitchMoveMode(MoveMode.charge);
                            isDetectTarget = false; // Disable normal detection during charge
                            return;
                        }
                    }
                    else
                    {
                        // During normal modes (wait/chase), use normal charge range
                        inChargeRange = distance >= chargeMinDistance && distance <= chargeExtendedRadius;
                        rangeType = "NORMAL";

                        if (showChaseDebug && inChargeVision)
                        {
                            Debug.Log($"NORMAL CHARGE CHECK: Angle={angleToTarget:F1}°/{chargeForwardVisionAngle / 2:F1}°, Distance={distance:F1}m, Range={chargeMinDistance:F1}m-{chargeExtendedRadius:F1}m, InRange={inChargeRange}, LOS={chargeLineOfSight}");
                        }

                        if (inChargeVision && inChargeRange && chargeLineOfSight)
                        {
                            if (showChaseDebug)
                            {
                                Debug.Log($"CHARGE SUCCESS: Target detected in {rangeType} range! Distance={distance:F1}m (Max: {chargeExtendedRadius:F1}m) - INITIATING CHARGE!");
                            }

                            // Directly initiate charge without search phase
                            chargeTargetPosition = currentTarget.position;
                            isCharging = true;
                            lastChargeTime = Time.time;
                            SwitchMoveMode(MoveMode.charge);
                            isDetectTarget = false; // Disable normal detection during charge
                            return;
                        }
                    }
                }

                // === NORMAL CHASE DETECTION (with minimum distance to prevent attack overlap) ===
                if (moveMode != MoveMode.charge && moveMode != MoveMode.attack)
                {
                    // Only use FORWARD VISION for chase detection (remove dual vision system)
                    bool inForwardVision = angleToTarget < forwardVisionAngle / 2;
                    bool inChaseRange = distance >= minChaseDetectionDistance && distance <= (viewRadius + forwardVisionBonus);
                    bool lineOfSight = !Physics.Raycast(transform.position, direction, distance, ObstacleMask, QueryTriggerInteraction.Ignore);

                    if (showChaseDebug && inForwardVision)
                    {
                        Debug.Log($"CHASE CHECK: Angle={angleToTarget:F1}°/{forwardVisionAngle / 2:F1}°, Distance={distance:F1}m, Range={minChaseDetectionDistance:F1}m-{viewRadius + forwardVisionBonus:F1}m, InRange={inChaseRange}, LOS={lineOfSight}");
                    }

                    if (inForwardVision && inChaseRange && lineOfSight)
                    {
                        isDetectTarget = true;

                        if (moveMode == MoveMode.wait)
                        {
                            if (showChaseDebug) Debug.Log($"CHASE: Target detected in wait mode at {distance:F1}m - let Waiting() handle it");
                        }
                        else if (moveMode == MoveMode.chase)
                        {
                            // Already in chase, continue
                        }
                        else if (moveMode == MoveMode.chargeSearch)
                        {
                            // During charge search, normal vision detection is secondary to charge detection
                            if (showChaseDebug) Debug.Log($"CHASE: Target detected during charge search - continue search or switch to chase");
                        }
                        else
                        {
                            if (showChaseDebug) Debug.Log($"CHASE: Target detected at {distance:F1}m, switching to chase");
                            SwitchMoveMode(MoveMode.chase);
                        }
                    }
                    else
                    {
                        isDetectTarget = false;
                        if (showChaseDebug && range.Length > 0)
                        {
                            if (!inForwardVision)
                            {
                                Debug.Log($"CHASE: Target outside forward vision angle ({angleToTarget:F1}° > {forwardVisionAngle / 2:F1}°)");
                            }
                            else if (!inChaseRange)
                            {
                                if (distance < minChaseDetectionDistance)
                                {
                                    Debug.Log($"CHASE: Target too close for chase ({distance:F1}m < {minChaseDetectionDistance:F1}m) - attack range");
                                }
                                else
                                {
                                    Debug.Log($"CHASE: Target too far ({distance:F1}m > {viewRadius + forwardVisionBonus:F1}m)");
                                }
                            }
                            else
                            {
                                Debug.Log("CHASE: Target blocked by obstacle");
                            }
                        }
                    }
                }
                else
                {
                    // During charge mode, disable normal detection to prevent interference
                    isDetectTarget = false;
                }
            }
            else
            {
                isDetectTarget = false;
                currentTarget = null;
            }
        }

        private void HandleRotation()
        {
            if (moveMode == MoveMode.dying) return;
            if (!useCustomRotation) return;

            Vector3 direction = Vector3.zero;

            if (moveMode == MoveMode.wait)
            {
                if (!isDetectTarget || currentTarget == null) return;
                direction = (currentTarget.position - transform.position).normalized;
            }
            else if (moveMode == MoveMode.chase)
            {
                if (currentTarget == null) return;
                direction = (currentTarget.position - transform.position).normalized;
            }
            else if (moveMode == MoveMode.attack)
            {
                if (currentTarget == null) return;
                direction = (currentTarget.position - transform.position).normalized;
            }
            else if (moveMode == MoveMode.chargeSearch)
            {
                // Let the rotation in ChargeSearching() method handle this
                return;
            }
            else if (moveMode == MoveMode.charge)
            {
                if (!isCharging) return;
                direction = (chargeTargetPosition - transform.position).normalized;
            }
            else
            {
                return;
            }

            direction.y = 0; // Keep rotation only on Y-axis

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);

                float currentRotationSpeed = rotationSpeed;

                if (moveMode == MoveMode.chase)
                {
                    currentRotationSpeed = rotationSpeed * 1.5f; // 50% faster rotation when chasing
                }
                else if (moveMode == MoveMode.wait)
                {
                    currentRotationSpeed = rotationSpeed * 0.8f; // Slower rotation when waiting (more cautious)
                }
                else if (moveMode == MoveMode.attack)
                {
                    currentRotationSpeed = rotationSpeed * 3f; // Very fast rotation when attacking to face target
                }
                else if (moveMode == MoveMode.charge)
                {
                    currentRotationSpeed = rotationSpeed * 2f; // Very fast rotation when charging
                }

                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    currentRotationSpeed * Time.deltaTime
                );

                if (showChaseDebug && moveMode == MoveMode.wait)
                {
                    float angle = Vector3.Angle(transform.forward, direction);
                    Debug.Log($"Wait Mode Rotation: Angle to target = {angle:F1}°");
                }
            }
        }

        // Debug method to manually set current parameter values
        private void SetChargeSearchBonus(float value)
        {
            currentChargeSearchVisionBonus = value;
            Debug.Log($"CurrentChargeSearchVisionBonus set to: {currentChargeSearchVisionBonus}");
        }

        // Method to ensure correct parameter values
        [System.Obsolete("Use for debugging only")]
        public void ForceCorrectParameters()
        {
            chargeForwardVisionBonus = 18f;
            chargeSearchVisionBonusStart = 18.5f;
            chargeSearchVisionBonusMax = 30f;
            currentChargeSearchVisionBonus = chargeSearchVisionBonusStart;
            Debug.Log("Parameters forcefully corrected!");
        }

        // Property to get the current charge search range (gradual expansion)
        public float GetCurrentChargeSearchRange()
        {
            return viewRadius + currentChargeSearchVisionBonus;
        }

        // Property to get the max charge search range
        public float GetMaxChargeSearchRange()
        {
            return viewRadius + chargeSearchVisionBonusMax;
        }

        // Property to get the correct charge range
        public float GetCorrectChargeRange()
        {
            return viewRadius + chargeForwardVisionBonus;
        }

        private bool CheckAttackConditions()
        {
            if (currentTarget == null) return false;

            Vector3 direction = (currentTarget.position - transform.position).normalized;
            float angleToTarget = Vector3.Angle(transform.forward, direction);
            float distance = Vector3.Distance(transform.position, currentTarget.position);

            // Check if target is in front of Takau (within attack angle)
            bool inAttackAngle = angleToTarget < attackAngle / 2;

            // Check range - use extended range if target is in forward vision
            bool inForwardVision = angleToTarget < forwardVisionAngle / 2;
            float effectiveAttackRange = inForwardVision ? attackRange + attackForwardBonus : attackRange;
            bool inAttackRange = distance <= effectiveAttackRange;

            // Check line of sight
            bool lineOfSight = !Physics.Raycast(transform.position, direction, distance, ObstacleMask, QueryTriggerInteraction.Ignore);

            if (showChaseDebug && inAttackAngle && inAttackRange)
            {
                string rangeType = inForwardVision ? "EXTENDED" : "NORMAL";
                Debug.Log($"Attack check: Angle={angleToTarget:F1}°/{attackAngle / 2:F1}°, Distance={distance:F1}m, Range={effectiveAttackRange:F1}m ({rangeType}), LOS={lineOfSight}");
            }

            return inAttackAngle && inAttackRange && lineOfSight;
        }

        private void OnPlayerAttacked()
        {
            if (showChaseDebug) Debug.Log("Takau attack animation completed!");

            // Attack animation selesai, tapi ini TIDAK otomatis membunuh player
            // Player hanya akan mati jika berada dalam radiusHit (di method Chasing atau saat contact)

            if (currentTarget != null)
            {
                float distanceToPlayer = Vector3.Distance(transform.position, currentTarget.position);

                if (showChaseDebug)
                {
                    Debug.Log($"Attack completed! Player distance: {distanceToPlayer:F1}m");
                    Debug.Log($"RadiusHit (death zone): {radiusHit:F1}m");
                    Debug.Log($"Player status: {(distanceToPlayer <= radiusHit ? "DEAD" : "SAFE")}");
                }

                // HANYA jika player sangat dekat (dalam radiusHit) maka player mati
                if (distanceToPlayer <= radiusHit)
                {
                    if (showChaseDebug) Debug.Log("Player was too close during attack - GAME OVER!");
                    isHit = true;
                    OnPlayerCaught(); // Trigger game over
                }
                else
                {
                    if (showChaseDebug) Debug.Log("Player survived the attack - was outside death radius!");
                    // Player selamat dari attack karena tidak dalam radiusHit
                    // Tidak ada damage, hanya visual effect attack
                }
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (agent == null) return;

            // Stopping distance
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, agent.stoppingDistance);

            // Normal view radius
            Gizmos.color = isDetectTarget ? Color.green : Color.blue;
            Gizmos.DrawWireSphere(transform.position, viewRadius);

            // Extended forward vision radius
            Gizmos.color = isDetectTarget ? new Color(0f, 1f, 0f, 0.3f) : new Color(0f, 0f, 1f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, viewRadius + forwardVisionBonus);

            // DEATH ZONE (radiusHit) - Inner circle where player dies
            Gizmos.color = isHit ? Color.red : new Color(1f, 0f, 0f, 0.8f);
            Gizmos.DrawWireSphere(transform.position, radiusHit);
            Gizmos.DrawSphere(transform.position, radiusHit * 0.1f); // Small center dot for visibility

            // Chase lose distance
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, loseTargetDistance);

            // Min chase distance
            Gizmos.color = new Color(1f, 0.5f, 0f); // Orange color
            Gizmos.DrawWireSphere(transform.position, minChaseDistance);

            // Min chase DETECTION distance (prevents attack/chase overlap)
            Gizmos.color = new Color(0f, 1f, 1f, 0.5f); // Cyan color for detection minimum
            Gizmos.DrawWireSphere(transform.position, minChaseDetectionDistance);

            // === ATTACK RANGE VISUALIZATION ===
            // Normal attack range
            Gizmos.color = moveMode == MoveMode.attack ? Color.red : new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, attackRange);

            // Extended attack range (with forward bonus)
            Gizmos.color = moveMode == MoveMode.attack ? new Color(1f, 0.5f, 0.5f) : new Color(1f, 0f, 0f, 0.2f);
            Gizmos.DrawWireSphere(transform.position, attackRange + attackForwardBonus);

            // Attack vision cone
            float halfAttackAngle = attackAngle / 2f;
            Quaternion leftAttackRotation = Quaternion.AngleAxis(-halfAttackAngle, Vector3.up);
            Quaternion rightAttackRotation = Quaternion.AngleAxis(halfAttackAngle, Vector3.up);
            Vector3 leftAttackDirection = leftAttackRotation * transform.forward;
            Vector3 rightAttackDirection = rightAttackRotation * transform.forward;

            Gizmos.color = moveMode == MoveMode.attack ? Color.red : new Color(1f, 0f, 0f, 0.4f);
            Gizmos.DrawRay(transform.position, leftAttackDirection * (attackRange + attackForwardBonus));
            Gizmos.DrawRay(transform.position, rightAttackDirection * (attackRange + attackForwardBonus));

            // Attack range labels
            Vector3 attackRangeLabel = transform.position + transform.forward * attackRange;
            UnityEditor.Handles.Label(attackRangeLabel, $"Attack: {attackRange:F1}m");

            Vector3 attackExtendedLabel = transform.position + transform.forward * (attackRange + attackForwardBonus);
            UnityEditor.Handles.Label(attackExtendedLabel, $"Attack+: {attackRange + attackForwardBonus:F1}m");

            // Line to target with different colors based on mode
            if (currentTarget != null && isDetectTarget)
            {
                switch (moveMode)
                {
                    case MoveMode.chase:
                        Gizmos.color = Color.red;
                        break;
                    case MoveMode.wait:
                        Gizmos.color = Color.yellow;
                        break;
                    case MoveMode.attack:
                        Gizmos.color = Color.magenta;
                        break;
                    default:
                        Gizmos.color = Color.green;
                        break;
                }
                Gizmos.DrawLine(transform.position, currentTarget.position);

                // Draw distance text (in Scene view)
                float dist = Vector3.Distance(transform.position, currentTarget.position);
                Vector3 midPoint = (transform.position + currentTarget.position) / 2;

                UnityEditor.Handles.Label(midPoint, $"{dist:F1}m");
            }

            // === SIMPLIFIED VISION SYSTEM (Forward Vision Only) ===
            // Forward vision cone for CHASE (primary detection)
            float halfForwardFov = forwardVisionAngle / 2f;
            Quaternion leftForwardRotation = Quaternion.AngleAxis(-halfForwardFov, Vector3.up);
            Quaternion rightForwardRotation = Quaternion.AngleAxis(halfForwardFov, Vector3.up);
            Vector3 leftForwardDirection = leftForwardRotation * transform.forward;
            Vector3 rightForwardDirection = rightForwardRotation * transform.forward;

            Gizmos.color = isDetectTarget ? new Color(0f, 1f, 0f, 0.8f) : new Color(0f, 0f, 1f, 0.6f);
            Gizmos.DrawRay(transform.position, leftForwardDirection * (viewRadius + forwardVisionBonus));
            Gizmos.DrawRay(transform.position, rightForwardDirection * (viewRadius + forwardVisionBonus));

            // Label for chase vision system
            Vector3 chaseVisionLabel = transform.position + transform.forward * (viewRadius + forwardVisionBonus);
            UnityEditor.Handles.Label(chaseVisionLabel, $"Chase Vision: {viewRadius + forwardVisionBonus:F1}m / {forwardVisionAngle}°");

            // Show minimum chase detection distance
            Vector3 minChaseLabel = transform.position + transform.forward * minChaseDetectionDistance;
            UnityEditor.Handles.Label(minChaseLabel, $"Min Chase: {minChaseDetectionDistance:F1}m");

            // Charge vision cone (always visible for debugging)
            float halfChargeFov = chargeForwardVisionAngle / 2f;
            Quaternion leftChargeRotation = Quaternion.AngleAxis(-halfChargeFov, Vector3.up);
            Quaternion rightChargeRotation = Quaternion.AngleAxis(halfChargeFov, Vector3.up);
            Vector3 leftChargeDirection = leftChargeRotation * transform.forward;
            Vector3 rightChargeDirection = rightChargeRotation * transform.forward;

            // Different ranges and colors based on mode
            float chargeRange = viewRadius + chargeForwardVisionBonus;
            float chargeSearchRange = Application.isPlaying ? viewRadius + currentChargeSearchVisionBonus : viewRadius + chargeSearchVisionBonusStart;

            if (moveMode == MoveMode.chargeSearch)
            {
                // Create a pulsing effect for charge search mode with gradual expansion visualization
                float expansionProgress = currentChargeSearchTime / chargeSearchTime; // 0 to 1
                float pulseIntensity = 0.5f + 0.4f * Mathf.Sin(Time.time * 4f); // Pulse between 0.5 and 0.9

                // 1. Normal charge range (dimmed)
                Gizmos.color = new Color(0.3f, 0f, 0.6f, 0.2f); // Very dim purple for normal charge range
                Gizmos.DrawRay(transform.position, leftChargeDirection * chargeRange);
                Gizmos.DrawRay(transform.position, rightChargeDirection * chargeRange);
                Gizmos.DrawWireSphere(transform.position, chargeRange);

                // 2. GRADUAL EXPANDING RANGE - Show current expansion level
                float currentSearchRange = viewRadius + currentChargeSearchVisionBonus;
                Gizmos.color = new Color(1f, expansionProgress, 1f, pulseIntensity); // Color changes with expansion
                Gizmos.DrawRay(transform.position, leftChargeDirection * currentSearchRange);
                Gizmos.DrawRay(transform.position, rightChargeDirection * currentSearchRange);

                // Current expansion range sphere with pulse
                Gizmos.color = new Color(1f, expansionProgress, 1f, pulseIntensity * 0.4f);
                Gizmos.DrawWireSphere(transform.position, currentSearchRange);

                // 3. MAX SEARCH RANGE - Show target max range as faint outline
                float maxSearchRange = viewRadius + chargeSearchVisionBonusMax;
                Gizmos.color = new Color(1f, 0f, 1f, 0.1f); // Very faint magenta for max range preview
                Gizmos.DrawWireSphere(transform.position, maxSearchRange);

                // Additional visual: Draw the "expansion waves" to show growing effect
                for (int i = 0; i < 3; i++)
                {
                    float waveRadius = chargeRange + (currentChargeSearchVisionBonus - chargeForwardVisionBonus) * (i + 1) / 3f;
                    if (waveRadius <= currentSearchRange)
                    {
                        float waveAlpha = pulseIntensity * 0.15f * (1f - i * 0.3f);
                        Gizmos.color = new Color(1f, 0.5f, 1f, waveAlpha);
                        Gizmos.DrawWireSphere(transform.position, waveRadius);
                    }
                }

                // Draw "search beams" - additional rays to emphasize active searching
                for (int i = -2; i <= 2; i++)
                {
                    float searchAngle = i * 5f; // Every 5 degrees
                    Quaternion searchRotation = Quaternion.AngleAxis(searchAngle, Vector3.up);
                    Vector3 searchDirection = searchRotation * transform.forward;

                    Gizmos.color = new Color(1f, 1f, 0f, pulseIntensity * 0.6f); // Yellow search beams
                    Gizmos.DrawRay(transform.position, searchDirection * currentSearchRange);
                }

                // Labels for ranges with expansion info
                Vector3 normalChargeLabelPos = transform.position + transform.forward * chargeRange + Vector3.up;
                UnityEditor.Handles.Label(normalChargeLabelPos, $"Normal Charge: {chargeRange:F1}m");

                Vector3 currentSearchLabelPos = transform.position + transform.forward * currentSearchRange + Vector3.up * 2f;
                UnityEditor.Handles.Label(currentSearchLabelPos, $"★ EXPANDING: {currentSearchRange:F1}m ({expansionProgress * 100:F0}%) ★");

                Vector3 maxSearchLabelPos = transform.position + transform.forward * maxSearchRange + Vector3.up * 3f;
                UnityEditor.Handles.Label(maxSearchLabelPos, $"Max Target: {maxSearchRange:F1}m");

                // Show the expansion progress clearly
                Vector3 progressLabelPos = transform.position + Vector3.up * 4f;
                UnityEditor.Handles.Label(progressLabelPos, $"EXPANSION: {currentChargeSearchVisionBonus:F1}m / {chargeSearchVisionBonusMax:F1}m");
                UnityEditor.Handles.Label(progressLabelPos + Vector3.up * 0.5f, $"Progress: {expansionProgress * 100:F0}% ({currentChargeSearchTime:F1}s/{chargeSearchTime:F1}s)");
            }
            else if (moveMode == MoveMode.charge)
            {
                // Show normal charge range during actual charge
                Gizmos.color = new Color(0.5f, 0f, 1f); // Bright purple when charging
                Gizmos.DrawRay(transform.position, leftChargeDirection * chargeRange);
                Gizmos.DrawRay(transform.position, rightChargeDirection * chargeRange);

                // Charge extended range sphere
                Gizmos.color = new Color(0.5f, 0f, 1f, 0.2f);
                Gizmos.DrawWireSphere(transform.position, chargeRange);

                // Label for charge range
                Vector3 chargeLabelPos = transform.position + transform.forward * chargeRange;
                UnityEditor.Handles.Label(chargeLabelPos, $"Charging: {chargeRange:F1}m");
            }
            else
            {
                // Show ranges when inactive (for comparison and editor setup)

                // Normal charge range
                Gizmos.color = new Color(0.5f, 0f, 1f, 0.4f); // Transparent purple for normal charge
                Gizmos.DrawRay(transform.position, leftChargeDirection * chargeRange);
                Gizmos.DrawRay(transform.position, rightChargeDirection * chargeRange);
                Gizmos.DrawWireSphere(transform.position, chargeRange);

                // EDITOR MODE: Show search start range preview
                if (!Application.isPlaying)
                {
                    float searchStartRange = viewRadius + chargeSearchVisionBonusStart;
                    float searchMaxRange = viewRadius + chargeSearchVisionBonusMax;

                    // Search start range (what you see when you start search)
                    Gizmos.color = new Color(1f, 0.5f, 1f, 0.3f); // Light magenta for search start
                    Gizmos.DrawRay(transform.position, leftChargeDirection * searchStartRange);
                    Gizmos.DrawRay(transform.position, rightChargeDirection * searchStartRange);
                    Gizmos.DrawWireSphere(transform.position, searchStartRange);

                    // Search max range (target when fully expanded)
                    Gizmos.color = new Color(1f, 0f, 1f, 0.2f); // Transparent magenta for search max
                    Gizmos.DrawWireSphere(transform.position, searchMaxRange);

                    // Draw expansion area between start and max
                    Gizmos.color = new Color(1f, 0.8f, 1f, 0.1f);
                    for (float r = searchStartRange; r <= searchMaxRange; r += 1f)
                    {
                        Gizmos.DrawWireSphere(transform.position, r);
                    }
                }
                else
                {
                    // PLAY MODE: Show current search range as dotted/dashed effect
                    Gizmos.color = new Color(1f, 0f, 1f, 0.2f); // Very transparent magenta for search range preview
                    Gizmos.DrawWireSphere(transform.position, chargeSearchRange);
                }

                // Labels for ranges
                Vector3 chargeLabelPos = transform.position + transform.forward * chargeRange;
                UnityEditor.Handles.Label(chargeLabelPos, $"Charge: {chargeRange:F1}m");

                if (!Application.isPlaying)
                {
                    // EDITOR MODE LABELS
                    Vector3 searchStartPos = transform.position + transform.forward * (viewRadius + chargeSearchVisionBonusStart);
                    UnityEditor.Handles.Label(searchStartPos, $"Search Start: {viewRadius + chargeSearchVisionBonusStart:F1}m");

                    Vector3 searchMaxPos = transform.position + transform.forward * (viewRadius + chargeSearchVisionBonusMax);
                    UnityEditor.Handles.Label(searchMaxPos, $"Search Max: {viewRadius + chargeSearchVisionBonusMax:F1}m");

                    // Show expansion info
                    Vector3 expansionInfoPos = transform.position + Vector3.up * 3f;
                    UnityEditor.Handles.Label(expansionInfoPos, $"SEARCH EXPANSION: {chargeSearchVisionBonusStart:F1}m → {chargeSearchVisionBonusMax:F1}m");
                    UnityEditor.Handles.Label(expansionInfoPos + Vector3.up * 0.5f, $"Over {chargeSearchTime:F1} seconds");
                }
                else
                {
                    // PLAY MODE LABELS
                    Vector3 searchPreviewPos = transform.position + transform.forward * chargeSearchRange;
                    UnityEditor.Handles.Label(searchPreviewPos, $"Search Range: {chargeSearchRange:F1}m");
                }
            }

            // Label for charge angle (always visible)
            Vector3 angleLabel = transform.position + transform.forward * (chargeRange * 0.7f);
            UnityEditor.Handles.Label(angleLabel, $"Charge Angle: {chargeForwardVisionAngle}°");

            // Additional comparison labels
            Vector3 comparisonPos = transform.position + Vector3.up * 2f; // Above Takau
            if (moveMode == MoveMode.chargeSearch && Application.isPlaying)
            {
                UnityEditor.Handles.Label(comparisonPos, $"SEARCH MODE: {chargeSearchRange:F1}m vs Normal: {chargeRange:F1}m");
                UnityEditor.Handles.Label(comparisonPos + Vector3.up * 0.5f, $"Extended by: +{currentChargeSearchVisionBonus - chargeForwardVisionBonus:F1}m");
            }
            else if (moveMode == MoveMode.chase && Application.isPlaying)
            {
                // CHASE MODE: Show acceleration info
                float chaseAccelProgress = Mathf.Clamp01(currentChaseAccelTime / chaseAccelerationTime);
                UnityEditor.Handles.Label(comparisonPos, $"CHASE: Speed {agent.speed:F1}m/s, Accel {agent.acceleration:F1}m/s² ({chaseAccelProgress * 100:F0}%)");
                UnityEditor.Handles.Label(comparisonPos + Vector3.up * 0.5f, $"Acceleration: {chaseStartSpeed:F1}→{chaseSpeed:F1} m/s, 8→15 m/s²");
            }
            else if (moveMode == MoveMode.charge && Application.isPlaying)
            {
                // CHARGE MODE: Show acceleration info
                float chargeAccelProgress = Mathf.Clamp01(currentChargeAccelTime / chargeAccelerationTime);
                UnityEditor.Handles.Label(comparisonPos, $"CHARGE: Speed {agent.speed:F1}m/s, Accel {agent.acceleration:F1}m/s² ({chargeAccelProgress * 100:F0}%)");
                UnityEditor.Handles.Label(comparisonPos + Vector3.up * 0.5f, $"Acceleration: {chargeStartSpeed:F1}→{chargeSpeed:F1} m/s, 20→50 m/s²");
            }
            else if (!Application.isPlaying)
            {
                // EDITOR MODE: Show setup info
                float searchStartRange = viewRadius + chargeSearchVisionBonusStart;
                float searchMaxRange = viewRadius + chargeSearchVisionBonusMax;
                UnityEditor.Handles.Label(comparisonPos, $"EDITOR: Normal: {chargeRange:F1}m | Search: {searchStartRange:F1}m→{searchMaxRange:F1}m");
                UnityEditor.Handles.Label(comparisonPos + Vector3.up * 0.5f, $"Search expansion: +{chargeSearchVisionBonusStart:F1}m to +{chargeSearchVisionBonusMax:F1}m");
            }
            else
            {
                UnityEditor.Handles.Label(comparisonPos, $"Normal: {chargeRange:F1}m | Search: {chargeSearchRange:F1}m");
            }

            // Visual ring indicators for different charge ranges
            if (moveMode == MoveMode.chargeSearch)
            {
                // Create a "pulse" effect for search mode
                float pulseAlpha = 0.3f + 0.2f * Mathf.Sin(Time.time * 3f); // Pulsing between 0.3 and 0.5

                // Inner ring (normal charge range)
                Gizmos.color = new Color(0.5f, 0f, 1f, pulseAlpha);
                Gizmos.DrawWireSphere(transform.position, chargeRange);

                // Outer ring (extended search range)
                Gizmos.color = new Color(1f, 0f, 1f, pulseAlpha);
                Gizmos.DrawWireSphere(transform.position, chargeSearchRange);

                // Connect the rings with lines to show expansion
                for (int i = 0; i < 8; i++)
                {
                    float angle = i * 45f * Mathf.Deg2Rad;
                    Vector3 innerPoint = transform.position + new Vector3(Mathf.Sin(angle) * chargeRange, 0, Mathf.Cos(angle) * chargeRange);
                    Vector3 outerPoint = transform.position + new Vector3(Mathf.Sin(angle) * chargeSearchRange, 0, Mathf.Cos(angle) * chargeSearchRange);

                    Gizmos.color = new Color(1f, 0f, 1f, 0.3f);
                    Gizmos.DrawLine(innerPoint, outerPoint);
                }
            }

            // Charge min distance circle
            Gizmos.color = new Color(0.5f, 0f, 0.5f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, chargeMinDistance);

            // Charge target position
            if (isCharging && moveMode == MoveMode.charge)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(chargeTargetPosition, 1f);
                Gizmos.DrawLine(transform.position, chargeTargetPosition);

                UnityEditor.Handles.Label(chargeTargetPosition, "CHARGE TARGET");
            }

            // Draw forward vision arc
            Gizmos.color = isDetectTarget ? new Color(1f, 0f, 0f, 0.2f) : new Color(1f, 1f, 0f, 0.2f);
            Vector3 arcCenter = transform.position + transform.forward * (viewRadius + forwardVisionBonus * 0.5f);

            // Agent path visualization
            if (agent.hasPath)
            {
                Gizmos.color = Color.black;
                Vector3[] path = agent.path.corners;
                for (int i = 0; i < path.Length - 1; i++)
                {
                    Gizmos.DrawLine(path[i], path[i + 1]);
                }
            }
        }
        private void OnGUI()
        {
            if (!showChaseDebug) return;

            GUILayout.BeginArea(new Rect(10, 10, 420, 400));
            GUILayout.Label("=== TAKAU AI DEBUG ===");
            GUILayout.Label($"Mode: {moveMode}");
            GUILayout.Label($"Target: {(currentTarget ? currentTarget.name : "None")}");
            GUILayout.Label($"Detect Target: {isDetectTarget}");
            GUILayout.Label($"Is Hit: {isHit}");

            // PARAMETER DEBUG - Show actual parameter values
            GUILayout.Label("=== PARAMETERS DEBUG ===");
            GUILayout.Label($"viewRadius: {viewRadius:F1}m");
            GUILayout.Label($"forwardVisionBonus: {forwardVisionBonus:F1}m");
            GUILayout.Label($"forwardVisionAngle: {forwardVisionAngle:F1}°");
            GUILayout.Label($"minChaseDetectionDistance: {minChaseDetectionDistance:F1}m");
            GUILayout.Label($"chargeForwardVisionBonus: {chargeForwardVisionBonus:F1}m");
            GUILayout.Label($"currentChargeSearchVisionBonus: {currentChargeSearchVisionBonus:F1}m");
            GUILayout.Label($"chargeSearchVisionBonusStart: {chargeSearchVisionBonusStart:F1}m");
            GUILayout.Label($"chargeSearchVisionBonusMax: {chargeSearchVisionBonusMax:F1}m");
            GUILayout.Label($"attackRange: {attackRange:F1}m");
            GUILayout.Label($"attackAngle: {attackAngle:F1}°");
            GUILayout.Label($"attackForwardBonus: {attackForwardBonus:F1}m");

            // CALCULATED RANGES
            float calculatedChaseRange = viewRadius + forwardVisionBonus;
            float calculatedCharge = viewRadius + chargeForwardVisionBonus;
            float calculatedChargeSearch = viewRadius + currentChargeSearchVisionBonus;
            float calculatedAttack = attackRange;
            float calculatedAttackExtended = attackRange + attackForwardBonus;

            GUILayout.Label("=== CALCULATED RANGES ===");
            GUILayout.Label($"Chase: {minChaseDetectionDistance:F1}m - {calculatedChaseRange:F1}m (Forward Vision Only)");
            GUILayout.Label($"Charge: {calculatedCharge:F1}m ({viewRadius:F1} + {chargeForwardVisionBonus:F1})");
            GUILayout.Label($"ChargeSearch: {calculatedChargeSearch:F1}m ({viewRadius:F1} + {currentChargeSearchVisionBonus:F1})");
            GUILayout.Label($"Attack: {calculatedAttack:F1}m");
            GUILayout.Label($"Attack Extended: {calculatedAttackExtended:F1}m ({attackRange:F1} + {attackForwardBonus:F1})");

            if (currentTarget != null)
            {
                float dist = Vector3.Distance(transform.position, currentTarget.position);
                GUILayout.Label($"Distance: {dist:F2}m");

                Vector3 direction = (currentTarget.position - transform.position).normalized;
                float angle = Vector3.Angle(transform.forward, direction);
                GUILayout.Label($"Angle to Target: {angle:F1}°");

                // FOV Check details (simplified to forward vision only)
                bool inForwardVision = angle < forwardVisionAngle / 2;
                bool inAttackFOV = angle < attackAngle / 2;
                GUILayout.Label($"In Forward Vision ({forwardVisionAngle}°): {inForwardVision}");
                GUILayout.Label($"In Attack FOV ({attackAngle}°): {inAttackFOV}");

                // Range checks
                bool inChaseRange = dist >= minChaseDetectionDistance && dist <= calculatedChaseRange;
                float effectiveAttackRange = inForwardVision ? attackRange + attackForwardBonus : attackRange;
                bool inAttackRange = dist <= effectiveAttackRange;

                GUILayout.Label($"Chase Range: {(inChaseRange ? "YES" : "NO")} ({minChaseDetectionDistance:F1}m-{calculatedChaseRange:F1}m)");
                GUILayout.Label($"Attack Range: {(inAttackRange ? "YES" : "NO")} ({effectiveAttackRange:F1}m {(inForwardVision ? "EXTENDED" : "NORMAL")})");

                // Range zone identification
                string currentZone = "UNKNOWN";
                if (dist <= attackRange) currentZone = "ATTACK ZONE";
                else if (dist <= effectiveAttackRange && inAttackFOV) currentZone = "ATTACK EXTENDED";
                else if (dist < minChaseDetectionDistance) currentZone = "TOO CLOSE (No Chase)";
                else if (dist <= calculatedChaseRange && inForwardVision) currentZone = "CHASE ZONE";
                else currentZone = "OUT OF RANGE";

                GUILayout.Label($"★ CURRENT ZONE: {currentZone} ★");

                // Obstacle check
                bool blocked = Physics.Raycast(transform.position, direction, dist, ObstacleMask);
                GUILayout.Label($"Line of Sight: {(blocked ? "BLOCKED" : "CLEAR")}");

                // === ATTACK STATUS ===
                float timeSinceLastAttack = Time.time - lastAttackTime;
                bool attackReady = timeSinceLastAttack >= attackCooldown;
                bool attackConditions = CheckAttackConditions();

                GUILayout.Label($"=== ATTACK STATUS ===");
                GUILayout.Label($"In Attack FOV ({attackAngle}°): {inAttackFOV}");
                GUILayout.Label($"In Attack Range ({effectiveAttackRange:F1}m): {inAttackRange}");
                GUILayout.Label($"Attack Ready: {(attackReady ? "YES" : $"NO ({attackCooldown - timeSinceLastAttack:F1}s)")}");
                GUILayout.Label($"★ ATTACK CONDITIONS MET: {(attackConditions ? "YES - READY TO ATTACK!" : "NO")} ★");

                // Charge vision check with real-time status
                bool inChargeFOV = angle < chargeForwardVisionAngle / 2;
                bool inChargeRange = dist >= chargeMinDistance && dist <= (viewRadius + chargeForwardVisionBonus);
                bool inChargeSearchRange = dist >= chargeMinDistance && dist <= (viewRadius + currentChargeSearchVisionBonus);
                float timeSinceLastCharge = Time.time - lastChargeTime;
                bool chargeReady = timeSinceLastCharge >= chargeCooldown;
                bool chargeConditionsMet = chargeReady && inChargeFOV && inChargeRange && !blocked;
                bool chargeSearchConditionsMet = chargeReady && inChargeFOV && inChargeSearchRange && !blocked;

                GUILayout.Label($"=== CHARGE STATUS ===");
                GUILayout.Label($"In Charge FOV ({chargeForwardVisionAngle}°): {inChargeFOV}");
                GUILayout.Label($"In Charge Range ({chargeMinDistance:F1}m-{viewRadius + chargeForwardVisionBonus:F1}m): {inChargeRange}");
                GUILayout.Label($"In Charge SEARCH Range ({chargeMinDistance:F1}m-{viewRadius + currentChargeSearchVisionBonus:F1}m): {inChargeSearchRange}");
                GUILayout.Label($"Charge Ready: {(chargeReady ? "YES" : $"NO ({chargeCooldown - timeSinceLastCharge:F1}s)")}");

                // Show comparison between normal and search ranges
                float normalChargeRange = viewRadius + chargeForwardVisionBonus;
                float extendedSearchRange = viewRadius + currentChargeSearchVisionBonus;
                float searchBonus = currentChargeSearchVisionBonus - chargeForwardVisionBonus;

                GUILayout.Label($"RANGE COMPARISON:");
                GUILayout.Label($"  Normal Charge: {normalChargeRange:F1}m");
                GUILayout.Label($"  Search Extended: {extendedSearchRange:F1}m (+{searchBonus:F1}m)");

                if (moveMode == MoveMode.chargeSearch)
                {
                    GUILayout.Label($"*** CHARGE SEARCH MODE: USING EXTENDED {extendedSearchRange:F1}m RANGE ***");
                    GUILayout.Label($"CHARGE SEARCH CONDITIONS MET: {(chargeSearchConditionsMet ? "YES - READY TO CHARGE!" : "NO")}");

                    // Additional debug for charge search
                    GUILayout.Label($"--- EXTENDED RANGE DEBUG ---");
                    GUILayout.Label($"Current Distance: {dist:F1}m");
                    GUILayout.Label($"Extended Max Range: {extendedSearchRange:F1}m");
                    GUILayout.Label($"Normal Max Range: {normalChargeRange:F1}m");
                    GUILayout.Label($"In Extended Range: {(dist <= extendedSearchRange ? "YES" : "NO")}");
                    GUILayout.Label($"In Normal Range: {(dist <= normalChargeRange ? "YES" : "NO")}");
                    GUILayout.Label($"Extended Bonus Working: {(dist > normalChargeRange && dist <= extendedSearchRange ? "YES!" : "NO")}");
                }
                else
                {
                    GUILayout.Label($"NORMAL CHARGE CONDITIONS MET: {(chargeConditionsMet ? "YES - READY TO CHARGE!" : "NO")}");
                }
            }

            GUILayout.Label($"Chase Time: {currentTimeChasing:F1}s");
            GUILayout.Label($"Wait Time: {currentTimeWaiting:F1}s");
            GUILayout.Label($"Agent Speed: {agent.velocity.magnitude:F2}");
            GUILayout.Label($"Effective Speed: {currentEffectiveSpeed:F1}m/s");
            GUILayout.Label($"Agent Acceleration: {agent.acceleration:F1}m/s²");
            GUILayout.Label($"Custom Rotation: {useCustomRotation}");
            GUILayout.Label($"Rotation Speed: {rotationSpeed}°/s");

            // ACCELERATION DEBUG
            GUILayout.Label("=== ACCELERATION ===");
            if (moveMode == MoveMode.chase)
            {
                float chaseAccelProgress = Mathf.Clamp01(currentChaseAccelTime / chaseAccelerationTime);
                GUILayout.Label($"Chase Accel: {chaseAccelProgress * 100:F0}% ({currentChaseAccelTime:F1}s/{chaseAccelerationTime:F1}s)");
                GUILayout.Label($"Speed: {chaseStartSpeed:F1} → {chaseSpeed:F1} m/s (Current: {agent.speed:F1})");
                GUILayout.Label($"Acceleration: 8 → 15 m/s² (Current: {agent.acceleration:F1})");
            }
            else if (moveMode == MoveMode.charge)
            {
                float chargeAccelProgress = Mathf.Clamp01(currentChargeAccelTime / chargeAccelerationTime);
                GUILayout.Label($"Charge Accel: {chargeAccelProgress * 100:F0}% ({currentChargeAccelTime:F1}s/{chargeAccelerationTime:F1}s)");
                GUILayout.Label($"Speed: {chargeStartSpeed:F1} → {chargeSpeed:F1} m/s (Current: {agent.speed:F1})");
                GUILayout.Label($"Acceleration: 20 → 50 m/s² (Current: {agent.acceleration:F1})");
            }
            else
            {
                GUILayout.Label($"No acceleration (Mode: {moveMode})");
                GUILayout.Label($"Current Agent Acceleration: {agent.acceleration:F1}m/s²");
            }

            // Mode-specific info
            switch (moveMode)
            {
                case MoveMode.wait:
                    GUILayout.Label($"Wait Status: {(isDetectTarget ? "READY TO CHASE" : "SCANNING...")}");
                    float waitTimeSinceLastCharge = Time.time - lastChargeTime;
                    bool waitChargeReady = waitTimeSinceLastCharge >= chargeCooldown;
                    GUILayout.Label($"Charge Ready: {(waitChargeReady ? "YES" : $"NO ({chargeCooldown - waitTimeSinceLastCharge:F1}s)")}");
                    break;
                case MoveMode.chase:
                    GUILayout.Label($"Chase Status: HUNTING");
                    float chaseAccelProgress = Mathf.Clamp01(currentChaseAccelTime / chaseAccelerationTime);
                    GUILayout.Label($"Chase Acceleration: {chaseAccelProgress * 100:F0}%");
                    GUILayout.Label($"Speed: {chaseStartSpeed:F1} → {agent.speed:F1} → {chaseSpeed:F1} m/s");
                    GUILayout.Label($"NavAgent Acceleration: {agent.acceleration:F1}m/s²");
                    break;
                case MoveMode.chargeSearch:
                    float searchProgress = currentChargeSearchTime / chargeSearchTime;
                    GUILayout.Label($"Charge Search: {currentChargeSearchTime:F1}s / {chargeSearchTime:F1}s");
                    GUILayout.Label($"Vision Expansion: {currentChargeSearchVisionBonus:F1}m ({searchProgress * 100:F0}%)");
                    GUILayout.Label($"Range: {chargeSearchVisionBonusStart:F1}m → {chargeSearchVisionBonusMax:F1}m");
                    if (currentTarget != null)
                    {
                        bool chargeTargetValid = CheckChargeTarget();
                        bool chargeSearchTargetValid = CheckChargeTargetDuringSearch();
                        GUILayout.Label($"Normal Charge Target Valid: {chargeTargetValid}");
                        GUILayout.Label($"EXPANDING Search Target Valid: {chargeSearchTargetValid}");
                    }
                    break;
                case MoveMode.charge:
                    GUILayout.Label($"CHARGING!");
                    float chargeAccelProgress = Mathf.Clamp01(currentChargeAccelTime / chargeAccelerationTime);
                    GUILayout.Label($"Charge Acceleration: {chargeAccelProgress * 100:F0}%");
                    GUILayout.Label($"Speed: {chargeStartSpeed:F1} → {agent.speed:F1} → {chargeSpeed:F1} m/s");
                    GUILayout.Label($"NavAgent Acceleration: {agent.acceleration:F1}m/s²");
                    if (isCharging)
                    {
                        float distToChargeTarget = Vector3.Distance(transform.position, chargeTargetPosition);
                        GUILayout.Label($"Dist to Charge Target: {distToChargeTarget:F1}m");
                    }
                    break;
                case MoveMode.attack:
                    GUILayout.Label($"★★★ ATTACKING PLAYER! ★★★");
                    GUILayout.Label($"Attack Time: {currentAttackTime:F1}s / {attackDuration:F1}s");
                    float attackProgress = (currentAttackTime / attackDuration) * 100f;
                    GUILayout.Label($"Attack Progress: {attackProgress:F0}%");

                    float timeSinceLastAttack = Time.time - lastAttackTime;
                    bool attackReadyAgain = timeSinceLastAttack >= attackCooldown;
                    GUILayout.Label($"Attack Ready: {(attackReadyAgain ? "YES" : $"NO ({attackCooldown - timeSinceLastAttack:F1}s)")}");

                    if (currentTarget != null)
                    {
                        bool attackConditions = CheckAttackConditions();
                        GUILayout.Label($"Attack Conditions Met: {(attackConditions ? "YES" : "NO")}");

                        float dist = Vector3.Distance(transform.position, currentTarget.position);
                        Vector3 direction = (currentTarget.position - transform.position).normalized;
                        float angle = Vector3.Angle(transform.forward, direction);
                        bool inForwardVision = angle < forwardVisionAngle / 2;
                        float effectiveAttackRange = inForwardVision ? attackRange + attackForwardBonus : attackRange;

                        GUILayout.Label($"Distance: {dist:F1}m");
                        GUILayout.Label($"Angle: {angle:F1}° / {attackAngle / 2:F1}°");
                        GUILayout.Label($"In Attack Range: {(dist <= effectiveAttackRange ? "YES" : "NO")}");
                        GUILayout.Label($"Effective Range: {effectiveAttackRange:F1}m ({(inForwardVision ? "EXTENDED" : "NORMAL")})");
                    }
                    break;
            }

            GUILayout.EndArea();
        }
#endif


    }
}
