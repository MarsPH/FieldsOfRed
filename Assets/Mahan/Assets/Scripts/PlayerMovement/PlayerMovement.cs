using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float jumpSpeed = 8f;
    public float gravity = 20f;
    public bool limitDiagonalSpeed = true;
    public bool toggleRun = false;
    public bool airControl = false;

    private float inputX;
    private float inputY;
    private float speed;
    private float antiBumpFactor = 0.75f;
    private int antiBunnyHopFactor = 1;
    private int jumpTimer;
    private bool playerControl;
    private Vector3 moveDirection = Vector3.zero;

    private CharacterController controller;
    private Transform myTransform;

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
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                speed = (speed == walkSpeed) ? runSpeed : walkSpeed;
            }
        }
        else
        {
            speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
        }
    }

    void FixedUpdate()
    {
        float inputModifyFactor =
            (inputX != 0f && inputY != 0f && limitDiagonalSpeed) ? 0.7071f : 1f;

        if (controller.isGrounded)
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

        moveDirection.y -= gravity * Time.deltaTime;
        controller.Move(moveDirection * Time.deltaTime);
    }
}