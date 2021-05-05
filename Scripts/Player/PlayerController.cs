using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour {

    //\===========================================================================================
    //\ Variables
    //\===========================================================================================

    #region Variables

    [SerializeField]
    private float m_jumpHeight;        // How high the player can jump.
    [SerializeField]
    private bool m_disableJumping = false;
    [SerializeField]
    private float m_adjustedJumpHeight;
    [SerializeField]
    private float m_walkSpeed;         // How fast the player walks.
    [SerializeField]
    private float m_runSpeedMultiplier;// How fast the player runs.
    [SerializeField]
    private float m_rotateSpeed;       // How fast the camera can turn.
    [SerializeField]
    private float m_gravity;           // Gravity of the player
    [SerializeField]
    [Range(0.5f, 0.99f)]
    private float m_movementSlowDown;  // The amount the player will slow down every frame when not pressing movement keys
    [SerializeField]
    private float m_cameraLookLimitAngle = 45f;
    [SerializeField]
    private bool m_lockCursor = true;  // A bool to control the state of the cursor

    public bool m_stopMovement = false;

    private CharacterController m_cc = null;  // The objects character controller
    private Vector3 m_moveDir;                // Vector3 used for controller the character controller
    private Transform m_cameraTransform;      // Transform of the gameobject containing the camera

    #endregion

    //\===========================================================================================
    //\ Unity Methods
    //\===========================================================================================

    #region Unity Methods
    private void Awake()
    {
        m_cc = GetComponent<CharacterController>();
        m_adjustedJumpHeight = m_jumpHeight * 0.01f;
        m_cameraTransform = transform.GetChild(0);

        // Set the cursor state
        if (m_lockCursor)
            Cursor.lockState = CursorLockMode.Locked;

        // Set the player camera rotation to be 0 (straight forward) at start
        transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, 0f, transform.localEulerAngles.z);
        m_cameraTransform.localEulerAngles = new Vector3(0f, m_cameraTransform.localEulerAngles.y, m_cameraTransform.localEulerAngles.z);
    }

    private void OnValidate()
    {
        m_adjustedJumpHeight = m_jumpHeight * 0.01f;
    }

    void Update()//Updates the players position, rotation, animation depending on inputs.
	{
        // If the cursor is not locked to the screen then don't rotate the camera
        if (Cursor.lockState != CursorLockMode.None && !m_stopMovement)
        {

            // Rotation of the camera
            float rotX = Input.GetAxis("Mouse X") * Time.deltaTime * m_rotateSpeed;
            // This value needs to be clamped as if the mouse moves too fast the camera can flip and get stuck,
            // this only happens in extreme examples where the game freezes and the mouse moves the entire screen
            // however it is still common
            float rotY = Mathf.Clamp(Input.GetAxis("Mouse Y") * Time.deltaTime * m_rotateSpeed, -25, 25);

            // We will rotate this object on the X axis and the camera object (both are separate) on the Y axis
            // This may seem counter-intuitive or a bit unnecessary however if we rotate the same object by these
            // Amounts the camera will start to flip upside down
            transform.Rotate(0, rotX, 0);

            // This code checks for the current angle of the camera and stops any more rotation if the camera has
            // gone too far up or down, this stops the camera from flipping upside down and doing other weird things
            float yLookAngle = m_cameraTransform.localEulerAngles.x + -rotY;
            if (yLookAngle >= 0 && yLookAngle <= 90)
            {
                if (yLookAngle > m_cameraLookLimitAngle)
                    yLookAngle = m_cameraLookLimitAngle;
            }
            else if (yLookAngle <= 360 && yLookAngle >= 270)
            {
                yLookAngle = yLookAngle - 360f;
                if (yLookAngle < -m_cameraLookLimitAngle)
                    yLookAngle = -m_cameraLookLimitAngle;
            }

            // Applying the camera x rotation (the result affects the y axis)
            m_cameraTransform.localEulerAngles = new Vector3(yLookAngle, m_cameraTransform.localEulerAngles.y, m_cameraTransform.localEulerAngles.z);


            // Movement when grounded
            if (m_cc.isGrounded)
            {
                float moveSpeedVertical = 0f;
                float moveSpeedHorizontal = 0f;

                moveSpeedVertical = Input.GetAxis("Vertical") * Time.deltaTime;
                moveSpeedHorizontal = Input.GetAxis("Horizontal") * Time.deltaTime;

                // Slowing down the player
                m_moveDir.x *= m_movementSlowDown;
                m_moveDir.z *= m_movementSlowDown;

                // Some temporary movement vectors so that we can have strafing as well as walking forwards and backwards
                Vector3 moveDirVertical = new Vector3();
                Vector3 moveDirHorizontal = new Vector3();

                // Forward/backword movement
                if (Input.GetAxisRaw("Vertical") != 0 && Input.GetButton("Sprint"))//If the player is running.
                {
                    moveDirVertical = transform.forward * moveSpeedVertical * (m_walkSpeed * m_runSpeedMultiplier);
                }
                else if (Input.GetAxisRaw("Vertical") != 0)//If the player is walking.
                {
                    moveDirVertical = transform.forward * moveSpeedVertical * m_walkSpeed;
                }

                // Strafe movement
                if (Input.GetAxisRaw("Horizontal") != 0 && Input.GetButton("Sprint"))
                {
                    moveDirHorizontal = transform.right * moveSpeedHorizontal * (m_walkSpeed * m_runSpeedMultiplier);
                }
                else if (Input.GetAxisRaw("Horizontal") != 0)
                {
                    moveDirHorizontal = transform.right * moveSpeedHorizontal * m_walkSpeed;
                }

                // Adding the movement vectors together
                m_moveDir = moveDirHorizontal + moveDirVertical;

                // There is a bug where if the player is strafing and moving the movement speed will add together because of the previous line
                // causing the player to move faster than they should be.
                // This check attempts to limit that
                if (moveDirHorizontal.magnitude > 0 && moveDirVertical.magnitude > 0f)
                {
                    m_moveDir *= 0.75f;
                }

                // Jumping
                if (Input.GetButton("Jump") && !m_disableJumping)
                {
                    m_moveDir.y = m_adjustedJumpHeight;
                }
            }
        }
        else
        {
            m_moveDir.x = 0f;
            m_moveDir.z = 0f;
        }

        // Applying gravity
        m_moveDir.y -= m_gravity * Time.deltaTime;
        //Move the player forwards or backwards at a lower speed depending on vertical input.
        m_cc.Move(m_moveDir);
    }

	#endregion
}
