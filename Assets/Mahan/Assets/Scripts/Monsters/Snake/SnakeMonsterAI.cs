using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SnakeMonsterAI : MonoBehaviour
{
    public enum SnakeState
    {
        Patrol,
        Chase
    }

    [Header("References")]
    public Transform player;
    public Camera playerCamera;
    public NavMeshAgent agent;
    public GameObject bodyPrefab;
    public Transform headVisual;
    public AudioSource audioSource;
    public LanternToggle playerLantern;

    [Header("Startup Activation")]
    public bool useStartupActivation = true;
    public float activationRadius = 12f;

    [Header("Body")]
    public int startBodyCount = 6;
    public float bodyFollowSpeed = 10f;
    public float segmentSpacing = 0.9f;

    [Header("Patrol")]
    public float patrolRadius = 12f;
    public float patrolPointReachDistance = 1.2f;
    public float patrolWaitTime = 1.5f;

    [Header("Sounds")]
    public AudioClip patrolLoop;
    public AudioClip chaseLoop;
    public AudioClip frontBangClip;

    [Header("Front Look Bang")]
    public float bangCheckDistance = 8f;
    [Range(-1f, 1f)] public float lookDotThreshold = 0.9f;
    [Range(-1f, 1f)] public float frontFaceDotThreshold = 0.35f;
    public float bangCooldown = 1f;
    public bool onlyInChase = false;

    [Header("Debug")]
    public bool drawDebug = true;
    public float debugRayLength = 3f;

    [Header("Movement Speeds")]
    public float patrolSpeed = 2.5f;
    public float chaseSpeed = 5f;
    public float rotationSpeed = 8f;

    private SnakeState currentState = SnakeState.Patrol;

    private List<GameObject> bodyParts = new List<GameObject>();
    private float patrolWaitCounter;
    private bool hasPatrolDestination;

    private float patrolLoopTime;
    private float chaseLoopTime;

    private bool wasLookingAtFront;
    private float bangTimer;

    private bool debugLookingAtHead;
    private bool debugSeeingFrontFace;
    private bool debugShouldBang;
    private float debugLookDot;
    private float debugFrontFaceDot;

    private bool isActivated;

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

        if (headVisual == null)
            headVisual = transform;

        if (playerCamera == null)
            playerCamera = Camera.main;

        agent.updateRotation = false;
        agent.updateUpAxis = true;

        for (int i = 0; i < startBodyCount; i++)
        {
            GrowSnake();
        }

        currentState = SnakeState.Patrol;
        ApplyStateSettings(currentState);

        isActivated = !useStartupActivation;

        if (isActivated)
        {
            PlayStateLoop(currentState);
            PickNewPatrolPoint();
        }
        else
        {
            ForceIdleState();
        }
    }

    void Update()
    {
        if (player == null || agent == null || playerLantern == null)
            return;

        if (bangTimer > 0f)
            bangTimer -= Time.deltaTime;

        if (!isActivated)
        {
            CheckStartupActivation();
            RotateHeadVisual();
            UpdateBodyFollow();
            return;
        }

        UpdateStateMachine();
        RotateHeadVisual();
        UpdateBodyFollow();
        UpdateFrontLookBang();
    }

    void CheckStartupActivation()
    {
        Vector3 snakePos = transform.position;
        Vector3 playerPos = player.position;

        snakePos.y = 0f;
        playerPos.y = 0f;

        float sqrDistance = (playerPos - snakePos).sqrMagnitude;
        float sqrActivationRadius = activationRadius * activationRadius;

        if (sqrDistance <= sqrActivationRadius)
        {
            ActivateSnake();
        }
    }

    void ActivateSnake()
    {
        isActivated = true;

        ApplyStateSettings(currentState);
        PlayStateLoop(currentState);

        if (currentState == SnakeState.Patrol)
            PickNewPatrolPoint();
    }

    void ForceIdleState()
    {
        if (agent != null)
        {
            agent.ResetPath();
            agent.velocity = Vector3.zero;
            agent.isStopped = true;
        }

        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.clip = null;
        }

        hasPatrolDestination = false;
        patrolWaitCounter = 0f;
        wasLookingAtFront = false;
    }

    void UpdateStateMachine()
    {
        if (agent.isStopped)
            agent.isStopped = false;

        SnakeState targetState = playerLantern.IsOn() ? SnakeState.Chase : SnakeState.Patrol;

        if (currentState != targetState)
            ChangeState(targetState);

        switch (currentState)
        {
            case SnakeState.Patrol:
                UpdatePatrol();
                break;

            case SnakeState.Chase:
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
        agent.speed = chaseSpeed;
        agent.SetDestination(player.position);
    }

    void ChangeState(SnakeState newState)
    {
        if (currentState == newState)
            return;

        SaveCurrentLoopTime();

        currentState = newState;

        ApplyStateSettings(currentState);
        PlayStateLoop(currentState);

        if (currentState == SnakeState.Patrol)
        {
            patrolWaitCounter = 0f;
            PickNewPatrolPoint();
        }
    }

    void ApplyStateSettings(SnakeState state)
    {
        switch (state)
        {
            case SnakeState.Patrol:
                agent.speed = patrolSpeed;
                break;

            case SnakeState.Chase:
                agent.speed = chaseSpeed;
                break;
        }
    }

    void PlayStateLoop(SnakeState state)
    {
        switch (state)
        {
            case SnakeState.Patrol:
                PlayLoop(patrolLoop, patrolLoopTime);
                break;

            case SnakeState.Chase:
                PlayLoop(chaseLoop, chaseLoopTime);
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
        else if (audioSource.clip == chaseLoop)
            chaseLoopTime = currentTime;
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

    void RotateHeadVisual()
    {
        if (headVisual == null)
            return;

        Vector3 velocity = agent.velocity;
        velocity.y = 0f;

        if (velocity.sqrMagnitude > 0.05f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(velocity.normalized, Vector3.up);
            headVisual.rotation = Quaternion.Slerp(headVisual.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    void UpdateBodyFollow()
    {
        for (int i = 0; i < bodyParts.Count; i++)
        {
            Transform body = bodyParts[i].transform;
            Transform leader = (i == 0) ? transform : bodyParts[i - 1].transform;

            Vector3 toLeader = leader.position - body.position;
            toLeader.y = 0f;

            if (toLeader.sqrMagnitude > 0.0001f)
            {
                Vector3 desiredPos = leader.position - toLeader.normalized * segmentSpacing;

                body.position = Vector3.Lerp(body.position, desiredPos, bodyFollowSpeed * Time.deltaTime);

                Quaternion targetRot = Quaternion.LookRotation(toLeader.normalized, Vector3.up);
                body.rotation = Quaternion.Slerp(body.rotation, targetRot, rotationSpeed * Time.deltaTime);
            }
        }
    }

    void UpdateFrontLookBang()
    {
        debugLookingAtHead = false;
        debugSeeingFrontFace = false;
        debugShouldBang = false;
        debugLookDot = -999f;
        debugFrontFaceDot = -999f;

        if (frontBangClip == null || playerCamera == null || headVisual == null || audioSource == null)
            return;

        if (onlyInChase && currentState != SnakeState.Chase)
        {
            wasLookingAtFront = false;
            return;
        }

        Vector3 cameraPos = playerCamera.transform.position;
        Vector3 toSnakeHead = headVisual.position - cameraPos;

        float distance = toSnakeHead.magnitude;
        if (distance > bangCheckDistance || distance < 0.001f)
        {
            wasLookingAtFront = false;
            return;
        }

        Vector3 cameraForward = playerCamera.transform.forward.normalized;
        Vector3 dirToSnakeHead = toSnakeHead.normalized;

        debugLookDot = Vector3.Dot(cameraForward, dirToSnakeHead);
        debugLookingAtHead = debugLookDot >= lookDotThreshold;

        Vector3 snakeForward = headVisual.forward.normalized;
        Vector3 headToCamera = (cameraPos - headVisual.position).normalized;

        debugFrontFaceDot = Vector3.Dot(snakeForward, headToCamera);
        debugSeeingFrontFace = debugFrontFaceDot >= frontFaceDotThreshold;

        debugShouldBang = debugLookingAtHead && debugSeeingFrontFace;

        if (debugShouldBang && !wasLookingAtFront && bangTimer <= 0f)
        {
            audioSource.PlayOneShot(frontBangClip);
            bangTimer = bangCooldown;
        }

        wasLookingAtFront = debugShouldBang;
    }

    public void GrowSnake()
    {
        Vector3 spawnPos = transform.position;

        if (bodyParts.Count > 0)
            spawnPos = bodyParts[bodyParts.Count - 1].transform.position;

        GameObject newBody = Instantiate(bodyPrefab, spawnPos, Quaternion.identity);
        bodyParts.Add(newBody);
    }

    void OnDrawGizmos()
    {
        if (!drawDebug)
            return;

        if (useStartupActivation)
        {
            Gizmos.color = isActivated ? Color.green : new Color(1f, 0.5f, 0f);
            Gizmos.DrawWireSphere(transform.position, activationRadius);
        }

        if (headVisual != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(headVisual.position, headVisual.forward.normalized * debugRayLength);

            Gizmos.color = debugSeeingFrontFace ? Color.green : Color.red;
            Gizmos.DrawWireSphere(headVisual.position, 0.25f);
        }

        if (playerCamera != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(playerCamera.transform.position, playerCamera.transform.forward.normalized * debugRayLength);
        }

        if (playerCamera != null && headVisual != null)
        {
            Gizmos.color = debugLookingAtHead ? Color.green : Color.red;
            Gizmos.DrawLine(playerCamera.transform.position, headVisual.position);

            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(playerCamera.transform.position, bangCheckDistance);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!drawDebug)
            return;

        if (useStartupActivation)
        {
            Gizmos.color = isActivated ? Color.green : new Color(1f, 0.5f, 0f);
            Gizmos.DrawWireSphere(transform.position, activationRadius);
        }

        if (headVisual != null)
        {
            Debug.DrawRay(headVisual.position, headVisual.forward.normalized * debugRayLength, Color.blue);
        }

        if (playerCamera != null)
        {
            Debug.DrawRay(playerCamera.transform.position, playerCamera.transform.forward.normalized * debugRayLength, Color.yellow);
        }

        if (playerCamera != null && headVisual != null)
        {
            Color lineColor = debugShouldBang ? Color.magenta : (debugLookingAtHead ? Color.green : Color.red);
            Debug.DrawLine(playerCamera.transform.position, headVisual.position, lineColor);
        }
    }
}