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
    private float currentBaseSpeed;
    private float antiBumpFactor = 0.75f;
    private int antiBunnyHopFactor = 1;
    private int jumpTimer;
    private Vector3 moveDirection = Vector3.zero;

    private bool movementLocked;
    private bool jumpLocked;
    private float speedMultiplier = 1f;

    private CharacterController controller;
    private Transform myTransform;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        myTransform = transform;
        currentBaseSpeed = walkSpeed;
        jumpTimer = antiBunnyHopFactor;
    }

    private void Update()
    {
        ReadInput();
        HandleMovement();
    }

    private void ReadInput()
    {
        if (movementLocked)
        {
            inputX = 0f;
            inputY = 0f;
            return;
        }

        inputX = Input.GetAxis("Horizontal");
        inputY = Input.GetAxis("Vertical");

        if (toggleRun)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                currentBaseSpeed = Mathf.Approximately(currentBaseSpeed, walkSpeed) ? runSpeed : walkSpeed;
            }
        }
        else
        {
            currentBaseSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
        }
    }

    private void HandleMovement()
    {
        float inputModifyFactor =
            (inputX != 0f && inputY != 0f && limitDiagonalSpeed) ? 0.7071f : 1f;

        float finalSpeed = currentBaseSpeed * speedMultiplier;

        if (controller.isGrounded)
        {
            moveDirection = new Vector3(inputX * inputModifyFactor, -antiBumpFactor, inputY * inputModifyFactor);
            moveDirection = myTransform.TransformDirection(moveDirection) * finalSpeed;

            if (!jumpLocked)
            {
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
        }
        else
        {
            if (airControl && !movementLocked)
            {
                Vector3 airMove = new Vector3(inputX * inputModifyFactor, 0f, inputY * inputModifyFactor);
                airMove = myTransform.TransformDirection(airMove) * finalSpeed;

                moveDirection.x = airMove.x;
                moveDirection.z = airMove.z;
            }
        }

        moveDirection.y -= gravity * Time.deltaTime;
        controller.Move(moveDirection * Time.deltaTime);
    }

    public void SetMovementLocked(bool locked)
    {
        movementLocked = locked;
    }

    public void SetJumpLocked(bool locked)
    {
        jumpLocked = locked;
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        speedMultiplier = Mathf.Max(0f, multiplier);
    }

    public void ResetMovementModifiers()
    {
        movementLocked = false;
        jumpLocked = false;
        speedMultiplier = 1f;
    }
}