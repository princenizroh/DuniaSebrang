using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DS
{
    public class FearSystemManager : MonoBehaviour
    {
        [Header("Fear Settings")]
        public float scaleSpeed = 2f; 
        public float tapDecreaseAmount = 0.2f; 
        public float fearIncreaseRate = 0.2f;
        public float naturalFearGain = 0.1f; 
        public float calmThreshold = 1f; 
        public float minScale = 0f; 
        public float maxScale = 5f; 

        [Header("View Settings")]
        public float viewAngle = 60f; 
        public float viewDistance = 10f; 
        public LayerMask enemyLayer; 

        [Header("AI References")]
        public TakauAI[] takauAIs;
        public KuntiAI[] kuntiAIs;
        public HandAI[] handAIs; 

        [Header("Chase Fear Settings")]
        public float chaseFearIncreaseRate = 0.5f;

        [Header("Tap Input Settings")]
        public float tapBufferDecayRate = 5f; // Kecepatan smooth decrease setelah tap

        private Collider fearCollider;
        private Transform playerTransform;
        
        // INPUT BUFFER SYSTEM - INI YANG BARU
        private float tapBuffer = 0f;

        void Start()
        {
            fearCollider = GetComponent<Collider>(); 
            playerTransform = transform; 
        }

        void Update()
        {
            bool enemyInSight = CheckEnemyInView();
            bool enemyChasing = CheckEnemyChasing();

            // HOLD CTRL - Increase fear (TIDAK BERUBAH)
            if (Input.GetKey(KeyCode.RightControl))
            {
                AdjustFear(scaleSpeed * Time.deltaTime);
            }

            // TAP CTRL - Decrease fear (SISTEM BARU)
            if (Input.GetKeyDown(KeyCode.RightControl))
            {
                tapBuffer += tapDecreaseAmount; // Tambah ke buffer
            }

            // SMOOTH DECREASE dari buffer
            if (tapBuffer > 0)
            {
                float decreaseThisFrame = tapBufferDecayRate * Time.deltaTime;
                decreaseThisFrame = Mathf.Min(decreaseThisFrame, tapBuffer);
                
                AdjustFear(-decreaseThisFrame);
                tapBuffer -= decreaseThisFrame;
            }

            // Enemy detection (TIDAK BERUBAH)
            if (enemyInSight)
            {
                AdjustFear(fearIncreaseRate * Time.deltaTime);
            }

            if (enemyChasing)
            {
                AdjustFear(chaseFearIncreaseRate * Time.deltaTime);
            }

            fearCollider.enabled = transform.localScale.x > 0.01f;
        }

        private void AdjustFear(float amount)
        {
            float newScale = Mathf.Clamp(transform.localScale.y + amount, minScale, maxScale);
            transform.localScale = new Vector3(newScale, newScale, newScale);
        }

        private bool CheckEnemyInView()
        {
            Collider[] enemies = Physics.OverlapSphere(playerTransform.position, viewDistance, enemyLayer);

            foreach (Collider enemy in enemies)
            {
                Vector3 dirToEnemy = (enemy.transform.position - playerTransform.position).normalized;
                float angleToEnemy = Vector3.Angle(playerTransform.forward, dirToEnemy);

                if (angleToEnemy < viewAngle / 2f)
                {
                    return true;
                }
            }
            return false;
        }

        private bool CheckEnemyChasing()
        {
            foreach (var ai in takauAIs)
            {
                if (ai != null && ai.moveMode == MoveMode.chase)
                {
                    float dist = Vector3.Distance(ai.transform.position, playerTransform.position);
                    if (dist < viewDistance && dist > ai.attackRange)
                        return true;
                }
            }
            foreach (var ai in kuntiAIs)
            {
                if (ai != null && ai.moveMode == MoveMode.chase)
                {
                    float dist = Vector3.Distance(ai.transform.position, playerTransform.position);
                    if (dist < viewDistance && dist > ai.attackRange)
                        return true;
                }
            }
            foreach (var ai in handAIs)
            {
                if (ai != null && ai.moveMode == MoveMode.chase)
                {
                    float dist = Vector3.Distance(ai.transform.position, playerTransform.position);
                    if (dist < viewDistance && dist > ai.attackRange)
                        return true;
                }
            }
            return false;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            // Draw view distance
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, viewDistance);

            // Draw view angle
            Vector3 forward = transform.forward;
            float halfAngle = viewAngle / 2f;
            Quaternion leftRay = Quaternion.AngleAxis(-halfAngle, Vector3.up);
            Quaternion rightRay = Quaternion.AngleAxis(halfAngle, Vector3.up);
            Vector3 leftDir = leftRay * forward;
            Vector3 rightDir = rightRay * forward;
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, leftDir * viewDistance);
            Gizmos.DrawRay(transform.position, rightDir * viewDistance);

            // Draw label for radius
            Handles.color = Color.white;
            Handles.Label(transform.position + Vector3.up * 1.5f, $"Fear View Radius: {viewDistance}m\nFear Angle: {viewAngle}°");
        }
#endif
    }
}