using System.Collections.Generic;
using LumiKit.Core;
using LumiKit.Demo;
using LumiKit.UI.Widgets;
using TMPro;
using UnityEngine;

namespace LumiKit.UI
{
    /// <summary>
    /// Panel lateral de parámetros: escucha la selección, lee el EffectDefinition del objeto
    /// activo e instancia un widget por parámetro. Mecánica 2 del GDD.
    /// </summary>
    /// <remarks>
    /// D-003: el panel no sabe qué efecto está mostrando. Lo único específico de un tipo es
    /// PrefabFor, que traduce ParameterType a prefab.
    /// LK-10 se consume por evento: no hay Update ni sondeo de Selected.
    /// </remarks>
    [AddComponentMenu("LumiKit/Parameter Panel UI")]
    [DisallowMultipleComponent]
    public class ParameterPanelUI : MonoBehaviour
    {
        [Header("Fuente de la selección")]
        [SerializeField] private ObjectSelector _selector;

        [Header("Jerarquía del panel")]
        [Tooltip("Contenedor visual. Se apaga cuando no hay nada seleccionado.")]
        [SerializeField] private GameObject _surface;
        [Tooltip("Padre de los widgets. No debe contener nada más: se vacía en cada reconstrucción.")]
        [SerializeField] private RectTransform _content;
        [SerializeField] private TMP_Text _effectNameLabel;

        [Header("Prefabs de widget")]
        [Tooltip("Widget del tipo Float. Color, Boolean y Enum llegan en LK-11b.")]
        [SerializeField] private SliderParameterWidget _floatWidgetPrefab;

        private readonly List<ParameterWidgetBase> _widgets = new List<ParameterWidgetBase>();

        private void OnEnable()
        {
            if (_selector == null)
            {
                Debug.LogWarning(
                    $"[LumiKit] ParameterPanelUI en '{name}' sin ObjectSelector asignado. Se desactiva.", this);
                enabled = false;
                return;
            }

            // Apagar el propio GameObject dejaría el componente muerto y el panel no volvería
            // a aparecer nunca. _surface tiene que ser un hijo.
            if (_surface == gameObject)
            {
                Debug.LogWarning(
                    $"[LumiKit] ParameterPanelUI en '{name}': _surface no puede ser el propio objeto. Se ignora.", this);
                _surface = null;
            }

            _selector.OnSelectionChanged += HandleSelectionChanged;

            // El panel puede activarse con algo ya seleccionado: el evento de LK-10 sólo se
            // dispara al cambiar, así que el estado inicial se lee aquí.
            Build(_selector.Selected);
        }

        private void OnDisable()
        {
            if (_selector != null)
            {
                _selector.OnSelectionChanged -= HandleSelectionChanged;
            }

            Clear();
            ShowSurface(false);
        }

        /// <summary>
        /// Relee del EffectController el valor de cada widget vivo. Es la vía para reflejar un
        /// cambio hecho fuera del panel.
        /// </summary>
        /// <remarks>
        /// Sin llamador todavía: lo necesitan ResetToDefaults desde el botón Reset (LK-11b) y
        /// SetEffectEnabled desde la comparación con TAB (LK-24), que escriben en el controller
        /// sin pasar por los widgets. Existe desde ya para no tener que tocar este panel después
        /// de verificarlo. En el editor se puede disparar desde el menú contextual del componente.
        /// </remarks>
        [ContextMenu("Refresh from controller")]
        public void RefreshFromController()
        {
            for (int i = 0; i < _widgets.Count; i++)
            {
                if (_widgets[i] != null)
                {
                    _widgets[i].Refresh();
                }
            }
        }

        private void HandleSelectionChanged(EffectController selected)
        {
            Build(selected);
        }

        /// <summary>
        /// Vacía el panel y lo vuelve a poblar con el objeto recibido. null lo deja oculto.
        /// </summary>
        private void Build(EffectController controller)
        {
            Clear();

            if (controller == null)
            {
                ShowSurface(false);
                return;
            }

            EffectDefinition definition = controller.Definition;
            if (definition == null)
            {
                Debug.LogWarning(
                    $"[LumiKit] '{controller.name}' no tiene EffectDefinition: no hay nada que mostrar.", this);
                ShowSurface(false);
                return;
            }

            if (_effectNameLabel != null)
            {
                _effectNameLabel.text = string.IsNullOrEmpty(definition.DisplayNameEs)
                    ? definition.name
                    : definition.DisplayNameEs;
            }

            if (_content == null)
            {
                Debug.LogWarning($"[LumiKit] ParameterPanelUI en '{name}' sin contenedor de widgets.", this);
                ShowSurface(false);
                return;
            }

            List<ParameterType> omitted = null;
            IReadOnlyList<EffectParameter> parameters = definition.Parameters;

            for (int i = 0; i < parameters.Count; i++)
            {
                EffectParameter parameter = parameters[i];
                if (parameter == null || string.IsNullOrEmpty(parameter.PropertyName))
                {
                    continue;
                }

                ParameterWidgetBase prefab = PrefabFor(parameter.Type);
                if (prefab == null)
                {
                    if (omitted == null)
                    {
                        omitted = new List<ParameterType>();
                    }

                    if (!omitted.Contains(parameter.Type))
                    {
                        omitted.Add(parameter.Type);
                    }

                    continue;
                }

                ParameterWidgetBase widget = Instantiate(prefab, _content);
                widget.name = $"Widget_{parameter.PropertyName}";
                widget.Initialize(parameter, controller);
                _widgets.Add(widget);
            }

            // Un aviso por reconstrucción, no uno por parámetro: con cuatro tipos y un solo
            // widget implementado, lo contrario llenaría la consola en cada clic.
            if (omitted != null)
            {
                Debug.LogWarning(
                    $"[LumiKit] {definition.name}: todavía no hay widget para {string.Join(", ", omitted)} (LK-11b). Esos parámetros no se muestran.",
                    this);
            }

            ShowSurface(_widgets.Count > 0);
        }

        /// <summary>
        /// Prefab del widget de un tipo, o null si ese tipo todavía no tiene widget.
        /// Único punto del panel que distingue tipos; los widgets no se enteran (D-003).
        /// </summary>
        private ParameterWidgetBase PrefabFor(ParameterType type)
        {
            switch (type)
            {
                case ParameterType.Float:
                    return _floatWidgetPrefab;
                default:
                    // Color, Boolean y Enum → LK-11b.
                    return null;
            }
        }

        /// <summary>
        /// Destruye los widgets vivos. Recorrido inverso porque destruir de arriba abajo
        /// reindexa los hijos y salta uno de cada dos: es el bug de widgets duplicados del
        /// GDD (línea 1249).
        /// </summary>
        private void Clear()
        {
            _widgets.Clear();

            if (_content == null)
            {
                return;
            }

            for (int i = _content.childCount - 1; i >= 0; i--)
            {
                Destroy(_content.GetChild(i).gameObject);
            }
        }

        private void ShowSurface(bool visible)
        {
            if (_surface != null && _surface.activeSelf != visible)
            {
                _surface.SetActive(visible);
            }
        }
    }
}
