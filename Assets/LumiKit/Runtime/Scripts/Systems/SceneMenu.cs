using System;
using System.Collections.Generic;
using LumiKit.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LumiKit.Systems
{
    /// <summary>Qué hace una entrada de SceneMenu.</summary>
    public enum MenuAction
    {
        LoadScene,
        Quit
    }

    /// <summary>Una entrada de SceneMenu: un botón.</summary>
    [Serializable]
    public class MenuEntry
    {
        [SerializeField] private string _label;
        [SerializeField] private LumiButtonStyle _style = LumiButtonStyle.Secondary;
        [SerializeField] private MenuAction _action = MenuAction.LoadScene;

        [Tooltip("Sólo con LoadScene: nombre de la escena, como figura en la lista del build.")]
        [SerializeField] private string _sceneName;

        public string Label => _label;
        public LumiButtonStyle Style => _style;
        public MenuAction Action => _action;
        public string SceneName => _sceneName;
    }

    /// <summary>
    /// Botones de navegación generados desde una lista (LK-13): uno por entrada, clonando un
    /// LumiButton plantilla. Cambiar la lista en el Inspector cambia los botones.
    /// </summary>
    /// <remarks>
    /// En Systems y no en UI: llama a SceneLoader, y Systems está por encima de UI (GDD §4.4),
    /// como UISoundTrigger. Los sonidos de hover y clic vienen con la plantilla (UISoundTrigger).
    /// </remarks>
    [AddComponentMenu("LumiKit/Scene Menu")]
    [DisallowMultipleComponent]
    public class SceneMenu : MonoBehaviour
    {
        [Tooltip("Botón que se clona por entrada. Se queda inactivo.")]
        [SerializeField] private LumiButton _buttonTemplate;

        [Tooltip("Padre de los botones, con su layout group.")]
        [SerializeField] private RectTransform _container;

        [Tooltip("Ancho de cada botón. 0: el del texto más el padding de LumiTheme a cada lado.")]
        [SerializeField] private float _buttonWidth;

        [SerializeField] private List<MenuEntry> _entries = new List<MenuEntry>();

        private void Awake()
        {
            if (_buttonTemplate == null || _container == null)
            {
                Debug.LogWarning($"[LumiKit] SceneMenu en '{name}' sin plantilla o sin contenedor. No crea botones.", this);
                return;
            }

            _buttonTemplate.gameObject.SetActive(false);

            for (int i = 0; i < _entries.Count; i++)
            {
                CreateButton(_entries[i]);
            }
        }

        private void CreateButton(MenuEntry entry)
        {
            LumiButton button = Instantiate(_buttonTemplate, _container);
            button.name = $"Button_{entry.Label}";

            // Inactivo no pinta: el estilo se aplica al activarlo (Selectable.OnEnable).
            button.Style = entry.Style;
            TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
            if (label != null)
            {
                label.text = entry.Label;
            }

            button.gameObject.SetActive(true);

            // Se mide ya activo: el texto calcula su ancho con la fuente cargada.
            LayoutElement layout = button.GetComponent<LayoutElement>();
            if (layout != null)
            {
                float labelWidth = label != null ? label.preferredWidth : 0f;
                layout.preferredWidth = _buttonWidth > 0f
                    ? _buttonWidth
                    : labelWidth + 2f * LumiTheme.BUTTON_PADDING;
            }

            button.onClick.AddListener(() => Run(entry));
        }

        private static void Run(MenuEntry entry)
        {
            switch (entry.Action)
            {
                case MenuAction.Quit:
                    SceneLoader.Quit();
                    break;
                default:
                    SceneLoader.Load(entry.SceneName);
                    break;
            }
        }
    }
}
