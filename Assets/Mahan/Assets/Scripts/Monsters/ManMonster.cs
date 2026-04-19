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
    public AudioSource musicSource;
    public AudioSource footstepSource;
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

    [Header("State Music")]
    public AudioClip patrolLoop;
    public AudioClip slowChaseLoop;
    public AudioClip fastChaseLoop;

    [Header("Footsteps")]
    public AudioClip patrolFootsteps;
    public AudioClip slowChaseFootsteps;
    public AudioClip fastChaseFootsteps;
    public float footstepMinVelocity = 0.15f;

    private MonsterState currentState = MonsterState.Patrol;
    private float patrolWaitCounter;
    private bool hasPatrolDestination;

    private float patrolLoopTime;
    private float slowChaseLoopTime;
    private float fastChaseLoopTime;

    void Reset()
    {
        agent = GetComponent<NavMeshAgent>();

        AudioSource[] sources = GetComponents<AudioSource>();
        if (sources.Length > 0)
            musicSource = sources[0];
        if (sources.Length > 1)
            footstepSource = sources[1];
    }

    void Start()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (visualRoot == null)
            visualRoot = transform;

        agent.updateRotation = false;
        agent.updateUpAxis = true;

        if (musicSource != null)
            musicSource.loop = true;

        if (footstepSource != null)
            footstepSource.loop = true;

        currentState = MonsterState.Patrol;

        ApplyStateSettings(currentState);
        PlayStateMusic(currentState);
        SetFootstepClipForState(currentState);

        PickNewPatrolPoint();
    }

    void Update()
    {
        if (player == null || playerHiding == null || playerLantern == null || agent == null)
            return;

        UpdateStateMachine();
        RotateVisual();
        UpdateAnimator();
        UpdateFootstepsPlayback();
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

        SaveCurrentMusicTime();

        currentState = newState;

        ApplyStateSettings(currentState);
        PlayStateMusic(currentState);
        SetFootstepClipForState(currentState);

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

    void PlayStateMusic(MonsterState state)
    {
        switch (state)
        {
            case MonsterState.Patrol:
                PlayMusicLoop(patrolLoop, patrolLoopTime);
                break;

            case MonsterState.ChaseSlow:
                PlayMusicLoop(slowChaseLoop, slowChaseLoopTime);
                break;

            case MonsterState.ChaseFast:
                PlayMusicLoop(fastChaseLoop, fastChaseLoopTime);
                break;
        }
    }

    void SaveCurrentMusicTime()
    {
        if (musicSource == null || musicSource.clip == null)
            return;

        float currentTime = musicSource.time;

        if (musicSource.clip == patrolLoop)
            patrolLoopTime = currentTime;
        else if (musicSource.clip == slowChaseLoop)
            slowChaseLoopTime = currentTime;
        else if (musicSource.clip == fastChaseLoop)
            fastChaseLoopTime = currentTime;
    }

    void PlayMusicLoop(AudioClip clip, float savedTime)
    {
        if (musicSource == null)
            return;

        if (clip == null)
        {
            musicSource.Stop();
            musicSource.clip = null;
            return;
        }

        if (musicSource.clip == clip && musicSource.isPlaying)
            return;

        musicSource.Stop();
        musicSource.clip = clip;
        musicSource.loop = true;

        if (clip.length > 0f)
            musicSource.time = Mathf.Repeat(savedTime, clip.length);
        else
            musicSource.time = 0f;

        musicSource.Play();
    }

    void SetFootstepClipForState(MonsterState state)
    {
        if (footstepSource == null)
            return;

        AudioClip targetClip = null;

        switch (state)
        {
            case MonsterState.Patrol:
                targetClip = patrolFootsteps;
                break;

            case MonsterState.ChaseSlow:
                targetClip = slowChaseFootsteps;
                break;

            case MonsterState.ChaseFast:
                targetClip = fastChaseFootsteps;
                break;
        }

        if (footstepSource.clip == targetClip)
            return;

        bool wasPlaying = footstepSource.isPlaying;
        footstepSource.Stop();
        footstepSource.clip = targetClip;
        footstepSource.loop = true;

        if (wasPlaying && targetClip != null)
            footstepSource.Play();
    }

    void UpdateFootstepsPlayback()
    {
        if (footstepSource == null)
            return;

        bool shouldPlay = agent.velocity.magnitude > footstepMinVelocity && footstepSource.clip != null;

        if (shouldPlay)
        {
            if (!footstepSource.isPlaying)
                footstepSource.Play();
        }
        else
        {
            if (footstepSource.isPlaying)
                footstepSource.Stop();
        }
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