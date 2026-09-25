using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LumiKit.UI
{
    /// <summary>
    /// Nivel del botón en la jerarquía del GDD §2.7 (líneas 549-577).
    /// </summary>
    public enum LumiButtonStyle
    {
        Primary,
        Secondary,
        Tertiary,
        Destructive
    }

    /// <summary>
    /// Botón del pack: fondo, borde y texto viran a la vez por estado, con el fundido de 150 ms
    /// del GDD, y la escala baja a 0.98 al presionar (GDD §2.7, líneas 547-588).
    /// </summary>
    /// <remarks>
    /// Hereda de Button para no reimplementar navegación, interactable ni onClick. El ColorBlock
    /// de uGUI sólo tiñe un gráfico y aquí hay tres: transition va a None y el pintado lo lleva
    /// DoStateTransition, con CrossFadeColor, la misma API que usa Selectable por dentro.
    ///
    /// Los tres gráficos van en blanco en el prefab: CrossFadeColor escribe en el CanvasRenderer,
    /// que multiplica con el color del Graphic.
    ///
    /// No son del GDD (spec de LK-50): Tertiary y Destructive deshabilitados, y Selected pintado
    /// como reposo (o como hover con el cursor encima). El fundido es lineal y no ease-out:
    /// CrossFadeColor no tiene curva.
    /// </remarks>
    [AddComponentMenu("LumiKit/Lumi Button")]
    public class LumiButton : Button
    {
        // Columnas de _stateColors.
        private const int NORMAL = 0;
        private const int HOVER = 1;
        private const int PRESSED = 2;
        private const int DISABLED = 3;

        [SerializeField] private LumiButtonStyle _style = LumiButtonStyle.Secondary;
        [Tooltip("Relleno. Blanco en el prefab: el color lo pone el componente.")]
        [SerializeField] private Image _fill;
        [Tooltip("Borde. Opcional: los niveles sin borde lo dejan transparente.")]
        [SerializeField] private Image _border;
        [Tooltip("Etiqueta. Blanca en el prefab, como el relleno.")]
        [SerializeField] private TMP_Text _label;

        // Selectable da Pressed mientras el botón siga pulsado, aunque el cursor haya salido
        // (currentSelectionState, Selectable.cs línea 609). Para que al arrastrar fuera vuelva a
        // reposo, el componente lleva la cuenta del puntero por su lado.
        private bool _pointerInside;
        private bool _pointerDown;

        /// <summary>
        /// Filas: LumiButtonStyle. Columnas: reposo, hover, presionado, deshabilitado.
        /// "Sin borde" es Transparent: la jerarquía de objetos es la misma en los cuatro niveles.
        /// </summary>
        private static readonly StateColors[,] _stateColors =
        {
            {
                // Primary (GDD líneas 553-558).
                new StateColors(LumiTheme.LumiCyan, LumiTheme.TextOnAccent, LumiTheme.Transparent),
                new StateColors(LumiTheme.LumiCyanHover, LumiTheme.TextOnAccent, LumiTheme.Transparent),
                new StateColors(LumiTheme.LumiCyanPressed, LumiTheme.TextOnAccent, LumiTheme.Transparent),
                new StateColors(LumiTheme.SurfaceElevated, LumiTheme.TextMuted, LumiTheme.Transparent)
            },
            {
                // Secondary (GDD líneas 562-567).
                new StateColors(LumiTheme.Transparent, LumiTheme.TextPrimary, LumiTheme.BorderStrong),
                new StateColors(LumiTheme.SurfaceElevated, LumiTheme.TextPrimary, LumiTheme.LumiCyan),
                new StateColors(LumiTheme.Surface, LumiTheme.LumiCyan, LumiTheme.LumiCyan),
                new StateColors(LumiTheme.Transparent, LumiTheme.TextMuted, LumiTheme.Border)
            },
            {
                // Tertiary (GDD líneas 571-575). Deshabilitado: interpretación, el GDD no lo define.
                new StateColors(LumiTheme.Transparent, LumiTheme.TextSecondary, LumiTheme.Transparent),
                new StateColors(LumiTheme.SurfaceElevated, LumiTheme.TextPrimary, LumiTheme.Transparent),
                new StateColors(LumiTheme.Surface, LumiTheme.LumiCyan, LumiTheme.Transparent),
                new StateColors(LumiTheme.Transparent, LumiTheme.TextMuted, LumiTheme.Transparent)
            },
            {
                // Destructive (GDD línea 577): el secundario con borde y texto en rojo.
                // Deshabilitado: interpretación, el del secundario, apagado y sin rojo.
                new StateColors(LumiTheme.Transparent, LumiTheme.SignalRed, LumiTheme.SignalRed),
                new StateColors(LumiTheme.SurfaceElevated, LumiTheme.SignalRed, LumiTheme.SignalRed),
                new StateColors(LumiTheme.Surface, LumiTheme.SignalRed, LumiTheme.SignalRed),
                new StateColors(LumiTheme.Transparent, LumiTheme.TextMuted, LumiTheme.Border)
            }
        };

        protected override void Awake()
        {
            base.Awake();

            // Con ColorTint, el ColorBlock teñiría el relleno encima de lo que pinta este componente.
            transition = Transition.None;
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            _pointerInside = true;
            base.OnPointerEnter(eventData);
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            _pointerInside = false;
            base.OnPointerExit(eventData);
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            // Mismo filtro que Selectable: sólo el botón izquierdo pulsa.
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                _pointerDown = true;
            }

            base.OnPointerDown(eventData);
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                _pointerDown = false;
            }

            base.OnPointerUp(eventData);
        }

        /// <summary>
        /// Al desactivarse, Selectable olvida el puntero. Con transition None no toca los
        /// gráficos, así que la escala se devuelve aquí: si no, se quedaría encogido.
        /// </summary>
        protected override void InstantClearState()
        {
            base.InstantClearState();
            _pointerInside = false;
            _pointerDown = false;
            transform.localScale = Vector3.one;
        }

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }

            int column = ColumnFor(state);
            StateColors colors = _stateColors[(int)_style, column];
            float duration = instant ? 0f : LumiTheme.TRANSITION_SECONDS;

            Paint(_fill, colors.Fill, duration);
            Paint(_border, colors.Border, duration);
            Paint(_label, colors.Label, duration);

            // Sin interpolar: suavizarla pediría un Update por botón. El pivot va al centro
            // (lo fija el generador), así que encoge hacia el centro.
            transform.localScale = column == PRESSED ? Vector3.one * LumiTheme.PRESS_SCALE : Vector3.one;
        }

        private int ColumnFor(SelectionState state)
        {
            switch (state)
            {
                case SelectionState.Highlighted:
                    return HOVER;
                case SelectionState.Pressed:
                    // Pulsado con el ratón y arrastrado fuera: reposo. Pulsado con Submit
                    // (teclado o mando) no hay puntero, y se ve presionado.
                    return _pointerDown && !_pointerInside ? NORMAL : PRESSED;
                case SelectionState.Selected:
                    return _pointerInside ? HOVER : NORMAL;
                case SelectionState.Disabled:
                    return DISABLED;
                default:
                    return NORMAL;
            }
        }

        private static void Paint(Graphic graphic, Color color, float duration)
        {
            if (graphic != null)
            {
                graphic.CrossFadeColor(color, duration, true, true);
            }
        }

        private readonly struct StateColors
        {
            public readonly Color Fill;
            public readonly Color Label;
            public readonly Color Border;

            public StateColors(Color fill, Color label, Color border)
            {
                Fill = fill;
                Label = label;
                Border = border;
            }
        }
    }
}
