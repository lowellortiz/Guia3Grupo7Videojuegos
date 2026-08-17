using UnityEngine;

public class PlacementZone : MonoBehaviour
{
    [SerializeField] private UIInteraction ui;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Cube"))
            return;

        Rigidbody rb = other.GetComponent<Rigidbody>();

        if (rb == null)
            return;

        Debug.Log("Cubo colocado correctamente");

        ui.CubePlaced();
    }
}