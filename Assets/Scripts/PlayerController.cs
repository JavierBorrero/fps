using System.Runtime.InteropServices;
using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Camera Holder")]
    public GameObject cameraHolder;

    [Header("Mouse Sens")]
    public float mouseSensitivity;

    [Header("Movement")]
    public float sprintSpeed;
    public float walkSpeed;
    private bool isSprinting;

    [Header("Jump")]
    public float jumpForce;

    public float smoothTime;

    float verticalLookRotation;
    bool grounded;
    Vector3 smoothMoveVelocity;
    Vector3 moveAmount;
    Vector2 mouseInput;
    public Vector2 keyboardInput;

    public Vector3 moveDir;

    Rigidbody rb;
    PhotonView pv;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        pv = GetComponent<PhotonView>();
    }

    void Start()
    {
        isSprinting = false;

        // Evitar problemas de camara entre jugadores
        if (!pv.IsMine)
        {
            Destroy(GetComponentInChildren<Camera>().gameObject);
            Destroy(rb);
        }
    }

    void Update()
    {
        // Si no soy el dueño devuelvo return
        if (!pv.IsMine) return;

        Look();
        Move();
    }

    void FixedUpdate()
    {
        if (!pv.IsMine) return;

        rb.MovePosition(rb.position + transform.TransformDirection(moveAmount) * Time.fixedDeltaTime);
    }

    void Look()
    {
        transform.Rotate(Vector3.up * mouseInput.x * mouseSensitivity);

        verticalLookRotation += mouseInput.y * mouseSensitivity;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -90f, 90f);

        cameraHolder.transform.localEulerAngles = Vector3.left * verticalLookRotation;
    }

    void Move()
    {
        moveDir = new Vector3(keyboardInput.x, 0, keyboardInput.y).normalized;

        moveAmount = Vector3.SmoothDamp(moveAmount, moveDir * (isSprinting ? sprintSpeed : walkSpeed), ref smoothMoveVelocity, smoothTime);
    }

    void Jump()
    {
        rb.AddForce(transform.up * jumpForce);
    }

    public void SetGroundedState(bool _grounded)
    {
        grounded = _grounded;
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        mouseInput = context.ReadValue<Vector2>();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        keyboardInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && grounded)
        {
            Jump();
        }
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.performed && grounded)
        {
            isSprinting = true;
        }
    }
}
