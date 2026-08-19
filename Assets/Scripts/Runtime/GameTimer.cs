using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameTimer : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Text timerLabel;

    private float tiempoInicio;
    private float tiempoActual;

    private bool iniciado = false;
    private bool finalizado = false;

    public float TiempoTranscurrido => tiempoActual;
    public bool Finalizado => finalizado;

    private void Start()
    {
        ActualizarTexto(0f);
    }

    private void Update()
    {
        if (!iniciado)
        {
            DetectarPrimerInput();
            return;
        }

        if (finalizado)
            return;

        tiempoActual = Time.time - tiempoInicio;

        ActualizarTexto(tiempoActual);
    }

    private void DetectarPrimerInput()
    {
        bool primerInput = false;

        // Para probar con mouse dentro del Editor
        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            primerInput = true;
        }

        // Para celular
        if (Touchscreen.current != null &&
            Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            primerInput = true;
        }

        if (primerInput)
        {
            IniciarCronometro();
        }
    }

    public void IniciarCronometro()
    {
        if (iniciado)
            return;

        iniciado = true;
        tiempoInicio = Time.time;

        Debug.Log("Cronometro iniciado");
    }

    public void FinalizarCronometro()
    {
        if (!iniciado || finalizado)
            return;

        finalizado = true;

        tiempoActual = Time.time - tiempoInicio;

        ActualizarTexto(tiempoActual);

        Debug.Log(
            "Cronometro finalizado: " +
            tiempoActual.ToString("F1") +
            " segundos"
        );
    }

    public void ReiniciarCronometro()
    {
        iniciado = false;
        finalizado = false;

        tiempoInicio = 0f;
        tiempoActual = 0f;

        ActualizarTexto(0f);
    }

    private void ActualizarTexto(float tiempo)
    {
        if (timerLabel == null)
            return;

        int minutos = Mathf.FloorToInt(tiempo / 60f);
        float segundos = tiempo % 60f;

        timerLabel.text =
            $"{minutos:00}:{segundos:00.0}";
    }
}
