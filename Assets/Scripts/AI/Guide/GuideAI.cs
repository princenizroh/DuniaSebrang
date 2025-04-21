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
        [field: SerializeField] public GuideState guideState { get; private set; }
        private Animator animator;

        [Header("Steering")]
        [field: SerializeField] public float patrolSpeed { get; private set; }

        [Header("Transform")]
        [field: SerializeField] public Transform[] patrolPoint { get; private set; }
        [field: SerializeField] public NavMeshAgent agent { get; private set; }

        [field: SerializeField] private float currentTimeWaiting;
        [field: SerializeField] private Vector3 destination;
        [field: SerializeField] private int index_patrolPoint;

        private void Awake()
        {
            animator = GetComponentInChildren<Animator>();
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
            animator.SetBool("isPatrol", false);
            agent.isStopped = true;
        }
        private void Patroling()
        {
            agent.speed = patrolSpeed;

            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                SwitchGuideMode(GuideState.idle);
            }
        }

        private void SwitchGuideMode (GuideState _guideState)
        {
            switch (_guideState)
            {
                case GuideState.idle:
                    agent.destination = transform.position;
                    currentTimeWaiting = 0;
                    break;
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
            }
        }
    }   
}
