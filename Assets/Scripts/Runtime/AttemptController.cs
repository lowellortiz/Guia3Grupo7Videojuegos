using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Cierra el intento: al apilar los 4 cubos muestra el panel de resultados con el tiempo
/// y el desglose de errores, y deja listo el siguiente intento. Necesario para poder
/// correr las 10 pruebas con usuarios seguidas sin tocar el editor entre una y otra.
/// </summary>
public class AttemptController : MonoBehaviour
{
    /// <summary>Misma clave que borra el menu Guia3/Datos del editor.</summary>
    private const string AttemptKey = "Guia3.Attempt";

    [Header("UI")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private Text resultLabel;
    [SerializeField] private Button resetButton;
    [SerializeField] private Button menuButton;

    [Header("Referencias")]
    [SerializeField] private CubeCounter cubeCounter;
    [SerializeField] private GameTimer gameTimer;
    [SerializeField] private ErrorCounter errorCounter;

    [Header("Configuracion")]
    [SerializeField] private string menuSceneName = "Guia3_Menu";

    [Tooltip("Espera antes de tapar la pantalla, para que se vea la torre terminada.")]
    [SerializeField] private float delayAntesDelPanel = 2f;

    [Header("Boton de rescate")]
    [Tooltip("Boton centrado que aparece si el usuario lleva demasiado tiempo atascado.")]
    [SerializeField] private Button rescueButton;

    [Tooltip("Segundos de intento tras los cuales aparece el boton de rescate.")]
    [SerializeField] private float segundosParaRescate = 120f;

    private bool completado;

    private void OnEnable()
    {
        if (cubeCounter != null) cubeCounter.Completado += MostrarResultados;
        if (resetButton != null) resetButton.onClick.AddListener(Reiniciar);
        if (menuButton != null) menuButton.onClick.AddListener(VolverAlMenu);
        if (rescueButton != null) rescueButton.onClick.AddListener(ReiniciarPorAtasco);
    }

    private void OnDisable()
    {
        if (cubeCounter != null) cubeCounter.Completado -= MostrarResultados;
        if (resetButton != null) resetButton.onClick.RemoveListener(Reiniciar);
        if (menuButton != null) menuButton.onClick.RemoveListener(VolverAlMenu);
        if (rescueButton != null) rescueButton.onClick.RemoveListener(ReiniciarPorAtasco);
    }

    private void Start()
    {
        if (resultPanel != null)
            resultPanel.SetActive(false);

        if (rescueButton != null)
            rescueButton.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (rescueButton == null) return;

        // Se mide con el cronometro del intento, que arranca en el primer input del usuario
        // y no al cargar la escena: asi los 2 minutos son de juego real.
        bool mostrar = !completado
                       && gameTimer != null
                       && gameTimer.TiempoTranscurrido >= segundosParaRescate;

        if (rescueButton.gameObject.activeSelf != mostrar)
            rescueButton.gameObject.SetActive(mostrar);
    }

    /// <summary>
    /// Salida de emergencia para el usuario atascado. Se registra como solicitud de ayuda
    /// antes de recargar, que es una de las variables que la guia pide observar.
    /// </summary>
    public void ReiniciarPorAtasco()
    {
        if (errorCounter != null)
            errorCounter.RegistrarAyuda();

        Reiniciar();
    }

    // El cronometro ya se detuvo antes de llegar aqui, asi que esperar no altera el tiempo
    // que se reporta: solo deja ver la torre terminada antes de tapar la pantalla.
    private void MostrarResultados() => StartCoroutine(MostrarTrasEspera());

    private IEnumerator MostrarTrasEspera()
    {
        completado = true;

        // Si el rescate estaba en pantalla, sobra: el panel trae su propio REINICIAR.
        if (rescueButton != null)
            rescueButton.gameObject.SetActive(false);

        if (delayAntesDelPanel > 0f)
            yield return new WaitForSeconds(delayAntesDelPanel);

        int intento = PlayerPrefs.GetInt(AttemptKey, 0) + 1;
        PlayerPrefs.SetInt(AttemptKey, intento);
        PlayerPrefs.Save();

        if (resultPanel != null)
            resultPanel.SetActive(true);

        if (resultLabel == null) yield break;

        float tiempo = gameTimer != null ? gameTimer.TiempoTranscurrido : 0f;
        int minutos = Mathf.FloorToInt(tiempo / 60f);
        float segundos = tiempo % 60f;

        string texto = $"Intento {intento} completado\n\n" +
                       $"Tiempo total: {minutos:00}:{segundos:00.0}\n";

        if (errorCounter != null)
        {
            texto += $"Errores de agarre: {errorCounter.ErroresAgarre}\n" +
                     $"Errores de colocacion: {errorCounter.ErroresSoltado}\n" +
                     $"Cubos derribados: {errorCounter.ErroresCaida}\n" +
                     $"Total de errores: {errorCounter.TotalErrores}";

            errorCounter.MostrarResumen();
        }

        resultLabel.text = texto;
    }

    /// <summary>
    /// Recarga la escena en vez de reiniciar a mano: el estado del intento vive repartido
    /// en el cronometro, los contadores y las cajas (posicion, layer, isKinematic), y
    /// recargar es la unica forma de garantizar que no quede nada del intento anterior.
    /// </summary>
    public void Reiniciar()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void VolverAlMenu()
    {
        SceneManager.LoadScene(menuSceneName);
    }
}
