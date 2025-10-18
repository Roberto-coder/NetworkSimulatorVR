using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    #region Singleton
    public static PlayerController Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }
    #endregion

    [Header("Configuración de Movimiento")]
    [SerializeField] private float walkSpeed = 2.0f;
    [SerializeField] private float runSpeed = 4.0f;
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] public Transform cameraTransform; // Arrastra tu cámara aquí

    [Header("Configuración de Gravedad")]
    [SerializeField] private float gravityValue = -9.81f;
    [SerializeField] private float jumpSpeed = 5.0f;
    [SerializeField] public LayerMask groundLayer; 

    [Header("Selección de Input")]
    [SerializeField] private InputMode currentInputMode = InputMode.Xbox;
    public enum InputMode { Xbox, MetaXR }

    // Componentes y referencias
    private CharacterController controller;
    private IPlayerInput inputSource;
    private float verticalVelocity;
    private float cameraPitch = 0f; // Para rotación vertical de cámara

    void Start()
    {
        controller = GetComponent<CharacterController>();
        SetInputMode(currentInputMode);

        if (cameraTransform == null)
        {
            Debug.LogError("¡No se asignó el 'cameraTransform' en el PlayerController!");
            // Busca la cámara principal como fallback (puede no ser la correcta)
            cameraTransform = Camera.main.transform;
        }
    }

    public void SetInputMode(InputMode newMode)
    {
        currentInputMode = newMode;
        switch (currentInputMode)
        {
            case InputMode.Xbox:
                inputSource = new XboxInput();
                Debug.Log("Input seteado a: Xbox");
                break;
            case InputMode.MetaXR:
                inputSource = new MetaXRInput();
                Debug.Log("Input seteado a: MetaXR");
                break;
        }
    }

    void Update()
    {
        // --- 1. Obtener todos los inputs desde la interfaz ---
        Vector2 moveInput = inputSource.GetMoveInput();
        Vector2 rotInput = inputSource.GetRotationInput();
        bool isSprinting = inputSource.GetSprintInput();
        bool isJumping = inputSource.GetJumpInputDown();

        // --- 2. Lógica de Rotación (de tu script) ---

        // Rotación Horizontal (Yaw) - Rota todo el CharacterController
        transform.Rotate(Vector3.up, rotInput.x * rotationSpeed * Time.deltaTime);

        // Rotación Vertical (Pitch) - Rota solo la cámara
        // Usamos una variable 'cameraPitch' para evitar problemas con los ángulos Euler
        cameraPitch -= rotInput.y * rotationSpeed * Time.deltaTime;
        cameraPitch = Mathf.Clamp(cameraPitch, -80f, 80f);
        cameraTransform.localEulerAngles = new Vector3(cameraPitch, 0, 0);

        // --- 3. Lógica de Gravedad (de tu script) ---
        if (IsGrounded()) 
        {
            // Resetea la velocidad vertical (si no está saltando)
            if (verticalVelocity < 0)
            {
                verticalVelocity = -2f;
            }

            // Comprueba si se presionó el botón de salto
            if (isJumping)
            {
                // Aplica la velocidad de salto
                verticalVelocity = jumpSpeed;
            }
        }
        // Aplica gravedad CADA frame (esté o no en el suelo)
        verticalVelocity += gravityValue * Time.deltaTime;

        // --- 4. Lógica de Movimiento (de tu script) ---
        float currentSpeed = isSprinting ? runSpeed : walkSpeed;

        // Movimiento relativo a la cámara
        Vector3 moveDirection = (cameraTransform.forward * moveInput.y + cameraTransform.right * moveInput.x);
        moveDirection.y = 0; // No queremos volar
        moveDirection.Normalize();

        // Aplicar movimiento
        controller.Move(moveDirection * currentSpeed * Time.deltaTime);

        // Aplicar gravedad
        controller.Move(new Vector3(0, verticalVelocity, 0) * Time.deltaTime);
    }

    // --- Función IsGrounded (copiada de tu script) ---
    bool IsGrounded()
    {
        // Ajusta los valores 0.1f (altura) y 0.3f (distancia) según tu CharacterController
        Vector3 start = transform.position + Vector3.up * 0.1f;
        return Physics.Raycast(start, Vector3.down, 0.3f, groundLayer);
    }
}