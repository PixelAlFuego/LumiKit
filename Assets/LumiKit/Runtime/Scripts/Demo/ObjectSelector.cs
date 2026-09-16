using System;
using LumiKit.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace LumiKit.Demo
{
    /// <summary>
    /// Selección de objetos de la demo 2D con el botón izquierdo. Sólo un objeto activo
    /// a la vez; el clic en vacío deselecciona. Mecánica 1 del GDD.
    /// </summary>
    /// <remarks>
    /// Input leído directamente de Mouse.current (D-006). El botón derecho y la rueda no se
    /// leen aquí: son de la cámara (LK-12). El raycast es por código con Physics2D; la escena
    /// no lleva Physics2DRaycaster.
    /// </remarks>
    [AddComponentMenu("LumiKit/Object Selector")]
    [DisallowMultipleComponent]
    public class ObjectSelector : MonoBehaviour
    {
        [Tooltip("Cámara desde la que sale el raycast. Si se deja vacío, se usa Camera.main.")]
        [SerializeField] private Camera _camera;

        [Tooltip("Capas que ve el raycast. Lo que quede fuera no se puede seleccionar.")]
        [SerializeField] private LayerMask _selectableLayers = ~0;

        /// <summary>
        /// Objeto activo, o null si no hay ninguno. Para el GameObject, Selected.gameObject;
        /// para el efecto que expone, Selected.Definition.
        /// </summary>
        public EffectController Selected { get; private set; }

        /// <summary>
        /// Vía única de notificación: se dispara sólo cuando la selección cambia, con el nuevo
        /// valor (null al deseleccionar). Reclicar el objeto ya activo no lo dispara.
        /// </summary>
        public event Action<EffectController> OnSelectionChanged;

        private void Awake()
        {
            if (_camera == null)
            {
                _camera = Camera.main;
            }
        }

        private void Update()
        {
            if (_camera == null)
            {
                Debug.LogWarning(
                    $"[LumiKit] ObjectSelector en '{name}' no tiene cámara y no hay Camera.main. Se desactiva.", this);
                enabled = false;
                return;
            }

            Mouse mouse = Mouse.current;
            if (mouse == null || !mouse.leftButton.wasPressedThisFrame)
            {
                return;
            }

            // El clic sobre la UI no toca la selección: ni selecciona ni deselecciona.
            if (IsPointerOverUI())
            {
                return;
            }

            SetSelection(Pick(mouse.position.ReadValue()));
        }

        /// <summary>Deselecciona por código. Dispara el evento si había algo seleccionado.</summary>
        public void ClearSelection()
        {
            SetSelection(null);
        }

        /// <summary>
        /// Lanza el raycast y devuelve el EffectController del impacto, o null.
        /// </summary>
        /// <remarks>
        /// GetRayIntersection y no OverlapPoint: tiene en cuenta la Z del collider, así que
        /// entre dos sprites a distinta profundidad gana el de delante. Physics2D no ve
        /// colliders 3D, de modo que un objeto con BoxCollider nunca entra aquí.
        /// Un collider de la capa sin EffectController devuelve null y por tanto deselecciona,
        /// igual que un clic en vacío. No se avisa por consola: el clic es demasiado frecuente.
        /// </remarks>
        private EffectController Pick(Vector2 screenPosition)
        {
            RaycastHit2D hit = Physics2D.GetRayIntersection(
                _camera.ScreenPointToRay(screenPosition), Mathf.Infinity, _selectableLayers);

            if (hit.collider == null)
            {
                return null;
            }

            // Sólo el GameObject del collider: ni padres ni hijos. Los prefabs demo llevan
            // collider y EffectController en la raíz (GDD §4.8).
            return hit.collider.GetComponent<EffectController>();
        }

        private void SetSelection(EffectController controller)
        {
            if (Selected == controller)
            {
                return;
            }

            Selected = controller;
            OnSelectionChanged?.Invoke(Selected);
        }

        /// <summary>
        /// True si el cursor está sobre UI de EventSystem. Se consulta en Update, no desde
        /// callbacks de input.
        /// </summary>
        /// <remarks>
        /// EventSystem.current puede ser null mientras no haya UI en la escena: sin la
        /// comprobación, el primer clic lanzaría NullReferenceException. Sin EventSystem no hay
        /// bloqueo. OnGUI (IMGUI) no pasa por EventSystem y no cuenta como UI.
        /// </remarks>
        private static bool IsPointerOverUI()
        {
            EventSystem eventSystem = EventSystem.current;
            return eventSystem != null && eventSystem.IsPointerOverGameObject();
        }
    }
}
