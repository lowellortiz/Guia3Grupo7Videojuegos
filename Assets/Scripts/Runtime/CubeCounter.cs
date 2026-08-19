using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CubeCounter : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Text counterLabel;

    [Header("Cronometro")]
    [SerializeField] private GameTimer gameTimer;

    private HashSet<GameObject> cubosCorrectos = new HashSet<GameObject>();

    /// <summary>Se dispara cuando los 4 cubos quedan apilados. Lo escucha AttemptController.</summary>
    public event System.Action Completado;

    public int Cubos => cubosCorrectos.Count;

    private void Start()
    {
        ActualizarTexto();
    }

    public void RegistrarCubo(GameObject cubo)
    {
        if (cubo == null)
            return;

        // Evita contar el mismo cubo varias veces
        if (cubosCorrectos.Add(cubo))
        {
            ActualizarTexto();

            Debug.Log("Cubo correcto. Total: " + cubosCorrectos.Count);

            if (cubosCorrectos.Count >= 4)
            {
                Debug.Log("Los 4 cubos fueron colocados.");

                if (gameTimer != null)
                    gameTimer.FinalizarCronometro();

                Completado?.Invoke();
            }
        }
    }

    public void QuitarCubo(GameObject cubo)
    {
        if (cubo == null)
            return;

        if (cubosCorrectos.Remove(cubo))
        {
            ActualizarTexto();

            Debug.Log("Cubo retirado. Total: " + cubosCorrectos.Count);
        }
    }

    private void ActualizarTexto()
    {
        if (counterLabel != null)
        {
            counterLabel.text =
                "Cubos apilados  " +
                cubosCorrectos.Count +
                " / 4";
        }
    }
}