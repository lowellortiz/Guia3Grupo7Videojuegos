using UnityEngine;

public class CubeFallDetector : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private ErrorCounter errorCounter;

    private bool tocandoSuelo = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Floor") && !tocandoSuelo)
        {
            tocandoSuelo = true;

            if (errorCounter != null)
            {
                errorCounter.RegistrarCaida();
            }

            Debug.Log("Caida detectada: " + gameObject.name);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Floor"))
        {
            tocandoSuelo = false;
        }
    }
}