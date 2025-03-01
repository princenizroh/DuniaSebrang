using UnityEngine;

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

        private Collider fearCollider;
        private Transform playerTransform; 

        void Start()
        {
            fearCollider = GetComponent<Collider>(); 
            playerTransform = transform; 
        }

        void Update()
        {
            bool enemyInSight = CheckEnemyInView();

            if (Input.GetKey(KeyCode.K))
            {
                AdjustFear(scaleSpeed * Time.deltaTime);
            }

            if (Input.GetKeyDown(KeyCode.K)) 
            {
                AdjustFear(-tapDecreaseAmount);
            }

            if (enemyInSight)
            {
                AdjustFear(fearIncreaseRate * Time.deltaTime);
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
    }
}
