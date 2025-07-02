using UnityEngine;
using UnityEngine.AI;
namespace DS
{
    public class TakauAI : MonoBehaviour
    {
        [field: SerializeField] public MoveMode moveMode { get; private set; }
        private Animator animator;
        private Rigidbody rb;

        [Header("Steering")]
        [field: SerializeField] public float chaseSpeed { get; private set; } = 5f;
        [field: SerializeField] public float maxTimeChasing { get; private set; } = 8f;
        [field: SerializeField] public float maxTimeWaiting { get; private set; } = 3f;
        [field: SerializeField] public float radiusHit { get; private set; } = 1.5f;
        
        [Header("Chase Behavior")]
        [field: SerializeField] public float loseTargetDistance { get; private set; } = 15f;
        [field: SerializeField] public float minChaseDistance { get; private set; } = 2f;
        [field: SerializeField] public bool showChaseDebug { get; private set; } = true;

        [Header("Rotation Settings")]
        [field: SerializeField] public float rotationSpeed { get; private set; } = 120f;
        [field: SerializeField] public bool useCustomRotation { get; private set; } = true;
        [field: SerializeField] public float rotationSmoothness { get; private set; } = 0.1f;

        [Header("Transform")]
        [field: SerializeField] public NavMeshAgent agent { get; private set; }
        [field: SerializeField] public Transform currentTarget { get; private set; }

        [Header("Field of View")]
        [field: SerializeField] public float viewRadius { get; private set; } = 12f;
        [field: SerializeField] public float viewAngle { get; private set; } = 90f;
        [field: SerializeField] public float forwardVisionBonus { get; private set; } = 5f;
        [field: SerializeField] public float forwardVisionAngle { get; private set; } = 45f;
        [field: SerializeField] public LayerMask TargetMask { get; private set; }
        [field: SerializeField] public LayerMask ObstacleMask { get; private set; }

        [Header("Boss Stats")]
        [field: SerializeField] public float maxHealth { get; private set; } = 100f;
        [field: SerializeField] public float currentHealth { get; private set; }
        [field: SerializeField] public float attackRange { get; private set; } = 2f;
        [field: SerializeField] public float attackCooldown { get; private set; } = 2f;
        [field: SerializeField] private float lastAttackTime;

        [Header("Charge Attack System")]
        [field: SerializeField] public float chargeSpeed { get; private set; } = 20f; // Lebih cepat lagi
        [field: SerializeField] public float chargeDistance { get; private set; } = 25f; // Jarak charge yang lebih jauh
        [field: SerializeField] public float chargeMinDistance { get; private set; } = 8f; // Jarak minimum untuk charge
        [field: SerializeField] public float chargeSearchTime { get; private set; } = 1.5f; // Lebih cepat mencari
        [field: SerializeField] public float chargeCooldownTime { get; private set; } = 4f; // Cooldown lebih singkat
        [field: SerializeField] public float chargeSearchRotationSpeed { get; private set; } = 90f; // Rotasi lebih cepat
        [field: SerializeField] public float chargeChancePerSecond { get; private set; } = 0.5f; // 50% chance per detik saat chase
        [field: SerializeField] public float directChargeChance { get; private set; } = 0.7f; // 70% chance langsung charge dari wait
        [field: SerializeField] public float chargeAnimationSpeedMultiplier { get; private set; } = 2.5f;
        [field: SerializeField] public bool showChargeDebug { get; private set; } = true;
        
        // Charge state variables
        [field: SerializeField] private bool isCharging { get; set; }
        [field: SerializeField] private bool isChargeCooldown { get; set; }
        [field: SerializeField] private Vector3 chargeTargetPosition { get; set; }
        [field: SerializeField] private Vector3 chargeStartPosition { get; set; }
        [field: SerializeField] private float currentChargeSearchTime { get; set; }
        [field: SerializeField] private float currentChargeCooldownTime { get; set; }
        [field: SerializeField] private float chargeSearchRotation { get; set; }

        [field: SerializeField] public bool isDetectTarget { get; private set; }
        [field: SerializeField] private bool isHit;
        [field: SerializeField] private float currentTimeChasing, currentTimeWaiting;

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
                
            // Initialize boss health
            currentHealth = maxHealth;
            
            // Configure NavMeshAgent rotation settings
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
            
