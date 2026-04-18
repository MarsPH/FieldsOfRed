using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    private float inputX;
    private float inputY;

    public float walkSpeed = 3.0f;
    public float runSpeed = 6.0f;
    public bool limitDiagonalSpeed = true;
    public bool toggleRun = false;
    public float jumpSpeed = 8.0f;
    public float defGravity = 20.0f;
    public bool airControl = false;

    public GameObject neckJoint;

    private float antiBumpFactor = .75f;
    private int antiBunnyHopFactor = 1;

    private bool grounded;
    private bool running;
    private CharacterController controller;
    private Transform myTransform;
    private Vector3 moveDirection = Vector3.zero;
    private float speed;
    private int jumpTimer;
    private bool playerControl = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        myTransform = transform;
        speed = walkSpeed;
        jumpTimer = antiBunnyHopFactor;
    }

    void Update()
    {
        inputX = Input.GetAxis("Horizontal");
        inputY = Input.GetAxis("Vertical");

        if (toggleRun)
        {
            if (Input.GetButtonDown("Run"))
            {
                speed = (speed == walkSpeed ? runSpeed : walkSpeed);
            }
        }
        else
        {
            speed = Input.GetButton("Run") ? runSpeed : walkSpeed;
        }
    }

    void FixedUpdate()
    {
        float inputModifyFactor = (inputX != 0.0f && inputY != 0.0f && limitDiagonalSpeed) ? .7071f : 1.0f;

        float maxRot = 1.5f;
        float rate = 2.0f;
        float currentAngle = neckJoint.transform.rotation.eulerAngles.y;

        if (speed > walkSpeed)
        {
            running = true;
            neckJoint.transform.rotation = Quaternion.Lerp(
                neckJoint.transform.rotation,
                Quaternion.Euler(maxRot * 2, currentAngle, 0),
                Time.deltaTime * rate
            );
        }
        else
        {
            running = false;
            neckJoint.transform.rotation = Quaternion.Lerp(
                neckJoint.transform.rotation,
                Quaternion.Euler(inputY * maxRot * 0.1f, currentAngle, -inputX * maxRot * 0.5f),
                Time.deltaTime * rate
            );
        }

        if (grounded)
        {
            moveDirection = new Vector3(inputX * inputModifyFactor, -antiBumpFactor, inputY * inputModifyFactor);
            moveDirection = myTransform.TransformDirection(moveDirection) * speed;
            playerControl = true;

            if (!Input.GetButton("Jump"))
            {
                jumpTimer++;
            }
            else if (jumpTimer >= antiBunnyHopFactor)
            {
                moveDirection.y = jumpSpeed;
                jumpTimer = 0;
            }
        }
        else
        {
            if (airControl && playerControl)
            {
                moveDirection.x = inputX * speed * inputModifyFactor;
                moveDirection.z = inputY * speed * inputModifyFactor;
                moveDirection = myTransform.TransformDirection(moveDirection);
            }
        }

        moveDirection.y -= defGravity * Time.deltaTime;
        grounded = (controller.Move(moveDirection * Time.deltaTime) & CollisionFlags.Below) != 0;
    }
}