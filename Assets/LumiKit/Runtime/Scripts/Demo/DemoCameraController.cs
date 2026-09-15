using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace LumiKit.Demo
{
    /// <summary>
    /// Cámara ortográfica de la demo 2D. Paneo con WASD y con botón derecho, zoom con la
    /// rueda, siempre dentro de un límite. Mecánica 4 del GDD.
    /// </summary>
    /// <remarks>
    /// Input leído directamente de Keyboard.current y Mouse.current (D-006).
    /// El botón izquierdo no se lee aquí: está reservado para la selección (LK-10).
    /// </remarks>
    [AddComponentMenu("LumiKit/Demo Camera Controller")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public class DemoCameraController : MonoBehaviour
    {
        private const float MIN_ORTHOGRAPHIC_SIZE = 0.1f;

        [Tooltip("Unidades de mundo por segundo al mover con WASD.")]
        [SerializeField] private float _panSpeed = 5f;

        [Tooltip("Zona permitida para el centro de la cámara, en unidades de mundo.")]
        [SerializeField] private Rect _bounds = new Rect(-10f, -6f, 20f, 12f);

        [Tooltip("Cambio de Size por cada frame en que gira la rueda.")]
        [SerializeField] private float _zoomStep = 0.5f;

        [Tooltip("Size mínimo: el zoom más cercano.")]
        [SerializeField] private float _minOrthographicSize = 2f;

        [Tooltip("Size máximo: el zoom más lejano.")]
        [SerializeField] private float _maxOrthographicSize = 10f;

        private Camera _camera;
        private bool _isPanning;
        private Vector2 _lastPointerPosition;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
        }

        private void OnDisable()
        {
            _isPanning = false;
        }

        private void OnValidate()
        {
            _minOrthographicSize = Mathf.Max(MIN_ORTHOGRAPHIC_SIZE, _minOrthographicSize);
            _maxOrthographicSize = Mathf.Max(_minOrthographicSize, _maxOrthographicSize);
        }

        private void Update()
        {
            if (!_camera.orthographic)
            {
                Debug.LogWarning(
                    $"[LumiKit] DemoCameraController en '{name}' necesita Projection = Orthographic. Se desactiva.", this);
                enabled = false;
                return;
            }

            Keyboard keyboard = Keyboard.current;
            Mouse mouse = Mouse.current;
            bool pointerOverUI = IsPointerOverUI();

            // El zoom va primero: el paneo con ratón convierte píxeles a mundo con el Size final.
            UpdateZoom(mouse, pointerOverUI);
            UpdateKeyboardPan(keyboard, pointerOverUI);
            UpdateMousePan(mouse, pointerOverUI);
            ClampToBounds();
        }

        /// <summary>
        /// Zoom hacia el centro de la pantalla. Sólo cuenta el signo de la rueda: la magnitud
        /// de scroll cambia según plataforma. Size se acota cada frame, también si se edita
        /// en el Inspector durante Play.
        /// </summary>
        private void UpdateZoom(Mouse mouse, bool pointerOverUI)
        {
            float size = _camera.orthographicSize;

            if (mouse != null && !pointerOverUI)
            {
                float scroll = mouse.scroll.ReadValue().y;
                if (!Mathf.Approximately(scroll, 0f))
                {
                    // Rueda hacia delante (positivo) acerca: Size baja.
                    size -= Mathf.Sign(scroll) * _zoomStep;
                }
            }

            _camera.orthographicSize = Mathf.Clamp(size, _minOrthographicSize, _maxOrthographicSize);
        }

        private void UpdateKeyboardPan(Keyboard keyboard, bool pointerOverUI)
        {
            // GDD §1.4 regla 5: WASD no mueve la cámara con el cursor sobre la UI.
            if (keyboard == null || pointerOverUI)
            {
                return;
            }

            Vector2 direction = Vector2.zero;
            if (keyboard.wKey.isPressed)
            {
                direction.y += 1f;
            }
            if (keyboard.sKey.isPressed)
            {
                direction.y -= 1f;
            }
            if (keyboard.dKey.isPressed)
            {
                direction.x += 1f;
            }
            if (keyboard.aKey.isPressed)
            {
                direction.x -= 1f;
            }

            // Normalizado para que la diagonal no sea más rápida.
            MoveBy(direction.normalized * (_panSpeed * Time.deltaTime));
        }

        /// <summary>
        /// Paneo 1:1 con botón derecho: la escena se desplaza lo mismo que el cursor, en
        /// unidades de mundo. Sólo empieza fuera de la UI; una vez empezado, sigue hasta soltar.
        /// </summary>
        private void UpdateMousePan(Mouse mouse, bool pointerOverUI)
        {
            if (mouse == null || !mouse.rightButton.isPressed)
            {
                _isPanning = false;
                return;
            }

            Vector2 pointerPosition = mouse.position.ReadValue();

            if (mouse.rightButton.wasPressedThisFrame)
            {
                _isPanning = !pointerOverUI;
            }
            else if (_isPanning)
            {
                // Cursor hacia arriba → la cámara baja → la escena sube con el cursor.
                Vector3 delta = ScreenToWorld(_lastPointerPosition) - ScreenToWorld(pointerPosition);
                MoveBy(delta);
            }

            _lastPointerPosition = pointerPosition;
        }

        /// <summary>
        /// True si el cursor está sobre UI de EventSystem. Se consulta en Update, no desde
        /// callbacks de input. Sin EventSystem en la escena no hay bloqueo.
        /// </summary>
        /// <remarks>
        /// OnGUI (IMGUI) no pasa por EventSystem y no cuenta como UI. La cámara no debe
        /// llevar Physics2DRaycaster: con él, los sprites también contarían como UI.
        /// </remarks>
        private static bool IsPointerOverUI()
        {
            EventSystem eventSystem = EventSystem.current;
            return eventSystem != null && eventSystem.IsPointerOverGameObject();
        }

        private Vector3 ScreenToWorld(Vector2 screenPosition)
        {
            return _camera.ScreenToWorldPoint(
                new Vector3(screenPosition.x, screenPosition.y, _camera.nearClipPlane));
        }

        /// <summary>Mueve sólo X e Y. Z y rotación nunca cambian.</summary>
        private void MoveBy(Vector3 delta)
        {
            Vector3 position = transform.position;
            position.x += delta.x;
            position.y += delta.y;
            transform.position = position;
        }

        private void ClampToBounds()
        {
            Vector3 position = transform.position;
            position.x = Mathf.Clamp(position.x, _bounds.xMin, _bounds.xMax);
            position.y = Mathf.Clamp(position.y, _bounds.yMin, _bounds.yMax);
            transform.position = position;
        }
    }
}
