using LumiKit.Core;
using UnityEngine;
using UnityEngine.UI;

namespace LumiKit.UI.Widgets
{
    /// <summary>
    /// Widget de un parámetro Boolean: interruptor de pista 36×20 con la manija a un lado o
    /// al otro. GDD §1.3 Mecánica 2 (línea 52) y §2.8 (línea 603).
    /// </summary>
    /// <remarks>
    /// No usa el checkmark del Toggle de uGUI: el aspecto de interruptor sale de pintar la
    /// pista y mover la manija. Por eso Toggle.graphic se deja vacío en el prefab y de los
    /// colores se encarga este script.
    ///
    /// El salto de la manija es instantáneo. Los 150 ms de ui-style piden animación, que es
    /// LK-22: aquí no se mete una corrutina por un widget.
    ///
    /// Un MaterialPropertyBlock no puede activar keywords de shader (D-005), así que el valor
    /// viaja como float 0/1 y su efecto visual depende del grafo. Sin shaders del pack todavía
    /// (LK-01), esto sólo se verifica por el dato.
    /// </remarks>
    [AddComponentMenu("LumiKit/Toggle Parameter Widget")]
    [DisallowMultipleComponent]
    public class ToggleParameterWidget : ParameterWidgetBase
    {
        [SerializeField] private Toggle _toggle;
        [SerializeField] private Image _track;
        [SerializeField] private RectTransform _handle;
        [SerializeField] private Image _handleImage;

        protected override ParameterType SupportedType => ParameterType.Boolean;

        protected override void OnInitialize()
        {
            if (_toggle == null)
            {
                Debug.LogWarning($"[LumiKit] {name}: widget de Boolean sin Toggle asignado.", this);
                return;
            }

            _toggle.onValueChanged.AddListener(HandleToggleChanged);
            OnRefresh();
        }

        protected override void OnRefresh()
        {
            bool value = Controller.GetBool(Parameter.PropertyName);

            if (_toggle != null)
            {
                _toggle.SetIsOnWithoutNotify(value);
            }

            Paint(value);
        }

        private void HandleToggleChanged(bool value)
        {
            if (!IsInitialized)
            {
                return;
            }

            Controller.SetBool(Parameter.PropertyName, value);
            Paint(value);
            RaiseValueChanged();
        }

        private void Paint(bool value)
        {
            if (_track != null)
            {
                _track.color = value ? LumiTheme.LumiCyan : LumiTheme.Border;
            }

            if (_handleImage != null)
            {
                _handleImage.color = value ? LumiTheme.TextPrimary : LumiTheme.TextMuted;
            }

            if (_handle != null)
            {
                // La manija está anclada al borde izquierdo de la pista, así que su posición
                // va del aire inicial al aire inicial más el recorrido.
                float inset = (LumiTheme.TOGGLE_TRACK_HEIGHT - LumiTheme.TOGGLE_HANDLE_SIZE) * 0.5f;
                float travel = LumiTheme.TOGGLE_TRACK_WIDTH - LumiTheme.TOGGLE_HANDLE_SIZE - inset * 2f;

                _handle.anchoredPosition = new Vector2(inset + (value ? travel : 0f), 0f);
            }
        }

        private void OnDestroy()
        {
            if (_toggle != null)
            {
                _toggle.onValueChanged.RemoveListener(HandleToggleChanged);
            }
        }
    }
}
