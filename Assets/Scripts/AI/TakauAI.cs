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
        
        [Header("Charge System")]
        [field: SerializeField] public float chargeSpeed { get; private set; } = 12f;
        [field: SerializeField] public float chargeDistance { get; private set; } = 20f;
        [field: SerializeField] public float chargeSearchTime { get; private set; } = 4f;
        [field: SerializeField] public float chargeCooldown { get; private set; } = 8f;
        [field: SerializeField] public float chargeForwardVisionAngle { get; private set; } = 30f;
        [field: SerializeField] public float chargeForwardVisionBonus { get; private set; } = 8f;
        [field: SerializeField] public float chargeMinDistance { get; private set; } = 5f;

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

        [field: SerializeField] public bool isDetectTarget { get; private set; }
        [field: SerializeField] private bool isHit;
        [field: SerializeField] private float currentTimeChasing, currentTimeWaiting;
        [field: SerializeField] private float currentChargeSearchTime, lastChargeTime;
        [field: SerializeField] private Vector3 chargeTargetPosition;
        [field: SerializeField] private bool isCharging;

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
            
            // Initialize charge system
            lastChargeTime = -chargeCooldown; // Allow immediate charge
            
            moveMode = MoveMode.wait;
        }

        private void Update()
        {
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
            }

            FieldOfView();
            HandleRotation();
        }

        private void Chasing()
        {
            if (currentTarget == null) // Jika tidak ada target, kembali ke wait
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
            
            if (distanceToTarget > loseTargetDistance) // Jika target terlalu jauh, hentikan chase
            {
                if (showChaseDebug) Debug.Log("Takau: Target too far away, losing chase");
                SwitchMoveMode(MoveMode.wait);
                return;
            }
            
            if (distanceToTarget <= radiusHit) // Jika sangat dekat dengan target (hit detection)
            {
                if (!isHit)
                {
                    Debug.Log("Takau caught the player - Game Over!");
                    isHit = true;
                    OnPlayerCaught(); // Trigger game over logic atau damage player
                }
                return;
            }            
            agent.speed = chaseSpeed; // Set chase speed and destination
            
            if (distanceToTarget <= minChaseDistance)  // Jika sudah dekat tapi belum hit, perlambat untuk lebih menakutkan
            {
                agent.speed = chaseSpeed * 0.7f; // Perlambat 30%
                if (showChaseDebug) Debug.Log("Takau: Close to target, slowing down");
            }
            
            agent.destination = currentTarget.position; // Set destination (NavMesh will handle pathfinding)
            if(currentTimeChasing > maxTimeChasing)  // Chase timeout logic - jika chase terlalu lama tanpa hasil
            {
                if (showChaseDebug) Debug.Log("Takau: Chase timeout, switching to wait");
                SwitchMoveMode(MoveMode.wait);
            } 
            else if(!isDetectTarget) 
            {
                currentTimeChasing += Time.deltaTime; // Masih chase tapi target tidak terdeteksi FOV (mungkin di belakang obstacle)
                if (showChaseDebug) Debug.Log($"Takau: Target not in FOV, chase time: {currentTimeChasing:F1}s");
            } 
            else if(isDetectTarget)
            {
                currentTimeChasing = 0; // Reset timer jika target masih terlihat
            }
        }
        
        private void OnPlayerCaught()
        {
            if (showChaseDebug) Debug.Log("Takau successfully caught the player!");
            agent.isStopped = true;
            isDetectTarget = false;
            
            // Bisa tambahkan:
            // - Game over logic
            // - Damage player
            // - Play death animation
            // - Trigger cutscene
        }

        private void Waiting()
        {
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
            
            if(currentTimeWaiting > maxTimeWaiting)  // Timer logic hanya jalan jika tidak detect target
            {
                if (showChaseDebug) Debug.Log("Takau: Wait time finished, ready to hunt again");
                
                isHit = false;
                currentTimeWaiting = 0; // Reset wait timer and continue waiting
                
                // Note: Charge detection is now handled automatically in FieldOfView()
                // No need to manually switch to chargeSearch mode
            } 
            else 
            {
                currentTimeWaiting += Time.deltaTime;
            }
        }

        private void ChargeSearching()
        {
            // Rotate to scan for targets during charge search
            transform.Rotate(0, 45f * Time.deltaTime, 0); // Slow rotation while searching
            agent.destination = transform.position;
            agent.speed = 0;
            
            if (showChaseDebug) 
            {
                Debug.Log($"Takau charge searching... Time: {currentChargeSearchTime:F1}s / {chargeSearchTime:F1}s");
            }
            
            // Check for charge target in extended forward vision
            if (CheckChargeTarget())
            {
                if (showChaseDebug) Debug.Log("Takau: Charge target found! Initiating charge!");
                InitiateCharge();
                return;
            }
            
            // Timeout - return to normal behavior
            if (currentChargeSearchTime > chargeSearchTime)
            {
                if (showChaseDebug) Debug.Log("Takau: Charge search timeout, returning to wait");
                
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
        
        private void Charging()
        {
            if (!isCharging) return;
            
            agent.speed = chargeSpeed;
            agent.destination = chargeTargetPosition;
            
            float distanceToChargeTarget = Vector3.Distance(transform.position, chargeTargetPosition);
            
            if (showChaseDebug) 
            {
                Debug.Log($"Takau charging! Distance to target: {distanceToChargeTarget:F2}m");
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
        
        private bool CheckChargeTarget()
        {
            if (currentTarget == null) return false;
            
            Vector3 direction = (currentTarget.position - transform.position).normalized;
            float angleToTarget = Vector3.Angle(transform.forward, direction);
            float distance = Vector3.Distance(transform.position, currentTarget.position);
            
            // Check if target is in charge forward vision cone
            bool inChargeVision = angleToTarget < chargeForwardVisionAngle / 2;
            bool inChargeRange = distance >= chargeMinDistance && distance <= (viewRadius + chargeForwardVisionBonus);
            bool lineOfSight = !Physics.Raycast(transform.position, direction, distance, ObstacleMask, QueryTriggerInteraction.Ignore);
            
            if (showChaseDebug && inChargeVision && inChargeRange)
            {
                Debug.Log($"Charge target check: Angle={angleToTarget:F1}°, Distance={distance:F1}m, LOS={lineOfSight}");
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
                    if (showChaseDebug) Debug.Log("Takau: Exiting charge search mode");
                    break;
                case MoveMode.charge:
                    if (showChaseDebug) Debug.Log("Takau: Exiting charge mode");
                    break;
            }
            
            // Enter new mode
            switch (_moveMode)
            {
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
                    currentChargeSearchTime = 0;
                    agent.isStopped = false;
                    if (showChaseDebug) Debug.Log("Takau: Entering Charge Search Mode - Looking for charge target!");
                    break;
                case MoveMode.charge:
                    agent.isStopped = false;
                    if (showChaseDebug) Debug.Log("Takau: Entering Charge Mode - CHARGING!");
                    break;
            }
            
            moveMode = _moveMode;
            Debug.Log($"Takau: Mode switched to {moveMode}");
        }



        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Light"))
            {
                Debug.Log("Enemy mendeteksi FearShield! Mulai mengejar.");
                SwitchMoveMode(MoveMode.chase);
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
            float extendedRadius = viewRadius + forwardVisionBonus;
            float chargeExtendedRadius = viewRadius + chargeForwardVisionBonus;
            
            Collider[] range = Physics.OverlapSphere(transform.position, Mathf.Max(extendedRadius, chargeExtendedRadius), TargetMask, QueryTriggerInteraction.Ignore);

            if(range.Length > 0) {

                currentTarget = range[0].transform;
               
                Vector3 direction = (currentTarget.position - transform.position).normalized;
                float angleToTarget = Vector3.Angle(transform.forward, direction);
                float distance = Vector3.Distance(transform.position, currentTarget.position);

                // Check charge detection first (higher priority)
                bool inChargeVision = angleToTarget < chargeForwardVisionAngle / 2;
                bool inChargeRange = distance >= chargeMinDistance && distance <= chargeExtendedRadius;
                bool chargeLineOfSight = !Physics.Raycast(transform.position, direction, distance, ObstacleMask, QueryTriggerInteraction.Ignore);
                
                // Check if charge is available (cooldown finished)
                float timeSinceLastCharge = Time.time - lastChargeTime;
                bool chargeReady = timeSinceLastCharge >= chargeCooldown;
                
                // CHARGE DETECTION (highest priority if charge is ready and not already charging)
                if (chargeReady && inChargeVision && inChargeRange && chargeLineOfSight && moveMode != MoveMode.charge && moveMode != MoveMode.chargeSearch)
                {
                    if (showChaseDebug) 
                    {
                        Debug.Log($"CHARGE: Target detected in charge vision! Angle={angleToTarget:F1}°, Distance={distance:F1}m - INITIATING CHARGE!");
                    }
                    
                    // Directly initiate charge without search phase
                    chargeTargetPosition = currentTarget.position;
                    isCharging = true;
                    lastChargeTime = Time.time;
                    SwitchMoveMode(MoveMode.charge);
                    isDetectTarget = false; // Disable normal detection during charge
                    return;
                }

                bool isInFOV = false;
                float effectiveViewRadius = viewRadius;
                
                // Normal chase detection (not during charge modes)
                if (moveMode != MoveMode.charge && moveMode != MoveMode.chargeSearch)
                {
                    // Check if target is in forward extended vision cone (for chase)
                    if (angleToTarget < forwardVisionAngle / 2)
                    {
                        effectiveViewRadius = extendedRadius;
                        isInFOV = true;
                        if (showChaseDebug) Debug.Log($"FOV: Target in EXTENDED forward vision (angle: {angleToTarget:F1}°, dist: {distance:F1}m)");
                    }
                    else if (angleToTarget < viewAngle / 2)
                    {
                        effectiveViewRadius = viewRadius;
                        isInFOV = (distance <= viewRadius);
                        if (showChaseDebug) Debug.Log($"FOV: Target in normal vision (angle: {angleToTarget:F1}°, dist: {distance:F1}m)");
                    }
                    
                    // Process detection for chase modes
                    if (isInFOV && distance <= effectiveViewRadius) {
                        
                        if(!Physics.Raycast(transform.position, direction, distance, ObstacleMask, QueryTriggerInteraction.Ignore)) {
                            isDetectTarget = true;

                            if(moveMode == MoveMode.wait) {
                                if (showChaseDebug) Debug.Log($"FOV: Target detected in wait mode at {distance:F1}m - let Waiting() handle it");
                            }
                            else if(moveMode == MoveMode.chase) {
                                // Already in chase, continue
                            }
                            else {
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
                }
                else
                {
                    // During charge modes, disable normal detection to prevent interference
                    isDetectTarget = false;
                }
            } else {
                isDetectTarget = false;
                currentTarget = null;
            }
        }
#if UNITY_EDITOR
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
            
            // Hit radius (game over zone)
            Gizmos.color = isHit ? Color.red : Color.magenta;
            Gizmos.DrawWireSphere(transform.position, radiusHit);
            
            // Chase lose distance
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, loseTargetDistance);
            
            // Min chase distance
            Gizmos.color = new Color(1f, 0.5f, 0f); // Orange color
            Gizmos.DrawWireSphere(transform.position, minChaseDistance);

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

            // Normal Field of view visualization (CHASE VISION)
            float halfFov = viewAngle / 2f;
            Quaternion leftRayRotation = Quaternion.AngleAxis(-halfFov, Vector3.up);
            Quaternion rightRayRotation = Quaternion.AngleAxis(halfFov, Vector3.up);
            Vector3 leftRayDirection = leftRayRotation * transform.forward;
            Vector3 rightRayDirection = rightRayRotation * transform.forward;
            
            Gizmos.color = isDetectTarget ? Color.red : Color.white;
            Gizmos.DrawRay(transform.position, leftRayDirection * viewRadius);
            Gizmos.DrawRay(transform.position, rightRayDirection * viewRadius);
            
            #if UNITY_EDITOR
            // Label for normal vision
            Vector3 normalVisionLabel = transform.position + transform.forward * viewRadius;
            UnityEditor.Handles.Label(normalVisionLabel, $"Normal Vision: {viewRadius:F1}m / {viewAngle}°");
            #endif
            
            // Extended forward vision cone (CHASE EXTENDED VISION)
            float halfForwardFov = forwardVisionAngle / 2f;
            Quaternion leftForwardRotation = Quaternion.AngleAxis(-halfForwardFov, Vector3.up);
            Quaternion rightForwardRotation = Quaternion.AngleAxis(halfForwardFov, Vector3.up);
            Vector3 leftForwardDirection = leftForwardRotation * transform.forward;
            Vector3 rightForwardDirection = rightForwardRotation * transform.forward;
            
            Gizmos.color = isDetectTarget ? new Color(1f, 0f, 0f, 0.7f) : new Color(1f, 1f, 1f, 0.7f);
            Gizmos.DrawRay(transform.position, leftForwardDirection * (viewRadius + forwardVisionBonus));
            Gizmos.DrawRay(transform.position, rightForwardDirection * (viewRadius + forwardVisionBonus));
            
            #if UNITY_EDITOR
            // Label for extended chase vision
            Vector3 extendedVisionLabel = transform.position + transform.forward * (viewRadius + forwardVisionBonus);
            UnityEditor.Handles.Label(extendedVisionLabel, $"Extended Chase: {viewRadius + forwardVisionBonus:F1}m / {forwardVisionAngle}°");
            #endif
            
            // Charge vision cone (always visible for debugging)
            float halfChargeFov = chargeForwardVisionAngle / 2f;
            Quaternion leftChargeRotation = Quaternion.AngleAxis(-halfChargeFov, Vector3.up);
            Quaternion rightChargeRotation = Quaternion.AngleAxis(halfChargeFov, Vector3.up);
            Vector3 leftChargeDirection = leftChargeRotation * transform.forward;
            Vector3 rightChargeDirection = rightChargeRotation * transform.forward;
            
            // Use different colors based on mode
            if (moveMode == MoveMode.chargeSearch || moveMode == MoveMode.charge)
            {
                Gizmos.color = new Color(0.5f, 0f, 1f); // Bright purple when active
            }
            else
            {
                Gizmos.color = new Color(0.5f, 0f, 1f, 0.4f); // Transparent purple when inactive
            }
            
            float chargeRange = viewRadius + chargeForwardVisionBonus;
            Gizmos.DrawRay(transform.position, leftChargeDirection * chargeRange);
            Gizmos.DrawRay(transform.position, rightChargeDirection * chargeRange);
            
            // Charge extended range sphere
            Gizmos.color = new Color(0.5f, 0f, 1f, 0.2f);
            Gizmos.DrawWireSphere(transform.position, chargeRange);
            
            #if UNITY_EDITOR
            // Label for charge range
            Vector3 chargeLabelPos = transform.position + transform.forward * chargeRange;
            UnityEditor.Handles.Label(chargeLabelPos, $"Charge Range: {chargeRange:F1}m");
            
            // Label for charge angle
            Vector3 angleLabel = transform.position + transform.forward * (chargeRange * 0.7f);
            UnityEditor.Handles.Label(angleLabel, $"Charge Angle: {chargeForwardVisionAngle}°");
            #endif
            
            // Charge min distance circle
            Gizmos.color = new Color(0.5f, 0f, 0.5f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, chargeMinDistance);
            
            // Charge target position
            if (isCharging && moveMode == MoveMode.charge)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(chargeTargetPosition, 1f);
                Gizmos.DrawLine(transform.position, chargeTargetPosition);
                
                #if UNITY_EDITOR
                UnityEditor.Handles.Label(chargeTargetPosition, "CHARGE TARGET");
                #endif
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
            
            GUILayout.BeginArea(new Rect(10, 10, 380, 300));
            GUILayout.Label("=== TAKAU AI DEBUG ===");
            GUILayout.Label($"Mode: {moveMode}");
            GUILayout.Label($"Target: {(currentTarget ? currentTarget.name : "None")}");
            GUILayout.Label($"Detect Target: {isDetectTarget}");
            GUILayout.Label($"Is Hit: {isHit}");
            
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
                
                // Charge vision check with real-time status
                bool inChargeFOV = angle < chargeForwardVisionAngle / 2;
                bool inChargeRange = dist >= chargeMinDistance && dist <= (viewRadius + chargeForwardVisionBonus);
                float timeSinceLastCharge = Time.time - lastChargeTime;
                bool chargeReady = timeSinceLastCharge >= chargeCooldown;
                bool chargeConditionsMet = chargeReady && inChargeFOV && inChargeRange && !blocked;
                
                GUILayout.Label($"=== CHARGE STATUS ===");
                GUILayout.Label($"In Charge FOV ({chargeForwardVisionAngle}°): {inChargeFOV}");
                GUILayout.Label($"In Charge Range ({chargeMinDistance:F1}m-{viewRadius + chargeForwardVisionBonus:F1}m): {inChargeRange}");
                GUILayout.Label($"Charge Ready: {(chargeReady ? "YES" : $"NO ({chargeCooldown - timeSinceLastCharge:F1}s)")}");
                GUILayout.Label($"ALL CHARGE CONDITIONS MET: {(chargeConditionsMet ? "YES - READY TO CHARGE!" : "NO")}");
            }
            
            GUILayout.Label($"Chase Time: {currentTimeChasing:F1}s");
            GUILayout.Label($"Wait Time: {currentTimeWaiting:F1}s");
            GUILayout.Label($"Agent Speed: {agent.velocity.magnitude:F2}");
            GUILayout.Label($"Custom Rotation: {useCustomRotation}");
            GUILayout.Label($"Rotation Speed: {rotationSpeed}°/s");
            
            // Mode-specific info
            switch(moveMode)
            {
                case MoveMode.wait:
                    GUILayout.Label($"Wait Status: {(isDetectTarget ? "READY TO CHASE" : "SCANNING...")}");
                    float timeSinceLastCharge = Time.time - lastChargeTime;
                    bool chargeReady = timeSinceLastCharge >= chargeCooldown;
                    GUILayout.Label($"Charge Ready: {(chargeReady ? "YES" : $"NO ({chargeCooldown - timeSinceLastCharge:F1}s)")}");
                    break;
                case MoveMode.chase:
                    GUILayout.Label($"Chase Status: HUNTING");
                    break;
                case MoveMode.chargeSearch:
                    GUILayout.Label($"Charge Search: {currentChargeSearchTime:F1}s / {chargeSearchTime:F1}s");
                    if (currentTarget != null)
                    {
                        bool chargeTargetValid = CheckChargeTarget();
                        GUILayout.Label($"Charge Target Valid: {chargeTargetValid}");
                    }
                    break;
                case MoveMode.charge:
                    GUILayout.Label($"CHARGING!");
                    if (isCharging)
                    {
                        float distToChargeTarget = Vector3.Distance(transform.position, chargeTargetPosition);
                        GUILayout.Label($"Dist to Charge Target: {distToChargeTarget:F1}m");
                    }
                    break;
            }
            
            GUILayout.EndArea();
        }
#endif
        private void HandleRotation()
        {
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

    }
}
