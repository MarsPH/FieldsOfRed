using UnityEngine;

public class LanternSwing : MonoBehaviour
{
    public CharacterController controller;

    [Header("Movement Swing")]
    public float sideSwingAmount = 0.08f;
    public float forwardSwingAmount = 0.06f;

    [Header("Look Swing")]
    public float mouseXSwingAmount = 3f;
    public float mouseYSwingAmount = 2f;

    [Header("Limits")]
    public float maxMoveAngle = 8f;
    public float maxLookAngle = 6f;
    public float smoothSpeed = 8f;

    [Header("Bob")]
    public float bobAmount = 0.03f;
    public float bobSpeed = 7f;

    private Quaternion startLocalRotation;
    private Vector3 startLocalPosition;
    private Vector3 lastVelocity;

    private float currentMoveSide;
    private float currentMoveForward;
    private float currentLookSide;
    private float currentLookForward;
    private float bobTimer;

    void Start()
    {
        startLocalRotation = transform.localRotation;
        startLocalPosition = transform.localPosition;
    }

    void LateUpdate()
    {
        if (controller == null)
            return;

        Vector3 velocity = controller.velocity;
        Vector3 accel = (velocity - lastVelocity) / Mathf.Max(Time.deltaTime, 0.0001f);
        lastVelocity = velocity;

        Vector3 localAccel = controller.transform.InverseTransformDirection(accel);

        float targetMoveSide = Mathf.Clamp(localAccel.x * -sideSwingAmount, -maxMoveAngle, maxMoveAngle);
        float targetMoveForward = Mathf.Clamp(localAccel.z * -forwardSwingAmount, -maxMoveAngle, maxMoveAngle);

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        float targetLookSide = Mathf.Clamp(mouseX * -mouseXSwingAmount, -maxLookAngle, maxLookAngle);
        float targetLookForward = Mathf.Clamp(mouseY * mouseYSwingAmount, -maxLookAngle, maxLookAngle);

        currentMoveSide = Mathf.Lerp(currentMoveSide, targetMoveSide, smoothSpeed * Time.deltaTime);
        currentMoveForward = Mathf.Lerp(currentMoveForward, targetMoveForward, smoothSpeed * Time.deltaTime);

        currentLookSide = Mathf.Lerp(currentLookSide, targetLookSide, smoothSpeed * Time.deltaTime);
        currentLookForward = Mathf.Lerp(currentLookForward, targetLookForward, smoothSpeed * Time.deltaTime);

        float finalSide = currentMoveSide + currentLookSide;
        float finalForward = currentMoveForward + currentLookForward;

        Quaternion targetRotation = startLocalRotation * Quaternion.Euler(finalForward, 0f, finalSide);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, smoothSpeed * Time.deltaTime);

        Vector3 horizontalVelocity = new Vector3(controller.velocity.x, 0f, controller.velocity.z);
        float speed = horizontalVelocity.magnitude;

        if (controller.isGrounded && speed > 0.1f)
        {
            bobTimer += Time.deltaTime * bobSpeed * speed;
        }
        else
        {
            bobTimer = 0f;
        }

        float bobY = Mathf.Sin(bobTimer) * bobAmount;
        Vector3 targetPosition = startLocalPosition + new Vector3(0f, bobY, 0f);

        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, smoothSpeed * Time.deltaTime);
    }
}