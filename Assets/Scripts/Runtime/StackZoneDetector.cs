using UnityEngine;

public class StackZoneDetector : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private CubeCounter cubeCounter;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Cube"))
        {
            cubeCounter.RegistrarCubo(other.gameObject);

            Debug.Log("Entro a PuntoB: " + other.gameObject.name);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Cube"))
        {
            cubeCounter.QuitarCubo(other.gameObject);

            Debug.Log("Salio de PuntoB: " + other.gameObject.name);
        }
    }
}