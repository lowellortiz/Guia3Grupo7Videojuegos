using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Apilado automatico en PuntoB. Apilar a mano es demasiado dificil, asi que la caja
/// no se coloca donde el jugador la suelta: si la suelta dentro del radio, la zona se
/// queda con ella, la encaja sola en el piso que le toca y la bloquea para que nada
/// pueda moverla despues.
/// </summary>
public class AutoStackZone : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Base de la pila. La tapa de PlataformaB, no el suelo.")]
    [SerializeField] private Transform stackOrigin;
    [Tooltip("Cubo translucido que muestra donde caera la siguiente caja.")]
    [SerializeField] private GameObject nextSlotGhost;
    [SerializeField] private HoldToGrabCube carry;
    [SerializeField] private CubeCounter cubeCounter;
    [SerializeField] private ErrorCounter errorCounter;

    [Header("Configuracion")]
    [Tooltip("Distancia horizontal desde el centro de la pila para aceptar la caja.")]
    [SerializeField] private float placementRadius = 1.5f;
    [SerializeField] private int requiredCubes = 4;
    [SerializeField] private float snapDuration = 0.25f;
    [Tooltip("Alto de caja a usar mientras no haya ninguna apilada todavia.")]
    [SerializeField] private float cubeSizeFallback = 0.5f;

    private readonly List<Rigidbody> stacked = new List<Rigidbody>();

    // Se mide de la primera caja real en vez de asumir 0.5: el mesh del FBX puede
    // no coincidir con su BoxCollider, y entonces la torre saldria con huecos.
    private float measuredCubeSize = -1f;

    private float CubeSize => measuredCubeSize > 0f ? measuredCubeSize : cubeSizeFallback;

    public bool IsFull => stacked.Count >= requiredCubes;

    public Vector3 NextSlotCenter =>
        stackOrigin.position + Vector3.up * (CubeSize * (stacked.Count + 0.5f));

    private void Start()
    {
        if (stackOrigin == null)
            Debug.LogError("AutoStackZone: falta asignar stackOrigin.", this);

        if (nextSlotGhost != null)
            nextSlotGhost.SetActive(false);
    }

    private void Update()
    {
        if (nextSlotGhost == null || stackOrigin == null) return;

        // El fantasma solo tiene sentido mientras el jugador lleva una caja en las manos.
        bool mostrar = carry != null && carry.IsCarrying && !IsFull;

        if (nextSlotGhost.activeSelf != mostrar)
            nextSlotGhost.SetActive(mostrar);

        if (mostrar)
        {
            nextSlotGhost.transform.position = NextSlotCenter;
            // El mesh del fantasma es un cubo primitivo de 1 m, asi que la escala ES el tamano.
            nextSlotGhost.transform.localScale = Vector3.one * CubeSize;
        }
    }

    /// <summary>
    /// Ofrece una caja a la pila. Devuelve true si la zona se queda con ella (encaje y
    /// bloqueo); false si el jugador la solto lejos y debe caer con fisica normal.
    /// </summary>
    public bool TryPlace(Rigidbody body, Collider col)
    {
        if (body == null || stackOrigin == null) return false;

        if (IsFull)
        {
            RegistrarRechazo();
            return false;
        }

        // El proyecto tiene autoSyncTransforms apagado: sin esto, bounds seguiria en la
        // posicion que tenia la caja antes de que LateUpdate la pegara al HoldAnchor.
        Physics.SyncTransforms();

        // El radio se mide sobre la CAJA, no sobre el jugador: el HoldAnchor va por
        // delante de la camara, asi que el gesto se siente como "ponerla ahi".
        Vector3 cubeCenter = col != null ? col.bounds.center : body.position;
        Vector3 aPlano = cubeCenter - stackOrigin.position;
        aPlano.y = 0f;

        if (aPlano.magnitude > placementRadius)
        {
            RegistrarRechazo();
            return false;
        }

        // Si ya esta en la pila, la zona se queda con ella igual: devolverla a la fisica
        // derrumbaria la torre.
        if (stacked.Contains(body))
        {
            Debug.LogWarning("AutoStackZone: " + body.name + " ya estaba apilada.", this);
            return true;
        }

        stacked.Add(body);

        // Se blinda YA, no al final del encaje: durante esos 0.25 s el jugador puede
        // apuntarle y volver a agarrarla, y entonces el encaje y el agarre se pelean
        // por el transform y la caja entraria dos veces en la pila.
        Blindar(body);

        StartCoroutine(EncajarYBloquear(body, col));
        return true;
    }

    /// <summary>
    /// Saca la caja del alcance del jugador. HoldToGrabCube solo hace raycast contra el
    /// layer 8 (Interactable), asi que moverla a Default es la forma mas barata de que
    /// no se pueda volver a agarrar.
    /// </summary>
    private void Blindar(Rigidbody body)
    {
        body.gameObject.layer = 0; // Default

        // Apoyarse en la pila no debe contar como caida.
        CubeFallDetector fallDetector = body.GetComponent<CubeFallDetector>();
        if (fallDetector != null)
            fallDetector.enabled = false;
    }

    private IEnumerator EncajarYBloquear(Rigidbody body, Collider col)
    {
        Vector3 desdePos = body.transform.position;
        Quaternion desdeRot = body.transform.rotation;
        Quaternion haciaRot = stackOrigin.rotation;

        // Alto real y desfase del pivote se miden con la rotacion FINAL ya puesta: mientras
        // el jugador la carga, la caja hereda la inclinacion de la camara y su AABB no
        // corresponde al lado del cubo. Se restaura para que la interpolacion arranque bien.
        Vector3 pivotOffset = Vector3.zero;
        if (col != null)
        {
            body.transform.rotation = haciaRot;
            Physics.SyncTransforms();

            if (measuredCubeSize <= 0f)
                measuredCubeSize = col.bounds.size.y;

            pivotOffset = body.transform.position - col.bounds.center;

            body.transform.rotation = desdeRot;
            Physics.SyncTransforms();
        }

        // Se calcula despues de medir, y con stacked.Count - 1 porque esta caja ya entro
        // en la lista y NextSlotCenter apuntaria un piso mas arriba.
        int slot = stacked.Count - 1;
        Vector3 slotCenter = stackOrigin.position + Vector3.up * (CubeSize * (slot + 0.5f));
        Vector3 destino = slotCenter + pivotOffset;

        float t = 0f;
        while (t < 1f && body != null)
        {
            t += Time.deltaTime / Mathf.Max(0.01f, snapDuration);
            float suave = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t));

            body.transform.position = Vector3.Lerp(desdePos, destino, suave);
            body.transform.rotation = Quaternion.Slerp(desdeRot, haciaRot, suave);
            yield return null;
        }

        if (body == null) yield break;

        body.transform.position = destino;
        body.transform.rotation = haciaRot;

        Bloquear(body);

        if (cubeCounter != null)
            cubeCounter.RegistrarCubo(body.gameObject);

        Debug.Log($"Caja apilada en el piso {slot + 1}: {body.gameObject.name}");
    }

    private void Bloquear(Rigidbody body)
    {
        // Cinematico = nada lo empuja. Ni el jugador, ni otra caja, ni una que caiga encima.
        body.isKinematic = true;
        body.useGravity = false;
        body.linearVelocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
    }

    private void RegistrarRechazo()
    {
        if (errorCounter != null)
            errorCounter.RegistrarErrorSoltado();
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (stackOrigin == null) return;

        Gizmos.color = new Color(0.4f, 1f, 0.6f, 0.6f);
        Vector3 centro = stackOrigin.position;

        // Radio de colocacion
        for (int i = 0; i < 48; i++)
        {
            float a = i / 48f * Mathf.PI * 2f;
            float b = (i + 1) / 48f * Mathf.PI * 2f;
            Gizmos.DrawLine(
                centro + new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * placementRadius,
                centro + new Vector3(Mathf.Cos(b), 0f, Mathf.Sin(b)) * placementRadius);
        }

        // Pisos de la pila
        float size = CubeSize;
        for (int i = 0; i < requiredCubes; i++)
            Gizmos.DrawWireCube(centro + Vector3.up * (size * (i + 0.5f)), Vector3.one * size);
    }
#endif
}
