using LumiKit.Core;
using LumiKit.Utils;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LumiKit.Demo
{
    /// <summary>
    /// Comparación antes / después: mientras TAB está pulsado, el objeto seleccionado se ve
    /// sin efecto; al soltar, el efecto vuelve. Mecánica 3 del GDD.
    /// </summary>
    /// <remarks>
    /// Apaga el efecto con EffectController.SetEffectEnabled, que escribe _EffectEnabled por
    /// MaterialPropertyBlock (D-001, D-005). Input leído de Keyboard.current (D-006).
    /// No conoce la UI: SetEffectEnabled no cambia el valor de ningún widget, así que el panel
    /// no necesita refrescarse.
    /// </remarks>
    [AddComponentMenu("LumiKit/Comparison Toggle")]
    [DisallowMultipleComponent]
    public class ComparisonToggle : MonoBehaviour
    {
        [Tooltip("Selector del que sale el objeto activo. Si se deja vacío, se busca en este mismo GameObject.")]
        [SerializeField] private ObjectSelector _selector;

        /// <summary>
        /// El único activo de la escena. Con dos, el segundo guardaría el efecto ya apagado por
        /// el primero y, según el orden en que restauren, el efecto no volvería nunca.
        /// </summary>
        private static ComparisonToggle _active;

        private EffectController _comparing;
        private bool _savedEnabled;

        private void Awake()
        {
            if (_selector == null)
            {
                _selector = GetComponent<ObjectSelector>();
            }
        }

        private void OnEnable()
        {
            if (_selector == null)
            {
                Debug.LogWarning(
                    $"[LumiKit] ComparisonToggle en '{name}' no tiene ObjectSelector. Se desactiva.", this);
                enabled = false;
                return;
            }

            if (_active != null && _active != this)
            {
                Debug.LogWarning(
                    $"[LumiKit] Ya hay un ComparisonToggle activo en '{_active.name}'. El de '{name}' se desactiva.", this);
                enabled = false;
                return;
            }

            _active = this;
        }

        /// <summary>
        /// Componente apagado, escena descargada o fin de Play: el objeto comparado recupera su
        /// estado. Si ya se destruyó, no hay nada que restaurar.
        /// </summary>
        private void OnDisable()
        {
            Restore();

            if (_active == this)
            {
                _active = null;
            }
        }

        /// <summary>
        /// En LateUpdate y no en Update: así corre después del ObjectSelector y el objeto recién
        /// elegido con TAB pulsado no se ve con efecto ni un frame.
        /// </summary>
        private void LateUpdate()
        {
            EffectController desired = GetDesiredTarget();

            // Comparación de Unity: un objeto destruido cuenta como null.
            if (desired == _comparing)
            {
                return;
            }

            Restore();

            if (desired != null)
            {
                _savedEnabled = desired.IsEffectEnabled;
                desired.SetEffectEnabled(false);
                _comparing = desired;
            }
        }

        /// <summary>
        /// Objeto que debe verse sin efecto en este frame: el seleccionado mientras TAB está
        /// pulsado, salvo que se esté escribiendo en un campo de texto (LK-52). Si no, null.
        /// </summary>
        private EffectController GetDesiredTarget()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || !keyboard.tabKey.isPressed || TextInputFocus.IsTyping)
            {
                return null;
            }

            return _selector.Selected;
        }

        /// <summary>
        /// Devuelve el objeto comparado al estado que tenía antes de TAB, no a true: si el efecto
        /// ya estaba apagado, soltar no lo enciende.
        /// </summary>
        private void Restore()
        {
            if (_comparing != null)
            {
                _comparing.SetEffectEnabled(_savedEnabled);
            }

            _comparing = null;
        }
    }
}
