using System.Collections.Generic;
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
    /// Generador de los prefabs de UI del panel (LK-11a y LK-11b) y del montaje del HUD.
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
        private const string COLOR_PREFAB_PATH = PREFAB_FOLDER + "/PRF_Widget_Color.prefab";
        private const string TOGGLE_PREFAB_PATH = PREFAB_FOLDER + "/PRF_Widget_Toggle.prefab";
        private const string ENUM_OPTION_PREFAB_PATH = PREFAB_FOLDER + "/PRF_Widget_EnumOption.prefab";
        private const string ENUM_PREFAB_PATH = PREFAB_FOLDER + "/PRF_Widget_Enum.prefab";
        private const string PANEL_PREFAB_PATH = PREFAB_FOLDER + "/PRF_ParameterPanel.prefab";

        private const string CANVAS_NAME = "UI_Root";
        private const string EVENT_SYSTEM_NAME = "EventSystem";
        private const string LOG = "[LumiKit] ";

        // Sprites del pack (LK-51), a 3× con PPU 300: docs/reference/UI_ART_BRIEF.md. Blancos con
        // la forma en el alfa; el color lo pone LumiTheme. Todos obligatorios salvo el riel y el aro.
        private const string SPRITE_FOLDER = "Assets/LumiKit/Sprites/UI";
        private const string SPRITE_RECT_R6 = SPRITE_FOLDER + "/SPR_UI_Rect_R6.png";
        private const string SPRITE_RECT_R6_OUTLINE = SPRITE_FOLDER + "/SPR_UI_Rect_R6_Outline.png";
        private const string SPRITE_RECT_R4 = SPRITE_FOLDER + "/SPR_UI_Rect_R4.png";
        private const string SPRITE_PILL = SPRITE_FOLDER + "/SPR_UI_Pill.png";
        private const string SPRITE_CIRCLE = SPRITE_FOLDER + "/SPR_UI_Circle.png";
        // El generador ya no la usa (Sesión 10): apunta a una pieza suelta del kit, que sigue en el pack.
        private const string SPRITE_RING = SPRITE_FOLDER + "/SPR_UI_Ring.png";
        private const string SPRITE_TRACK = SPRITE_FOLDER + "/SPR_UI_Track.png";

        // Fuentes del pack (LK-22b): TMP_FontAsset Static de Assets/LumiKit/Fonts/, horneadas en cada
        // texto del prefab. LumiTheme sólo tiene los tamaños: una clase estática no guarda una fuente.
        private const string FONT_FOLDER = "Assets/LumiKit/Fonts";
        private const string FONT_DISPLAY = FONT_FOLDER + "/SpaceGrotesk-Medium SDF.asset";
        private const string FONT_LABEL = FONT_FOLDER + "/Inter-Medium SDF.asset";
        private const string FONT_MONO = FONT_FOLDER + "/JetBrainsMono-Regular SDF.asset";
        // Ningún texto del panel la usa todavía: entra con la familia Body en LK-25. Hasta entonces
        // RequireFonts() no la exige, porque el generador exige sólo lo que usa.
        private const string FONT_BODY = FONT_FOLDER + "/Inter-Regular SDF.asset";

        /// <summary>
        /// Familia tipográfica de un texto (ui-style.md > Tipografía). Enum y no string: una
        /// familia mal escrita tiene que ser un error de compilación, no un texto invisible.
        /// </summary>
        private enum FontFamily
        {
            Display,
            Label,
            Mono
        }

        // ── Menús ──────────────────────────────────────────────────────────────────────

        [MenuItem("LumiKit/UI/Generar prefabs del panel (LK-11)", false, 100)]
        public static void GeneratePrefabs()
        {
            if (!HasDefaultFont())
            {
                return;
            }

            if (!RequireFonts())
            {
                return;
            }

            if (!RequireSprites())
            {
                return;
            }

            // Orden: los widgets antes que el panel, porque el panel guarda referencias a
            // sus prefabs; y el botón de opción antes que el widget de Enum, por lo mismo.
            string[] targets =
            {
                SLIDER_PREFAB_PATH, COLOR_PREFAB_PATH, TOGGLE_PREFAB_PATH,
                ENUM_OPTION_PREFAB_PATH, ENUM_PREFAB_PATH, PANEL_PREFAB_PATH
            };

            List<string> existing = new List<string>();
            for (int i = 0; i < targets.Length; i++)
            {
                if (AssetExists(targets[i]))
                {
                    existing.Add(targets[i]);
                }
            }

            if (existing.Count > 0)
            {
                Debug.LogError($"{LOG}No se sobrescribe nada. Ya existen: {string.Join(", ", existing)}");
                EditorUtility.DisplayDialog(
                    "LumiKit",
                    $"Ya existen {existing.Count} de los {targets.Length} prefabs y no se ha tocado nada. La consola los lista; bórralos a mano para regenerar.",
                    "Vale");
                return;
            }

            SliderParameterWidget slider = SaveAsPrefab<SliderParameterWidget>(BuildSliderWidget(), SLIDER_PREFAB_PATH);
            ColorParameterWidget color = SaveAsPrefab<ColorParameterWidget>(BuildColorWidget(), COLOR_PREFAB_PATH);
            ToggleParameterWidget toggle = SaveAsPrefab<ToggleParameterWidget>(BuildToggleWidget(), TOGGLE_PREFAB_PATH);
            Button option = SaveAsPrefab<Button>(BuildEnumOption(), ENUM_OPTION_PREFAB_PATH);
            EnumParameterWidget enumWidget = SaveAsPrefab<EnumParameterWidget>(BuildEnumWidget(option), ENUM_PREFAB_PATH);

            GameObject panelRoot = BuildPanel(slider, color, toggle, enumWidget);
            PrefabUtility.SaveAsPrefabAsset(panelRoot, PANEL_PREFAB_PATH);
            Object.DestroyImmediate(panelRoot);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"{LOG}Generados {targets.Length} prefabs en '{PREFAB_FOLDER}'. Sin verificar en el editor.");
        }

        [MenuItem("LumiKit/UI/Montar HUD en la escena abierta (LK-11)", false, 101)]
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

        /// <summary>
        /// Guarda la jerarquía temporal como prefab, la borra de la escena y devuelve el
        /// componente del asset: ésa es la referencia que se serializa en el panel.
        /// </summary>
        private static T SaveAsPrefab<T>(GameObject root, string path) where T : Component
        {
            GameObject asset = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);

            if (asset == null)
            {
                Debug.LogError($"{LOG}No se pudo guardar '{path}'.");
                return null;
            }

            return asset.GetComponent<T>();
        }

        // ── Prefab del widget de Float ─────────────────────────────────────────────────

        /// <summary>
        /// Fila de un parámetro Float: etiqueta arriba a la izquierda, valor arriba a la
        /// derecha con ancho fijo, y el slider abajo ocupando todo el ancho.
        /// </summary>
        private static GameObject BuildSliderWidget()
        {
            GameObject root = CreateRow("Widget_Slider", LumiTheme.WIDGET_ROW_HEIGHT, out _);
            TextMeshProUGUI label = CreateRowLabel(
                root.transform, LumiTheme.SLIDER_VALUE_WIDTH + LumiTheme.SPACING, LumiTheme.WIDGET_LABEL_HEIGHT);

            TextMeshProUGUI value = CreateText(
                "Value", root.transform, FontFamily.Mono, LumiTheme.TEXT_MONO, LumiTheme.TextPrimary, TextAlignmentOptions.MidlineRight);
            RectTransform valueRect = value.rectTransform;
            valueRect.anchorMin = new Vector2(1f, 1f);
            valueRect.anchorMax = new Vector2(1f, 1f);
            valueRect.pivot = new Vector2(1f, 1f);
            valueRect.anchoredPosition = Vector2.zero;
            valueRect.sizeDelta = new Vector2(LumiTheme.SLIDER_VALUE_WIDTH, LumiTheme.WIDGET_LABEL_HEIGHT);
            value.text = "0.00";

            // Slider, anclado abajo y a todo el ancho de la fila.
            Slider slider = CreateSlider("Slider", root.transform);
            RectTransform sliderRect = slider.GetComponent<RectTransform>();
            sliderRect.anchorMin = new Vector2(0f, 0f);
            sliderRect.anchorMax = new Vector2(1f, 0f);
            sliderRect.offsetMin = Vector2.zero;
            sliderRect.offsetMax = new Vector2(0f, LumiTheme.SLIDER_HANDLE_SIZE);

            SliderParameterWidget widget = root.AddComponent<SliderParameterWidget>();
            SerializedObject serialized = new SerializedObject(widget);
            serialized.FindProperty("_label").objectReferenceValue = label;
            serialized.FindProperty("_slider").objectReferenceValue = slider;
            serialized.FindProperty("_valueLabel").objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            return root;
        }

        /// <summary>
        /// Slider completo con riel, relleno y manija, con las medidas de ui-style. Lo usan el
        /// widget de Float y los tres canales del de Color. El RectTransform lo coloca quien
        /// llama: cada widget lo pone en un sitio distinto de su fila.
        /// </summary>
        private static Slider CreateSlider(string name, Transform parent)
        {
            GameObject sliderGo = CreateUIObject(name, parent);

            Image background = CreateImage("Background", sliderGo.transform, LumiTheme.Border, SPRITE_TRACK, Image.Type.Sliced);
            RectTransform backgroundRect = background.rectTransform;
            backgroundRect.anchorMin = new Vector2(0f, 0.5f);
            backgroundRect.anchorMax = new Vector2(1f, 0.5f);
            backgroundRect.anchoredPosition = Vector2.zero;
            backgroundRect.sizeDelta = new Vector2(0f, LumiTheme.SLIDER_TRACK_HEIGHT);

            // El área de relleno se encoge media manija por lado: así el relleno termina donde
            // está el centro de la manija y no se adelanta a ella.
            GameObject fillArea = CreateUIObject("Fill Area", sliderGo.transform);
            RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
            fillAreaRect.anchorMin = new Vector2(0f, 0.5f);
            fillAreaRect.anchorMax = new Vector2(1f, 0.5f);
            fillAreaRect.anchoredPosition = Vector2.zero;
            fillAreaRect.sizeDelta = new Vector2(-LumiTheme.SLIDER_HANDLE_SIZE, LumiTheme.SLIDER_TRACK_HEIGHT);

            Image fill = CreateImage("Fill", fillArea.transform, LumiTheme.LumiCyan, SPRITE_TRACK, Image.Type.Sliced);
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

            // Manija: círculo cian macizo con el núcleo claro encima, insertado los 2 px de borde de
            // ui-style. Con un aro debajo, los bordes suavizados de los dos dejaban ver el panel entre
            // ellos: un hilo oscuro. Sobre cian macizo, el borde del núcleo se funde con cian (LK-51).
            Image handle = CreateImage("Handle", handleArea.transform, LumiTheme.LumiCyan, SPRITE_CIRCLE, Image.Type.Simple);
            RectTransform handleRect = handle.rectTransform;
            handleRect.anchorMin = new Vector2(0f, 0f);
            handleRect.anchorMax = new Vector2(0f, 1f);
            handleRect.sizeDelta = new Vector2(LumiTheme.SLIDER_HANDLE_SIZE, 0f);

            Image handleCore = CreateImage("Handle Core", handle.transform, LumiTheme.TextPrimary, SPRITE_CIRCLE, Image.Type.Simple);
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

            return slider;
        }

        /// <summary>
        /// Raíz de una fila de widget: anclada arriba, a todo el ancho, con el LayoutElement que
        /// el VerticalLayoutGroup del panel usa para repartir alturas.
        /// </summary>
        private static GameObject CreateRow(string name, float height, out LayoutElement layout)
        {
            GameObject root = CreateUIObject(name, null);
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(0f, height);

            layout = root.AddComponent<LayoutElement>();
            layout.minHeight = height;
            layout.preferredHeight = height;

            return root;
        }

        /// <summary>
        /// Etiqueta del parámetro, arriba a la izquierda, dejando a la derecha el hueco que
        /// ocupe el control de la fila. El texto real lo escribe ParameterWidgetBase.
        /// </summary>
        private static TextMeshProUGUI CreateRowLabel(Transform parent, float rightInset, float height)
        {
            TextMeshProUGUI label = CreateText(
                "Label", parent, FontFamily.Label, LumiTheme.TEXT_LABEL, LumiTheme.TextSecondary, TextAlignmentOptions.MidlineLeft);

            RectTransform rect = label.rectTransform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.offsetMin = new Vector2(0f, -height);
            rect.offsetMax = new Vector2(-rightInset, 0f);
            label.text = "Parámetro";

            return label;
        }

        // ── Prefab del widget de Color ─────────────────────────────────────────────────

        /// <summary>
        /// Fila plegada con la muestra a la derecha y, debajo, el bloque desplegable con los
        /// tres canales y las seis muestras. Nace plegado, con el bloque desactivado.
        /// </summary>
        private static GameObject BuildColorWidget()
        {
            GameObject root = CreateRow("Widget_Color", LumiTheme.COLOR_ROW_HEIGHT, out LayoutElement layout);
            TextMeshProUGUI label = CreateRowLabel(
                root.transform, LumiTheme.COLOR_SWATCH_WIDTH + LumiTheme.SPACING, LumiTheme.COLOR_ROW_HEIGHT);

            // La muestra es el botón que despliega: uno aparte gastaría ancho del panel.
            Image swatch = CreateImage("Swatch", root.transform, Color.white, SPRITE_RECT_R4, Image.Type.Sliced);
            RectTransform swatchRect = swatch.rectTransform;
            swatchRect.anchorMin = new Vector2(1f, 1f);
            swatchRect.anchorMax = new Vector2(1f, 1f);
            swatchRect.pivot = new Vector2(1f, 1f);
            swatchRect.anchoredPosition =
                new Vector2(0f, -(LumiTheme.COLOR_ROW_HEIGHT - LumiTheme.COLOR_SWATCH_HEIGHT) * 0.5f);
            swatchRect.sizeDelta = new Vector2(LumiTheme.COLOR_SWATCH_WIDTH, LumiTheme.COLOR_SWATCH_HEIGHT);

            Button swatchButton = swatch.gameObject.AddComponent<Button>();
            swatchButton.targetGraphic = swatch;

            GameObject expand = CreateUIObject("Expand", root.transform);
            RectTransform expandRect = expand.GetComponent<RectTransform>();
            expandRect.anchorMin = new Vector2(0f, 1f);
            expandRect.anchorMax = new Vector2(1f, 1f);
            expandRect.pivot = new Vector2(0.5f, 1f);
            expandRect.anchoredPosition = new Vector2(0f, -LumiTheme.COLOR_ROW_HEIGHT);
            expandRect.sizeDelta = new Vector2(0f, LumiTheme.COLOR_EXPAND_HEIGHT);

            VerticalLayoutGroup expandGroup = expand.AddComponent<VerticalLayoutGroup>();
            expandGroup.spacing = LumiTheme.SPACING;
            expandGroup.childAlignment = TextAnchor.UpperLeft;
            expandGroup.childControlWidth = true;
            expandGroup.childControlHeight = true;
            expandGroup.childForceExpandWidth = true;
            expandGroup.childForceExpandHeight = false;

            Slider red = CreateChannel("Channel_R", expand.transform, "R");
            Slider green = CreateChannel("Channel_G", expand.transform, "G");
            Slider blue = CreateChannel("Channel_B", expand.transform, "B");

            // Seis muestras de la paleta (GDD línea 599). El color de cada botón vive en su
            // propia Image: es de ahí de donde lo lee el widget.
            GameObject palette = CreateUIObject("Palette", expand.transform);
            LayoutElement paletteLayout = palette.AddComponent<LayoutElement>();
            paletteLayout.minHeight = LumiTheme.COLOR_PALETTE_HEIGHT;
            paletteLayout.preferredHeight = LumiTheme.COLOR_PALETTE_HEIGHT;

            HorizontalLayoutGroup paletteGroup = palette.AddComponent<HorizontalLayoutGroup>();
            paletteGroup.spacing = LumiTheme.SPACING * 0.5f;
            paletteGroup.childAlignment = TextAnchor.MiddleLeft;
            paletteGroup.childControlWidth = true;
            paletteGroup.childControlHeight = true;
            paletteGroup.childForceExpandWidth = true;
            paletteGroup.childForceExpandHeight = true;

            Color[] presets =
            {
                LumiTheme.LumiCyan, LumiTheme.LumiMagenta, LumiTheme.LumiViolet,
                LumiTheme.SignalAmber, LumiTheme.SignalGreen, LumiTheme.TextPrimary
            };

            Button[] paletteButtons = new Button[presets.Length];
            for (int i = 0; i < presets.Length; i++)
            {
                Image preset = CreateImage($"Preset_{i}", palette.transform, presets[i], SPRITE_RECT_R4, Image.Type.Sliced);
                Button presetButton = preset.gameObject.AddComponent<Button>();
                presetButton.targetGraphic = preset;
                paletteButtons[i] = presetButton;
            }

            ColorParameterWidget widget = root.AddComponent<ColorParameterWidget>();
            SerializedObject serialized = new SerializedObject(widget);
            serialized.FindProperty("_label").objectReferenceValue = label;
            serialized.FindProperty("_swatchButton").objectReferenceValue = swatchButton;
            serialized.FindProperty("_swatchImage").objectReferenceValue = swatch;
            serialized.FindProperty("_expandRoot").objectReferenceValue = expand;
            serialized.FindProperty("_redSlider").objectReferenceValue = red;
            serialized.FindProperty("_greenSlider").objectReferenceValue = green;
            serialized.FindProperty("_blueSlider").objectReferenceValue = blue;
            serialized.FindProperty("_layout").objectReferenceValue = layout;

            SerializedProperty buttons = serialized.FindProperty("_paletteButtons");
            buttons.arraySize = paletteButtons.Length;
            for (int i = 0; i < paletteButtons.Length; i++)
            {
                buttons.GetArrayElementAtIndex(i).objectReferenceValue = paletteButtons[i];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();

            expand.SetActive(false);

            return root;
        }

        /// <summary>Fila de un canal: la letra a la izquierda y el slider ocupando el resto.</summary>
        private static Slider CreateChannel(string name, Transform parent, string channel)
        {
            GameObject row = CreateUIObject(name, parent);
            LayoutElement layout = row.AddComponent<LayoutElement>();
            layout.minHeight = LumiTheme.COLOR_CHANNEL_HEIGHT;
            layout.preferredHeight = LumiTheme.COLOR_CHANNEL_HEIGHT;

            TextMeshProUGUI letter = CreateText(
                "Letter", row.transform, FontFamily.Mono, LumiTheme.TEXT_MONO, LumiTheme.TextSecondary, TextAlignmentOptions.MidlineLeft);
            RectTransform letterRect = letter.rectTransform;
            letterRect.anchorMin = new Vector2(0f, 0f);
            letterRect.anchorMax = new Vector2(0f, 1f);
            letterRect.pivot = new Vector2(0f, 0.5f);
            letterRect.anchoredPosition = Vector2.zero;
            letterRect.sizeDelta = new Vector2(LumiTheme.COLOR_CHANNEL_LABEL_WIDTH, 0f);
            letter.text = channel;

            Slider slider = CreateSlider("Slider", row.transform);
            RectTransform sliderRect = slider.GetComponent<RectTransform>();
            sliderRect.anchorMin = new Vector2(0f, 0.5f);
            sliderRect.anchorMax = new Vector2(1f, 0.5f);
            sliderRect.offsetMin =
                new Vector2(LumiTheme.COLOR_CHANNEL_LABEL_WIDTH, -LumiTheme.SLIDER_HANDLE_SIZE * 0.5f);
            sliderRect.offsetMax = new Vector2(0f, LumiTheme.SLIDER_HANDLE_SIZE * 0.5f);

            return slider;
        }

        // ── Prefab del widget de Boolean ───────────────────────────────────────────────

        /// <summary>
        /// Interruptor de pista 36×20 con manija redonda. El Toggle de uGUI aporta el clic y el
        /// estado; el color y el lado de la manija los pinta ToggleParameterWidget.
        /// </summary>
        private static GameObject BuildToggleWidget()
        {
            GameObject root = CreateRow("Widget_Toggle", LumiTheme.TOGGLE_ROW_HEIGHT, out _);
            TextMeshProUGUI label = CreateRowLabel(
                root.transform, LumiTheme.TOGGLE_TRACK_WIDTH + LumiTheme.SPACING, LumiTheme.TOGGLE_ROW_HEIGHT);

            Image track = CreateImage("Track", root.transform, LumiTheme.Border, SPRITE_PILL, Image.Type.Sliced);
            RectTransform trackRect = track.rectTransform;
            trackRect.anchorMin = new Vector2(1f, 0.5f);
            trackRect.anchorMax = new Vector2(1f, 0.5f);
            trackRect.pivot = new Vector2(1f, 0.5f);
            trackRect.anchoredPosition = Vector2.zero;
            trackRect.sizeDelta = new Vector2(LumiTheme.TOGGLE_TRACK_WIDTH, LumiTheme.TOGGLE_TRACK_HEIGHT);

            Image handle = CreateImage("Handle", track.transform, LumiTheme.TextMuted, SPRITE_CIRCLE, Image.Type.Simple);
            RectTransform handleRect = handle.rectTransform;
            handleRect.anchorMin = new Vector2(0f, 0.5f);
            handleRect.anchorMax = new Vector2(0f, 0.5f);
            handleRect.pivot = new Vector2(0f, 0.5f);
            handleRect.sizeDelta = new Vector2(LumiTheme.TOGGLE_HANDLE_SIZE, LumiTheme.TOGGLE_HANDLE_SIZE);
            handleRect.anchoredPosition =
                new Vector2((LumiTheme.TOGGLE_TRACK_HEIGHT - LumiTheme.TOGGLE_HANDLE_SIZE) * 0.5f, 0f);
            handle.raycastTarget = false;

            // Sin checkmark y sin transición propia: el aspecto lo lleva el widget.
            Toggle toggle = track.gameObject.AddComponent<Toggle>();
            toggle.targetGraphic = track;
            toggle.graphic = null;
            toggle.toggleTransition = Toggle.ToggleTransition.None;
            toggle.SetIsOnWithoutNotify(false);

            ToggleParameterWidget widget = root.AddComponent<ToggleParameterWidget>();
            SerializedObject serialized = new SerializedObject(widget);
            serialized.FindProperty("_label").objectReferenceValue = label;
            serialized.FindProperty("_toggle").objectReferenceValue = toggle;
            serialized.FindProperty("_track").objectReferenceValue = track;
            serialized.FindProperty("_handle").objectReferenceValue = handleRect;
            serialized.FindProperty("_handleImage").objectReferenceValue = handle;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            return root;
        }

        // ── Prefabs del widget de Enum ─────────────────────────────────────────────────

        /// <summary>
        /// Botón de una opción del enum. Es su propio prefab porque el número de opciones lo
        /// decide el EffectParameter: el widget instancia uno por entrada.
        /// </summary>
        private static GameObject BuildEnumOption()
        {
            GameObject root = CreateUIObject("Option", null);

            Image background = root.AddComponent<Image>();
            background.color = LumiTheme.SurfaceElevated;
            background.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_RECT_R6);
            background.type = Image.Type.Sliced;

            LayoutElement layout = root.AddComponent<LayoutElement>();
            layout.minHeight = LumiTheme.ENUM_OPTION_HEIGHT;
            layout.preferredHeight = LumiTheme.ENUM_OPTION_HEIGHT;
            layout.flexibleWidth = 1f;

            Button button = root.AddComponent<Button>();
            button.targetGraphic = background;

            TextMeshProUGUI label = CreateText(
                "Label", root.transform, FontFamily.Label, LumiTheme.TEXT_LABEL, LumiTheme.TextSecondary, TextAlignmentOptions.Center);
            RectTransform labelRect = label.rectTransform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            label.text = "Opción";

            return root;
        }

        /// <summary>
        /// Etiqueta arriba y la fila de botones segmentados debajo, repartiéndose el ancho a
        /// partes iguales. El contenedor nace vacío: lo puebla el widget.
        /// </summary>
        private static GameObject BuildEnumWidget(Button optionPrefab)
        {
            GameObject root = CreateRow("Widget_Enum", LumiTheme.ENUM_ROW_HEIGHT, out _);
            TextMeshProUGUI label = CreateRowLabel(root.transform, 0f, LumiTheme.WIDGET_LABEL_HEIGHT);

            GameObject options = CreateUIObject("Options", root.transform);
            RectTransform optionsRect = options.GetComponent<RectTransform>();
            optionsRect.anchorMin = new Vector2(0f, 0f);
            optionsRect.anchorMax = new Vector2(1f, 0f);
            optionsRect.pivot = new Vector2(0.5f, 0f);
            optionsRect.offsetMin = Vector2.zero;
            optionsRect.offsetMax = new Vector2(0f, LumiTheme.ENUM_OPTION_HEIGHT);

            HorizontalLayoutGroup group = options.AddComponent<HorizontalLayoutGroup>();
            group.spacing = LumiTheme.SPACING * 0.5f;
            group.childAlignment = TextAnchor.MiddleLeft;
            group.childControlWidth = true;
            group.childControlHeight = true;
            group.childForceExpandWidth = true;
            group.childForceExpandHeight = true;

            EnumParameterWidget widget = root.AddComponent<EnumParameterWidget>();
            SerializedObject serialized = new SerializedObject(widget);
            serialized.FindProperty("_label").objectReferenceValue = label;
            serialized.FindProperty("_optionsRoot").objectReferenceValue = optionsRect;
            serialized.FindProperty("_optionPrefab").objectReferenceValue = optionPrefab;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            return root;
        }

        // ── Prefab del panel ───────────────────────────────────────────────────────────

        /// <summary>
        /// Raíz transparente a pantalla completa con la superficie de 280 px pegada a la
        /// derecha. La raíz nunca se apaga: es quien escucha la selección.
        /// </summary>
        private static GameObject BuildPanel(
            SliderParameterWidget slider,
            ColorParameterWidget color,
            ToggleParameterWidget toggle,
            EnumParameterWidget enumWidget)
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
                "EffectName", header.transform, FontFamily.Display, LumiTheme.TEXT_H2, LumiTheme.TextPrimary, TextAlignmentOptions.MidlineLeft);
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

            Button resetButton = BuildFooter(surface.transform);

            ParameterPanelUI panel = root.AddComponent<ParameterPanelUI>();
            SerializedObject serialized = new SerializedObject(panel);
            serialized.FindProperty("_surface").objectReferenceValue = surface.gameObject;
            serialized.FindProperty("_content").objectReferenceValue = contentRect;
            serialized.FindProperty("_effectNameLabel").objectReferenceValue = effectName;
            serialized.FindProperty("_floatWidgetPrefab").objectReferenceValue = slider;
            serialized.FindProperty("_colorWidgetPrefab").objectReferenceValue = color;
            serialized.FindProperty("_toggleWidgetPrefab").objectReferenceValue = toggle;
            serialized.FindProperty("_enumWidgetPrefab").objectReferenceValue = enumWidget;
            serialized.FindProperty("_resetButton").objectReferenceValue = resetButton;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            // Arranca oculto: el panel se despliega al seleccionar (GDD línea 136).
            surface.gameObject.SetActive(false);

            return root;
        }

        /// <summary>
        /// Pie fijo al fondo del panel (GDD línea 609) con el botón Reset de la Mecánica 5.
        /// Devuelve ese botón, que es lo único que el panel necesita cablear.
        /// </summary>
        /// <remarks>
        /// Nace con un solo hijo y childForceExpandWidth: hoy Reset ocupa el ancho útil y el
        /// Copiar de LK-29 entra como segundo hijo sin tocar este layout.
        /// </remarks>
        private static Button BuildFooter(Transform surface)
        {
            GameObject footer = CreateUIObject("Footer", surface);
            RectTransform footerRect = footer.GetComponent<RectTransform>();
            footerRect.anchorMin = new Vector2(0f, 0f);
            footerRect.anchorMax = new Vector2(1f, 0f);
            footerRect.pivot = new Vector2(0.5f, 0f);
            footerRect.anchoredPosition = Vector2.zero;
            footerRect.sizeDelta = new Vector2(0f, LumiTheme.PANEL_FOOTER_HEIGHT);

            // Separador de 1 px en el canto de arriba. ignoreLayout porque el pie es un
            // HorizontalLayoutGroup y si no lo colocaría en fila junto al botón.
            Image separator = CreateImage("Separator", footer.transform, LumiTheme.Border, null, Image.Type.Simple);
            RectTransform separatorRect = separator.rectTransform;
            separatorRect.anchorMin = new Vector2(0f, 1f);
            separatorRect.anchorMax = new Vector2(1f, 1f);
            separatorRect.pivot = new Vector2(0.5f, 1f);
            separatorRect.anchoredPosition = Vector2.zero;
            separatorRect.sizeDelta = new Vector2(0f, LumiTheme.PANEL_BORDER);
            separator.raycastTarget = false;
            separator.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;

            int padding = (int)LumiTheme.PANEL_PADDING;
            HorizontalLayoutGroup group = footer.AddComponent<HorizontalLayoutGroup>();
            group.padding = new RectOffset(padding, padding, padding, padding);
            group.spacing = LumiTheme.SPACING;
            group.childAlignment = TextAnchor.MiddleCenter;
            group.childControlWidth = true;
            group.childControlHeight = true;
            group.childForceExpandWidth = true;
            group.childForceExpandHeight = false;

            return CreateButton("Reset", footer.transform, "Reset");
        }

        /// <summary>
        /// Botón secundario del GDD (líneas 562-567) como LumiButton (LK-50). Raíz = borde, hijo
        /// insertado 1 px = relleno, y el relleno es el targetGraphic.
        /// </summary>
        /// <remarks>
        /// El borde es un contorno con el centro vacío (SPR_UI_Rect_R6_Outline, LK-51): en reposo,
        /// detrás del relleno transparente sólo queda el panel. Fondo, borde y texto los pinta
        /// LumiButton por estado, así que los tres gráficos van en blanco: su CanvasRenderer
        /// multiplica con el color del Graphic.
        /// </remarks>
        private static Button CreateButton(string name, Transform parent, string label)
        {
            Image border = CreateImage(name, parent, Color.white, SPRITE_RECT_R6_OUTLINE, Image.Type.Sliced);
            GameObject root = border.gameObject;

            // LumiButton escala la raíz al presionar: con el pivot al centro, encoge hacia el centro.
            border.rectTransform.pivot = new Vector2(0.5f, 0.5f);

            LayoutElement layout = root.AddComponent<LayoutElement>();
            layout.minHeight = LumiTheme.BUTTON_HEIGHT;
            layout.preferredHeight = LumiTheme.BUTTON_HEIGHT;
            layout.flexibleWidth = 1f;

            Image fill = CreateImage("Fill", root.transform, Color.white, SPRITE_RECT_R6, Image.Type.Sliced);
            RectTransform fillRect = fill.rectTransform;
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = new Vector2(LumiTheme.BUTTON_BORDER, LumiTheme.BUTTON_BORDER);
            fillRect.offsetMax = new Vector2(-LumiTheme.BUTTON_BORDER, -LumiTheme.BUTTON_BORDER);

            // Radio 5 y no 6: el relleno va 1 px por dentro del contorno y, con el radio de su
            // sprite, dejaría una cuña de fondo en cada esquina. uGUI divide el borde del 9-slice
            // por este multiplicador. Supone que RADIUS es el radio dibujado en SPR_UI_Rect_R6.
            fill.pixelsPerUnitMultiplier = LumiTheme.RADIUS / (LumiTheme.RADIUS - LumiTheme.BUTTON_BORDER);

            TextMeshProUGUI text = CreateText(
                "Label", root.transform, FontFamily.Label, LumiTheme.TEXT_LABEL, Color.white, TextAlignmentOptions.Center);
            RectTransform textRect = text.rectTransform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            text.text = label;

            LumiButton button = root.AddComponent<LumiButton>();
            button.targetGraphic = fill;
            button.transition = Selectable.Transition.None;

            SerializedObject serialized = new SerializedObject(button);
            serialized.FindProperty("_style").enumValueIndex = (int)LumiButtonStyle.Secondary;
            serialized.FindProperty("_fill").objectReferenceValue = fill;
            serialized.FindProperty("_border").objectReferenceValue = border;
            serialized.FindProperty("_label").objectReferenceValue = text;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            return button;
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
        /// Sin los TMP Essential Resources no hay shader ni ajustes de TMP: las fuentes del pack usan
        /// TMP_SDF.shader, que viene con ellos, y los textos saldrían rosas o invisibles. La fuente por
        /// defecto es la señal de que están importados. Mejor abortar que generar un prefab mudo.
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

        /// <summary>
        /// Sin las fuentes del pack el HUD caería en LiberationSans. Aborta si falta alguna de las
        /// tres que usa el generador, nombrándolas una a una (LK-22b).
        /// </summary>
        /// <remarks>
        /// Se carga como TMP_FontAsset: otro tipo de asset en esa ruta cuenta como ausente.
        /// FONT_BODY no está: ningún texto la usa todavía.
        /// </remarks>
        private static bool RequireFonts()
        {
            string[] required = { FONT_DISPLAY, FONT_LABEL, FONT_MONO };

            List<string> missing = new List<string>();
            for (int i = 0; i < required.Length; i++)
            {
                if (AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(required[i]) == null)
                {
                    missing.Add(required[i]);
                }
            }

            if (missing.Count == 0)
            {
                return true;
            }

            for (int i = 0; i < missing.Count; i++)
            {
                Debug.LogError($"{LOG}Falta la fuente '{missing[i]}' o no es un TMP_FontAsset.");
            }

            EditorUtility.DisplayDialog(
                "LumiKit",
                $"Faltan {missing.Count} de las {required.Length} fuentes del pack y no se ha tocado nada. La consola las lista.\n\nVer docs/specs/LK-22b_PackFonts.md.",
                "Vale");
            return false;
        }

        /// <summary>
        /// Sin los sprites del pack la UI saldría sin forma. Aborta si falta alguno de los cinco
        /// obligatorios, nombrándolos uno a uno; el riel es opcional y sólo avisa.
        /// </summary>
        /// <remarks>
        /// Se carga como Sprite y no se mira el archivo: un PNG importado como Default cuenta
        /// como ausente, que es lo que es para una Image.
        /// </remarks>
        private static bool RequireSprites()
        {
            string[] required =
            {
                SPRITE_RECT_R6, SPRITE_RECT_R6_OUTLINE, SPRITE_RECT_R4,
                SPRITE_PILL, SPRITE_CIRCLE
            };

            if (AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_TRACK) == null)
            {
                Debug.Log($"{LOG}Sin '{SPRITE_TRACK}': el riel del slider sale con esquinas rectas. Es opcional.");
            }

            List<string> missing = new List<string>();
            for (int i = 0; i < required.Length; i++)
            {
                if (AssetDatabase.LoadAssetAtPath<Sprite>(required[i]) == null)
                {
                    missing.Add(required[i]);
                }
            }

            if (missing.Count == 0)
            {
                return true;
            }

            for (int i = 0; i < missing.Count; i++)
            {
                Debug.LogError($"{LOG}Falta '{missing[i]}' o no está importado como Sprite.");
            }

            EditorUtility.DisplayDialog(
                "LumiKit",
                $"Faltan {missing.Count} de los {required.Length} sprites obligatorios y no se ha tocado nada. La consola los lista.\n\nVer docs/specs/LK-51_UISpriteArt.md.",
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

        /// <summary>
        /// Image con uno de los sprites del pack, o maciza si spritePath es null. Si la ruta no
        /// carga (sólo puede pasar con el riel opcional) también sale maciza.
        /// </summary>
        private static Image CreateImage(string name, Transform parent, Color color, string spritePath, Image.Type type)
        {
            GameObject go = CreateUIObject(name, parent);
            Image image = go.AddComponent<Image>();
            image.color = color;

            if (!string.IsNullOrEmpty(spritePath))
            {
                image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
                image.type = type;
            }

            return image;
        }

        private static TextMeshProUGUI CreateText(
            string name, Transform parent, FontFamily family, float size, Color color, TextAlignmentOptions alignment)
        {
            GameObject go = CreateUIObject(name, parent);
            TextMeshProUGUI text = go.AddComponent<TextMeshProUGUI>();
            // Asignar la fuente cambia también el material al de su atlas (TextMeshProUGUI.LoadFontAsset):
            // el prefab no se queda con el de LiberationSans.
            text.font = LoadFont(family);
            text.fontSize = size;
            text.color = color;
            text.alignment = alignment;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.raycastTarget = false;
            return text;
        }

        /// <summary>Fuente de cada familia. RequireFonts() ya ha comprobado que existen.</summary>
        private static TMP_FontAsset LoadFont(FontFamily family)
        {
            switch (family)
            {
                case FontFamily.Display:
                    return AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_DISPLAY);
                case FontFamily.Mono:
                    return AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_MONO);
                case FontFamily.Label:
                default:
                    return AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_LABEL);
            }
        }
    }
}
