using System.Collections;
using System.Runtime.InteropServices;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.InputSystem;
// Importante para el Hashtable a la hora de sincronizar items entre clientes
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerController : MonoBehaviourPunCallbacks
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

    // Gun Items
    [Header("Items")]
    public Item[] items;
    int itemIndex;
    int previousItemIndex = -1;


    float verticalLookRotation;
    public bool grounded;
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

        // Cliente (yo)
        if (pv.IsMine)
        {
            EquipItem(0);
        }
        // Otros jugadores
        else
        {
            Destroy(GetComponentInChildren<Camera>().gameObject);
            Destroy(rb);
        }
    }

    void Update()
    {
        // Si no soy el dueño devuelvo return (Lado del cliente)
        if (!pv.IsMine) return;

        Look();
        Move();

        // *** Nota: Esta parte de codigo esta usando el Input antiguo
        // De momento no se como implementar esto usando Input System asique se va a quedar asi
        // Recorremos toda la lista de items que tiene el personaje
        for (int i = 0; i < items.Length; i++)
        {
            // Cuando pulsamos la tecla i + 1 (0 + 1 = Tecla 1)
            // Llamamos a la funcion EquipItem con el indice que hayamos pulsado
            if (Input.GetKeyDown((i + 1).ToString()))
            {
                EquipItem(i);
                break;
            }
        }
    }

    void FixedUpdate()
    {
        if (!pv.IsMine)
        {
            return;
        }

        rb.MovePosition(rb.position + transform.TransformDirection(moveAmount) * Time.fixedDeltaTime);
    }

    // === MOVIMIENTO DE CAMARA ===
    void Look()
    {
        transform.Rotate(Vector3.up * mouseInput.x * mouseSensitivity);

        verticalLookRotation += mouseInput.y * mouseSensitivity;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -90f, 90f);

        cameraHolder.transform.localEulerAngles = Vector3.left * verticalLookRotation;
    }

    // === MOVIMIENTO DEL PERSONAJE ===
    void Move()
    {
        moveDir = new Vector3(keyboardInput.x, 0, keyboardInput.y).normalized;

        moveAmount = Vector3.SmoothDamp(moveAmount, moveDir * (isSprinting ? sprintSpeed : walkSpeed), ref smoothMoveVelocity, smoothTime);
    }

    // === SALTO ===
    void Jump()
    {
        rb.AddForce(transform.up * jumpForce);
    }

    void EquipItem(int _index)
    {
        if (_index == previousItemIndex) return;

        itemIndex = _index;

        items[itemIndex].itemGameObject.SetActive(true);

        if (previousItemIndex != -1)
        {
            items[previousItemIndex].itemGameObject.SetActive(false);
        }

        previousItemIndex = itemIndex;

        if (pv.IsMine)
        {
            Hashtable hash = new Hashtable();
            hash.Add("itemIndex", itemIndex);
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (!pv.IsMine && targetPlayer == pv.Owner)
        {
            // cast (int) para evitar error: cannot convert from 'object' to 'int'
            EquipItem((int)changedProps["itemIndex"]);
        }
    }

    // === SET GROUNDED ===
    public void SetGroundedState(bool _grounded)
    {
        grounded = _grounded;
    }

    // === INPUT SYSTEM ===
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
        // No funciona al mantener pulsado
        // problema del input system (button en lugar de any)
        if (context.performed && grounded)
        {
            Jump();
        }
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        // Si se mantiene presionada la tecla
        if (context.performed && grounded)
        {
            isSprinting = true;
        }
        // Cuando se suelta ponemos la variable en false
        else if (context.canceled)
        {
            isSprinting = false;
        }
    }
}
