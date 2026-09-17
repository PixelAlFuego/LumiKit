using System.Globalization;
using LumiKit.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LumiKit.UI.Widgets
{
    /// <summary>
    /// Widget de un parámetro Float: slider horizontal con el valor numérico a la derecha.
    /// GDD §1.3 Mecánica 2 (línea 50) y §2.8 (línea 595).
    /// </summary>
    /// <remarks>
    /// El prefab lo genera ParameterPanelBuilder (D-002). Este componente no crea jerarquía:
    /// espera _slider y _valueLabel ya asignados.
    /// </remarks>
    [AddComponentMenu("LumiKit/Slider Parameter Widget")]
    [DisallowMultipleComponent]
    public class SliderParameterWidget : ParameterWidgetBase
    {
        /// <summary>
        /// Cultura invariante: el punto decimal no debe cambiar con el idioma del sistema
        /// mientras el ancho del campo sea fijo (48 px, ui-style.md).
        /// </summary>
        private const string VALUE_FORMAT = "0.00";

        [SerializeField] private Slider _slider;
        [Tooltip("Valor numérico. Ancho fijo para que el layout no salte al arrastrar.")]
        [SerializeField] private TMP_Text _valueLabel;

        protected override ParameterType SupportedType => ParameterType.Float;

        protected override void OnInitialize()
        {
            if (_slider == null)
            {
                Debug.LogWarning($"[LumiKit] {name}: widget de Float sin Slider asignado.", this);
                return;
            }

            // Un rango vacío llega de un EffectDefinition mal rellenado, no de un error de
            // código: se avisa una vez y el control queda visible pero bloqueado.
            if (Parameter.MaxValue <= Parameter.MinValue)
            {
                Debug.LogWarning(
                    $"[LumiKit] '{Parameter.PropertyName}': rango vacío ({Parameter.MinValue} … {Parameter.MaxValue}). Slider bloqueado.",
                    this);
                _slider.interactable = false;
            }
            else
            {
                _slider.minValue = Parameter.MinValue;
                _slider.maxValue = Parameter.MaxValue;
            }

            _slider.wholeNumbers = false;
            _slider.onValueChanged.AddListener(HandleSliderChanged);

            OnRefresh();
        }

        /// <summary>
        /// SetValueWithoutNotify evita el rebote: asignar Slider.value dispararía
        /// onValueChanged y reescribiría el controller con el valor que acaba de leerse.
        /// </summary>
        protected override void OnRefresh()
        {
            if (_slider == null)
            {
                return;
            }

            float value = Controller.GetFloat(Parameter.PropertyName);
            _slider.SetValueWithoutNotify(value);
            WriteValueLabel(value);
        }

        private void HandleSliderChanged(float value)
        {
            if (!IsInitialized)
            {
                return;
            }

            Controller.SetFloat(Parameter.PropertyName, value);
            WriteValueLabel(value);
            RaiseValueChanged();
        }

        private void WriteValueLabel(float value)
        {
            if (_valueLabel != null)
            {
                _valueLabel.text = value.ToString(VALUE_FORMAT, CultureInfo.InvariantCulture);
            }
        }

        private void OnDestroy()
        {
            if (_slider != null)
            {
                _slider.onValueChanged.RemoveListener(HandleSliderChanged);
            }
        }
    }
}
