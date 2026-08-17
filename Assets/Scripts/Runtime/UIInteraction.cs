using UnityEngine;
using TMPro;

public class UIInteraction : MonoBehaviour
{
    [SerializeField] private TMP_Text estadoTexto;
    [SerializeField] private TMP_Text cubosTexto;

    private int cubosColocados = 0;

    private void Start()
    {
        estadoTexto.text = "Toca y mantén para tomar un cubo";
        ActualizarCubos();
    }

    public void CubeGrabbed()
    {
        estadoTexto.text = "Sosteniendo cubo...";
    }

    public void CubeReleased()
    {
        estadoTexto.text = "Cubo soltado";
    }

    public void CubePlaced()
    {
        cubosColocados++;

        estadoTexto.text = "¡Cubo colocado correctamente!";

        ActualizarCubos();
    }

    private void ActualizarCubos()
    {
        cubosTexto.text = "Cubos: " + cubosColocados + " / 4";
    }
}