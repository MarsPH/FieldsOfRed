using UnityEngine;
using UnityEngine.AI;

public class ManMonster : MonoBehaviour
{
    public enum MonsterState
    {
        Patrol,
        ChaseSlow,
        ChaseFast
    }

    [Header("References")]
    public Transform player;
    public PlayerHiding playerHiding;
    public LanternToggle playerLantern;
    public NavMeshAgent agent;
    public Transform visualRoot;
    public AudioSource audioSource;
    public Animator animator;

    [Header("Patrol")]
    public float patrolRadius = 12f;
    public float patrolPointReachDistance = 1.2f;
    public float patrolWaitTime = 1.5f;

    [Header("Movement")]
    public float patrolSpeed = 2.5f;
    public float slowChaseSpeed = 3.5f;
    public float fastChaseSpeed = 5.5f;
    public float rotationSpeed = 8f;

    [Header("Sounds")]
    public AudioClip patrolLoop;
    public AudioClip slowChaseLoop;
    public AudioClip fastChaseLoop;

    private MonsterState currentState = MonsterState.Patrol;
    private float patrolWaitCounter;
    private bool hasPatrolDestination;

    void Reset()
    {
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (visualRoot == null)
            visualRoot = transform;

        agent.updateRotation = false;
        agent.updateUpAxis = true;

        ChangeState(MonsterState.Patrol);
        PickNewPatrolPoint();
    }

    void Update()
    {
        if (player == null || playerHiding == null || playerLantern == null || agent == null)
            return;

        UpdateStateMachine();
        RotateVisual();
        UpdateAnimator();
    }

    void UpdateStateMachine()
    {
        if (playerHiding.IsHidden || playerHiding.IsTransitioning)
        {
            if (currentState != MonsterState.Patrol)
            {
                ChangeState(MonsterState.Patrol);
                PickNewPatrolPoint();
            }

            UpdatePatrol();
            return;
        }

        if (playerLantern.IsOn())
        {
            if (currentState != MonsterState.ChaseFast)
                ChangeState(MonsterState.ChaseFast);

            agent.speed = fastChaseSpeed;
            agent.SetDestination(player.position);
        }
        else
        {
            if (currentState != MonsterState.ChaseSlow)
                ChangeState(MonsterState.ChaseSlow);

            agent.speed = slowChaseSpeed;
            agent.SetDestination(player.position);
        }
    }

    void UpdatePatrol()
    {
        agent.speed = patrolSpeed;

        if (!hasPatrolDestination || (!agent.pathPending && agent.remainingDistance <= patrolPointReachDistance))
        {
            patrolWaitCounter += Time.deltaTime;

            if (patrolWaitCounter >= patrolWaitTime)
            {
                patrolWaitCounter = 0f;
                PickNewPatrolPoint();
            }
        }
    }

    void PickNewPatrolPoint()
    {
        for (int i = 0; i < 20; i++)
        {
            Vector3 randomPoint = transform.position + Random.insideUnitSphere * patrolRadius;
            randomPoint.y = transform.position.y;

            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, patrolRadius, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
                hasPatrolDestination = true;
                return;
            }
        }

        hasPatrolDestination = false;
    }

    void RotateVisual()
    {
        Vector3 velocity = agent.velocity;
        velocity.y = 0f;

        if (velocity.sqrMagnitude > 0.05f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(velocity.normalized, Vector3.up);
            visualRoot.rotation = Quaternion.Slerp(visualRoot.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    void ChangeState(MonsterState newState)
    {
        if (currentState == newState)
            return;

        currentState = newState;

        switch (currentState)
        {
            case MonsterState.Patrol:
                PlayLoop(patrolLoop);
                break;

            case MonsterState.ChaseSlow:
                PlayLoop(slowChaseLoop);
                break;

            case MonsterState.ChaseFast:
                PlayLoop(fastChaseLoop);
                break;
        }
    }

    void PlayLoop(AudioClip clip)
    {
        if (audioSource == null)
            return;

        if (clip == null)
        {
            audioSource.Stop();
            return;
        }

        if (audioSource.clip == clip && audioSource.isPlaying)
            return;

        audioSource.clip = clip;
        audioSource.loop = true;
        audioSource.Play();
    }

    void UpdateAnimator()
    {
        if (animator == null)
            return;

        float speedPercent = agent.velocity.magnitude / fastChaseSpeed;
        animator.SetFloat("Speed", speedPercent);
    }
}