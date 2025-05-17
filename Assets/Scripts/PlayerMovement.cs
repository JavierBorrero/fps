using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody rb;

    [Header("Orientation")]
    public Transform orientation;

    [Header("Movement")]
    public float walkSpeed;
    public float sprintSpeed;
    public MovementState state;
    public Vector3 moveDirection;
    private float moveSpeed;    
    private bool shiftPressedDown;

    public enum MovementState
    {
        walking,
        sprinting,
        crouching,
        air,
    }

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;
    private bool controlPressedDown;

    [Header("Jump")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    public bool readyToJump;

    [Header("Ground Check")]
    public float groundDrag;
    public float playerHeight;
    public LayerMask ground;
    public bool grounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    [Header("Input")]
    public Vector2 readKeyInputs;
    

    [Header("UI DEBUG")]
    public TextMeshProUGUI speedDebug;

    void Start()
    {
        readyToJump = true;
        startYScale = transform.localScale.y;
    }

    void Update()
    {
        // ======== DEBUG ========
        speedDebug.SetText("Speed: " + rb.velocity.magnitude);

        GroundCheck();
        SpeedControl();
        StateHandler();
        PlayerCrouch();
    }

    void FixedUpdate()
    {
        MovePlayer();
    }

    // === START MOVEMENT ===
    private void MovePlayer()
    {
        moveDirection = orientation.forward * readKeyInputs.y + orientation.right * readKeyInputs.x;

        if(OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection() * moveSpeed * 20f, ForceMode.Force);

            // Evitar que el personaje bote al subir una rampa
            if(rb.velocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }
            
        else if(grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        else if(!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);

        rb.useGravity = !OnSlope();
    }

    private void SpeedControl()
    {
        // Ajutas la velocidad en Slope
        if(OnSlope() && !exitingSlope)
        {
            if(rb.velocity.magnitude > moveSpeed)
                rb.velocity = rb.velocity.normalized * moveSpeed;
        }
        else
        {
            // Obtener velocidad horizontal (Solo nos interesa XZ)
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            // La magnitud del vector es su longitud, si queremos saber si vamos demasiado rapido
            // comparamos la magnitud con la velocidad. Ej si la magnitud es 12 y mi moveSpeed es 7, tendremos que frenar
            if(flatVel.magnitude > moveSpeed) {
                // 
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }   
    }

    private void StateHandler()
    {
        // Sprint
        if(grounded && shiftPressedDown)
        {
            state = MovementState.sprinting;
            moveSpeed = sprintSpeed;
        }
        // Crouch
        else if(grounded && controlPressedDown)
        {
            state = MovementState.crouching;
            moveSpeed = crouchSpeed;
        }
        // Walk
        else if(grounded)
        {
            state = MovementState.walking;
            moveSpeed = walkSpeed;
        }
        // Air
        else
        {
            state = MovementState.air;
        }
    }
    // === END MOVEMENT ===

    // === START GROUND CHECK ===
    private void GroundCheck()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, ground);

        if (grounded)
            rb.drag = groundDrag;
        else
            rb.drag = 0;
    }
    // === END GROUND CHECK ===

    // === START JUMP ===
    private void Jump()
    {
        if(readyToJump && grounded)
        {
            exitingSlope = true;
            
            readyToJump = false;

            // reset y velocity
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

            // Invoke ResetJump usando el `jumpCooldown` como delay
            // Haciendo esto será posible seguir saltando si se mantiene la tecla de salto
            Invoke(nameof(ResetJump), jumpCooldown);
        }
        
    }

    private void ResetJump()
    {
        readyToJump = true;

        exitingSlope = false;
    }
    // === END JUMP ===

    // === START CROUCH ===
    private void PlayerCrouch()
    {
        if(controlPressedDown && state == MovementState.crouching)
        {
            // Cambiamos la escala en `Y` de nuestro personaje para que este agachado
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);

            // Cuando agachamos nuestro personaje, no se agacha directamente en el suelo su altura se reduce en 
            // el aire y parece que esta flotando. Para evitar esto añadimos una fuerza hacia abajo
            rb.AddForce(Vector3.down * 2f, ForceMode.Force);
        }
        else
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }
    }
    // === END CROUCH ===

    // === START SLOPE CHECK ===
    private bool OnSlope()
    {
        if(Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        // No golpea nada
        return false;
    }

    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }
    // === END SLOPE CHECK ===

    // === START INPUT SYSTEM ===
    public void OnMove(InputAction.CallbackContext context)
    {
        readKeyInputs = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        Jump();
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if(context.performed)
            shiftPressedDown = true;
        else if (context.canceled)
            shiftPressedDown = false;
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        if(context.performed)
            controlPressedDown = true;
        else if(context.canceled)
            controlPressedDown = false;
    }
    // === END INPUT SYSTEM
}
