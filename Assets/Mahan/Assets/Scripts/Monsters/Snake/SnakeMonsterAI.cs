using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SnakeMonsterAI : MonoBehaviour
{
    public enum SnakeState
    {
        Patrol,
        Chase,
        Search
    }

    [Header("References")]
    public Transform player;
    public NavMeshAgent agent;
    public GameObject bodyPrefab;
    public Transform headVisual;
    public AudioSource audioSource;

    [Header("Body")]
    public int startBodyCount = 6;
    public int gap = 10;
    public float bodyFollowSpeed = 10f;

    [Header("Patrol")]
    public float patrolRadius = 12f;
    public float patrolPointReachDistance = 1.2f;
    public float patrolWaitTime = 1.5f;

    [Header("Detection")]
    public float detectionRange = 12f;
    [Range(0f, 180f)] public float viewAngle = 90f;
    public float eyeHeight = 1.2f;
    public LayerMask visionBlockLayers;
    public LayerMask playerLayers;
    public float loseSightDelay = 2f;

    [Header("Search")]
    public float searchTime = 4f;

    [Header("Sounds")]
    public AudioClip patrolLoop;
    public AudioClip detectClip;
    public AudioClip chaseLoop;
    public AudioClip lostClip;

    [Header("Movement Speeds")]
    public float patrolSpeed = 2.5f;
    public float chaseSpeed = 5f;
    public float searchSpeed = 3.2f;
    public float rotationSpeed = 8f;

    private SnakeState currentState = SnakeState.Patrol;

    private List<GameObject> bodyParts = new List<GameObject>();
    private List<Vector3> positionHistory = new List<Vector3>();

    private float patrolWaitCounter;
    private float loseSightCounter;
    private float searchCounter;

    private Vector3 lastSeenPlayerPosition;
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

        if (headVisual == null)
            headVisual = transform;

        agent.updateRotation = false;
        agent.updateUpAxis = true;

        for (int i = 0; i < startBodyCount; i++)
        {
            GrowSnake();
        }

        positionHistory.Insert(0, transform.position);

        ChangeState(SnakeState.Patrol);
        PickNewPatrolPoint();
    }

    void Update()
    {
        if (player == null || agent == null)
            return;

        if (positionHistory.Count == 0 || Vector3.Distance(positionHistory[0], transform.position) > 0.1f)
        {
            positionHistory.Insert(0, transform.position);
        }

        UpdateStateMachine();
        RotateHeadVisual();
        UpdateBodyFollow();
    }

    void UpdateStateMachine()
    {
        bool canSeePlayer = CanSeePlayer();

        switch (currentState)
        {
            case SnakeState.Patrol:
                agent.speed = patrolSpeed;

                if (canSeePlayer)
                {
                    lastSeenPlayerPosition = player.position;
                    ChangeState(SnakeState.Chase);
                    return;
                }

                if (!hasPatrolDestination || (!agent.pathPending && agent.remainingDistance <= patrolPointReachDistance))
                {
                    patrolWaitCounter += Time.deltaTime;

                    if (patrolWaitCounter >= patrolWaitTime)
                    {
                        patrolWaitCounter = 0f;
                        PickNewPatrolPoint();
                    }
                }
                break;

            case SnakeState.Chase:
                agent.speed = chaseSpeed;

                if (canSeePlayer)
                {
                    loseSightCounter = 0f;
                    lastSeenPlayerPosition = player.position;
                    agent.SetDestination(player.position);
                }
                else
                {
                    loseSightCounter += Time.deltaTime;

                    if (loseSightCounter < loseSightDelay)
                    {
                        agent.SetDestination(lastSeenPlayerPosition);
                    }
                    else
                    {
                        ChangeState(SnakeState.Search);
                    }
                }
                break;

            case SnakeState.Search:
                agent.speed = searchSpeed;
                searchCounter += Time.deltaTime;

                if (canSeePlayer)
                {
                    lastSeenPlayerPosition = player.position;
                    ChangeState(SnakeState.Chase);
                    return;
                }

                if (!agent.pathPending && agent.remainingDistance <= patrolPointReachDistance)
                {
                    if (searchCounter >= searchTime)
                    {
                        ChangeState(SnakeState.Patrol);
                        PickNewPatrolPoint();
                    }
                    else
                    {
                        PickSearchPointNearLastSeen();
                    }
                }
                break;
        }
    }

    bool CanSeePlayer()
    {
        Vector3 origin = transform.position + Vector3.up * eyeHeight;
        Vector3 target = player.position + Vector3.up * 1f;
        Vector3 directionToPlayer = target - origin;

        float distanceToPlayer = directionToPlayer.magnitude;
        if (distanceToPlayer > detectionRange)
            return false;

        Vector3 visionForward = headVisual != null ? headVisual.forward : transform.forward;
        visionForward.y = 0f;
        visionForward.Normalize();

        Vector3 flatDirectionToPlayer = directionToPlayer;
        flatDirectionToPlayer.y = 0f;
        flatDirectionToPlayer.Normalize();

        float angle = Vector3.Angle(visionForward, flatDirectionToPlayer);
        if (angle > viewAngle * 0.5f)
            return false;

        if (Physics.Linecast(origin, target, out RaycastHit hit, visionBlockLayers | playerLayers))
        {
            if (((1 << hit.collider.gameObject.layer) & playerLayers) != 0)
            {
                return true;
            }

            return false;
        }

        return false;
    }

    void ChangeState(SnakeState newState)
    {
        if (currentState == newState)
            return;

        currentState = newState;

        switch (currentState)
        {
            case SnakeState.Patrol:
                loseSightCounter = 0f;
                searchCounter = 0f;
                PlayLoop(patrolLoop);
                break;

            case SnakeState.Chase:
                loseSightCounter = 0f;
                searchCounter = 0f;
                PlayDetectThenLoop();
                break;

            case SnakeState.Search:
                searchCounter = 0f;
                agent.SetDestination(lastSeenPlayerPosition);

                if (lostClip != null)
                    audioSource.PlayOneShot(lostClip);

                PlayLoop(patrolLoop);
                break;
        }
    }

    void PlayDetectThenLoop()
    {
        if (audioSource == null)
            return;

        if (detectClip != null)
            audioSource.PlayOneShot(detectClip);

        if (chaseLoop != null)
        {
            audioSource.clip = chaseLoop;
            audioSource.loop = true;
            audioSource.Play();
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

    void PickSearchPointNearLastSeen()
    {
        for (int i = 0; i < 15; i++)
        {
            Vector3 randomPoint = lastSeenPlayerPosition + Random.insideUnitSphere * 4f;
            randomPoint.y = lastSeenPlayerPosition.y;

            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 4f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
                return;
            }
        }

        agent.SetDestination(lastSeenPlayerPosition);
    }

    void RotateHeadVisual()
    {
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
        float segmentSpacing = 0.9f; // tune this to match your body mesh length

        for (int i = 0; i < bodyParts.Count; i++)
        {
            Transform body = bodyParts[i].transform;
            Transform leader = (i == 0) ? transform : bodyParts[i - 1].transform;

            Vector3 toLeader = leader.position - body.position;
            toLeader.y = 0f;

            if (toLeader.sqrMagnitude > 0.0001f)
            {
                Vector3 desiredPos = leader.position - toLeader.normalized * segmentSpacing;

                body.position = Vector3.Lerp(
                    body.position,
                    desiredPos,
                    bodyFollowSpeed * Time.deltaTime
                );

                Quaternion targetRot = Quaternion.LookRotation(toLeader.normalized, Vector3.up);
                body.rotation = Quaternion.Slerp(
                    body.rotation,
                    targetRot,
                    rotationSpeed * Time.deltaTime
                );
            }
        }
    }

    public void GrowSnake()
    {
        Vector3 spawnPos = transform.position;

        if (bodyParts.Count > 0)
            spawnPos = bodyParts[bodyParts.Count - 1].transform.position;

        GameObject newBody = Instantiate(bodyPrefab, spawnPos, Quaternion.identity);
        bodyParts.Add(newBody);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Vector3 forward = headVisual != null ? headVisual.forward : transform.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude > 0.001f)
            forward.Normalize();
        else
            forward = transform.forward;

        Vector3 left = Quaternion.Euler(0f, -viewAngle * 0.5f, 0f) * forward;
        Vector3 right = Quaternion.Euler(0f, viewAngle * 0.5f, 0f) * forward;

        Vector3 eyePos = transform.position + Vector3.up * eyeHeight;

        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(eyePos, left * detectionRange);
        Gizmos.DrawRay(eyePos, right * detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(lastSeenPlayerPosition, 0.25f);
    }
}