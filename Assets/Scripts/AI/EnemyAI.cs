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
        public Animator Animator { get; private set; }
        public Rigidbody Rigidbody { get; private set; }

        [Header("Steering")]
        [field: SerializeField] public float patrolSpeed { get; private set; }
        [field: SerializeField] public float chaseSpeed { get; private set; }
        [field: SerializeField] public float maxTimeChasing { get; private set; }

        [Header("Transform")]
        [field: SerializeField] public Transform[] patrolPoint { get; private set; }
        private NavMeshAgent Agent;
        [field: SerializeField] public Transform currentTarget { get; private set; }
        private bool isChasing = false;

        [Header("Field of View")]
        [field: SerializeField] public float viewRadius { get; private set; }
        [field: SerializeField] public float viewAngle { get; private set; }
        [field: SerializeField] public LayerMask targetMask { get; private set; }
        [field: SerializeField] public LayerMask obstacleMask { get; private set; }

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
            // GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            // if (playerObj != null)
            // {
            //     player = playerObj.transform;
            // }
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
            // if (isChasing && player != null)
            // {
            //     agent.SetDestination(player.position);
            // }
        }

        private void Patroling()
        {
            Agent.speed = patrolSpeed;

            // Agent.destination = destination = patrolPoint[index_patrolPoint].position;

            if (Agent.remainingDistance < Agent.stoppingDistance)
            {
                // index_patrolPoint = (index_patrolPoint + 1) % patrolPoint.Length;
                // Debug.Log("Change Patrol to " + Agent.destination);
                SwitchMoveMode(MoveMode.wait);
            }
        }

        private void Chasing()
        {
            Agent.speed = chaseSpeed;
            Agent.destination = currentTarget.position;
            Collider[] col = Physics.OverlapSphere(transform.position, viewRadius, targetMask, QueryTriggerInteraction.Ignore);

            if(col.Length > 0 && !isHit) {
                Debug.Log("permainan berakhir");
                isHit = true;
            }

            if(currentTimeChasing > maxTimeChasing) {
                isChasing = false;
                moveMode = MoveMode.patrol;
                currentTimeChasing = 0;
            } else {
                currentTimeChasing += Time.deltaTime;
            }
        }

        private void Waiting()
        {
            // Agent.destination = transform.position;

            if(currentTimeWaiting > maxTimeChasing) {
                // isChasing = true;
                // Debug.Log("Enemy Menunggu terlalu lama, kembali mengejar");
                // moveMode = MoveMode.chase;
                // currentTimeWaiting = 0;
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
                isChasing = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            // Jika FearShield keluar dari area deteksi, berhenti mengejar
            if (other.CompareTag("Light"))
            {
                Debug.Log("FearShield keluar dari jangkauan! Enemy berhenti mengejar.");
                isChasing = false;
                // agent.ResetPath();
            }
        }

        private void FieldOfView()
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, viewRadius, targetMask, QueryTriggerInteraction.Ignore);

            if (hitColliders.Length > 0)
            {
                currentTarget = hitColliders[0].transform;
                Vector3 dirToTarget = (currentTarget.position - transform.position).normalized;

                if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2)
                {
                    float dstToTarget = Vector3.Distance(transform.position, currentTarget.position);

                    if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, obstacleMask, QueryTriggerInteraction.Ignore))
                    {
                        isDetectTarget = true;

                        if (moveMode != MoveMode.chase)
                        {
                        }
                    }
                    else
                    {
                        isDetectTarget = false;
                    }
                }
                else
                {
                    isDetectTarget = false;
                }
            }
            else
            {
                isDetectTarget = false;
            }
        }
    }
}
