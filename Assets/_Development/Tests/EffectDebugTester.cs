using LumiKit.Core;
using LumiKit.Demo;
using UnityEngine;

namespace LumiKit.Development
{
    /// <summary>
    /// CÓDIGO DESECHABLE. Banco de pruebas de LK-09 mientras no existe la UI (LK-11).
    /// </summary>
    /// <remarks>
    /// Vive en Assets/_Development/, fuera del pack: no se exporta, no está en CODEMAP
    /// ni en el BACKLOG. Se borra al cerrar la Fase 5.
    ///
    /// Montaje:
    ///   1. Objeto con Renderer y material "Universal Render Pipeline/Unlit".
    ///   2. EffectController con un EFF_Debug.asset que declare un parámetro
    ///      de tipo Color y PropertyName exactamente "_BaseColor".
    ///   3. Este componente en el mismo objeto. Play.
    ///   4. LK-10: asignar el ObjectSelector de la escena en _selector.
    /// </remarks>
    public class EffectDebugTester : MonoBehaviour
    {
        [SerializeField] private EffectController _controller;
        [Tooltip("Debe existir en el EffectDefinition y ser de tipo Color.")]
        [SerializeField] private string _colorProperty = "_BaseColor";
        [Tooltip("Sube esto si el texto se ve diminuto en pantallas grandes.")]
        [SerializeField] private float _uiScale = 1.5f;

        [Tooltip("LK-10. Sin outline todavía, esta es la única forma de ver qué está seleccionado.")]
        [SerializeField] private ObjectSelector _selector;

        private float _red = 1f;
        private float _green = 1f;
        private float _blue = 1f;
        private bool _synced;
        private string _selectedName = "ninguno";
        private int _selectionChanges;

        private void Awake()
        {
            if (_controller == null)
            {
                _controller = GetComponent<EffectController>();
            }
        }

        /// <summary>
        /// LK-10 se consume por evento, no sondeando Selected cada frame: es lo que hará
        /// ParameterPanelUI (LK-11). El estado inicial se lee una vez al suscribirse y no
        /// cuenta como cambio.
        /// </summary>
        private void OnEnable()
        {
            if (_selector == null)
            {
                return;
            }

            _selector.OnSelectionChanged += HandleSelectionChanged;
            _selectedName = NameOf(_selector.Selected);
        }

        private void OnDisable()
        {
            if (_selector != null)
            {
                _selector.OnSelectionChanged -= HandleSelectionChanged;
            }
        }

        private void HandleSelectionChanged(EffectController selected)
        {
            _selectedName = NameOf(selected);
            _selectionChanges++;
        }

        private static string NameOf(EffectController controller)
        {
            return controller == null ? "ninguno" : controller.gameObject.name;
        }

        /// <summary>
        /// Lee el valor inicial del controller una sola vez. Si la propiedad no existe
        /// o no es Color, el propio EffectController lo avisa por consola; se marca
        /// como sincronizado igualmente para no repetir el warning cada frame.
        /// </summary>
        private void SyncFromController()
        {
            if (_synced || _controller == null)
            {
                return;
            }

            _synced = true;

            Color current = _controller.GetColor(_colorProperty);
            _red = current.r;
            _green = current.g;
            _blue = current.b;
        }

        private void OnGUI()
        {
            GUIUtility.ScaleAroundPivot(Vector2.one * _uiScale, Vector2.zero);
            GUILayout.BeginArea(new Rect(10f, 10f, 300f, 290f), GUI.skin.box);

            GUILayout.Label(_selector != null
                ? $"LK-10 · seleccionado: {_selectedName} · cambios: {_selectionChanges}"
                : "LK-10 · sin ObjectSelector asignado");
            GUILayout.Space(6f);

            if (_controller == null)
            {
                GUILayout.Label("Sin EffectController asignado.");
                GUILayout.EndArea();
                return;
            }

            SyncFromController();

            GUILayout.Label("LK-09 · banco de pruebas");
            GUILayout.Label($"Propiedad: {_colorProperty}");
            GUILayout.Space(6f);

            float red = DrawChannel("R", _red);
            float green = DrawChannel("G", _green);
            float blue = DrawChannel("B", _blue);

            if (!Mathf.Approximately(red, _red)
                || !Mathf.Approximately(green, _green)
                || !Mathf.Approximately(blue, _blue))
            {
                _red = red;
                _green = green;
                _blue = blue;
                _controller.SetColor(_colorProperty, new Color(_red, _green, _blue, 1f));
            }

            GUILayout.Space(10f);

            if (GUILayout.Button(_controller.IsEffectEnabled
                ? "Efecto: ON — pulsa para apagar"
                : "Efecto: OFF — pulsa para encender"))
            {
                _controller.SetEffectEnabled(!_controller.IsEffectEnabled);
            }

            // Añadido al disparador para poder cerrar el criterio de ResetToDefaults,
            // que sin UI tampoco tendría forma de dispararse.
            if (GUILayout.Button("ResetToDefaults()"))
            {
                _controller.ResetToDefaults();
                _synced = false;
            }

            GUILayout.Space(6f);
            GUILayout.Label("Con un shader URP de stock el botón de efecto no cambia\n" +
                            "nada visible: _EffectEnabled no existe ahí. Es lo esperado,\n" +
                            "el criterio está diferido a LK-01.");

            GUILayout.EndArea();
        }

        private float DrawChannel(string label, float value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label($"{label} {value:0.00}", GUILayout.Width(60f));
            float result = GUILayout.HorizontalSlider(value, 0f, 1f);
            GUILayout.EndHorizontal();

            return result;
        }
    }
}
