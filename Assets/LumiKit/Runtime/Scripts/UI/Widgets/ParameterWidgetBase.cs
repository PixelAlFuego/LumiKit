using System;
using LumiKit.Core;
using TMPro;
using UnityEngine;

namespace LumiKit.UI.Widgets
{
    /// <summary>
    /// Contrato común de todos los widgets del panel de parámetros: se inicializan desde un
    /// EffectParameter y escriben en el EffectController que reciben.
    /// </summary>
    /// <remarks>
    /// D-003: ningún widget conoce un efecto concreto. Todo lo que necesita para configurarse
    /// (etiqueta, rango, tipo, valor por defecto) viaja dentro del EffectParameter.
    /// D-001: se escribe por EffectController, nunca tocando el material.
    /// El nombre de las subclases es <Tipo>ParameterWidget (D-008).
    /// </remarks>
    public abstract class ParameterWidgetBase : MonoBehaviour
    {
        [Tooltip("Etiqueta del parámetro. Texto en español fijo hasta LK-19.")]
        [SerializeField] private TMP_Text _label;

        private bool _initialized;

        /// <summary>Parámetro que edita este widget. Null hasta que Initialize tiene éxito.</summary>
        protected EffectParameter Parameter { get; private set; }

        /// <summary>Destino de las escrituras. Null hasta que Initialize tiene éxito.</summary>
        protected EffectController Controller { get; private set; }

        /// <summary>False mientras el widget no se haya inicializado correctamente.</summary>
        protected bool IsInitialized => _initialized;

        /// <summary>Tipo de parámetro que esta subclase sabe editar.</summary>
        protected abstract ParameterType SupportedType { get; }

        /// <summary>
        /// Se dispara después de escribir un valor en el controller. Sin consumidores
        /// todavía: lo esperan el portapapeles (LK-29) y los toasts (LK-34).
        /// </summary>
        public event Action<ParameterWidgetBase> OnValueChanged;

        /// <summary>
        /// Deja el widget listo para editar un parámetro. Lo llama ParameterPanelUI justo
        /// después de instanciar el prefab.
        /// </summary>
        /// <remarks>
        /// Un tipo que no coincide no es excepción: el widget se queda inerte y avisa. Así un
        /// EffectDefinition mal montado no rompe el panel entero.
        /// </remarks>
        public void Initialize(EffectParameter parameter, EffectController controller)
        {
            if (parameter == null || controller == null)
            {
                Debug.LogWarning($"[LumiKit] {name}: Initialize con parámetro o controller nulo.", this);
                return;
            }

            if (parameter.Type != SupportedType)
            {
                Debug.LogWarning(
                    $"[LumiKit] {name}: '{parameter.PropertyName}' es {parameter.Type} y este widget edita {SupportedType}.",
                    this);
                return;
            }

            Parameter = parameter;
            Controller = controller;
            _initialized = true;

            if (_label != null)
            {
                _label.text = string.IsNullOrEmpty(parameter.DisplayNameEs)
                    ? parameter.PropertyName
                    : parameter.DisplayNameEs;
            }

            OnInitialize();
        }

        /// <summary>
        /// Relee del controller el valor vivo del parámetro y lo vuelca en el control, sin
        /// notificar ni reescribir nada.
        /// </summary>
        /// <remarks>
        /// Es la vía para enterarse de un cambio hecho fuera del panel: ResetToDefaults y
        /// SetEffectEnabled (LK-09) los dispararán el botón Reset (LK-11b) y el TAB (LK-24).
        /// El widget no se suscribe a EffectController.OnValueChanged: eso haría que su propia
        /// escritura le volviera de rebote en mitad de un arrastre.
        /// </remarks>
        public void Refresh()
        {
            if (!_initialized)
            {
                return;
            }

            OnRefresh();
        }

        /// <summary>Cablea el control desde Parameter. Debe terminar llamando a OnRefresh().</summary>
        protected abstract void OnInitialize();

        /// <summary>Vuelca el valor del controller en el control, sin disparar callbacks.</summary>
        protected abstract void OnRefresh();

        /// <summary>Lo llama la subclase después de escribir en el controller.</summary>
        protected void RaiseValueChanged()
        {
            OnValueChanged?.Invoke(this);
        }
    }
}
