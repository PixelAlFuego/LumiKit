using System.Collections.Generic;
using LumiKit.Systems;
using LumiKit.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LumiKit.Editor
{
    /// <summary>
    /// Generador de la escena 01_MainMenu (LK-13): el menú de inicio mínimo con el que arranca el
    /// build. D-002: la escena no se monta a mano, sale de aquí.
    /// </summary>
    /// <remarks>
    /// Como DemoSceneBuilder: regenera desde cero y guarda la escena (D-013, ampliada a ésta).
    /// Sin diálogos: el verificador lo lanza por Unity MCP. BuildSceneMenu lo usa también la demo
    /// para su botón "Menú". El botón copia la estructura de ParameterPanelBuilder.CreateButton,
    /// que es privado, para no tocar un generador cerrado.
    /// </remarks>
    public static class MainMenuSceneBuilder
    {
        private const string LOG = "[LumiKit] ";

        private const string SCENE_PATH = "Assets/LumiKit/Scenes/01_MainMenu.unity";
        private const string DEMO_SCENE_PATH = "Assets/LumiKit/Scenes/02_Demo_2D.unity";
        private const string DEMO_SCENE_NAME = "02_Demo_2D";
        private const string AUDIO_PREFAB_PATH = "Assets/LumiKit/Prefabs/Systems/PRF_UIAudioManager.prefab";

        // Familias de ui-style.md > Tipografía. Display es Bold en el GDD; el pack sólo trae Medium.
        private const string FONT_DISPLAY_PATH = "Assets/LumiKit/Fonts/SpaceGrotesk-Medium SDF.asset";
        private const string FONT_LABEL_PATH = "Assets/LumiKit/Fonts/Inter-Medium SDF.asset";
        private const string FONT_CAPTION_PATH = "Assets/LumiKit/Fonts/Inter-Regular SDF.asset";
        private const string SPRITE_RECT_R6 = "Assets/LumiKit/Sprites/UI/SPR_UI_Rect_R6.png";
        private const string SPRITE_RECT_R6_OUTLINE = "Assets/LumiKit/Sprites/UI/SPR_UI_Rect_R6_Outline.png";

        private const string CANVAS_NAME = "UI_Root";
        private const string EVENT_SYSTEM_NAME = "EventSystem";
        private const string BUTTONS_NAME = "Buttons";
        private const string CAMERA_NAME = "Main Camera";
        private const float CAMERA_Z = -10f;

        private const string TITLE_TEXT = "LumiKit";
        private const string SUBTITLE_TEXT = "Shaders 2D para Unity 6 y URP";
        private const string VERSION_TEXT = "v0.1 MVP";

        // ui-style.md: +0.02em de tracking a 12 px o menos. En TMP, 1 = 0,01 em (sin comprobar).
        private const float CAPTION_CHARACTER_SPACING = 2f;

        /// <summary>Una entrada de SceneMenu, tal como la escribe el generador.</summary>
        internal readonly struct EntrySpec
        {
            public readonly string Label;
            public readonly LumiButtonStyle Style;
            public readonly MenuAction Action;
            public readonly string SceneName;

            public EntrySpec(string label, LumiButtonStyle style, MenuAction action, string sceneName)
            {
                Label = label;
                Style = style;
                Action = action;
                SceneName = sceneName;
            }
        }

        private static readonly EntrySpec[] MainEntries =
        {
            new EntrySpec("Demo 2D", LumiButtonStyle.Primary, MenuAction.LoadScene, DEMO_SCENE_NAME),
            new EntrySpec("Salir", LumiButtonStyle.Secondary, MenuAction.Quit, string.Empty)
        };

        // ── Menús ──────────────────────────────────────────────────────────────────────

        [MenuItem("LumiKit/Escenas/Generar 01_MainMenu (LK-13)", false, 399)]
        public static void Generate()
        {
            // Todas las comprobaciones antes de tocar nada: o se regenera todo o nada.
            if (!CanOpenScene())
            {
                return;
            }

            List<string> missing = new List<string>();
            AddIfMissing(missing, AssetDatabase.LoadAssetAtPath<SceneAsset>(SCENE_PATH), SCENE_PATH);
            AddIfMissing(missing, AssetDatabase.LoadAssetAtPath<GameObject>(AUDIO_PREFAB_PATH), AUDIO_PREFAB_PATH);
            AddIfMissing(missing, AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_DISPLAY_PATH), FONT_DISPLAY_PATH);
            AddIfMissing(missing, AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_LABEL_PATH), FONT_LABEL_PATH);
            AddIfMissing(missing, AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_CAPTION_PATH), FONT_CAPTION_PATH);
            AddIfMissing(missing, AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_RECT_R6), SPRITE_RECT_R6);
            AddIfMissing(missing, AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_RECT_R6_OUTLINE), SPRITE_RECT_R6_OUTLINE);
            if (missing.Count > 0)
            {
                Abort($"Faltan assets: {string.Join(", ", missing)}. No se ha tocado nada.");
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(SCENE_PATH, OpenSceneMode.Single);
            int removed = ClearRoots(scene);

            // Los assets se cargan después de OpenScene, al usarlos: cargados antes podrían quedar
            // descargados (lección de LK-14).
            BuildCamera();
            Canvas canvas = BuildCanvas();
            BuildEventSystem();
            BuildMenuColumn(canvas.transform);
            BuildVersion(canvas.transform);

            // En la raíz: DontDestroyOnLoad no funciona en un hijo. Sin ObjectSelector en el menú,
            // UIAudioBuilder.MountInScene no sirve aquí.
            GameObject audioPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(AUDIO_PREFAB_PATH);
            PrefabUtility.InstantiatePrefab(audioPrefab, scene);

            if (!CheckBuilt())
            {
                return;
            }

            if (!EditorSceneManager.SaveScene(scene))
            {
                Abort($"No se pudo guardar '{SCENE_PATH}'. La escena queda abierta sin guardar.");
                return;
            }

            Debug.Log(
                $"{LOG}'{SCENE_PATH}' regenerada y guardada (D-013); {removed} raíces anteriores borradas. " +
                "No deshagas con Ctrl+Z: relanza el menú. Sin verificar en el editor.");
        }

        // ── Para los dos generadores de escena ─────────────────────────────────────────

        /// <summary>
        /// Las dos escenas generadas (D-013) quedan marcadas tras generarse: el lienzo da tamaño a
        /// su raíz en la primera actualización del editor. Sus cambios sin guardar no bloquean.
        /// </summary>
        internal static bool IsGeneratedScene(string path)
        {
            return path == SCENE_PATH || path == DEMO_SCENE_PATH;
        }

        /// <summary>
        /// Contenedor con un SceneMenu y su plantilla de botón (inactiva). fitToContent añade un
        /// ContentSizeFitter: sólo si el padre no es un layout group.
        /// </summary>
        internal static SceneMenu BuildSceneMenu(
            Transform parent, string name, float buttonWidth, bool fitToContent, EntrySpec[] entries)
        {
            GameObject go = CreateUIObject(name, parent);

            VerticalLayoutGroup layout = go.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.spacing = LumiTheme.SPACING;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            if (fitToContent)
            {
                ContentSizeFitter fitter = go.AddComponent<ContentSizeFitter>();
                fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }

            LumiButton template = CreateButtonTemplate(go.transform);
            template.gameObject.SetActive(false);

            SceneMenu menu = go.AddComponent<SceneMenu>();
            SerializedObject serialized = new SerializedObject(menu);
            serialized.FindProperty("_buttonTemplate").objectReferenceValue = template;
            serialized.FindProperty("_container").objectReferenceValue = (RectTransform)go.transform;
            serialized.FindProperty("_buttonWidth").floatValue = buttonWidth;

            SerializedProperty list = serialized.FindProperty("_entries");
            list.arraySize = entries.Length;
            for (int i = 0; i < entries.Length; i++)
            {
                SerializedProperty element = list.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("_label").stringValue = entries[i].Label;
                element.FindPropertyRelative("_style").enumValueIndex = (int)entries[i].Style;
                element.FindPropertyRelative("_action").enumValueIndex = (int)entries[i].Action;
                element.FindPropertyRelative("_sceneName").stringValue = entries[i].SceneName ?? string.Empty;
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            return menu;
        }

        // ── Escena ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// OpenScene en modo Single cierra lo que haya abierto sin preguntar: con cambios sin guardar
        /// se perderían, y con un prefab abierto podría salir un diálogo que colgara el MCP.
        /// </summary>
        private static bool CanOpenScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Abort("Sal de Play antes de generar la escena.");
                return false;
            }

            if (PrefabStageUtility.GetCurrentPrefabStage() != null)
            {
                Abort("Hay un prefab abierto en Prefab Mode. Ciérralo antes de generar la escena.");
                return false;
            }

            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene open = SceneManager.GetSceneAt(i);
                if (open.isDirty && !IsGeneratedScene(open.path))
                {
                    Abort($"'{open.name}' tiene cambios sin guardar. Guárdala o descártala antes de generar.");
                    return false;
                }
            }

            return true;
        }

        private static int ClearRoots(Scene scene)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                Object.DestroyImmediate(roots[i]);
            }

            return roots.Length;
        }

        private static void BuildCamera()
        {
            GameObject go = new GameObject(CAMERA_NAME);
            go.tag = "MainCamera";
            go.transform.position = new Vector3(0f, 0f, CAMERA_Z);

            Camera camera = go.AddComponent<Camera>();
            camera.orthographic = true;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = LumiTheme.Void;

            // Como en la demo: lo añade el generador para que la escena no quede sucia al
            // seleccionar la cámara, con el post apagado.
            UniversalAdditionalCameraData cameraData = camera.GetUniversalAdditionalCameraData();
            cameraData.renderPostProcessing = false;

            // Sin AudioListener no suena nada (LK-23).
            go.AddComponent<AudioListener>();
        }

        private static Canvas BuildCanvas()
        {
            GameObject go = CreateUIObject(CANVAS_NAME, null);

            Canvas canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            // D-007: 1920×1080 con match = height, como el HUD de la demo.
            CanvasScaler scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(LumiTheme.REFERENCE_WIDTH, LumiTheme.REFERENCE_HEIGHT);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 1f;

            go.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        private static void BuildEventSystem()
        {
            // D-006: módulo del Input System, nunca StandaloneInputModule.
            GameObject go = new GameObject(EVENT_SYSTEM_NAME);
            go.AddComponent<EventSystem>();
            go.AddComponent<InputSystemUIInputModule>();
        }

        /// <summary>Título, subtítulo, hueco y botones, apilados y centrados en pantalla.</summary>
        private static void BuildMenuColumn(Transform canvas)
        {
            GameObject column = CreateUIObject("MenuColumn", canvas);
            RectTransform rect = (RectTransform)column.transform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;

            VerticalLayoutGroup layout = column.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.spacing = LumiTheme.SPACING;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            ContentSizeFitter fitter = column.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            CreateText(
                "Title", column.transform, FONT_DISPLAY_PATH, LumiTheme.TEXT_DISPLAY, LumiTheme.TextPrimary,
                TextAlignmentOptions.Center, TITLE_TEXT);
            CreateText(
                "Subtitle", column.transform, FONT_LABEL_PATH, LumiTheme.TEXT_H3, LumiTheme.TextSecondary,
                TextAlignmentOptions.Center, SUBTITLE_TEXT);

            GameObject gap = CreateUIObject("Gap", column.transform);
            LayoutElement gapLayout = gap.AddComponent<LayoutElement>();
            gapLayout.minHeight = LumiTheme.MENU_SECTION_GAP;
            gapLayout.preferredHeight = LumiTheme.MENU_SECTION_GAP;

            BuildSceneMenu(column.transform, BUTTONS_NAME, LumiTheme.MENU_BUTTON_WIDTH, false, MainEntries);
        }

        /// <summary>
        /// Versión abajo a la derecha. TextSecondary y no TextMuted: TextMuted sobre Void no llega
        /// al contraste de 4.5:1 de ui-style.md.
        /// </summary>
        private static void BuildVersion(Transform canvas)
        {
            TextMeshProUGUI text = CreateText(
                "Version", canvas, FONT_CAPTION_PATH, LumiTheme.TEXT_CAPTION, LumiTheme.TextSecondary,
                TextAlignmentOptions.BottomRight, VERSION_TEXT);
            text.characterSpacing = CAPTION_CHARACTER_SPACING;

            RectTransform rect = text.rectTransform;
            rect.anchorMin = new Vector2(1f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(1f, 0f);
            rect.anchoredPosition = new Vector2(-LumiTheme.PANEL_PADDING, LumiTheme.PANEL_PADDING);

            ContentSizeFitter fitter = text.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        /// <summary>Lo que tiene que haber antes de guardar. Si falta algo, la escena no se guarda.</summary>
        private static bool CheckBuilt()
        {
            List<string> missing = new List<string>();

            SceneMenu menu = Object.FindFirstObjectByType<SceneMenu>(FindObjectsInactive.Include);
            if (menu == null)
            {
                missing.Add("SceneMenu");
            }
            else if (new SerializedObject(menu).FindProperty("_entries").arraySize != MainEntries.Length)
            {
                missing.Add("entradas del SceneMenu");
            }

            if (Object.FindFirstObjectByType<UIAudioManager>(FindObjectsInactive.Include) == null)
            {
                missing.Add("UIAudioManager");
            }

            if (Object.FindFirstObjectByType<InputSystemUIInputModule>(FindObjectsInactive.Include) == null)
            {
                missing.Add("InputSystemUIInputModule");
            }

            if (missing.Count == 0)
            {
                return true;
            }

            Abort($"Falta {string.Join(", ", missing)}. La escena queda abierta y SIN guardar.");
            return false;
        }

        // ── Botón y textos ─────────────────────────────────────────────────────────────

        /// <summary>
        /// LumiButton con la estructura de ParameterPanelBuilder.CreateButton (LK-50): raíz = borde,
        /// hijo insertado 1 px = relleno (targetGraphic), etiqueta. Gráficos en blanco: los pinta
        /// LumiButton por estado. flexibleWidth 0: el ancho lo pone SceneMenu.
        /// </summary>
        private static LumiButton CreateButtonTemplate(Transform parent)
        {
            Image border = CreateImage("ButtonTemplate", parent, SPRITE_RECT_R6_OUTLINE);
            GameObject root = border.gameObject;

            // LumiButton escala la raíz al presionar: con el pivot al centro, encoge hacia el centro.
            border.rectTransform.pivot = new Vector2(0.5f, 0.5f);

            LayoutElement layout = root.AddComponent<LayoutElement>();
            layout.minHeight = LumiTheme.BUTTON_HEIGHT;
            layout.preferredHeight = LumiTheme.BUTTON_HEIGHT;
            layout.flexibleWidth = 0f;

            Image fill = CreateImage("Fill", root.transform, SPRITE_RECT_R6);
            RectTransform fillRect = fill.rectTransform;
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = new Vector2(LumiTheme.BUTTON_BORDER, LumiTheme.BUTTON_BORDER);
            fillRect.offsetMax = new Vector2(-LumiTheme.BUTTON_BORDER, -LumiTheme.BUTTON_BORDER);
            // Radio 5 y no 6, como en el panel: el relleno va 1 px por dentro del contorno.
            fill.pixelsPerUnitMultiplier = LumiTheme.RADIUS / (LumiTheme.RADIUS - LumiTheme.BUTTON_BORDER);

            TextMeshProUGUI label = CreateText(
                "Label", root.transform, FONT_LABEL_PATH, LumiTheme.TEXT_LABEL, Color.white,
                TextAlignmentOptions.Center, string.Empty);
            RectTransform labelRect = label.rectTransform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            LumiButton button = root.AddComponent<LumiButton>();
            button.targetGraphic = fill;
            button.transition = Selectable.Transition.None;

            // Hover y clic (LK-23), en el mismo GameObject que el Selectable.
            UISoundTrigger trigger = root.AddComponent<UISoundTrigger>();
            SerializedObject triggerSerialized = new SerializedObject(trigger);
            triggerSerialized.FindProperty("_playClick").boolValue = true;
            triggerSerialized.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject serialized = new SerializedObject(button);
            serialized.FindProperty("_style").enumValueIndex = (int)LumiButtonStyle.Secondary;
            serialized.FindProperty("_fill").objectReferenceValue = fill;
            serialized.FindProperty("_border").objectReferenceValue = border;
            serialized.FindProperty("_label").objectReferenceValue = label;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            return button;
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

        private static Image CreateImage(string name, Transform parent, string spritePath)
        {
            GameObject go = CreateUIObject(name, parent);
            Image image = go.AddComponent<Image>();
            image.color = Color.white;
            image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            image.type = Image.Type.Sliced;
            return image;
        }

        private static TextMeshProUGUI CreateText(
            string name, Transform parent, string fontPath, float size, Color color,
            TextAlignmentOptions alignment, string content)
        {
            GameObject go = CreateUIObject(name, parent);
            TextMeshProUGUI text = go.AddComponent<TextMeshProUGUI>();
            // Asignar la fuente cambia también el material al de su atlas.
            text.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontPath);
            text.fontSize = size;
            text.color = color;
            text.alignment = alignment;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.raycastTarget = false;
            text.text = content;
            return text;
        }

        // ── Utilidades ─────────────────────────────────────────────────────────────────

        private static void AddIfMissing(List<string> missing, Object asset, string path)
        {
            if (asset == null)
            {
                missing.Add(path);
            }
        }

        /// <summary>Sólo consola, sin diálogo modal: lo lanza también el verificador por Unity MCP.</summary>
        private static void Abort(string message)
        {
            Debug.LogError($"{LOG}{message}");
        }
    }
}
