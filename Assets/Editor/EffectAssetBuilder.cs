using LumiKit.Core;
using LumiKit.UI;
using UnityEditor;
using UnityEngine;

namespace LumiKit.Editor
{
    /// <summary>
    /// Generador de los assets de cada efecto 2D: el material por defecto (MAT_) y su
    /// EffectDefinition (EFF_). D-002: no se escriben a mano, se producen desde aquí.
    /// </summary>
    /// <remarks>
    /// A diferencia de los generadores de prefabs, si el asset ya existe lo actualiza en su sitio y
    /// conserva el GUID: TestBench y LK-14 los referencian, y regenerar obligaría a borrarlos.
    /// El material toma los valores por defecto del propio shader, sin SetFloat ni SetColor.
    /// Sin diálogos: el verificador lo lanza por Unity MCP. LK-03 y LK-02 añaden aquí su menú.
    /// </remarks>
    public static class EffectAssetBuilder
    {
        private const string LOG = "[LumiKit] ";

        private const string OUTLINE_SHADER_PATH = "Assets/LumiKit/Shaders/2D/SH_Outline2D.shader";
        private const string OUTLINE_MATERIAL_PATH = "Assets/LumiKit/Materials/2D/MAT_Outline2D_Default.mat";
        private const string OUTLINE_DEFINITION_PATH = "Assets/LumiKit/Runtime/Data/Effects/EFF_Outline2D.asset";

        private const string GLOW_SHADER_PATH = "Assets/LumiKit/Shaders/2D/SH_Glow2D.shader";
        private const string GLOW_MATERIAL_PATH = "Assets/LumiKit/Materials/2D/MAT_Glow2D_Default.mat";
        private const string GLOW_DEFINITION_PATH = "Assets/LumiKit/Runtime/Data/Effects/EFF_Glow2D.asset";

        // ── Menús ──────────────────────────────────────────────────────────────────────

        [MenuItem("LumiKit/Efectos/Generar Outline 2D (LK-01)", false, 300)]
        public static void GenerateOutline2D()
        {
            Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(OUTLINE_SHADER_PATH);
            if (shader == null)
            {
                Debug.LogError($"{LOG}No existe '{OUTLINE_SHADER_PATH}'. No se genera nada.");
                return;
            }

            bool materialCreated = CreateOrUpdateMaterial(shader, OUTLINE_MATERIAL_PATH);

            bool definitionCreated;
            EffectDefinition definition = LoadOrCreate(OUTLINE_DEFINITION_PATH, out definitionCreated);
            SerializedObject serialized = new SerializedObject(definition);
            SetIdentity(
                serialized, shader,
                "Contorno 2D", "Outline 2D",
                "Contorno de color alrededor de la silueta del sprite.", "Colored outline around the sprite silhouette.");

            SerializedProperty parameters = serialized.FindProperty("_parameters");
            parameters.arraySize = 3;
            SetParameter(
                parameters.GetArrayElementAtIndex(0), "Color del contorno", "Outline color", "_OutlineColor",
                ParameterType.Color, 0f, 1f, 0f, LumiTheme.LumiCyan, 0, new string[0]);
            SetParameter(
                parameters.GetArrayElementAtIndex(1), "Grosor", "Width", "_OutlineWidth",
                ParameterType.Float, 0f, 10f, 4f, Color.white, 0, new string[0]);
            SetParameter(
                parameters.GetArrayElementAtIndex(2), "Modo", "Mode", "_OutlineMode",
                ParameterType.Enum, 0f, 1f, 0f, Color.white, 0, new[] { "Sólido", "Punteado", "Animado" });

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
            AssetDatabase.SaveAssets();

            Debug.Log(
                $"{LOG}Outline 2D: '{OUTLINE_MATERIAL_PATH}' {Verb(materialCreated)} y '{OUTLINE_DEFINITION_PATH}' " +
                $"{Verb(definitionCreated)}. Sin verificar en el editor.");
        }

