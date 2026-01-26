using UnityEngine;
using UnityEngine.InputSystem; // Necesario para el nuevo sistema de inputs

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
    [SerializeField] private float rotationSpeed = 60f; // Ajustado para VR

    [Header("Configuración de Gravedad")]
    [SerializeField] private float gravityValue = -9.81f;
    [SerializeField] private float jumpSpeed = 5.0f;
    [SerializeField] public LayerMask groundLayer;

    [Header("Selección de Input")]
    [SerializeField] private InputMode currentInputMode = InputMode.Controller;
    public enum InputMode { Controller, Hands }

    [Header("Referencias de Inputs (Meta/OpenXR)")]
    // CAMBIO: Usamos 'Reference' en lugar de 'Property' para simplificar el Inspector
    public InputActionReference moveActionSource; 
    public InputActionReference turnActionSource;
    public InputActionReference jumpActionSource;
    public InputActionReference sprintActionSource;

    // Componentes y referencias
    private CharacterController controller;
    private float verticalVelocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        
        // En VR el Singleton es útil, pero asegúrate de que no choque con otras escenas
        SetInputMode(currentInputMode);
    }

    public void SetInputMode(InputMode newMode)
    {
        currentInputMode = newMode;
        Debug.Log($"Modo de Input cambiado a: {currentInputMode}");
    }

    void Update()
    {
        // --- 1. Obtener Inputs según el modo ---
        Vector2 moveInput = Vector2.zero;
        Vector2 rotInput = Vector2.zero;
        bool isSprinting = false;
        bool isJumping = false;

        if (currentInputMode == InputMode.Controller)
        {
            // Leemos directamente de las acciones de Unity Input System
            moveInput = moveActionSource.action != null ? moveActionSource.action.ReadValue<Vector2>() : Vector2.zero;
            rotInput = turnActionSource.action != null ? turnActionSource.action.ReadValue<Vector2>() : Vector2.zero;
            
            // Sprint y Salto (Opcional: verifica que la acción esté asignada)
            isSprinting = sprintActionSource.action != null && sprintActionSource.action.ReadValue<float>() > 0.5f;
            isJumping = jumpActionSource.action != null && jumpActionSource.action.WasPressedThisFrame();
        }
        else if (currentInputMode == InputMode.Hands)
        {
            // LÓGICA PARA MANOS:
            // Por defecto, las manos no tienen joysticks. 
            // Aquí podrías conectar un sistema de reconocimiento de gestos (ej. pellizcar para avanzar).
            // Por ahora, lo dejamos en cero para que no haya "drift" fantasma.
            moveInput = Vector2.zero;
            rotInput = Vector2.zero;
        }

        // --- 2. Lógica de Rotación (Solo Eje Y) ---
        // En VR SOLO rotamos al personaje horizontalmente. La cámara la mueve el usuario con su cabeza.
        transform.Rotate(Vector3.up, rotInput.x * rotationSpeed * Time.deltaTime);


        // --- 3. Lógica de Gravedad ---
        if (IsGrounded())
        {
            if (verticalVelocity < 0)
            {
                verticalVelocity = -2f; // Pequeña fuerza hacia abajo para mantenerlo pegado al suelo
            }

            if (isJumping)
            {
                verticalVelocity = jumpSpeed;
            }
        }
        
        verticalVelocity += gravityValue * Time.deltaTime;

        // --- 4. Lógica de Movimiento ---
        float currentSpeed = isSprinting ? runSpeed : walkSpeed;

        // Importante: Usamos 'transform' en lugar de 'cameraTransform' para la dirección base
        // para asegurar que el movimiento siga la orientación del cuerpo (XR Origin).
        // Si quieres que siga la cabeza, usa Camera.main.transform, pero ten cuidado con el mareo.
        Vector3 moveDirection = (transform.forward * moveInput.y + transform.right * moveInput.x);
        
        moveDirection.y = 0; 
        moveDirection.Normalize();

        // Aplicar movimiento
        controller.Move(moveDirection * currentSpeed * Time.deltaTime);

        // Aplicar gravedad
        controller.Move(new Vector3(0, verticalVelocity, 0) * Time.deltaTime);
    }

    bool IsGrounded()
    {
        // Ajuste para el Character Controller
        Vector3 start = transform.position + Vector3.up * 0.1f;
        return Physics.Raycast(start, Vector3.down, 0.3f, groundLayer);
    }
}