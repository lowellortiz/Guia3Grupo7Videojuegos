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
        // 1. Mover el objeto en el plano XZ
        Vector3 movement = new Vector3(moveInput.x, 0f, moveInput.y);
        transform.Translate(movement * moveSpeed * Time.deltaTime, Space.World);

        // 2. Refrescar textos en UI
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