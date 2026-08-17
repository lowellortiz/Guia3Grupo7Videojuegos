using UnityEngine;
using UnityEngine.InputSystem;

public class HoldToGrabCube : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform holdPoint;
    [SerializeField] private InputActionReference grabAction;

    [Header("Configuration")]
    [SerializeField] private float maxDistance = 4f;
    [SerializeField] private LayerMask interactableLayer;

    private Rigidbody grabbedObject;
    private Transform grabbedTransform;

    private void OnEnable()
    {
        grabAction.action.Enable();

        grabAction.action.performed += OnGrabPressed;
        grabAction.action.canceled += OnGrabReleased;
    }

    private void OnDisable()
    {
        grabAction.action.performed -= OnGrabPressed;
        grabAction.action.canceled -= OnGrabReleased;

        grabAction.action.Disable();
    }

    private void OnGrabPressed(InputAction.CallbackContext context)
    {
        if (grabbedObject == null)
        {
            TryGrab();
        }
    }

    private void OnGrabReleased(InputAction.CallbackContext context)
    {
        if (grabbedObject != null)
        {
            ReleaseObject();
        }
    }

    private void TryGrab()
    {
        Ray ray = playerCamera.ViewportPointToRay(
            new Vector3(0.5f, 0.5f, 0f)
        );

        if (Physics.Raycast(
            ray,
            out RaycastHit hit,
            maxDistance,
            interactableLayer))
        {
            Rigidbody rb = hit.collider.GetComponent<Rigidbody>();

            if (rb == null)
                return;

            grabbedObject = rb;
            grabbedTransform = rb.transform;

            grabbedObject.useGravity = false;
            grabbedObject.isKinematic = true;

            grabbedTransform.position = holdPoint.position;
            grabbedTransform.rotation = holdPoint.rotation;
        }
    }

    private void ReleaseObject()
    {
        grabbedObject.isKinematic = false;
        grabbedObject.useGravity = true;

        grabbedObject = null;
        grabbedTransform = null;
    }

    private void Update()
    {
        if (grabbedObject != null)
        {
            grabbedObject.transform.position = holdPoint.position;
            grabbedObject.transform.rotation = holdPoint.rotation;
        }
    }
}