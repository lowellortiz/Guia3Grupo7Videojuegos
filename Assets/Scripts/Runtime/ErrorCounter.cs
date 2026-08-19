using UnityEngine;
using UnityEngine.UI;

public class ErrorCounter : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Text errorLabel;

    private int totalErrores = 0;

    private int erroresAgarre = 0;
    private int erroresSoltado = 0;
    private int erroresCaida = 0;
    private int solicitudesAyuda = 0;

    public int TotalErrores => totalErrores;
    public int ErroresAgarre => erroresAgarre;
    public int ErroresSoltado => erroresSoltado;
    public int ErroresCaida => erroresCaida;
    public int SolicitudesAyuda => solicitudesAyuda;

    private void Start()
    {
        ActualizarTexto();
    }

    public void RegistrarErrorAgarre()
    {
        erroresAgarre++;
        totalErrores++;

        Debug.Log("Error de agarre. Total errores: " + totalErrores);

        ActualizarTexto();
    }

    public void RegistrarErrorSoltado()
    {
        erroresSoltado++;
        totalErrores++;

        Debug.Log("Error de soltado/apilamiento. Total errores: " + totalErrores);

        ActualizarTexto();
    }

    public void RegistrarCaida()
    {
        erroresCaida++;
        totalErrores++;

        Debug.Log("Cubo caido. Total errores: " + totalErrores);

        ActualizarTexto();
    }

    public void RegistrarAyuda()
    {
        solicitudesAyuda++;
        totalErrores++;

        Debug.Log("Solicitud de ayuda. Total errores: " + totalErrores);

        ActualizarTexto();
    }

    private void ActualizarTexto()
    {
        if (errorLabel != null)
        {
            errorLabel.text = "Errores  " + totalErrores;
        }
    }

    public void MostrarResumen()
    {
        Debug.Log("===== RESUMEN DE ERRORES =====");
        Debug.Log("Agarres fallidos: " + erroresAgarre);
        Debug.Log("Soltados/apilamientos incorrectos: " + erroresSoltado);
        Debug.Log("Caidas: " + erroresCaida);
        Debug.Log("Solicitudes de ayuda: " + solicitudesAyuda);
        Debug.Log("TOTAL: " + totalErrores);
    }

    public void ReiniciarErrores()
    {
        totalErrores = 0;

        erroresAgarre = 0;
        erroresSoltado = 0;
        erroresCaida = 0;
        solicitudesAyuda = 0;

        ActualizarTexto();
    }
   
}