using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace Guia3
{
    /// <summary>
    /// Zona tactil de mirada (patron "controles por zonas", seccion 5.4 de la guia).
    /// Solo toma el dedo cuyo contacto INICIAL cae en la mitad derecha de la pantalla y
    /// fuera de los controles de UI, de modo que el stick virtual y la camara nunca compiten
    /// por el mismo contacto. El dedo se rastrea por su identidad (finger index), no por el
    /// orden en la lista de contactos activos.
    /// </summary>
    public class TouchLookZone : MonoBehaviour
    {
        [Tooltip("Fraccion horizontal de pantalla a partir de la cual empieza la zona de mirada.")]
        [Range(0f, 1f)]
        [SerializeField] private float zoneStartX = 0.5f;

        [Tooltip("Rects de UI que nunca deben iniciar un arrastre de camara (boton Tomar, panel final).")]
        [SerializeField] private RectTransform[] blockingRects;

        /// <summary>Desplazamiento del dedo de mirada en este frame, en pixeles.</summary>
        public Vector2 LookDelta { get; private set; }

        /// <summary>True mientras un dedo esta arrastrando dentro de la zona de mirada.</summary>
        public bool IsLooking => lookFinger >= 0;

        private int lookFinger = -1;
        private Vector2 previousPosition;

        private void OnEnable()
        {
            EnhancedTouchSupport.Enable();
            lookFinger = -1;
        }

        private void OnDisable()
        {
            EnhancedTouchSupport.Disable();
            lookFinger = -1;
            LookDelta = Vector2.zero;
        }

        private void Update()
        {
            LookDelta = Vector2.zero;

            foreach (Touch touch in Touch.activeTouches)
            {
                int finger = touch.finger.index;

                if (touch.phase == TouchPhase.Began && lookFinger < 0 && IsInLookZone(touch.screenPosition))
                {
                    lookFinger = finger;
                    previousPosition = touch.screenPosition;
                    continue;
                }

                if (finger != lookFinger) continue;

                if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    lookFinger = -1;
                    continue;
                }

                LookDelta = touch.screenPosition - previousPosition;
                previousPosition = touch.screenPosition;
            }
        }

        private bool IsInLookZone(Vector2 screenPosition)
        {
            if (screenPosition.x < Screen.width * zoneStartX) return false;

            if (blockingRects != null)
            {
                foreach (RectTransform rect in blockingRects)
                {
                    if (rect == null || !rect.gameObject.activeInHierarchy) continue;
                    if (RectTransformUtility.RectangleContainsScreenPoint(rect, screenPosition, null))
                        return false;
                }
            }

            return true;
        }
    }
}
