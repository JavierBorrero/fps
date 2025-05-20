using ExitGames.Client.Photon.StructWrapping;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
// Importante para el Hashtable a la hora de sincronizar items entre clientes
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerController : MonoBehaviourPunCallbacks, IDamageable
{
    [Header("Healthbar")]
    public Image healthbarImage;
    public GameObject ui;

    [Header("Player Manager")]
    public PlayerManager playerManager;

    [Header("Camera Holder")]
    public GameObject cameraHolder;

    [Header("Mouse Sensitivity")]
    public float mouseSensitivity = 1f;

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

    const float maxHealth = 100f;
    float currentHealth = maxHealth;


    Rigidbody rb;
    PhotonView pv;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        pv = GetComponent<PhotonView>();
        // 0 es el indice del ViewID que hemos pasado en el `Instantiate` del PlayerManager
        // PhotonView.Find() nos va a dar el GameObject con ese ID
        playerManager = PhotonView.Find((int)pv.InstantiationData[0]).GetComponent<PlayerManager>();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        isSprinting = false;

        // Cliente (yo)
        if (pv.IsMine)
        {
            EquipItem(0);
        }
        // Componentes de otros jugadores que queremos eliminar porque no son utiles para nuestro jugador
        // El RB es para evitar doble input
        // El UI es para no tener los canvas de la healthbar superpuestos unos encima de otros.
        else
        {
            Destroy(GetComponentInChildren<Camera>().gameObject);
            Destroy(rb);
            Destroy(ui);
        }
    }

    void Update()
    {
        // Si no soy el dueño no hago nada
        if (!pv.IsMine) return;

        Look();
        Move();

        // *** Nota: Esta parte de codigo esta usando el Input antiguo
        // De momento no he conseguido implementar esto usando Input System
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

        if (Input.GetMouseButtonDown(0))
        {
            items[itemIndex].Use();
        }

        // En caso de que el personaje se caiga del mapa por alguna razon
        if (transform.position.y < -10f)
        {
            Die();
        }
    }

    void FixedUpdate()
    {
        // Si no soy el dueño no hago nada
        if (!pv.IsMine)
        {
            return;
        }

        rb.MovePosition(rb.position + transform.TransformDirection(moveAmount) * Time.fixedDeltaTime);
    }

    #region Vista, Movimiento, Salto y Grounded del Personaje

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
    
    // Esto se maneja desde PlayerGroundCheck.cs
    public void SetGroundedState(bool _grounded)
    {
        grounded = _grounded;
    }
    #endregion

    #region Player Equip Items

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

    // Actualizar el objeto en la mano para el resto de jugadores
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (changedProps.ContainsKey("itemIndex") && !pv.IsMine && targetPlayer == pv.Owner)
        {
            // cast (int) para evitar error: cannot convert from 'object' to 'int'
            EquipItem((int)changedProps["itemIndex"]);
        }
    }
    #endregion

    #region Player Damages and death

    // Esta funcion se llama cada vez que el jugador dispara y golpea a un jugador, y solo se llama desde el que ha disparado
    // RPC (Remote Procedure Call) Es una caracteristica de Photon y sirve para ejecutar metodos en el resto de jugadores
    // ** Explicacion extendida en el README
    public void TakeDamage(float damage)
    {
        pv.RPC(nameof(RPC_TakeDamage), pv.Owner, damage);
    }

    // Esta function se llama en el ordenador de cada jugador
    // RPC (Remote Procedure Call) Es una caracteristica de Photon y sirve para ejecutar metodos en el resto de jugadores
    // ** Explicacion extendida en el README
    [PunRPC]
    void RPC_TakeDamage(float damage, PhotonMessageInfo info)
    {
        currentHealth -= damage;

        // Porcentaje entre 0 y 1 de la vida que le queda al personaje.
        // si tenemos 100 de vida: 100/100 = 1 (100% de la barra de vida)
        // si tenemos 50 de vida: 50/100 = 0.5 (50% de la barra de vida)
        healthbarImage.fillAmount = currentHealth / maxHealth;

        if (currentHealth <= 0)
        {
            Die();
            // El parametro info es un parametro que rellena automaticamente Photon.
            // Es util porque podemos mandar el Player que ha enviado el mensaje y luego llamamos al metodo GetKill()
            PlayerManager.Find(info.Sender).GetKill();
        }
    }

    void Die()
    {
        playerManager.Die();
    }
    #endregion

    #region InputSystem

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

    public void OnTab(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Scoreboard.instance.canvasGroup.alpha = 1;
        }
        else if (context.canceled)
        {
            Scoreboard.instance.canvasGroup.alpha = 0;
        }
    }
    #endregion
}