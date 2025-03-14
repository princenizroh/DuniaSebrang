using UnityEngine;
using UnityEngine.AI;


namespace DS
{
    public enum MoveMode 
    {
        patrol,
        chase,
        wait
    }
    public class EnemyAI : MonoBehaviour
    {
        [field: SerializeField] public MoveMode moveMode { get; private set; }
        private Animator Animator;
        private Rigidbody Rigidbody;

        [Header("Steering")]
        [field: SerializeField] public float patrolSpeed { get; private set; }
        [field: SerializeField] public float chaseSpeed { get; private set; }
        [field: SerializeField] public float maxTimeChasing { get; private set; }
        [field: SerializeField] public float maxTimeWaiting { get; private set; }
        [field: SerializeField] public float radiusHit { get; private set; }

        [Header("Transform")]
        [field: SerializeField] public Transform[] patrolPoint { get; private set; }
        [field: SerializeField] public NavMeshAgent Agent { get; private set; }
        [field: SerializeField] public Transform currentTarget { get; private set; }
        private bool isChasing = false;

        [Header("Field of View")]
        [field: SerializeField] public float viewRadius { get; private set; }
        [field: SerializeField] public float viewAngle { get; private set; }
        [field: SerializeField] public LayerMask TargetMask { get; private set; }
        [field: SerializeField] public LayerMask ObstacleMask { get; private set; }

        [field: SerializeField] public bool isDetectTarget { get; private set; }
        [field: SerializeField] private Vector3 destination;
        [field: SerializeField] private int index_patrolPoint;
        [field: SerializeField] private bool isHit;
        [field: SerializeField] private float currentTimeChasing, currentTimeWaiting;

        private void Awake()
        {
            Animator = GetComponentInChildren<Animator>();
        }
        private void Start()
        {
            if (Agent == null)
                Agent = GetComponent<NavMeshAgent>();
            if (Agent.stoppingDistance < 0.5f)
                Agent.stoppingDistance = 0.5f;
        }

        private void Update()
        {
            switch(moveMode)
            {
                case MoveMode.patrol:
                    Patroling();
                    break;
                case MoveMode.chase:
                    Chasing();
                    break;
                case MoveMode.wait:
                    Waiting();
                    break;
            }

            FieldOfView();
            Animations();
        }

        private void Animations()
        {
           if (Animator == null) return;

            Animator.SetBool("isRunning", moveMode == MoveMode.chase);
        }

        private void Patroling()
        {
            Agent.speed = patrolSpeed;

            if (Agent.remainingDistance < Agent.stoppingDistance)
            {
                SwitchMoveMode(MoveMode.wait);
            }
        }

        private void Chasing()
        {
            Agent.speed = chaseSpeed;
            Agent.destination = currentTarget.position;
            Collider[] col = Physics.OverlapSphere(transform.position, radiusHit, TargetMask, QueryTriggerInteraction.Ignore);

            if(col.Length > 0 && !isHit) {
                Debug.Log("permainan berakhir");
                isHit = true;
            }

            if(currentTimeChasing > maxTimeChasing) {
                SwitchMoveMode(MoveMode.wait);
            } else if(!isDetectTarget) {
                currentTimeChasing += Time.deltaTime;
            } else if(isDetectTarget){
                currentTimeChasing = 0;
            }
        }

        private void Waiting()
        {

            if(currentTimeWaiting > maxTimeWaiting) {
                SwitchMoveMode(MoveMode.patrol);
            } else {
                currentTimeWaiting += Time.deltaTime;
            }
        }

        private void SwitchMoveMode (MoveMode _moveMode)
        {
            switch (_moveMode)
            {
                case MoveMode.patrol:
                    int lastIndex = index_patrolPoint;
                    int newIndex = (index_patrolPoint + 1) % patrolPoint.Length;

                    if (lastIndex == newIndex)
                    {
                        newIndex = (index_patrolPoint + 2) % patrolPoint.Length;
                        Debug.Log("Change Patrol to " + patrolPoint[newIndex].position);
                        return;
                    }

                    index_patrolPoint = newIndex;
                    Agent.destination = destination = patrolPoint[index_patrolPoint].position;
                    Debug.Log("Change Patrol to " + index_patrolPoint.ToString());
                    break;
                case MoveMode.chase:
                    currentTimeChasing = 0;
                    break;
                case MoveMode.wait:
                    Agent.destination = transform.position;
                    currentTimeWaiting = 0;
                    break;
            }
            moveMode = _moveMode;
            Debug.Log("Switch Move Mode to " + moveMode.ToString());
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
            Collider[] range = Physics.OverlapSphere(transform.position, viewRadius, TargetMask, QueryTriggerInteraction.Ignore);

            if(range.Length > 0) {

                currentTarget = range[0].transform;
               
                Vector3 direction = (currentTarget.position - transform.position).normalized;

                if(Vector3.Angle(transform.forward, direction) < viewAngle / 2) {
                    float distance = Vector3.Distance(transform.position, currentTarget.position);

                    if(!Physics.Raycast(transform.position, direction, distance, ObstacleMask, QueryTriggerInteraction.Ignore)) {
                        isDetectTarget = true;

                        if(moveMode != MoveMode.chase) {
                            SwitchMoveMode(MoveMode.chase);
                        }
                    } else {
                        isDetectTarget = false;
                    }
                } else {
                    isDetectTarget = false;
                }
            } else {
                isDetectTarget = false;
            }
        }

      private void OnDrawGizmos()
      {
          if(Agent == null) return;

          Gizmos.DrawWireSphere(transform.position, Agent.stoppingDistance);

          Gizmos.DrawWireSphere(transform.position, viewRadius);

          if(currentTarget != null && isDetectTarget)
              Gizmos.DrawLine(transform.position, currentTarget.position);

          float halfFov = viewAngle / 2f;
          Quaternion leftRayRotation = Quaternion.AngleAxis(-halfFov, Vector3.up);
          Quaternion rightRayRotation = Quaternion.AngleAxis(halfFov, Vector3.up);
          Vector3 leftRayDirection = leftRayRotation * transform.forward;
          Vector3 rightRayDirection = rightRayRotation * transform.forward;
                  
          Gizmos.DrawRay(transform.position, leftRayDirection * viewRadius);
          Gizmos.DrawRay(transform.position, rightRayDirection * viewRadius);
      }
    }

}