        [MenuItem("LumiKit/Efectos/Generar Glow 2D (LK-03)", false, 301)]
        public static void GenerateGlow2D()
        {
            Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(GLOW_SHADER_PATH);
            if (shader == null)
            {
                Debug.LogError($"{LOG}No existe '{GLOW_SHADER_PATH}'. No se genera nada.");
                return;
            }

            bool materialCreated = CreateOrUpdateMaterial(shader, GLOW_MATERIAL_PATH);

            bool definitionCreated;
            EffectDefinition definition = LoadOrCreate(GLOW_DEFINITION_PATH, out definitionCreated);
            SerializedObject serialized = new SerializedObject(definition);
            SetIdentity(
                serialized, shader,
                "Brillo 2D", "Glow 2D",
                "Brillo interior y exterior alrededor de la silueta, con pulso opcional.",
                "Inner and outer glow around the silhouette, with optional pulse.");

            // SetParameter escribe _defaultBool = false: coincide con _PulseEnabled = 0 del shader.
            SerializedProperty parameters = serialized.FindProperty("_parameters");
            parameters.arraySize = 4;
            SetParameter(
                parameters.GetArrayElementAtIndex(0), "Color del brillo", "Glow color", "_GlowColor",
                ParameterType.Color, 0f, 1f, 0f, LumiTheme.LumiViolet, 0, new string[0]);
            SetParameter(
                parameters.GetArrayElementAtIndex(1), "Intensidad", "Intensity", "_GlowIntensity",
                ParameterType.Float, 0f, 5f, 1.5f, Color.white, 0, new string[0]);
            SetParameter(
                parameters.GetArrayElementAtIndex(2), "Pulso", "Pulse", "_PulseEnabled",
                ParameterType.Boolean, 0f, 1f, 0f, Color.white, 0, new string[0]);
            SetParameter(
                parameters.GetArrayElementAtIndex(3), "Velocidad del pulso", "Pulse speed", "_PulseSpeed",
                ParameterType.Float, 0f, 3f, 1f, Color.white, 0, new string[0]);

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
            AssetDatabase.SaveAssets();

            Debug.Log(
                $"{LOG}Glow 2D: '{GLOW_MATERIAL_PATH}' {Verb(materialCreated)} y '{GLOW_DEFINITION_PATH}' " +
                $"{Verb(definitionCreated)}. Sin verificar en el editor.");
        }

        // ── Utilidades ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Crea el material con los valores por defecto del shader, o, si existe, le pone el shader y
        /// le devuelve esos valores copiándolos de un material temporal. Devuelve true si lo creó.
        /// </summary>
        private static bool CreateOrUpdateMaterial(Shader shader, string path)
        {
            Material fresh = new Material(shader);
            Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (existing == null)
            {
                AssetDatabase.CreateAsset(fresh, path);
                return true;
            }

            existing.shader = shader;
            existing.CopyPropertiesFromMaterial(fresh);
            Object.DestroyImmediate(fresh);
            EditorUtility.SetDirty(existing);
            return false;
        }

        private static EffectDefinition LoadOrCreate(string path, out bool created)
        {
            EffectDefinition definition = AssetDatabase.LoadAssetAtPath<EffectDefinition>(path);
            created = definition == null;
            if (created)
            {
                definition = ScriptableObject.CreateInstance<EffectDefinition>();
                AssetDatabase.CreateAsset(definition, path);
            }

            return definition;
        }

        private static void SetIdentity(
            SerializedObject serialized, Shader shader,
            string nameEs, string nameEn, string descriptionEs, string descriptionEn)
        {
            serialized.FindProperty("_displayNameEs").stringValue = nameEs;
            serialized.FindProperty("_displayNameEn").stringValue = nameEn;
            serialized.FindProperty("_descriptionEs").stringValue = descriptionEs;
            serialized.FindProperty("_descriptionEn").stringValue = descriptionEn;
            serialized.FindProperty("_shader").objectReferenceValue = shader;
        }

        /// <summary>
        /// Escribe todos los campos del parámetro, también los que su tipo no usa: al actualizar en
        /// su sitio no queda ningún valor viejo.
        /// </summary>
        private static void SetParameter(
            SerializedProperty parameter, string nameEs, string nameEn, string propertyName,
            ParameterType type, float min, float max, float defaultFloat, Color defaultColor,
            int defaultEnumIndex, string[] enumOptions)
        {
            parameter.FindPropertyRelative("_displayNameEs").stringValue = nameEs;
            parameter.FindPropertyRelative("_displayNameEn").stringValue = nameEn;
            parameter.FindPropertyRelative("_propertyName").stringValue = propertyName;
            parameter.FindPropertyRelative("_type").enumValueIndex = (int)type;
            parameter.FindPropertyRelative("_minValue").floatValue = min;
            parameter.FindPropertyRelative("_maxValue").floatValue = max;
            parameter.FindPropertyRelative("_defaultFloat").floatValue = defaultFloat;
            parameter.FindPropertyRelative("_defaultColor").colorValue = defaultColor;
            parameter.FindPropertyRelative("_defaultBool").boolValue = false;
            parameter.FindPropertyRelative("_defaultEnumIndex").intValue = defaultEnumIndex;

            SerializedProperty options = parameter.FindPropertyRelative("_enumOptions");
            options.arraySize = enumOptions.Length;
            for (int i = 0; i < enumOptions.Length; i++)
            {
                options.GetArrayElementAtIndex(i).stringValue = enumOptions[i];
            }
        }

        private static string Verb(bool created)
        {
            return created ? "creado" : "actualizado";
        }
    }
}