            // Start in wait mode
            moveMode = MoveMode.wait;
        }

        private void Update()
        {
            switch (moveMode)
            {
                case MoveMode.attack:
                    // Attacking();
                    animator.Play("Swiping");
                    animator.speed = 1f; // Normal speed for attack
                    break;
                case MoveMode.chase:
                    Chasing();
                    animator.Play("Run");
                    animator.speed = 1f; // Normal speed for chase
                    break;
                case MoveMode.wait:
                    Waiting();
                    animator.Play("Roaming");
                    animator.speed = 1f; // Normal speed for wait
                    break;
                case MoveMode.chargeSearch:
                    ChargeSearching();
                    animator.Play("Roaming"); // Bisa diganti ke animasi search khusus
                    animator.speed = 1f; // Normal speed for search
                    break;
                case MoveMode.charge:
                    Charging();
                    animator.Play("Run"); // Animasi charge lebih cepat
                    animator.speed = chargeAnimationSpeedMultiplier; // Speed multiplier untuk charge
                    break;
                case MoveMode.dying:
                    // Dying();
                    animator.Play("Dying");
                    animator.speed = 1f; // Normal speed for death
                    break;
            }

            FieldOfView();
            
            // Handle custom rotation after all movement logic
            HandleRotation();
        }

        // private void Attacking()
        // {
        //     // Stop moving during attack
        //     agent.speed = 0;
        //     agent.destination = transform.position;
            
        //     // Check if target is in attack range
        //     if (currentTarget != null)
        //     {
        //         float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);
                
        //         if (distanceToTarget <= attackRange)
        //         {
        //             // Face the target
        //             Vector3 direction = (currentTarget.position - transform.position).normalized;
        //             transform.rotation = Quaternion.LookRotation(direction);
                    
        //             // Attack if cooldown is ready
        //             if (Time.time >= lastAttackTime + attackCooldown)
        //             {
        //                 PerformAttack();
        //                 lastAttackTime = Time.time;
        //             }
        //         }
        //         else
        //         {
        //             // Target too far, switch to chase
        //             SwitchMoveMode(MoveMode.chase);
        //         }
        //     }
        //     else
        //     {
        //         // No target, go to wait mode
        //         SwitchMoveMode(MoveMode.wait);
        //     }
        // }
        
        // private void PerformAttack()
        // {
        //     Debug.Log("Takau performs attack!");
            
        //     // Check for targets in attack range
        //     Collider[] targets = Physics.OverlapSphere(transform.position, attackRange, TargetMask);
            
        //     foreach (Collider target in targets)
        //     {
        //         // Deal damage to player or trigger game over
        //         Debug.Log("Takau hits target: " + target.name);
        //         // Add damage logic here
        //     }
        // }

        private void Chasing()
        {
            // Jika tidak ada target, kembali ke wait
            if (currentTarget == null)
            {
                if (showChaseDebug) Debug.Log("Takau: No target found, switching to wait");
                SwitchMoveMode(MoveMode.wait);
                return;
            }
            
            float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);
            
            if (showChaseDebug) 
            {
                Debug.Log($"Takau chasing: Distance to target = {distanceToTarget:F2}m");
            }
            
            // CHECK FOR CHARGE OPPORTUNITY - prioritas tertinggi
            // Hanya lakukan charge jika tidak dalam cooldown dan jarak memungkinkan
            if (!isChargeCooldown && distanceToTarget >= chargeMinDistance && distanceToTarget <= chargeDistance)
            {
                // Peluang untuk charge berdasarkan parameter
                float chargeChance = chargeChancePerSecond * Time.deltaTime;
                if (Random.value < chargeChance)
                {
                    if (showChaseDebug) Debug.Log($"Takau: Initiating charge attack! Distance: {distanceToTarget:F2}m");
                    SwitchMoveMode(MoveMode.chargeSearch);
                    return;
                }
            }
            
            // Jika target terlalu jauh, hentikan chase
            if (distanceToTarget > loseTargetDistance)
            {
                if (showChaseDebug) Debug.Log("Takau: Target too far away, losing chase");
                SwitchMoveMode(MoveMode.wait);
                return;
            }
            
