using UnityEngine;
using UnityEngine.AI;

namespace DS
{
    public enum GuideState
    {
        idle,
        patrol
    }
    public class GuideAI : MonoBehaviour
    {
        public static GuideAI Instance { get; private set; }
        [field: SerializeField] public GuideState guideState { get; private set; }
        private Animator animator;

        [Header("Steering")]
        [field: SerializeField] public float patrolSpeed { get; private set; }

        [Header("Transform")]
        [field: SerializeField] public Transform[] patrolPoint { get; private set; }
        [field: SerializeField] public NavMeshAgent agent { get; private set; }
        [field: SerializeField] private Vector3 destination;
        [field: SerializeField] private int index_patrolPoint;

        [Header("Player Opening Guide")]
        [field: SerializeField] public PlayerOpeningGuide playerOpeningGuide { get; private set; }
        [field: SerializeField] public bool nextPatrolPoint = false;
        private void Awake()
        {
            animator = GetComponentInChildren<Animator>();

            if (Instance == null)
            {
                Instance = this;
            }
            if (playerOpeningGuide == null)
            {
                playerOpeningGuide = FindFirstObjectByType<PlayerOpeningGuide>();
            }

            if (playerOpeningGuide != null)
            {
                playerOpeningGuide.OnPlayerDetected += HandlePlayerDetected;
            }
            else
            {
                Debug.LogError("PlayerOpeningGuide not found! Make sure it is assigned in the Inspector or exists in the scene.");
            }
        }

        private void OnDestroy()
        {
            if (playerOpeningGuide != null)
            {
                playerOpeningGuide.OnPlayerDetected -= HandlePlayerDetected;
            }
        }
        private void HandlePlayerDetected()
        {
            Debug.Log("Player detected");
            SwitchGuideMode(GuideState.patrol);
        }   

        private void Start()
        {
            if (agent == null)
                agent = GetComponent<NavMeshAgent>();
            if (agent.stoppingDistance < 0.5f)
                agent.stoppingDistance = 0.5f;
        }

        private void Update()
        {
            switch(guideState)
            {
                case GuideState.idle:
                    Idle();
                    animator.Play("Idle");
                    break;
                case GuideState.patrol:
                    Patroling();
                    animator.Play("Walk");
                    break;
            }
        }

        private void Idle()
        {
            if (nextPatrolPoint == true)
            {
                SwitchGuideMode(GuideState.patrol);
            }
        }
        private void Patroling()
        {
            agent.speed = patrolSpeed;

            if (agent.remainingDistance < agent.stoppingDistance)
            {
                if (index_patrolPoint == patrolPoint.Length - 1)
                {
                    SwitchGuideMode(GuideState.idle);
                }
            }
        }

        private void SwitchGuideMode (GuideState _guideState)
        {
            switch (_guideState)
            {
                case GuideState.patrol:
                    int lastIndex = index_patrolPoint;
                    int newIndex = (index_patrolPoint + 1) % patrolPoint.Length;

                    if (lastIndex == newIndex)
                    {
                        newIndex = (index_patrolPoint + 2) % patrolPoint.Length;
                        Debug.Log("Change Patrol to " + patrolPoint[newIndex].position);
                        return;
                    }

                    index_patrolPoint = newIndex;
                    agent.destination = destination = patrolPoint[index_patrolPoint].position;
                    Debug.Log("Change Patrol to " + index_patrolPoint.ToString());
                    break;
                case GuideState.idle:
                    agent.destination = transform.position;
                    break;
            }
            guideState = _guideState;
        }
    }   
}
