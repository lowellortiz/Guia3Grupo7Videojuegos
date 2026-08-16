using UnityEngine;
using UnityEngine.EventSystems;

public class MobileCameraLook : MonoBehaviour, IDragHandler
{
    [Header("Referencias")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform cameraPivot;

    [Header("Configuracion")]
    [SerializeField] private float sensitivity = 0.15f;
    [SerializeField] private float maxVerticalAngle = 80f;

    private float verticalRotation = 0f;

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 delta = eventData.delta;

        // Movimiento horizontal
        float horizontal = delta.x * sensitivity;
        player.Rotate(0f, horizontal, 0f);

        // Movimiento vertical
        verticalRotation -= delta.y * sensitivity;

        verticalRotation = Mathf.Clamp(
            verticalRotation,
            -maxVerticalAngle,
            maxVerticalAngle
        );

        cameraPivot.localRotation =
            Quaternion.Euler(verticalRotation, 0f, 0f);
    }
}