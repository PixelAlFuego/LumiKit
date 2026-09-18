using System.Collections.Generic;
using LumiKit.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LumiKit.UI.Widgets
{
    /// <summary>
    /// Widget de un parámetro Enum: botones segmentados, uno por opción, con el activo
    /// resaltado. GDD §1.3 Mecánica 2 (línea 53).
    /// </summary>
    /// <remarks>
    /// Es el único widget cuyo número de controles depende del parámetro, así que instancia
    /// un prefab de botón por entrada de EffectParameter.EnumOptions. Las opciones se muestran
    /// tal como están escritas en el asset: la localización es LK-19.
    ///
    /// Los botones se reparten el ancho a partes iguales. Con más de cuatro opciones el texto
    /// se aprieta: ni se envuelve ni hay scroll horizontal (LK-22).
    /// </remarks>
    [AddComponentMenu("LumiKit/Enum Parameter Widget")]
    [DisallowMultipleComponent]
    public class EnumParameterWidget : ParameterWidgetBase
    {
        [Tooltip("Contenedor con HorizontalLayoutGroup donde se instancian las opciones.")]
        [SerializeField] private RectTransform _optionsRoot;
        [SerializeField] private Button _optionPrefab;

        private readonly List<Button> _options = new List<Button>();
        private readonly List<TMP_Text> _optionLabels = new List<TMP_Text>();

        protected override ParameterType SupportedType => ParameterType.Enum;

        protected override void OnInitialize()
        {
            if (_optionsRoot == null || _optionPrefab == null)
            {
                Debug.LogWarning($"[LumiKit] {name}: widget de Enum sin contenedor o sin prefab de opción.", this);
                return;
            }

            string[] names = Parameter.EnumOptions;
            if (names == null || names.Length == 0)
            {
                Debug.LogWarning(
                    $"[LumiKit] '{Parameter.PropertyName}' es Enum y no declara opciones: el widget queda vacío.", this);
                return;
            }

            ClearOptions();

            for (int i = 0; i < names.Length; i++)
            {
                Button option = Instantiate(_optionPrefab, _optionsRoot);
                option.name = $"Option_{i}";

                TMP_Text label = option.GetComponentInChildren<TMP_Text>();
                if (label != null)
                {
                    label.text = names[i];
                }

                // Copia local: sin ella todos los botones capturarían el último índice.
                int index = i;
                option.onClick.AddListener(() => Choose(index));

                _options.Add(option);
                _optionLabels.Add(label);
            }

            OnRefresh();
        }

        protected override void OnRefresh()
        {
            Paint(Controller.GetEnum(Parameter.PropertyName));
        }

        /// <summary>
        /// Escribe el índice y repinta con lo que haya quedado guardado, no con lo pulsado:
        /// EffectParameter.ClampEnumIndex puede acotarlo si el asset declara menos opciones.
        /// </summary>
        private void Choose(int index)
        {
            if (!IsInitialized)
            {
                return;
            }

            Controller.SetEnum(Parameter.PropertyName, index);
            Paint(Controller.GetEnum(Parameter.PropertyName));
            RaiseValueChanged();
        }

        private void Paint(int selected)
        {
            for (int i = 0; i < _options.Count; i++)
            {
                bool active = i == selected;

                Button option = _options[i];
                if (option != null && option.image != null)
                {
                    option.image.color = active ? LumiTheme.LumiCyan : LumiTheme.SurfaceElevated;
                }

                TMP_Text label = _optionLabels[i];
                if (label != null)
                {
                    label.color = active ? LumiTheme.TextOnAccent : LumiTheme.TextSecondary;
                }
            }
        }

        /// <summary>
        /// Defensivo: el panel siempre instancia el widget de cero, pero si alguien llamara a
        /// Initialize dos veces, las opciones no deben acumularse.
        /// </summary>
        private void ClearOptions()
        {
            for (int i = _options.Count - 1; i >= 0; i--)
            {
                if (_options[i] != null)
                {
                    _options[i].onClick.RemoveAllListeners();
                    Destroy(_options[i].gameObject);
                }
            }

            _options.Clear();
            _optionLabels.Clear();
        }

        private void OnDestroy()
        {
            for (int i = 0; i < _options.Count; i++)
            {
                if (_options[i] != null)
                {
                    _options[i].onClick.RemoveAllListeners();
                }
            }
        }
    }
}
