using UnityEngine;
using UnityEngine.InputSystem;

public class HoldToGrabCube : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform holdPoint;
    [SerializeField] private InputActionReference grabAction;
    [SerializeField] private ErrorCounter errorCounter;
    [SerializeField] private AutoStackZone stackZone;

    [Header("Configuration")]
    [SerializeField] private float maxDistance = 4f;
    [SerializeField] private LayerMask interactableLayer;

    private Rigidbody grabbedObject;
    private Transform grabbedTransform;
    private Collider grabbedCollider;

    private CharacterController characterController;
    private bool maskWarningShown;

    /// <summary>True mientras el jugador sostiene una caja. Lo lee AutoStackZone.</summary>
    public bool IsCarrying => grabbedObject != null;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void OnEnable()
    {
        if (grabAction != null && grabAction.action != null)
        {
            grabAction.action.Enable();
            grabAction.action.performed += OnGrabPressed;
            grabAction.action.canceled += OnGrabReleased;
        }
    }

    private void OnDisable()
    {
        if (grabAction != null && grabAction.action != null)
        {
            grabAction.action.performed -= OnGrabPressed;
            grabAction.action.canceled -= OnGrabReleased;
            grabAction.action.Disable();
        }
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
        if (playerCamera == null || holdPoint == null)
        {
            Debug.LogError("HoldToGrabCube: falta asignar playerCamera u holdPoint.", this);
            return;
        }

        // Si la mascara quedo vacia el raycast nunca impactaria nada: caer a "todo"
        // y avisar una sola vez, para que el fallo no sea silencioso.
        int mask = interactableLayer.value;
        if (mask == 0)
        {
            mask = ~0;
            if (!maskWarningShown)
            {
                maskWarningShown = true;
                Debug.LogWarning(
                    "HoldToGrabCube: 'Interactable Layer' esta vacio, usando todos los layers.",
                    this
                );
            }
        }

        Ray ray = playerCamera.ViewportPointToRay(
            new Vector3(0.5f, 0.5f, 0f)
        );

        if (Physics.Raycast(
            ray,
            out RaycastHit hit,
            maxDistance,
            mask,
            QueryTriggerInteraction.Ignore))
        {
            // Busca el Rigidbody en el objeto impactado O en cualquiera de sus padres
            Rigidbody rb = hit.collider.GetComponentInParent<Rigidbody>();

            if (rb == null)
            {
                RegistrarFallo();
                return;
            }

            grabbedObject = rb;
            grabbedTransform = rb.transform;
            grabbedCollider = hit.collider;

            grabbedObject.useGravity = false;
            grabbedObject.isKinematic = true;

            // Un collider cinematico SI frena a un CharacterController: mientras la caja
            // este cargada hay que ignorar la colision o el jugador se queda trabado.
            if (grabbedCollider != null && characterController != null)
                Physics.IgnoreCollision(grabbedCollider, characterController, true);

            grabbedTransform.position = holdPoint.position;
            grabbedTransform.rotation = holdPoint.rotation;
        }
        else
        {
            RegistrarFallo();
        }
    }

    private void RegistrarFallo()
    {
        if (errorCounter != null)
            errorCounter.RegistrarErrorAgarre();
    }

    private void ReleaseObject()
    {
        if (grabbedObject == null) return;

        // Se restaura ANTES de ceder la caja: si se apila, la torre debe volver a
        // frenar al jugador; si no se restaura aqui quedaria atravesable para siempre.
        if (grabbedCollider != null && characterController != null)
            Physics.IgnoreCollision(grabbedCollider, characterController, false);

        // Si la zona la acepta se queda con ella (la encaja y la deja cinematica);
        // aqui no hay que devolverle la fisica.
        bool colocada = stackZone != null && stackZone.TryPlace(grabbedObject, grabbedCollider);

        if (!colocada)
        {
            grabbedObject.isKinematic = false;
            grabbedObject.useGravity = true;

            // Sin esto la caja puede heredar un impulso residual al soltarla
            grabbedObject.linearVelocity = Vector3.zero;
            grabbedObject.angularVelocity = Vector3.zero;
        }

        grabbedObject = null;
        grabbedTransform = null;
        grabbedCollider = null;
    }

    // LateUpdate para colocar la caja DESPUES de que MobileCameraLook haya rotado
    // al Player en este mismo frame; si no, la caja va un frame atrasada al girar.
    private void LateUpdate()
    {
        if (grabbedObject != null && holdPoint != null)
        {
            grabbedTransform.position = holdPoint.position;
            grabbedTransform.rotation = holdPoint.rotation;
        }
    }
}