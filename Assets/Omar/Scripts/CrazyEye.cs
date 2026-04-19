using UnityEngine;

public class CrazyEye : MonoBehaviour
{
    [Header("Crazy Settings")]
    public float minSpinSpeed = 200f;
    public float maxSpinSpeed = 800f;
    public float minMoveTime = 0.03f;
    public float maxMoveTime = 0.25f;
    public float minStopTime = 0.05f;
    public float maxStopTime = 0.3f;

    [Header("Rotation Limit")]
    public float maxAngleOffset = 30f; // How far it can deviate from start rotation

    private Quaternion startRotation;
    private Quaternion targetRotation;
    private float currentSpeed;
    private float stateTimer;
    private bool isStopped = false;

    void Awake()
    {
        startRotation = transform.rotation;
        PickNewTarget();
    }

    void Update()
    {
        stateTimer -= Time.deltaTime;

        if (isStopped)
        {
            if (stateTimer <= 0f)
            {
                isStopped = false;
                PickNewTarget();
            }
        }
        else
        {
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                currentSpeed * Time.deltaTime
            );

            bool reachedTarget = Quaternion.Angle(transform.rotation, targetRotation) < 1f;

            if (reachedTarget || stateTimer <= 0f)
            {
                isStopped = true;
                stateTimer = Random.Range(minStopTime, maxStopTime);
            }
        }
    }

    void PickNewTarget()
    {
        // Random offset angles within the limit
        float randomX = Random.Range(-maxAngleOffset, maxAngleOffset);
        float randomY = Random.Range(-maxAngleOffset, maxAngleOffset);
        float randomZ = Random.Range(-maxAngleOffset, maxAngleOffset);

        targetRotation = startRotation * Quaternion.Euler(randomX, randomY, randomZ);
        currentSpeed = Random.Range(minSpinSpeed, maxSpinSpeed);
        stateTimer = Random.Range(minMoveTime, maxMoveTime);
    }
}