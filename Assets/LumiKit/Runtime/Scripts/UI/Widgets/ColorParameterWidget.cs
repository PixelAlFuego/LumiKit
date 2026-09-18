using LumiKit.Core;
using UnityEngine;
using UnityEngine.UI;

namespace LumiKit.UI.Widgets
{
    /// <summary>
    /// Widget de un parámetro Color: muestra plegada que al pulsarla despliega tres sliders
    /// RGB y las seis muestras de la paleta. GDD §1.3 Mecánica 2 (línea 51).
    /// </summary>
    /// <remarks>
    /// Unity no trae selector de color en runtime: ColorPicker vive en UnityEditor y no se
    /// puede exportar. De ahí los tres canales en vez de una rueda; la rueda y el campo
    /// hexadecimal del GDD (línea 599) son LK-22.
    ///
    /// El alfa no se edita: se conserva el del EffectParameter, porque ningún shader del pack
    /// lo usa todavía. Intensidad HDR tampoco: EffectParameter no la tiene.
    ///
    /// El estado desplegado no sobrevive a un cambio de selección. El panel destruye los
    /// widgets y los vuelve a crear, así que el widget nace plegado; recordarlo obligaría a
    /// ParameterPanelUI a guardar estado por parámetro, y no guarda ninguno.
    /// </remarks>
    [AddComponentMenu("LumiKit/Color Parameter Widget")]
    [DisallowMultipleComponent]
    public class ColorParameterWidget : ParameterWidgetBase
    {
        [Tooltip("La propia muestra es el botón que despliega el widget.")]
        [SerializeField] private Button _swatchButton;
        [SerializeField] private Image _swatchImage;
        [SerializeField] private GameObject _expandRoot;
        [SerializeField] private Slider _redSlider;
        [SerializeField] private Slider _greenSlider;
        [SerializeField] private Slider _blueSlider;
        [Tooltip("Muestras de la paleta. Cada botón lleva su color en su propia Image.")]
        [SerializeField] private Button[] _paletteButtons = new Button[0];
        [Tooltip("El del propio widget: alterna entre el alto plegado y el desplegado.")]
        [SerializeField] private LayoutElement _layout;

        protected override ParameterType SupportedType => ParameterType.Color;

        protected override void OnInitialize()
        {
            if (_swatchButton == null || _redSlider == null || _greenSlider == null || _blueSlider == null)
            {
                Debug.LogWarning($"[LumiKit] {name}: widget de Color sin cablear.", this);
                return;
            }

            _swatchButton.onClick.AddListener(ToggleExpanded);
            _redSlider.onValueChanged.AddListener(HandleChannelChanged);
            _greenSlider.onValueChanged.AddListener(HandleChannelChanged);
            _blueSlider.onValueChanged.AddListener(HandleChannelChanged);

            for (int i = 0; i < _paletteButtons.Length; i++)
            {
                Button swatch = _paletteButtons[i];
                if (swatch == null)
                {
                    continue;
                }

                // El color de la muestra es el de su Image: el tint de Button va por
                // CanvasRenderer y no altera este valor. Copia local para el closure.
                Color preset = swatch.image != null ? swatch.image.color : Color.white;
                swatch.onClick.AddListener(() => ApplyColor(preset));
            }

            SetExpanded(false);
            OnRefresh();
        }

        protected override void OnRefresh()
        {
            Color current = Controller.GetColor(Parameter.PropertyName);
            WriteChannels(current);
            PaintSwatch(current);
        }

        private void HandleChannelChanged(float value)
        {
            if (!IsInitialized)
            {
                return;
            }

            Write(new Color(_redSlider.value, _greenSlider.value, _blueSlider.value, Alpha()));
        }

        /// <summary>Aplica una muestra de la paleta: mueve los tres canales y escribe.</summary>
        private void ApplyColor(Color preset)
        {
            if (!IsInitialized)
            {
                return;
            }

            Color color = new Color(preset.r, preset.g, preset.b, Alpha());
            WriteChannels(color);
            Write(color);
        }

        private void Write(Color color)
        {
            Controller.SetColor(Parameter.PropertyName, color);
            PaintSwatch(color);
            RaiseValueChanged();
        }

        /// <summary>Alfa del parámetro: el widget no lo edita.</summary>
        private float Alpha()
        {
            return Parameter.DefaultColor.a;
        }

        /// <summary>Sin notificar: mover un canal no debe reescribir el controller en cadena.</summary>
        private void WriteChannels(Color color)
        {
            if (_redSlider != null)
            {
                _redSlider.SetValueWithoutNotify(color.r);
            }

            if (_greenSlider != null)
            {
                _greenSlider.SetValueWithoutNotify(color.g);
            }

            if (_blueSlider != null)
            {
                _blueSlider.SetValueWithoutNotify(color.b);
            }
        }

        /// <summary>
        /// La muestra se pinta siempre opaca: con el alfa del parámetro a 0 no se vería nada
        /// y parecería que el widget está roto.
        /// </summary>
        private void PaintSwatch(Color color)
        {
            if (_swatchImage != null)
            {
                _swatchImage.color = new Color(color.r, color.g, color.b, 1f);
            }
        }

        private void ToggleExpanded()
        {
            SetExpanded(_expandRoot == null || !_expandRoot.activeSelf);
        }

        /// <summary>
        /// Cambiar preferredHeight marca el layout como sucio, así que el VerticalLayoutGroup
        /// del panel recoloca las filas de debajo sin que nadie se lo pida.
        /// </summary>
        private void SetExpanded(bool expanded)
        {
            if (_expandRoot != null)
            {
                _expandRoot.SetActive(expanded);
            }

            if (_layout != null)
            {
                float height = expanded
                    ? LumiTheme.COLOR_ROW_HEIGHT_EXPANDED
                    : LumiTheme.COLOR_ROW_HEIGHT;

                _layout.minHeight = height;
                _layout.preferredHeight = height;
            }
        }

        private void OnDestroy()
        {
            if (_swatchButton != null)
            {
                _swatchButton.onClick.RemoveListener(ToggleExpanded);
            }

            if (_redSlider != null)
            {
                _redSlider.onValueChanged.RemoveListener(HandleChannelChanged);
            }

            if (_greenSlider != null)
            {
                _greenSlider.onValueChanged.RemoveListener(HandleChannelChanged);
            }

            if (_blueSlider != null)
            {
                _blueSlider.onValueChanged.RemoveListener(HandleChannelChanged);
            }

            // Los de la paleta son lambdas: no hay referencia con la que quitarlos una a una.
            for (int i = 0; i < _paletteButtons.Length; i++)
            {
                if (_paletteButtons[i] != null)
                {
                    _paletteButtons[i].onClick.RemoveAllListeners();
                }
            }
        }
    }
}
