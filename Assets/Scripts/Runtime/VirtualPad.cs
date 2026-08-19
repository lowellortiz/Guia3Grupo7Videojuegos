using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class VirtualPad : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Referencias UI")]
    [SerializeField] private Text moveVectorText; 
    [SerializeField] private Text normalizedDirText;

    private Vector2 moveInput;
    private CharacterController controller;
    private float verticalVelocity;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void OnEnable()
    {
        // Habilitar sensores físicos si son necesarios
        if (UnityEngine.InputSystem.Accelerometer.current != null)
            InputSystem.EnableDevice(UnityEngine.InputSystem.Accelerometer.current);

        if (UnityEngine.InputSystem.Gyroscope.current != null)
            InputSystem.EnableDevice(UnityEngine.InputSystem.Gyroscope.current);
    }

    // Este método es llamado por el componente 'Player Input' (Behavior: Send Messages)
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    private void Update()
    {
        // 1. Direccion relativa a hacia donde mira el jugador.
        //    MobileCameraLook rota el yaw de este transform, asi que forward/right
        //    ya son horizontales (el pitch vive en CameraPivot).
        Vector3 direction = transform.right * moveInput.x + transform.forward * moveInput.y;
        direction = Vector3.ClampMagnitude(direction, 1f);

        // 2. Mover con el CharacterController para tener colision y gravedad
        if (controller != null)
        {
            if (controller.isGrounded && verticalVelocity < 0f)
                verticalVelocity = -2f; // pega al suelo, evita el "escaloneo" al bajar
            else
                verticalVelocity += Physics.gravity.y * Time.deltaTime;

            Vector3 velocity = direction * moveSpeed + Vector3.up * verticalVelocity;
            controller.Move(velocity * Time.deltaTime);
        }
        else
        {
            transform.Translate(direction * moveSpeed * Time.deltaTime, Space.World);
        }

        // 3. Refrescar textos en UI
        UpdateUI();
    }

    private void UpdateUI()
    {
        Vector2 normalizedDir = moveInput.normalized;

        if (moveVectorText != null)
        {
            moveVectorText.text = $"Vector Movimiento: {moveInput:F2}";
        }

        if (normalizedDirText != null)
        {
            normalizedDirText.text = $"Dirección Norm.: {normalizedDir:F2} | Mag: {moveInput.magnitude:F2}";
        }
    }
}