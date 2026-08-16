using UnityEngine;
using UnityEngine.UI;

namespace Guia3
{
    /// <summary>
    /// Adapta el HUD y la camara cuando el usuario gira el telefono. La app permite
    /// vertical y horizontal, y cada orientacion necesita cosas distintas:
    /// la resolucion de referencia del CanvasScaler se voltea, los controles del pulgar
    /// se reubican y el FOV vertical de la camara baja en horizontal (si no, el mismo
    /// FOV que se ve bien en vertical produce un ojo de pez al ensanchar la pantalla).
    /// </summary>
    [DefaultExecutionOrder(-50)]
    public class OrientationLayout : MonoBehaviour
    {
        /// <summary>Un elemento de UI con una colocacion distinta por orientacion.</summary>
        [System.Serializable]
        public struct ResponsiveRect
        {
            public RectTransform target;

            public Vector2 portraitAnchor;
            public Vector2 portraitOffset;
            public Vector2 portraitSize;

            public Vector2 landscapeAnchor;
            public Vector2 landscapeOffset;
            public Vector2 landscapeSize;
        }

        [Header("Referencias")]
        [SerializeField] private CanvasScaler scaler;
        [Tooltip("Camara cuyo FOV se ajusta. Puede quedar vacio en el menu.")]
        [SerializeField] private Camera playerCamera;

        [Header("Resoluciones de referencia")]
        [SerializeField] private Vector2 portraitReference = new Vector2(1080f, 1920f);
        [SerializeField] private Vector2 landscapeReference = new Vector2(1920f, 1080f);

        [Header("Campo de vision (vertical, en grados)")]
        [Tooltip("En vertical el campo horizontal se estrecha, asi que conviene un FOV vertical alto.")]
        [SerializeField] private float portraitFov = 75f;
        [Tooltip("En horizontal el mismo FOV vertical se vuelve demasiado ancho; 52 deja ~82 grados horizontales.")]
        [SerializeField] private float landscapeFov = 52f;

        [Header("Elementos que se reubican")]
        [SerializeField] private ResponsiveRect[] elements;

        /// <summary>True mientras la pantalla esta en vertical.</summary>
        public bool IsPortrait { get; private set; }

        // null = todavia no se aplico ningun layout, asi que el primer Apply siempre entra.
        private bool? appliedPortrait;

        private void OnEnable()
        {
            appliedPortrait = null;
            Apply();
        }

        // La rotacion no dispara ningun evento en Unity, hay que consultarla. La comparacion
        // es de dos enteros por frame y el trabajo real solo ocurre cuando la orientacion cambia.
        private void Update() => Apply();

        private void Apply()
        {
            IsPortrait = Screen.height >= Screen.width;
            if (appliedPortrait == IsPortrait) return;
            appliedPortrait = IsPortrait;

            if (scaler != null)
            {
                scaler.referenceResolution = IsPortrait ? portraitReference : landscapeReference;
                // Match por el lado corto: evita que un telefono muy alargado encoja todo el HUD.
                scaler.matchWidthOrHeight = IsPortrait ? 0f : 1f;
            }

            if (playerCamera != null)
                playerCamera.fieldOfView = IsPortrait ? portraitFov : landscapeFov;

            if (elements == null) return;

            foreach (ResponsiveRect element in elements)
            {
                if (element.target == null) continue;

                Vector2 anchor = IsPortrait ? element.portraitAnchor : element.landscapeAnchor;
                element.target.anchorMin = anchor;
                element.target.anchorMax = anchor;
                element.target.anchoredPosition = IsPortrait ? element.portraitOffset : element.landscapeOffset;
                element.target.sizeDelta = IsPortrait ? element.portraitSize : element.landscapeSize;
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Cableado desde los builders del editor. Es publico porque el codigo de editor vive
        /// en otro assembly y no alcanza a los miembros internal.
        /// </summary>
        public void EditorConfigure(CanvasScaler canvasScaler, Camera camera, ResponsiveRect[] responsiveRects)
        {
            scaler = canvasScaler;
            playerCamera = camera;
            elements = responsiveRects;
        }
#endif
    }
}
