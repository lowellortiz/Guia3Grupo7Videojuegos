using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Boton del menu que cicla la preferencia de orientacion y muestra la actual en su etiqueta.
/// </summary>
public class OrientationOptionButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Text label;

    private void Reset()
    {
        button = GetComponent<Button>();
        label = GetComponentInChildren<Text>();
    }

    private void OnEnable()
    {
        if (button != null) button.onClick.AddListener(Ciclar);
        Refrescar();
    }

    private void OnDisable()
    {
        if (button != null) button.onClick.RemoveListener(Ciclar);
    }

    private void Ciclar()
    {
        OrientationSettings.Siguiente();
        Refrescar();
    }

    private void Refrescar()
    {
        if (label != null)
            label.text = OrientationSettings.Etiqueta(OrientationSettings.Actual);
    }
}