            // Jika sangat dekat dengan target (hit detection)
            if (distanceToTarget <= radiusHit)
            {
                if (!isHit)
                {
                    Debug.Log("Takau caught the player - Game Over!");
                    isHit = true;
                    // Trigger game over logic atau damage player
                    OnPlayerCaught();
                }
                return;
            }
            
            // Set chase speed and destination
            agent.speed = chaseSpeed;
            
            // Jika sudah dekat tapi belum hit, perlambat untuk lebih menakutkan
            if (distanceToTarget <= minChaseDistance)
            {
                agent.speed = chaseSpeed * 0.7f; // Perlambat 30%
                if (showChaseDebug) Debug.Log("Takau: Close to target, slowing down");
            }
            
            // Set destination (NavMesh will handle pathfinding)
            agent.destination = currentTarget.position;
            
            // Manual rotation akan dihandle oleh HandleRotation() method
            // Ini memberikan rotasi yang lebih responsif daripada NavMesh default

            // Chase timeout logic - jika chase terlalu lama tanpa hasil
            if(currentTimeChasing > maxTimeChasing) 
            {
                if (showChaseDebug) Debug.Log("Takau: Chase timeout, switching to wait");
                SwitchMoveMode(MoveMode.wait);
            } 
            else if(!isDetectTarget) 
            {
                // Masih chase tapi target tidak terdeteksi FOV (mungkin di belakang obstacle)
                currentTimeChasing += Time.deltaTime;
                if (showChaseDebug) Debug.Log($"Takau: Target not in FOV, chase time: {currentTimeChasing:F1}s");
            } 
            else if(isDetectTarget)
            {
                // Reset timer jika target masih terlihat
                currentTimeChasing = 0;
            }
        }
        
        private void OnPlayerCaught()
        {
            // Method untuk handle ketika player tertangkap
            // Bisa trigger game over scene, damage player, dll
            if (showChaseDebug) Debug.Log("Takau successfully caught the player!");
            
            // Stop movement
            agent.isStopped = true;
            
            // Disable further detection
            isDetectTarget = false;
            
            // Bisa tambahkan:
            // - Game over logic
            // - Damage player
            // - Play death animation
            // - Trigger cutscene
        }

        private void Waiting()
        {
            // Stop movement saat waiting
            agent.destination = transform.position;
            agent.speed = 0;
            
            // Handle charge cooldown first
            if (isChargeCooldown)
            {
                if (showChaseDebug) 
                {
                    Debug.Log($"Takau cooldown... Time: {currentChargeCooldownTime:F1}s / {chargeCooldownTime:F1}s");
                }
                
                // Check if player still in range during cooldown
                if (isDetectTarget && currentTarget != null)
                {
                    if (showChaseDebug) Debug.Log("Takau: Player detected during cooldown! Switching to chase.");
                    isChargeCooldown = false;
                    SwitchMoveMode(MoveMode.chase);
                    return;
                }
                
                // Cooldown timer
                if (currentChargeCooldownTime > chargeCooldownTime)
                {
                    if (showChaseDebug) Debug.Log("Takau: Cooldown finished, ready for next charge cycle");
                    isChargeCooldown = false;
                    currentTimeWaiting = 0; // Reset wait timer
                }
                else
                {
                    currentChargeCooldownTime += Time.deltaTime;
                }
                return;
            }
            
            if (showChaseDebug) 
            {
                Debug.Log($"Takau waiting... Time: {currentTimeWaiting:F1}s / {maxTimeWaiting:F1}s - Detect: {isDetectTarget}");
            }
            
            // PRIORITY: Jika detect target, cek apakah harus langsung charge atau chase
            if (isDetectTarget && currentTarget != null)
            {
                float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);
                
                // Jika jarak jauh dan tidak dalam cooldown, prioritaskan charge
                if (!isChargeCooldown && distanceToTarget >= chargeMinDistance && distanceToTarget <= chargeDistance)
                {
                    // Peluang langsung charge dari wait mode (lebih tinggi dari chase mode)
                    if (Random.value < directChargeChance)
                    {
                        if (showChaseDebug) Debug.Log($"Takau: Target detected at charge range! Direct charge attack! Distance: {distanceToTarget:F2}m");
                        SwitchMoveMode(MoveMode.chargeSearch);
                        return;
                    }
                }
                
                if (showChaseDebug) Debug.Log("Takau: Target detected during wait! Switching to chase immediately.");
                SwitchMoveMode(MoveMode.chase);
                return;
            }
            
            // Timer logic hanya jalan jika tidak detect target
            if(currentTimeWaiting > maxTimeWaiting) 
            {
                if (showChaseDebug) Debug.Log("Takau: Wait time finished, entering charge search mode");
                
                // Reset hit state jika ada
                isHit = false;
                
                // Masuk ke charge search mode instead of reset timer
                SwitchMoveMode(MoveMode.chargeSearch);
            } 
            else 
            {
                currentTimeWaiting += Time.deltaTime;
            }
        }

        private void ChargeSearching()
        {
            // Stop movement saat searching
            agent.destination = transform.position;
            agent.speed = 0;
            
            if (showChargeDebug) 
            {
                Debug.Log($"Takau charge searching... Time: {currentChargeSearchTime:F1}s / {chargeSearchTime:F1}s");
            }
            
            // Rotate slowly to search for player
            chargeSearchRotation += chargeSearchRotationSpeed * Time.deltaTime;
            Vector3 searchDirection = Quaternion.Euler(0, chargeSearchRotation, 0) * Vector3.forward;
            transform.rotation = Quaternion.LookRotation(searchDirection);
            
            // Check if player detected during search
            if (isDetectTarget && currentTarget != null)
            {
                if (showChargeDebug) Debug.Log("Takau: Target found during charge search! Initiating charge!");
                
                // Set charge target to current player position
                chargeTargetPosition = currentTarget.position;
                chargeStartPosition = transform.position;
                
                // Start charging
                SwitchMoveMode(MoveMode.charge);
                return;
            }
            
            // Search timeout
            if (currentChargeSearchTime > chargeSearchTime)
            {
                if (showChargeDebug) Debug.Log("Takau: Charge search timeout, entering cooldown");
                SwitchMoveMode(MoveMode.wait);
                isChargeCooldown = true;
                currentChargeCooldownTime = 0;
            }
            else
            {
                currentChargeSearchTime += Time.deltaTime;
            }
        }
        
        private void Charging()
        {
            if (showChargeDebug) 
            {
                float chargeProgress = Vector3.Distance(chargeStartPosition, transform.position);
                Debug.Log($"Takau charging! Progress: {chargeProgress:F1}m / {chargeDistance:F1}m");
            }
            
            // Set charge speed
            agent.speed = chargeSpeed;
            
            // Always move towards the locked target position
            agent.destination = chargeTargetPosition;
            
            // Face the charge direction
            Vector3 chargeDirection = (chargeTargetPosition - transform.position).normalized;
            if (chargeDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(chargeDirection);
            }
            
            // Check if charge distance reached
            float distanceTraveled = Vector3.Distance(chargeStartPosition, transform.position);
            if (distanceTraveled >= chargeDistance)
            {
                if (showChargeDebug) Debug.Log("Takau: Charge distance completed, entering cooldown");
                SwitchMoveMode(MoveMode.wait);
                isChargeCooldown = true;
                currentChargeCooldownTime = 0;
                return;
            }
            
            // Check if hit player during charge
            if (currentTarget != null)
            {
                float distanceToPlayer = Vector3.Distance(transform.position, currentTarget.position);
                if (distanceToPlayer <= radiusHit)
                {
                    if (!isHit)
                    {
                        Debug.Log("Takau charge attack hit! Game Over!");
                        isHit = true;
                        OnPlayerCaught();
                    }
                    return;
                }
            }
            
            // Check if reached destination (charge target position)
            if (agent.remainingDistance < agent.stoppingDistance)
            {
                if (showChargeDebug) Debug.Log("Takau: Reached charge target position, entering cooldown");
                SwitchMoveMode(MoveMode.wait);
                isChargeCooldown = true;
                currentChargeCooldownTime = 0;
            }
        }

        // private void Dying()
        // {
        //     // Stop all movement
        //     agent.isStopped = true;
        //     agent.velocity = Vector3.zero;
            
        //     // Disable further AI behavior
        //     isDetectTarget = false;
            
        //     // This method handles the death state
        //     // Could add death timer or destruction logic here
        //     Debug.Log("Takau is in dying state...");
        // }

        private void SwitchMoveMode (MoveMode _moveMode)
        {
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
                    if (showChargeDebug) Debug.Log("Takau: Exiting charge search mode");
                    break;
                case MoveMode.charge:
                    if (showChargeDebug) Debug.Log("Takau: Exiting charge mode");
                    break;
            }
            
            // Enter new mode
            switch (_moveMode)
            {
                case MoveMode.attack:
                    Debug.Log("Takau: Entering Attack Mode");
                    break;
                case MoveMode.chase:
                    currentTimeChasing = 0;
                    agent.isStopped = false;
                    if (showChaseDebug) Debug.Log("Takau: Entering Chase Mode - HUNTING!");
                    break;
                case MoveMode.wait:
                    agent.destination = transform.position;
                    currentTimeWaiting = 0;
                    agent.isStopped = false;
                    if (showChaseDebug) Debug.Log("Takau: Entering Wait Mode - Scanning...");
                    break;
                case MoveMode.chargeSearch:
                    agent.destination = transform.position;
                    currentChargeSearchTime = 0;
                    chargeSearchRotation = transform.eulerAngles.y; // Start from current rotation
                    agent.isStopped = false;
                    if (showChargeDebug) Debug.Log("Takau: Entering Charge Search Mode - Looking for target to charge!");
                    break;
                case MoveMode.charge:
                    agent.isStopped = false;
                    isCharging = true;
                    if (showChargeDebug) Debug.Log($"Takau: Entering Charge Mode - Target locked at {chargeTargetPosition}!");
                    break;
                case MoveMode.dying:
                    agent.destination = transform.position;
                    agent.isStopped = true;
                    Debug.Log("Takau is dying...");
                    break;
            }
            
            moveMode = _moveMode;
            Debug.Log($"Takau: Mode switched to {moveMode}");
        }



        private void OnTriggerEnter(Collider other)
        {
            // Jika menyentuh FearShield (bukan langsung Player), mulai mengejar
            if (other.CompareTag("Light"))
            {
                Debug.Log("Enemy mendeteksi FearShield! Mulai mengejar.");
                SwitchMoveMode(MoveMode.chase);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            // Jika FearShield keluar dari area deteksi, berhenti mengejar
            if (other.CompareTag("Light"))
            {
                Debug.Log("FearShield keluar dari jangkauan! Enemy berhenti mengejar.");
                // agent.ResetPath();
            }
        }

        private void FieldOfView()
        {
            // Extended forward vision: Check larger radius for targets in front
            float extendedRadius = viewRadius + forwardVisionBonus;
            
            Collider[] range = Physics.OverlapSphere(transform.position, extendedRadius, TargetMask, QueryTriggerInteraction.Ignore);

            if(range.Length > 0) {

                currentTarget = range[0].transform;
               
                Vector3 direction = (currentTarget.position - transform.position).normalized;
                float angleToTarget = Vector3.Angle(transform.forward, direction);
                float distance = Vector3.Distance(transform.position, currentTarget.position);

                // Enhanced FOV logic with extended forward vision
                bool isInFOV = false;
                float effectiveViewRadius = viewRadius;
                
                // Check if target is in forward extended vision cone
                if (angleToTarget < forwardVisionAngle / 2)
                {
                    // Target is in front - use extended radius
                    effectiveViewRadius = extendedRadius;
                    isInFOV = true;
                    if (showChaseDebug) Debug.Log($"FOV: Target in EXTENDED forward vision (angle: {angleToTarget:F1}°, dist: {distance:F1}m)");
                }
                else if (angleToTarget < viewAngle / 2)
                {
                    // Target is in normal FOV but not in forward cone - use normal radius
                    effectiveViewRadius = viewRadius;
                    isInFOV = (distance <= viewRadius);
                    if (showChaseDebug) Debug.Log($"FOV: Target in normal vision (angle: {angleToTarget:F1}°, dist: {distance:F1}m)");
                }
                
                // Check if target is within effective range and FOV
                if (isInFOV && distance <= effectiveViewRadius) {
                    
                    // Line of sight check
                    if(!Physics.Raycast(transform.position, direction, distance, ObstacleMask, QueryTriggerInteraction.Ignore)) {
                        isDetectTarget = true;

                        // Mode-specific detection handling
                        if(moveMode == MoveMode.wait) {
                            // Wait mode: detection akan dihandle oleh Waiting() method
                            if (showChaseDebug) Debug.Log($"FOV: Target detected in wait mode at {distance:F1}m - let Waiting() handle it");
                        }
                        else if(moveMode != MoveMode.chase && moveMode != MoveMode.dying) {
                            // Other modes: switch to chase immediately
                            if (showChaseDebug) Debug.Log($"FOV: Target detected at {distance:F1}m, switching to chase");
                            SwitchMoveMode(MoveMode.chase);
                        }
                    } else {
                        isDetectTarget = false;
                        if (showChaseDebug && range.Length > 0) Debug.Log("FOV: Target blocked by obstacle");
                    }
                } else {
                    isDetectTarget = false;
                    if (showChaseDebug && range.Length > 0) {
                        if (!isInFOV) {
                            Debug.Log($"FOV: Target outside view angle ({angleToTarget:F1}° > {viewAngle/2:F1}°)");
                        } else {
                            Debug.Log($"FOV: Target too far ({distance:F1}m > {effectiveViewRadius:F1}m)");
                        }
                    }
                }
            } else {
                isDetectTarget = false;
                currentTarget = null;
            }
        }

        private void OnDrawGizmos()
        {
            if(agent == null) return;

            // Stopping distance
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, agent.stoppingDistance);

            // Normal view radius
            Gizmos.color = isDetectTarget ? Color.green : Color.blue;
            Gizmos.DrawWireSphere(transform.position, viewRadius);
            
            // Extended forward vision radius
            Gizmos.color = isDetectTarget ? new Color(0f, 1f, 0f, 0.3f) : new Color(0f, 0f, 1f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, viewRadius + forwardVisionBonus);
            
            // Attack range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
            
            // Hit radius (game over zone)
            Gizmos.color = isHit ? Color.red : Color.magenta;
            Gizmos.DrawWireSphere(transform.position, radiusHit);
            
            // Chase lose distance
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, loseTargetDistance);
            
            // Min chase distance
            Gizmos.color = new Color(1f, 0.5f, 0f); // Orange color
            Gizmos.DrawWireSphere(transform.position, minChaseDistance);
            
            // Charge system visualization
            if (showChargeDebug)
            {
                // Charge minimum distance (inner ring)
                Gizmos.color = new Color(1f, 1f, 0f, 0.3f); // Yellow with transparency
                Gizmos.DrawWireSphere(transform.position, chargeMinDistance);
                
                // Charge maximum distance (outer ring)
                Gizmos.color = new Color(1f, 0f, 1f, 0.4f); // Purple for charge range
                Gizmos.DrawWireSphere(transform.position, chargeDistance);
                
                // Charge active state visualization
                if (moveMode == MoveMode.charge || moveMode == MoveMode.chargeSearch)
                {
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawWireSphere(transform.position, chargeDistance);
                    
                    // Show charge target if in charge mode
                    if (moveMode == MoveMode.charge)
                    {
                        Gizmos.color = Color.red;
                        Gizmos.DrawSphere(chargeTargetPosition, 0.5f);
                        Gizmos.DrawLine(transform.position, chargeTargetPosition);
                    }
                }
                
                // Charge cooldown visualization
                if (isChargeCooldown)
                {
                    Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.5f); // Gray for cooldown
                    Gizmos.DrawWireCube(transform.position + Vector3.up * 3f, Vector3.one * 0.5f);
                }
            }

            // Line to target with different colors based on mode
            if(currentTarget != null && isDetectTarget)
            {
                switch(moveMode)
                {
                    case MoveMode.chase:
                        Gizmos.color = Color.red;
                        break;
                    case MoveMode.wait:
                        Gizmos.color = Color.yellow;
                        break;
                    case MoveMode.chargeSearch:
                        Gizmos.color = Color.magenta;
                        break;
                    case MoveMode.charge:
                        Gizmos.color = Color.cyan;
                        break;
                    default:
                        Gizmos.color = Color.green;
                        break;
                }
                Gizmos.DrawLine(transform.position, currentTarget.position);
                
                // Draw distance text (in Scene view)
                float dist = Vector3.Distance(transform.position, currentTarget.position);
                Vector3 midPoint = (transform.position + currentTarget.position) / 2;
                
                #if UNITY_EDITOR
                UnityEditor.Handles.Label(midPoint, $"{dist:F1}m");
                #endif
            }
            
            // Enhanced charge progress visualization
            if (moveMode == MoveMode.charge && chargeStartPosition != Vector3.zero)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(chargeStartPosition, transform.position);
                
                #if UNITY_EDITOR
                float chargeProgress = Vector3.Distance(chargeStartPosition, transform.position);
                Vector3 chargeMidPoint = (chargeStartPosition + transform.position) / 2;
                UnityEditor.Handles.Label(chargeMidPoint, $"Charge: {chargeProgress:F1}m/{chargeDistance:F1}m");
                #endif
            }

            // Normal Field of view visualization
            float halfFov = viewAngle / 2f;
            Quaternion leftRayRotation = Quaternion.AngleAxis(-halfFov, Vector3.up);
            Quaternion rightRayRotation = Quaternion.AngleAxis(halfFov, Vector3.up);
            Vector3 leftRayDirection = leftRayRotation * transform.forward;
            Vector3 rightRayDirection = rightRayRotation * transform.forward;
            
            Gizmos.color = isDetectTarget ? Color.red : Color.white;
            Gizmos.DrawRay(transform.position, leftRayDirection * viewRadius);
            Gizmos.DrawRay(transform.position, rightRayDirection * viewRadius);
            
            // Extended forward vision cone
            float halfForwardFov = forwardVisionAngle / 2f;
            Quaternion leftForwardRotation = Quaternion.AngleAxis(-halfForwardFov, Vector3.up);
            Quaternion rightForwardRotation = Quaternion.AngleAxis(halfForwardFov, Vector3.up);
            Vector3 leftForwardDirection = leftForwardRotation * transform.forward;
            Vector3 rightForwardDirection = rightForwardRotation * transform.forward;
            
            Gizmos.color = isDetectTarget ? new Color(1f, 0f, 0f, 0.7f) : new Color(1f, 1f, 1f, 0.7f);
            Gizmos.DrawRay(transform.position, leftForwardDirection * (viewRadius + forwardVisionBonus));
            Gizmos.DrawRay(transform.position, rightForwardDirection * (viewRadius + forwardVisionBonus));
            
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

        // Debug helper method untuk monitoring Takau state
        private void OnGUI()
        {
            if (!showChaseDebug && !showChargeDebug) return;
            
            GUILayout.BeginArea(new Rect(10, 10, 400, 380));
            GUILayout.Label("=== TAKAU AI DEBUG ===");
            GUILayout.Label($"Mode: {moveMode}");
            GUILayout.Label($"Target: {(currentTarget ? currentTarget.name : "None")}");
            GUILayout.Label($"Detect Target: {isDetectTarget}");
            GUILayout.Label($"Is Hit: {isHit}");
            GUILayout.Label($"Health: {currentHealth}/{maxHealth}");
            
            if (currentTarget != null)
            {
                float dist = Vector3.Distance(transform.position, currentTarget.position);
                GUILayout.Label($"Distance: {dist:F2}m");
                
                Vector3 direction = (currentTarget.position - transform.position).normalized;
                float angle = Vector3.Angle(transform.forward, direction);
                GUILayout.Label($"Angle to Target: {angle:F1}°");
                
                // FOV Check details
                bool inNormalFOV = angle < viewAngle / 2;
                bool inExtendedFOV = angle < forwardVisionAngle / 2;
                GUILayout.Label($"In Normal FOV ({viewAngle}°): {inNormalFOV}");
                GUILayout.Label($"In Extended FOV ({forwardVisionAngle}°): {inExtendedFOV}");
                
                // Vision range info
                float effectiveRange = inExtendedFOV ? viewRadius + forwardVisionBonus : viewRadius;
                GUILayout.Label($"Effective Range: {effectiveRange:F1}m");
                GUILayout.Label($"Normal Range: {viewRadius:F1}m");
                GUILayout.Label($"Extended Range: {viewRadius + forwardVisionBonus:F1}m");
                
                // Obstacle check
                bool blocked = Physics.Raycast(transform.position, direction, dist, ObstacleMask);
                GUILayout.Label($"Line of Sight: {(blocked ? "BLOCKED" : "CLEAR")}");
            }
            
            GUILayout.Label($"Chase Time: {currentTimeChasing:F1}s");
            GUILayout.Label($"Wait Time: {currentTimeWaiting:F1}s");
            
            // Charge system info
            GUILayout.Label("--- CHARGE SYSTEM ---");
            GUILayout.Label($"Is Charging: {isCharging}");
            GUILayout.Label($"Charge Cooldown: {isChargeCooldown}");
            GUILayout.Label($"Charge Search Time: {currentChargeSearchTime:F1}s");
            GUILayout.Label($"Charge Cooldown Time: {currentChargeCooldownTime:F1}s");
            GUILayout.Label($"Charge Speed: {chargeSpeed}");
            GUILayout.Label($"Charge Distance: {chargeDistance}m");
            GUILayout.Label($"Charge Min Distance: {chargeMinDistance}m");
            GUILayout.Label($"Direct Charge Chance: {directChargeChance * 100:F0}%");
            GUILayout.Label($"Chase Charge Chance: {chargeChancePerSecond * 100:F0}%/s");
            
            if (moveMode == MoveMode.charge && chargeStartPosition != Vector3.zero)
            {
                float chargeProgress = Vector3.Distance(chargeStartPosition, transform.position);
                GUILayout.Label($"Charge Progress: {chargeProgress:F1}m / {chargeDistance:F1}m");
                GUILayout.Label($"Charge Target: {chargeTargetPosition}");
            }
            
            GUILayout.Label($"Agent Speed: {agent.velocity.magnitude:F2}");
            GUILayout.Label($"Custom Rotation: {useCustomRotation}");
            GUILayout.Label($"Rotation Speed: {rotationSpeed}°/s");
            
            // Mode-specific info
            switch(moveMode)
            {
                case MoveMode.wait:
                    string waitStatus = isChargeCooldown ? "COOLDOWN" : (isDetectTarget ? "READY TO CHASE" : "SCANNING...");
                    GUILayout.Label($"Wait Status: {waitStatus}");
                    break;
                case MoveMode.chase:
                    GUILayout.Label($"Chase Status: HUNTING");
                    break;
                case MoveMode.chargeSearch:
                    GUILayout.Label($"Charge Search: LOOKING FOR TARGET");
                    break;
                case MoveMode.charge:
                    GUILayout.Label($"Charge Status: CHARGING!");
                    break;
            }
            
            GUILayout.EndArea();
        }

        // public void TakeDamage(float damage)
        // {
        //     if (moveMode == MoveMode.dying) return;
            
        //     currentHealth -= damage;
        //     Debug.Log($"Takau took {damage} damage. Health: {currentHealth}/{maxHealth}");
            
        //     if (currentHealth <= 0)
        //     {
        //         currentHealth = 0;
        //         SwitchMoveMode(MoveMode.dying);
        //     }
        // }
    
        // public bool IsDead()
        // {
        //     return currentHealth <= 0;
        // }

        private void HandleRotation()
        {
            if (!useCustomRotation) return;
            
            // Special handling for charge search mode
            if (moveMode == MoveMode.chargeSearch)
            {
                // Rotation is handled in ChargeSearching() method
                return;
            }
            
            // Special handling for charge mode
            if (moveMode == MoveMode.charge)
            {
                // Rotation is handled in Charging() method
                return;
            }
            
            // Different rotation behavior based on mode
            if (moveMode == MoveMode.wait)
            {
                // In wait mode, only rotate if we actually detect the target
                if (!isDetectTarget || currentTarget == null) return;
            }
            else
            {
                // In other modes, rotate if we have a target (even if not in FOV)
                if (currentTarget == null) return;
            }
            
            Vector3 direction = (currentTarget.position - transform.position).normalized;
            direction.y = 0; // Keep rotation only on Y-axis
            
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                
                // Smooth rotation based on mode
                float currentRotationSpeed = rotationSpeed;
                
                // Faster rotation when chasing for more aggressive feel
                if (moveMode == MoveMode.chase)
                {
                    currentRotationSpeed = rotationSpeed * 1.5f; // 50% faster rotation when chasing
                }
                else if (moveMode == MoveMode.wait)
                {
                    currentRotationSpeed = rotationSpeed * 0.8f; // Slower rotation when waiting (more cautious)
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

    }
}
