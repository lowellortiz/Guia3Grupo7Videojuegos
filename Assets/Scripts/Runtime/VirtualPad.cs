using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI; // Agregamos esta línea arriba

public class VirtualPad : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Referencias UI")]
    // Cambiamos 'TMP_Text' por 'Text' si estás usando el texto tradicional de Unity
    [SerializeField] private Text moveVectorText; 
    [SerializeField] private Text normalizedDirText;

    private Vector2 moveInput;

    private void OnEnable()
    {
        // Solución al error de ambigüedad usando el namespace explícito de InputSystem
        if (UnityEngine.InputSystem.Accelerometer.current != null)
            InputSystem.EnableDevice(UnityEngine.InputSystem.Accelerometer.current);

        if (UnityEngine.InputSystem.Gyroscope.current != null)
            InputSystem.EnableDevice(UnityEngine.InputSystem.Gyroscope.current);
    }

    // Este método se vincula automáticamente al PlayerInput (Evento "Move")
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    private void Update()
    {
        // 1. Mover el objeto en los ejes X y Z (plano 3D)
        Vector3 movement = new Vector3(moveInput.x, 0f, moveInput.y);
        transform.Translate(movement * moveSpeed * Time.deltaTime, Space.World);

        // 2. Actualizar la interfaz de usuario
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