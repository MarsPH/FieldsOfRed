using UnityEngine;

public class EyeFollowPlayer : MonoBehaviour
{
    [Header("Target")]
    public Transform player;
    public Camera playerCamera; // Assign the player's camera

    [Header("Activation")]
    public float activationAngle = 15f;   // How many degrees off-center counts as "looking at it"
    public bool activated = false;        // Becomes true once the player looks at it (readonly at runtime)

    [Header("Settings")]
    public float rotationSpeed = 5f;
    public bool smoothRotation = true;

    private const float X_OFFSET = 90f;

    void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
            else
                Debug.LogWarning("EyeFollowPlayer: No player assigned and none found with tag 'Player'.");
        }

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            if (playerCamera == null)
                Debug.LogWarning("EyeFollowPlayer: No camera found. Assign playerCamera in Inspector.");
        }
    }

    void Update()
    {
        if (player == null || playerCamera == null) return;

        if (!activated)
        {
            CheckIfPlayerLooking();
            return; // Stay frozen until activated
        }

        RotateTowardsPlayer();
    }

    void CheckIfPlayerLooking()
    {
        // Direction from camera to eye
        Vector3 directionToEye = (transform.position - playerCamera.transform.position).normalized;

        // Angle between camera's forward and the direction toward the eye
        float angle = Vector3.Angle(playerCamera.transform.forward, directionToEye);

        if (angle <= activationAngle)
        {
            activated = true;
        }
    }

    void RotateTowardsPlayer()
    {
        Vector3 directionToPlayer = player.position - transform.position;

        if (directionToPlayer == Vector3.zero) return;

        Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
        targetRotation *= Quaternion.Euler(0f, 0f, 0f);

        if (smoothRotation)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime * rotationSpeed
            );
        }
        else
        {
            transform.rotation = targetRotation;
        }
    }
}