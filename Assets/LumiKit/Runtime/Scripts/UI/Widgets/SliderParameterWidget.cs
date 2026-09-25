using System.Globalization;
using LumiKit.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LumiKit.UI.Widgets
{
    /// <summary>
    /// Widget de un parámetro Float: slider horizontal con el valor numérico a la derecha.
    /// GDD §1.3 Mecánica 2 (línea 50) y §2.8 (línea 595).
    /// </summary>
    /// <remarks>
    /// El prefab lo genera ParameterPanelBuilder (D-002). Este componente no crea jerarquía:
    /// espera _slider y _valueLabel ya asignados. _valueInput y _editOutline hacen editable el
    /// valor (LK-52) y son opcionales: sin ellos el valor sólo se muestra, como antes.
    /// </remarks>
    [AddComponentMenu("LumiKit/Slider Parameter Widget")]
    [DisallowMultipleComponent]
    public class SliderParameterWidget : ParameterWidgetBase
    {
        /// <summary>
        /// Cultura invariante: el punto decimal no debe cambiar con el idioma del sistema
        /// mientras el ancho del campo sea fijo (LumiTheme.SLIDER_VALUE_WIDTH).
        /// </summary>
        private const string VALUE_FORMAT = "0.00";

        [SerializeField] private Slider _slider;
        [Tooltip("Valor numérico. Ancho fijo para que el layout no salte al arrastrar.")]
        [SerializeField] private TMP_Text _valueLabel;
        [Tooltip("Campo que hace editable el valor con un clic (LK-52). Opcional.")]
        [SerializeField] private TMP_InputField _valueInput;
        [Tooltip("Contorno del campo, visible sólo en edición. Lo enciende y apaga este widget.")]
        [SerializeField] private Image _editOutline;

        // Último texto que el widget escribió en el campo. Lo que llegue igual al terminar la
        // edición no se ha tocado y no se aplica.
        private string _writtenText;

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
                if (_valueInput != null)
                {
                    _valueInput.interactable = false;
                }
            }
            else
            {
                _slider.minValue = Parameter.MinValue;
                _slider.maxValue = Parameter.MaxValue;
            }

            _slider.wholeNumbers = false;
            _slider.onValueChanged.AddListener(HandleSliderChanged);
            HookValueInput();

            OnRefresh();
        }

        /// <summary>
        /// Engancha el campo del valor (LK-52). onValidateInput es un delegado y no se
        /// serializa: se asigna aquí y no en el generador.
        /// </summary>
        private void HookValueInput()
        {
            if (_valueInput == null)
            {
                return;
            }

            _valueInput.onValidateInput = ValidateValueChar;
            _valueInput.onSelect.AddListener(HandleValueSelect);
            _valueInput.onDeselect.AddListener(HandleValueDeselect);
            _valueInput.onEndEdit.AddListener(HandleValueEndEdit);
            SetEditOutline(false);
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
            string formatted = value.ToString(VALUE_FORMAT, CultureInfo.InvariantCulture);

            if (_valueInput != null)
            {
                // En edición no se pisa lo escrito: si el valor cambia desde fuera, lo pone al
                // día HandleValueEndEdit al terminar (LK-52).
                if (!_valueInput.isFocused)
                {
                    _writtenText = formatted;
                    _valueInput.SetTextWithoutNotify(formatted);
                }
            }
            else if (_valueLabel != null)
            {
                _valueLabel.text = formatted;
            }
        }

        /// <summary>
        /// Filtro de cada carácter tecleado (LK-52). No se usa CharacterValidation.Decimal: sólo
        /// acepta el separador decimal del idioma del sistema. TMP pasa el texto sin la parte
        /// seleccionada, así que con todo seleccionado se valida contra un texto vacío.
        /// </summary>
        /// <returns>El carácter, o '\0' para rechazarlo.</returns>
        private char ValidateValueChar(string text, int charIndex, char addedChar)
        {
            // Delante de un signo menos no entra nada.
            if (charIndex == 0 && text.Length > 0 && text[0] == '-')
            {
                return '\0';
            }

            if (addedChar >= '0' && addedChar <= '9')
            {
                return addedChar;
            }

            // Coma o punto, uno solo: se convierte a punto al aplicar.
            if (addedChar == '.' || addedChar == ',')
            {
                bool hasSeparator = text.IndexOf('.') >= 0 || text.IndexOf(',') >= 0;
                return hasSeparator ? '\0' : addedChar;
            }

            // El menos, sólo como primer carácter y sólo si el rango admite negativos.
            if (addedChar == '-' && charIndex == 0 && Parameter.MinValue < 0f)
            {
                return addedChar;
            }

            return '\0';
        }

        private void HandleValueSelect(string text)
        {
            SetEditOutline(true);
        }

        private void HandleValueDeselect(string text)
        {
            SetEditOutline(false);
        }

        /// <summary>
        /// Fin de la edición: Enter, Escape, clic fuera o el panel que se oculta. Todos pasan por
        /// DeactivateInputField, que lanza onEndEdit. Escape nunca aplica.
        /// </summary>
        /// <remarks>
        /// Si el panel se reconstruye con otro objeto a media edición, esto llega al widget viejo
        /// antes de que Destroy lo borre, al final del frame: su Controller es el objeto que se
        /// estaba editando, nunca el nuevo.
        /// </remarks>
        private void HandleValueEndEdit(string text)
        {
            SetEditOutline(false);

            // Controller destruido (el objeto se borró a media edición): nada a lo que aplicar.
            if (!IsInitialized || Controller == null)
            {
                return;
            }

            // Escape: TMP ya ha devuelto el texto original (restoreOriginalTextOnEscape). Texto sin
            // tocar: entrar y salir no redondea el valor a los dos decimales que se muestran.
            bool edited = !_valueInput.wasCanceled && text != _writtenText;
            if (edited && TryParseValue(text, out float typed))
            {
                // Por el slider: escribe el controller y lanza OnValueChanged por el camino de siempre.
                _slider.value = Parameter.ClampFloat(typed);
            }

            // En todos los casos, el valor vigente con el formato de siempre: también si lo
            // escrito no era un número o si el valor cambió desde fuera durante la edición.
            WriteValueLabel(Controller.GetFloat(Parameter.PropertyName));
            ReleaseValueSelection();
        }

        /// <summary>
        /// Coma o punto, siempre con InvariantCulture. Vacío, o "-" o "," sueltos: false.
        /// </summary>
        private static bool TryParseValue(string text, out float value)
        {
            if (string.IsNullOrEmpty(text))
            {
                value = 0f;
                return false;
            }

            return float.TryParse(
                text.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }

        /// <summary>
        /// Suelta la selección del EventSystem al terminar la edición: con el campo aún
        /// seleccionado, su selectedColor dejaría el fondo como en hover.
        /// </summary>
        /// <remarks>
        /// No se toca con un cambio de selección ya en marcha (clic en otro control: el
        /// EventSystem lo rechazaría con un error) ni con el campo apagándose con el panel.
        /// Mismo coste de foco con teclado que LumiButton: duda abierta en STATE.
        /// </remarks>
        private void ReleaseValueSelection()
        {
            EventSystem eventSystem = EventSystem.current;
            if (eventSystem == null || eventSystem.alreadySelecting || !_valueInput.isActiveAndEnabled)
            {
                return;
            }

            if (eventSystem.currentSelectedGameObject == _valueInput.gameObject)
            {
                eventSystem.SetSelectedGameObject(null);
            }
        }

        private void SetEditOutline(bool visible)
        {
            if (_editOutline != null)
            {
                _editOutline.enabled = visible;
            }
        }

        private void OnDestroy()
        {
            if (_slider != null)
            {
                _slider.onValueChanged.RemoveListener(HandleSliderChanged);
            }

            if (_valueInput != null)
            {
                _valueInput.onValidateInput = null;
                _valueInput.onSelect.RemoveListener(HandleValueSelect);
                _valueInput.onDeselect.RemoveListener(HandleValueDeselect);
                _valueInput.onEndEdit.RemoveListener(HandleValueEndEdit);
            }
        }
    }
}
