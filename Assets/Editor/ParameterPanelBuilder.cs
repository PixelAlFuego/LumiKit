using LumiKit.Demo;
using LumiKit.UI;
using LumiKit.UI.Widgets;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LumiKit.Editor
{
    /// <summary>
    /// Generador de los prefabs de UI de LK-11a y del montaje del HUD en la escena abierta.
    /// D-002: el prefab no se escribe a mano, se produce desde aquí.
    /// </summary>
    /// <remarks>
    /// Este script es la fuente de verdad; el .prefab es su salida. Medidas y colores salen
    /// de LumiTheme, que a su vez copia .claude/rules/ui-style.md.
    ///
    /// No sobrescribe nada: si un prefab ya existe, aborta. Para regenerar hay que borrarlo
    /// a mano desde el Project, decisión consciente de quien lo borra.
    /// </remarks>
    public static class ParameterPanelBuilder
    {
        private const string PREFAB_FOLDER = "Assets/LumiKit/Prefabs/UI";
        private const string SLIDER_PREFAB_PATH = PREFAB_FOLDER + "/PRF_Widget_Slider.prefab";
        private const string PANEL_PREFAB_PATH = PREFAB_FOLDER + "/PRF_ParameterPanel.prefab";

        private const string CANVAS_NAME = "UI_Root";
        private const string EVENT_SYSTEM_NAME = "EventSystem";
        private const string LOG = "[LumiKit] ";

        private const string SPRITE_UI = "UI/Skin/UISprite.psd";
        private const string SPRITE_KNOB = "UI/Skin/Knob.psd";

        // ── Menús ──────────────────────────────────────────────────────────────────────

        [MenuItem("LumiKit/UI/Generar prefabs del panel (LK-11a)", false, 100)]
        public static void GeneratePrefabs()
        {
            if (!HasDefaultFont())
            {
                return;
            }

            if (AssetExists(SLIDER_PREFAB_PATH) || AssetExists(PANEL_PREFAB_PATH))
            {
                string existing = AssetExists(SLIDER_PREFAB_PATH) ? SLIDER_PREFAB_PATH : PANEL_PREFAB_PATH;
                Debug.LogError($"{LOG}'{existing}' ya existe. No se sobrescribe nada: bórralo a mano en el Project si quieres regenerarlo.");
                EditorUtility.DisplayDialog("LumiKit", $"Ya existe:\n{existing}\n\nNo se ha tocado nada. Bórralo a mano para regenerar.", "Vale");
                return;
            }

            GameObject sliderRoot = BuildSliderWidget();
            GameObject sliderAsset = PrefabUtility.SaveAsPrefabAsset(sliderRoot, SLIDER_PREFAB_PATH);
            Object.DestroyImmediate(sliderRoot);

            SliderParameterWidget sliderPrefab = sliderAsset != null
                ? sliderAsset.GetComponent<SliderParameterWidget>()
                : null;

            GameObject panelRoot = BuildPanel(sliderPrefab);
            PrefabUtility.SaveAsPrefabAsset(panelRoot, PANEL_PREFAB_PATH);
            Object.DestroyImmediate(panelRoot);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"{LOG}Generados '{SLIDER_PREFAB_PATH}' y '{PANEL_PREFAB_PATH}'. Sin verificar en el editor.");
        }

        [MenuItem("LumiKit/UI/Montar HUD en la escena abierta (LK-11a)", false, 101)]
        public static void BuildSceneRig()
        {
            GameObject panelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PANEL_PREFAB_PATH);
            if (panelPrefab == null)
            {
                Debug.LogError($"{LOG}No existe '{PANEL_PREFAB_PATH}'. Ejecuta antes 'Generar prefabs del panel'.");
                return;
            }

            Scene scene = SceneManager.GetActiveScene();
            Canvas canvas = FindOrCreateCanvas();
            EnsureEventSystem();

            if (canvas.GetComponentInChildren<ParameterPanelUI>(true) != null)
            {
                Debug.LogWarning($"{LOG}'{canvas.name}' ya tiene un ParameterPanelUI. No se instancia otro.");
                return;
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(panelPrefab, canvas.transform);
            Undo.RegisterCreatedObjectUndo(instance, "Montar HUD LumiKit");

            RectTransform rect = instance.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            ObjectSelector selector = Object.FindFirstObjectByType<ObjectSelector>();
            ParameterPanelUI panel = instance.GetComponent<ParameterPanelUI>();

            if (selector != null && panel != null)
            {
                SerializedObject serialized = new SerializedObject(panel);
                serialized.FindProperty("_selector").objectReferenceValue = selector;
                serialized.ApplyModifiedProperties();
            }
            else
            {
                Debug.LogWarning($"{LOG}No hay ObjectSelector en la escena: asigna '_selector' a mano en el Inspector.");
            }

            EditorSceneManager.MarkSceneDirty(scene);
            Selection.activeGameObject = instance;

            Debug.Log($"{LOG}HUD montado en '{scene.name}'. La escena NO se ha guardado: revísala y guárdala tú.");
        }

        // ── Prefab del widget de Float ─────────────────────────────────────────────────

        /// <summary>
        /// Fila de un parámetro Float: etiqueta arriba a la izquierda, valor arriba a la
        /// derecha con ancho fijo, y el slider abajo ocupando todo el ancho.
        /// </summary>
        private static GameObject BuildSliderWidget()
        {
            GameObject root = CreateUIObject("Widget_Slider", null);
            RectTransform rootRect = root.GetComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0f, 1f);
            rootRect.anchorMax = new Vector2(1f, 1f);
            rootRect.pivot = new Vector2(0.5f, 1f);
            rootRect.sizeDelta = new Vector2(0f, LumiTheme.WIDGET_ROW_HEIGHT);

            LayoutElement layout = root.AddComponent<LayoutElement>();
            layout.minHeight = LumiTheme.WIDGET_ROW_HEIGHT;
            layout.preferredHeight = LumiTheme.WIDGET_ROW_HEIGHT;

            TextMeshProUGUI label = CreateText(
                "Label", root.transform, LumiTheme.TEXT_LABEL, LumiTheme.TextSecondary, TextAlignmentOptions.MidlineLeft);
            RectTransform labelRect = label.rectTransform;
            labelRect.anchorMin = new Vector2(0f, 1f);
            labelRect.anchorMax = new Vector2(1f, 1f);
            labelRect.offsetMin = new Vector2(0f, -LumiTheme.WIDGET_LABEL_HEIGHT);
            labelRect.offsetMax = new Vector2(-(LumiTheme.SLIDER_VALUE_WIDTH + LumiTheme.SPACING), 0f);
            label.text = "Parámetro";

            TextMeshProUGUI value = CreateText(
                "Value", root.transform, LumiTheme.TEXT_MONO, LumiTheme.TextPrimary, TextAlignmentOptions.MidlineRight);
            RectTransform valueRect = value.rectTransform;
            valueRect.anchorMin = new Vector2(1f, 1f);
            valueRect.anchorMax = new Vector2(1f, 1f);
            valueRect.pivot = new Vector2(1f, 1f);
            valueRect.anchoredPosition = Vector2.zero;
            valueRect.sizeDelta = new Vector2(LumiTheme.SLIDER_VALUE_WIDTH, LumiTheme.WIDGET_LABEL_HEIGHT);
            value.text = "0.00";

            // Slider, anclado abajo y a todo el ancho.
            GameObject sliderGo = CreateUIObject("Slider", root.transform);
            RectTransform sliderRect = sliderGo.GetComponent<RectTransform>();
            sliderRect.anchorMin = new Vector2(0f, 0f);
            sliderRect.anchorMax = new Vector2(1f, 0f);
            sliderRect.offsetMin = Vector2.zero;
            sliderRect.offsetMax = new Vector2(0f, LumiTheme.SLIDER_HANDLE_SIZE);

            Image background = CreateImage("Background", sliderGo.transform, LumiTheme.Border, SPRITE_UI, Image.Type.Sliced);
            RectTransform backgroundRect = background.rectTransform;
            backgroundRect.anchorMin = new Vector2(0f, 0.5f);
            backgroundRect.anchorMax = new Vector2(1f, 0.5f);
            backgroundRect.anchoredPosition = Vector2.zero;
            backgroundRect.sizeDelta = new Vector2(0f, LumiTheme.SLIDER_TRACK_HEIGHT);

            // El área de relleno se encoge media manija por lado: así el relleno termina
            // donde está el centro de la manija y no se adelanta a ella.
            GameObject fillArea = CreateUIObject("Fill Area", sliderGo.transform);
            RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
            fillAreaRect.anchorMin = new Vector2(0f, 0.5f);
            fillAreaRect.anchorMax = new Vector2(1f, 0.5f);
            fillAreaRect.anchoredPosition = Vector2.zero;
            fillAreaRect.sizeDelta = new Vector2(-LumiTheme.SLIDER_HANDLE_SIZE, LumiTheme.SLIDER_TRACK_HEIGHT);

            Image fill = CreateImage("Fill", fillArea.transform, LumiTheme.LumiCyan, SPRITE_UI, Image.Type.Sliced);
            RectTransform fillRect = fill.rectTransform;
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            GameObject handleArea = CreateUIObject("Handle Slide Area", sliderGo.transform);
            RectTransform handleAreaRect = handleArea.GetComponent<RectTransform>();
            handleAreaRect.anchorMin = Vector2.zero;
            handleAreaRect.anchorMax = Vector2.one;
            handleAreaRect.anchoredPosition = Vector2.zero;
            handleAreaRect.sizeDelta = new Vector2(-LumiTheme.SLIDER_HANDLE_SIZE, 0f);

            // Manija: círculo cian con el núcleo claro dentro. El borde de 2 px de ui-style
            // se consigue con dos imágenes concéntricas, sin sprites propios (LK-22).
            Image handle = CreateImage("Handle", handleArea.transform, LumiTheme.LumiCyan, SPRITE_KNOB, Image.Type.Simple);
            RectTransform handleRect = handle.rectTransform;
            handleRect.anchorMin = new Vector2(0f, 0f);
            handleRect.anchorMax = new Vector2(0f, 1f);
            handleRect.sizeDelta = new Vector2(LumiTheme.SLIDER_HANDLE_SIZE, 0f);

            Image handleCore = CreateImage("Handle Core", handle.transform, LumiTheme.TextPrimary, SPRITE_KNOB, Image.Type.Simple);
            RectTransform handleCoreRect = handleCore.rectTransform;
            handleCoreRect.anchorMin = Vector2.zero;
            handleCoreRect.anchorMax = Vector2.one;
            handleCoreRect.offsetMin = new Vector2(LumiTheme.SLIDER_HANDLE_BORDER, LumiTheme.SLIDER_HANDLE_BORDER);
            handleCoreRect.offsetMax = new Vector2(-LumiTheme.SLIDER_HANDLE_BORDER, -LumiTheme.SLIDER_HANDLE_BORDER);
            handleCore.raycastTarget = false;

            Slider slider = sliderGo.AddComponent<Slider>();
            slider.direction = Slider.Direction.LeftToRight;
            slider.fillRect = fillRect;
            slider.handleRect = handleRect;
            slider.targetGraphic = handle;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.wholeNumbers = false;
            slider.SetValueWithoutNotify(0f);

            SliderParameterWidget widget = root.AddComponent<SliderParameterWidget>();
            SerializedObject serialized = new SerializedObject(widget);
            serialized.FindProperty("_label").objectReferenceValue = label;
            serialized.FindProperty("_slider").objectReferenceValue = slider;
            serialized.FindProperty("_valueLabel").objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            return root;
        }

        // ── Prefab del panel ───────────────────────────────────────────────────────────

        /// <summary>
        /// Raíz transparente a pantalla completa con la superficie de 280 px pegada a la
        /// derecha. La raíz nunca se apaga: es quien escucha la selección.
        /// </summary>
        private static GameObject BuildPanel(SliderParameterWidget sliderPrefab)
        {
            GameObject root = CreateUIObject("ParameterPanel", null);
            RectTransform rootRect = root.GetComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            Image surface = CreateImage("Surface", root.transform, LumiTheme.Surface, null, Image.Type.Simple);
            RectTransform surfaceRect = surface.rectTransform;
            surfaceRect.anchorMin = new Vector2(1f, 0f);
            surfaceRect.anchorMax = new Vector2(1f, 1f);
            surfaceRect.pivot = new Vector2(1f, 0.5f);
            surfaceRect.anchoredPosition = Vector2.zero;
            surfaceRect.sizeDelta = new Vector2(LumiTheme.PANEL_WIDTH, 0f);

            // El fondo del panel bloquea el ratón: es lo que hace que arrastrar un slider no
            // panee la cámara ni cambie la selección (issue de LK-10 y LK-12).
            surface.raycastTarget = true;

            Image border = CreateImage("Border", surface.transform, LumiTheme.Border, null, Image.Type.Simple);
            RectTransform borderRect = border.rectTransform;
            borderRect.anchorMin = new Vector2(0f, 0f);
            borderRect.anchorMax = new Vector2(0f, 1f);
            borderRect.pivot = new Vector2(0f, 0.5f);
            borderRect.anchoredPosition = Vector2.zero;
            borderRect.sizeDelta = new Vector2(LumiTheme.PANEL_BORDER, 0f);
            border.raycastTarget = false;

            GameObject header = CreateUIObject("Header", surface.transform);
            RectTransform headerRect = header.GetComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0f, 1f);
            headerRect.anchorMax = new Vector2(1f, 1f);
            headerRect.pivot = new Vector2(0.5f, 1f);
            headerRect.anchoredPosition = Vector2.zero;
            headerRect.sizeDelta = new Vector2(0f, LumiTheme.PANEL_HEADER_HEIGHT);

            TextMeshProUGUI effectName = CreateText(
                "EffectName", header.transform, LumiTheme.TEXT_H2, LumiTheme.TextPrimary, TextAlignmentOptions.MidlineLeft);
            RectTransform effectNameRect = effectName.rectTransform;
            effectNameRect.anchorMin = Vector2.zero;
            effectNameRect.anchorMax = Vector2.one;
            effectNameRect.offsetMin = new Vector2(LumiTheme.PANEL_PADDING, 0f);
            effectNameRect.offsetMax = new Vector2(-LumiTheme.PANEL_PADDING, 0f);
            effectName.text = "Efecto";

            GameObject content = CreateUIObject("Content", surface.transform);
            RectTransform contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.anchoredPosition = new Vector2(0f, -LumiTheme.PANEL_HEADER_HEIGHT);
            contentRect.sizeDelta = Vector2.zero;

            int padding = (int)LumiTheme.PANEL_PADDING;
            VerticalLayoutGroup group = content.AddComponent<VerticalLayoutGroup>();
            group.padding = new RectOffset(padding, padding, padding, padding);
            group.spacing = LumiTheme.WIDGET_ROW_SPACING;
            group.childAlignment = TextAnchor.UpperLeft;
            group.childControlWidth = true;
            group.childControlHeight = true;
            group.childForceExpandWidth = true;
            group.childForceExpandHeight = false;

            // Sin scroll todavía (LK-11b): el contenedor crece hacia abajo con los widgets.
            ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            ParameterPanelUI panel = root.AddComponent<ParameterPanelUI>();
            SerializedObject serialized = new SerializedObject(panel);
            serialized.FindProperty("_surface").objectReferenceValue = surface.gameObject;
            serialized.FindProperty("_content").objectReferenceValue = contentRect;
            serialized.FindProperty("_effectNameLabel").objectReferenceValue = effectName;
            serialized.FindProperty("_floatWidgetPrefab").objectReferenceValue = sliderPrefab;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            // Arranca oculto: el panel se despliega al seleccionar (GDD línea 136).
            surface.gameObject.SetActive(false);

            return root;
        }

        // ── Escena ─────────────────────────────────────────────────────────────────────

        private static Canvas FindOrCreateCanvas()
        {
            GameObject existing = GameObject.Find(CANVAS_NAME);
            if (existing != null)
            {
                Canvas found = existing.GetComponent<Canvas>();
                if (found != null)
                {
                    return found;
                }
            }

            GameObject canvasGo = CreateUIObject(CANVAS_NAME, null);
            Undo.RegisterCreatedObjectUndo(canvasGo, "Crear UI_Root");

            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(LumiTheme.REFERENCE_WIDTH, LumiTheme.REFERENCE_HEIGHT);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 1f;

            canvasGo.AddComponent<GraphicRaycaster>();

            return canvas;
        }

        /// <summary>
        /// Crea el EventSystem si falta. Si ya hay uno con otro módulo de entrada, avisa y no
        /// lo toca: cambiarlo por sorpresa rompería el input de quien lo puso (D-006).
        /// </summary>
        private static void EnsureEventSystem()
        {
            EventSystem existing = Object.FindFirstObjectByType<EventSystem>();
            if (existing == null)
            {
                GameObject go = new GameObject(EVENT_SYSTEM_NAME);
                go.AddComponent<EventSystem>();
                go.AddComponent<InputSystemUIInputModule>();
                Undo.RegisterCreatedObjectUndo(go, "Crear EventSystem");
                return;
            }

            if (existing.GetComponent<InputSystemUIInputModule>() == null)
            {
                Debug.LogWarning(
                    $"{LOG}'{existing.name}' no usa InputSystemUIInputModule. D-006 lo exige; cámbialo a mano.", existing);
            }
        }

        // ── Utilidades ─────────────────────────────────────────────────────────────────

        private static bool AssetExists(string path)
        {
            return AssetDatabase.LoadAssetAtPath<Object>(path) != null;
        }

        /// <summary>
        /// Sin los TMP Essential Resources no hay fuente por defecto y los textos salen
        /// invisibles. Mejor abortar que generar un prefab mudo.
        /// </summary>
        private static bool HasDefaultFont()
        {
            if (TMP_Settings.defaultFontAsset != null)
            {
                return true;
            }

            Debug.LogError($"{LOG}Faltan los TMP Essential Resources: Window > TextMeshPro > Import TMP Essential Resources.");
            EditorUtility.DisplayDialog(
                "LumiKit",
                "Faltan los TMP Essential Resources.\n\nWindow > TextMeshPro > Import TMP Essential Resources, y vuelve a ejecutar el menú.",
                "Vale");
            return false;
        }

        private static GameObject CreateUIObject(string name, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));

            int uiLayer = LayerMask.NameToLayer("UI");
            if (uiLayer >= 0)
            {
                go.layer = uiLayer;
            }

            if (parent != null)
            {
                go.transform.SetParent(parent, false);
            }

            return go;
        }

        private static Image CreateImage(string name, Transform parent, Color color, string builtinSprite, Image.Type type)
        {
            GameObject go = CreateUIObject(name, parent);
            Image image = go.AddComponent<Image>();
            image.color = color;

            if (!string.IsNullOrEmpty(builtinSprite))
            {
                image.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(builtinSprite);
                image.type = type;
            }

            return image;
        }

        private static TextMeshProUGUI CreateText(
            string name, Transform parent, float size, Color color, TextAlignmentOptions alignment)
        {
            GameObject go = CreateUIObject(name, parent);
            TextMeshProUGUI text = go.AddComponent<TextMeshProUGUI>();
            text.fontSize = size;
            text.color = color;
            text.alignment = alignment;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.raycastTarget = false;
            return text;
        }
    }
}
