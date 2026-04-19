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

    private float patrolLoopTime;
    private float slowChaseLoopTime;
    private float fastChaseLoopTime;

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

        currentState = MonsterState.Patrol;
        ApplyStateSettings(currentState);
        PlayStateLoop(currentState);

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
        MonsterState targetState;

        if (playerHiding.IsHidden || playerHiding.IsTransitioning)
        {
            targetState = MonsterState.Patrol;
        }
        else
        {
            targetState = playerLantern.IsOn() ? MonsterState.ChaseFast : MonsterState.ChaseSlow;
        }

        if (currentState != targetState)
            ChangeState(targetState);

        switch (currentState)
        {
            case MonsterState.Patrol:
                UpdatePatrol();
                break;

            case MonsterState.ChaseSlow:
            case MonsterState.ChaseFast:
                UpdateChase();
                break;
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

    void UpdateChase()
    {
        if (player == null)
            return;

        agent.SetDestination(player.position);
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
        if (visualRoot == null)
            return;

        Vector3 velocity = agent.velocity;
        velocity.y = 0f;

        if (velocity.sqrMagnitude > 0.05f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(velocity.normalized, Vector3.up);
            visualRoot.rotation = Quaternion.Slerp(
                visualRoot.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }

    void ChangeState(MonsterState newState)
    {
        if (currentState == newState)
            return;

        SaveCurrentLoopTime();

        currentState = newState;

        ApplyStateSettings(currentState);
        PlayStateLoop(currentState);

        if (currentState == MonsterState.Patrol)
        {
            patrolWaitCounter = 0f;
            PickNewPatrolPoint();
        }
    }

    void ApplyStateSettings(MonsterState state)
    {
        switch (state)
        {
            case MonsterState.Patrol:
                agent.speed = patrolSpeed;
                break;

            case MonsterState.ChaseSlow:
                agent.speed = slowChaseSpeed;
                break;

            case MonsterState.ChaseFast:
                agent.speed = fastChaseSpeed;
                break;
        }
    }

    void PlayStateLoop(MonsterState state)
    {
        switch (state)
        {
            case MonsterState.Patrol:
                PlayLoop(patrolLoop, patrolLoopTime);
                break;

            case MonsterState.ChaseSlow:
                PlayLoop(slowChaseLoop, slowChaseLoopTime);
                break;

            case MonsterState.ChaseFast:
                PlayLoop(fastChaseLoop, fastChaseLoopTime);
                break;
        }
    }

    void SaveCurrentLoopTime()
    {
        if (audioSource == null || audioSource.clip == null)
            return;

        float currentTime = audioSource.time;

        if (audioSource.clip == patrolLoop)
            patrolLoopTime = currentTime;
        else if (audioSource.clip == slowChaseLoop)
            slowChaseLoopTime = currentTime;
        else if (audioSource.clip == fastChaseLoop)
            fastChaseLoopTime = currentTime;
    }

    void PlayLoop(AudioClip clip, float savedTime)
    {
        if (audioSource == null)
            return;

        if (clip == null)
        {
            audioSource.Stop();
            audioSource.clip = null;
            return;
        }

        if (audioSource.clip == clip && audioSource.isPlaying)
            return;

        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.loop = true;

        if (clip.length > 0f)
            audioSource.time = Mathf.Repeat(savedTime, clip.length);
        else
            audioSource.time = 0f;

        audioSource.Play();
    }

    void UpdateAnimator()
    {
        if (animator == null)
            return;

        float speedPercent = 0f;

        if (fastChaseSpeed > 0f)
            speedPercent = agent.velocity.magnitude / fastChaseSpeed;

        animator.SetFloat("Speed", speedPercent);
    }
}