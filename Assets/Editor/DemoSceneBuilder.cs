using System.Collections.Generic;
using LumiKit.Core;
using LumiKit.Demo;
using LumiKit.Systems;
using LumiKit.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace LumiKit.Editor
{
    /// <summary>
    /// Generador de la escena 02_Demo_2D (LK-14): la que se enseña y con la que arranca el build.
    /// D-002: la escena no se monta a mano, sale de aquí.
    /// </summary>
    /// <remarks>
    /// Regenera desde cero: abre la escena, destruye sus raíces y la reconstruye, así que lo hecho a
    /// mano en ella se pierde. Única excepción a "las escenas las guarda el usuario": ésta la guarda
    /// el propio generador (D-013). Sin diálogos: el verificador lo lanza por Unity MCP.
    /// El HUD y el audio no se duplican aquí: los montan ParameterPanelBuilder y UIAudioBuilder.
    /// </remarks>
    public static class DemoSceneBuilder
    {
        private const string LOG = "[LumiKit] ";

        private const string SCENE_PATH = "Assets/LumiKit/Scenes/02_Demo_2D.unity";
        private const string CRYSTAL_SPRITE_PATH = "Assets/LumiKit/Sprites/SPR_Crystal.png";
        private const string RUNE_SPRITE_PATH = "Assets/LumiKit/Sprites/SPR_RuneCoin.png";
        private const string OUTLINE_MATERIAL_PATH = "Assets/LumiKit/Materials/2D/MAT_Outline2D_Default.mat";
        private const string OUTLINE_DEFINITION_PATH = "Assets/LumiKit/Runtime/Data/Effects/EFF_Outline2D.asset";
        private const string PANEL_PREFAB_PATH = "Assets/LumiKit/Prefabs/UI/PRF_ParameterPanel.prefab";
        private const string AUDIO_PREFAB_PATH = "Assets/LumiKit/Prefabs/Systems/PRF_UIAudioManager.prefab";
        // Familia Label (ui-style.md > Tipografía), la misma que hornea ParameterPanelBuilder.
        private const string HINT_FONT_PATH = "Assets/LumiKit/Fonts/Inter-Medium SDF.asset";

        private const string SELECTABLE_LAYER = "Selectable";
        private const string UI_LAYER = "UI";
        private const string CANVAS_NAME = "UI_Root";
        private const string CRYSTAL_NAME = "SPR_Crystal";
        private const string RUNE_NAME = "SPR_RuneCoin";
        private const string SYSTEMS_NAME = "Systems";

        // Composición de partida: se ajusta aquí y se regenera. Con Size 2,5 los dos sprites
        // (2 u cada uno) caben a la izquierda del panel a 16:9 y 16:10.
        private const float CAMERA_SIZE = 2.5f;
        private const float CAMERA_Z = -10f;
        private const float MIN_CAMERA_SIZE = 1.5f;
        private const float MAX_CAMERA_SIZE = 5f;
        private const float OBJECT_OFFSET_X = 1.5f;
        private static readonly Rect CameraBounds = new Rect(-4f, -3f, 8f, 6f);

        // Temporal (usuario, Sesión 12): sin él, quien abra el build no sabe que existe TAB. Se quita
        // cuando llegue el indicador de controles del GDD (línea 137). "·" está en el atlas (183).
        private const string CONTROLS_HINT_NAME = "ControlsHint";
        private const string CONTROLS_HINT_TEXT = "Clic: seleccionar · Clic derecho: mover · Rueda: zoom · TAB: comparar";

        // ── Menús ──────────────────────────────────────────────────────────────────────

        [MenuItem("LumiKit/Escenas/Generar 02_Demo_2D (LK-14)", false, 400)]
        public static void Generate()
        {
            // Todas las comprobaciones antes de tocar nada: o se regenera todo o nada.
            if (!CanOpenScene())
            {
                return;
            }

            DemoAssets check = DemoAssets.Load();
            List<string> missing = check.FindMissing();
            AddIfMissing(missing, AssetDatabase.LoadAssetAtPath<GameObject>(PANEL_PREFAB_PATH), PANEL_PREFAB_PATH);
            AddIfMissing(missing, AssetDatabase.LoadAssetAtPath<GameObject>(AUDIO_PREFAB_PATH), AUDIO_PREFAB_PATH);
            AddIfMissing(missing, AssetDatabase.LoadAssetAtPath<SceneAsset>(SCENE_PATH), SCENE_PATH);
            if (missing.Count > 0)
            {
                Abort($"Faltan assets: {string.Join(", ", missing)}. No se ha tocado nada.");
                return;
            }

            // Sin forma física, el collider saldría vacío y el objeto no se podría seleccionar.
            if (check.Crystal.GetPhysicsShapeCount() == 0 || check.Rune.GetPhysicsShapeCount() == 0)
            {
                Abort("Un sprite no tiene forma física (Generate Physics Shape en su import). No se ha tocado nada.");
                return;
            }

            int selectableLayer = LayerMask.NameToLayer(SELECTABLE_LAYER);
            if (selectableLayer < 0)
            {
                Abort($"No existe la capa '{SELECTABLE_LAYER}' (Tags and Layers). No se ha tocado nada.");
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(SCENE_PATH, OpenSceneMode.Single);
            int removed = ClearRoots(scene);
            DemoAssets assets = DemoAssets.Load();

            Camera camera = BuildCamera();
            BuildDemoObject(CRYSTAL_NAME, assets.Crystal, -OBJECT_OFFSET_X, selectableLayer, assets.Material, assets.Definition);
            BuildDemoObject(RUNE_NAME, assets.Rune, OBJECT_OFFSET_X, selectableLayer, assets.Material, assets.Definition);
            // Antes que el HUD y el audio: los dos montajes buscan el ObjectSelector en la escena.
            ObjectSelector selector = BuildSystems(camera, selectableLayer);

            ParameterPanelBuilder.BuildSceneRig();
            UIAudioBuilder.MountInScene();
            BuildControlsHint(assets.HintFont);

            if (!CheckMounted(selector))
            {
                return;
            }

            if (!EditorSceneManager.SaveScene(scene))
            {
                Abort($"No se pudo guardar '{SCENE_PATH}'. La escena queda abierta sin guardar.");
                return;
            }

            Debug.Log(
                $"{LOG}'{SCENE_PATH}' regenerada y guardada (D-013); {removed} raíces anteriores borradas. Los avisos de " +
                "\"NO se ha guardado\" de arriba son de los montajes reutilizados. No deshagas con Ctrl+Z: relanza el menú. " +
                "Sin verificar en el editor.");
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

            // La propia 02_Demo_2D no cuenta: se regenera desde cero y sus cambios sin guardar se
            // perderían igual (D-013). Además queda marcada tras generarla: el lienzo da tamaño a su
            // RectTransform raíz en la primera actualización del editor, ya guardada la escena.
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene open = SceneManager.GetSceneAt(i);
                if (open.isDirty && open.path != SCENE_PATH)
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

        private static Camera BuildCamera()
        {
            GameObject go = new GameObject("Main Camera");
            go.tag = "MainCamera";
            go.transform.position = new Vector3(GetCameraOffsetX(), 0f, CAMERA_Z);

            Camera camera = go.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = CAMERA_SIZE;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = LumiTheme.Void;

            // Lo añade el generador y no el Inspector de URP al seleccionar la cámara: si no, la
            // escena quedaría sucia tras regenerar. Post apagado, como en la medida de color de LK-01.
            UniversalAdditionalCameraData cameraData = camera.GetUniversalAdditionalCameraData();
            cameraData.renderPostProcessing = false;

            // Sin AudioListener no suena nada (LK-23).
            go.AddComponent<AudioListener>();

            DemoCameraController controller = go.AddComponent<DemoCameraController>();
            SerializedObject serialized = new SerializedObject(controller);
            serialized.FindProperty("_bounds").rectValue = CameraBounds;
            serialized.FindProperty("_minOrthographicSize").floatValue = MIN_CAMERA_SIZE;
            serialized.FindProperty("_maxOrthographicSize").floatValue = MAX_CAMERA_SIZE;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            return camera;
        }

        /// <summary>
        /// Medio ancho del panel, en unidades de mundo: la cámara se desplaza eso a la derecha y la
        /// composición queda centrada en la zona libre a la izquierda del panel.
        /// </summary>
        /// <remarks>
        /// Con match = height (D-007) el panel ocupa PANEL_WIDTH / REFERENCE_HEIGHT de la altura de
        /// pantalla, y esa altura son 2 × Size unidades: el resultado no depende del aspecto.
        /// </remarks>
        private static float GetCameraOffsetX()
        {
            float panelWorldWidth = LumiTheme.PANEL_WIDTH / LumiTheme.REFERENCE_HEIGHT * 2f * CAMERA_SIZE;
            return panelWorldWidth * 0.5f;
        }

        private static void BuildDemoObject(
            string name, Sprite sprite, float x, int layer, Material material, EffectDefinition definition)
        {
            GameObject go = new GameObject(name);
            go.layer = layer;
            go.transform.position = new Vector3(x, 0f, 0f);

            SpriteRenderer spriteRenderer = go.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = sprite;

            // Por SerializedObject y no por sharedMaterial: D-001 sólo permite leerlo. Los dos sprites
            // comparten el material; cada uno lleva sus valores en su MaterialPropertyBlock.
            SerializedObject rendererSerialized = new SerializedObject(spriteRenderer);
            SerializedProperty materials = rendererSerialized.FindProperty("m_Materials");
            materials.arraySize = 1;
            materials.GetArrayElementAtIndex(0).objectReferenceValue = material;
            rendererSerialized.ApplyModifiedPropertiesWithoutUndo();

            PolygonCollider2D polygon = go.AddComponent<PolygonCollider2D>();
            CopyPhysicsShape(sprite, polygon);

            EffectController controller = go.AddComponent<EffectController>();
            SerializedObject serialized = new SerializedObject(controller);
            serialized.FindProperty("_targetRenderer").objectReferenceValue = spriteRenderer;
            serialized.FindProperty("_definition").objectReferenceValue = definition;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>
        /// Copia la forma física del sprite y no la deja a la generación automática del collider:
        /// con Mesh Type Full Rect (LK-01) el collider tiene que seguir la silueta, no el rectángulo.
        /// </summary>
        private static void CopyPhysicsShape(Sprite sprite, PolygonCollider2D polygon)
        {
            int count = sprite.GetPhysicsShapeCount();
            polygon.pathCount = count;

            List<Vector2> points = new List<Vector2>();
            for (int i = 0; i < count; i++)
            {
                points.Clear();
                sprite.GetPhysicsShape(i, points);
                polygon.SetPath(i, points);
            }
        }

        private static ObjectSelector BuildSystems(Camera camera, int selectableLayer)
        {
            GameObject go = new GameObject(SYSTEMS_NAME);

            ObjectSelector selector = go.AddComponent<ObjectSelector>();
            SerializedObject selectorSerialized = new SerializedObject(selector);
            selectorSerialized.FindProperty("_camera").objectReferenceValue = camera;
            selectorSerialized.FindProperty("_selectableLayers").intValue = 1 << selectableLayer;
            selectorSerialized.ApplyModifiedPropertiesWithoutUndo();

            ComparisonToggle toggle = go.AddComponent<ComparisonToggle>();
            SerializedObject toggleSerialized = new SerializedObject(toggle);
            toggleSerialized.FindProperty("_selector").objectReferenceValue = selector;
            toggleSerialized.ApplyModifiedPropertiesWithoutUndo();

            return selector;
        }

        /// <summary>
        /// Texto fijo con los controles, abajo a la izquierda y sin entrar bajo el panel. Primer hijo
        /// del lienzo: si se cruzan, el panel se pinta encima.
        /// </summary>
        /// <remarks>
        /// raycastTarget a false: si no, el cursor sobre el texto contaría como UI y bloquearía clic,
        /// rueda y paneo (IsPointerOverGameObject en LK-10 y LK-12).
        /// </remarks>
        private static void BuildControlsHint(TMP_FontAsset font)
        {
            GameObject canvas = GameObject.Find(CANVAS_NAME);
            if (canvas == null)
            {
                // CheckMounted lo da por fallo: sin lienzo tampoco hay panel.
                return;
            }

            GameObject go = new GameObject(CONTROLS_HINT_NAME, typeof(RectTransform));
            int uiLayer = LayerMask.NameToLayer(UI_LAYER);
            if (uiLayer >= 0)
            {
                go.layer = uiLayer;
            }

            go.transform.SetParent(canvas.transform, false);
            go.transform.SetAsFirstSibling();

            RectTransform rect = (RectTransform)go.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = Vector2.zero;
            rect.offsetMin = new Vector2(LumiTheme.PANEL_PADDING, LumiTheme.PANEL_PADDING);
            rect.offsetMax = new Vector2(
                -(LumiTheme.PANEL_WIDTH + LumiTheme.PANEL_PADDING),
                LumiTheme.PANEL_PADDING + LumiTheme.WIDGET_LABEL_HEIGHT);

            TextMeshProUGUI text = go.AddComponent<TextMeshProUGUI>();
            // Asignar la fuente cambia también el material al de su atlas.
            text.font = font;
            text.fontSize = LumiTheme.TEXT_LABEL;
            text.color = LumiTheme.TextSecondary;
            text.alignment = TextAlignmentOptions.BottomLeft;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.raycastTarget = false;
            text.text = CONTROLS_HINT_TEXT;
        }

        /// <summary>
        /// Los montajes reutilizados sólo avisan y vuelven si algo falla. Aquí se comprueba lo que
        /// tenían que dejar; si falta algo, la escena no se guarda.
        /// </summary>
        private static bool CheckMounted(ObjectSelector selector)
        {
            List<string> missing = new List<string>();

            ParameterPanelUI panel = Object.FindFirstObjectByType<ParameterPanelUI>(FindObjectsInactive.Include);
            if (panel == null)
            {
                missing.Add("ParameterPanelUI");
            }
            else if (new SerializedObject(panel).FindProperty("_selector").objectReferenceValue != selector)
            {
                missing.Add("'_selector' del panel");
            }

            if (Object.FindFirstObjectByType<UIAudioManager>(FindObjectsInactive.Include) == null)
            {
                missing.Add("UIAudioManager");
            }

            if (selector.GetComponent<UISelectionSound>() == null)
            {
                missing.Add("UISelectionSound");
            }

            if (GameObject.Find(CONTROLS_HINT_NAME) == null)
            {
                missing.Add(CONTROLS_HINT_NAME);
            }

            EffectController[] controllers = Object.FindObjectsByType<EffectController>(FindObjectsSortMode.None);
            for (int i = 0; i < controllers.Length; i++)
            {
                if (controllers[i].Definition == null)
                {
                    missing.Add($"EffectDefinition en '{controllers[i].name}'");
                }
            }

            if (missing.Count == 0)
            {
                return true;
            }

            Abort($"Falta {string.Join(", ", missing)}. La escena queda abierta y SIN guardar; revisa los avisos de arriba.");
            return false;
        }

        // ── Utilidades ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Assets con los que se construye la escena. Se cargan dos veces: antes de abrirla, para
        /// abortar sin tocar nada si falta alguno, y después, para construir.
        /// </summary>
        /// <remarks>
        /// OpenScene en modo Single descarga los assets que nada referencia y lo cargado antes puede
        /// quedar destruido: la primera verificación guardó _definition vacío así (causa probable, sin
        /// confirmar). El material y los sprites salieron bien, pero se recargan todos por igual.
        /// </remarks>
        private sealed class DemoAssets
        {
            public Sprite Crystal { get; private set; }
            public Sprite Rune { get; private set; }
            public Material Material { get; private set; }
            public EffectDefinition Definition { get; private set; }
            public TMP_FontAsset HintFont { get; private set; }

            public static DemoAssets Load()
            {
                return new DemoAssets
                {
                    Crystal = AssetDatabase.LoadAssetAtPath<Sprite>(CRYSTAL_SPRITE_PATH),
                    Rune = AssetDatabase.LoadAssetAtPath<Sprite>(RUNE_SPRITE_PATH),
                    Material = AssetDatabase.LoadAssetAtPath<Material>(OUTLINE_MATERIAL_PATH),
                    Definition = AssetDatabase.LoadAssetAtPath<EffectDefinition>(OUTLINE_DEFINITION_PATH),
                    HintFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(HINT_FONT_PATH)
                };
            }

            public List<string> FindMissing()
            {
                List<string> missing = new List<string>();
                AddIfMissing(missing, Crystal, CRYSTAL_SPRITE_PATH);
                AddIfMissing(missing, Rune, RUNE_SPRITE_PATH);
                AddIfMissing(missing, Material, OUTLINE_MATERIAL_PATH);
                AddIfMissing(missing, Definition, OUTLINE_DEFINITION_PATH);
                AddIfMissing(missing, HintFont, HINT_FONT_PATH);
                return missing;
            }
        }

        private static void AddIfMissing(List<string> missing, Object asset, string path)
        {
            if (asset == null)
            {
                missing.Add(path);
            }
        }

        /// <summary>
        /// Sólo consola, sin diálogo modal: el menú también lo lanza el verificador por Unity MCP,
        /// y un diálogo bloquearía el editor hasta que alguien lo cerrara.
        /// </summary>
        private static void Abort(string message)
        {
            Debug.LogError($"{LOG}{message}");
        }
    }
}
