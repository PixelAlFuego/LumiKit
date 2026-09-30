using System.Collections.Generic;
using LumiKit.Demo;
using LumiKit.Systems;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

namespace LumiKit.Editor
{
    /// <summary>
    /// Generador del prefab de audio de interfaz (LK-23) y de su montaje en la escena abierta.
    /// D-002: el prefab no se escribe a mano, se produce desde aquí.
    /// </summary>
    /// <remarks>
    /// El mixer no sale de aquí: lo crea el usuario (D-011), porque Unity no tiene API pública
    /// para crearlo. Este script sólo lo lee y aborta si falta.
    /// No sobrescribe nada: si el prefab ya existe, aborta. Para regenerar hay que borrarlo a mano.
    /// </remarks>
    public static class UIAudioBuilder
    {
        private const string PREFAB_PATH = "Assets/LumiKit/Prefabs/Systems/PRF_UIAudioManager.prefab";
        private const string MIXER_PATH = "Assets/LumiKit/Audio/Mixers/AMX_LumiKit.mixer";
        private const string HOVER_PATH = "Assets/LumiKit/Audio/SFX/SFX_UI_Hover.wav";
        private const string CLICK_PATH = "Assets/LumiKit/Audio/SFX/SFX_UI_Click.wav";
        private const string SELECT_PATH = "Assets/LumiKit/Audio/SFX/SFX_UI_Select.wav";
        private const string MUSIC_PATH = "Assets/LumiKit/Audio/Music/MUS_Ambient_Loop.mp3";

        private const string UI_GROUP = "UI";
        private const string MUSIC_GROUP = "Music";
        private const string MANAGER_NAME = "UIAudioManager";
        private const string LOG = "[LumiKit] ";

        // ── Menús ──────────────────────────────────────────────────────────────────────

        [MenuItem("LumiKit/Audio/Generar prefab de audio (LK-23)", false, 200)]
        public static void GeneratePrefab()
        {
            if (AssetDatabase.LoadAssetAtPath<Object>(PREFAB_PATH) != null)
            {
                Abort($"Ya existe '{PREFAB_PATH}' y no se ha tocado. Bórralo a mano para regenerar.");
                return;
            }

            AudioMixer mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(MIXER_PATH);
            if (mixer == null)
            {
                Abort($"No existe '{MIXER_PATH}'. Lo crea el usuario a mano (D-011).");
                return;
            }

            AudioMixerGroup uiGroup = FindGroup(mixer, UI_GROUP);
            AudioMixerGroup musicGroup = FindGroup(mixer, MUSIC_GROUP);
            if (uiGroup == null || musicGroup == null)
            {
                Abort($"'{MIXER_PATH}' necesita los grupos 'Master/{UI_GROUP}' y 'Master/{MUSIC_GROUP}'.");
                return;
            }

            AudioClip hover = AssetDatabase.LoadAssetAtPath<AudioClip>(HOVER_PATH);
            AudioClip click = AssetDatabase.LoadAssetAtPath<AudioClip>(CLICK_PATH);
            AudioClip select = AssetDatabase.LoadAssetAtPath<AudioClip>(SELECT_PATH);
            AudioClip music = AssetDatabase.LoadAssetAtPath<AudioClip>(MUSIC_PATH);

            List<string> missing = new List<string>();
            AddIfMissing(missing, hover, HOVER_PATH);
            AddIfMissing(missing, click, CLICK_PATH);
            AddIfMissing(missing, select, SELECT_PATH);
            AddIfMissing(missing, music, MUSIC_PATH);
            if (missing.Count > 0)
            {
                Abort($"Faltan clips: {string.Join(", ", missing)}");
                return;
            }

            GameObject root = new GameObject(MANAGER_NAME);

            AudioSource sfx = root.AddComponent<AudioSource>();
            sfx.playOnAwake = false;
            sfx.loop = false;
            sfx.spatialBlend = 0f;
            sfx.outputAudioMixerGroup = uiGroup;

            // Sin playOnAwake: sólo la instancia que sobrevive llama a Play (UIAudioManager).
            AudioSource musicSource = root.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = true;
            musicSource.spatialBlend = 0f;
            musicSource.clip = music;
            musicSource.outputAudioMixerGroup = musicGroup;

            UIAudioManager manager = root.AddComponent<UIAudioManager>();
            SerializedObject serialized = new SerializedObject(manager);
            serialized.FindProperty("_sfxSource").objectReferenceValue = sfx;
            serialized.FindProperty("_musicSource").objectReferenceValue = musicSource;
            serialized.FindProperty("_hoverClip").objectReferenceValue = hover;
            serialized.FindProperty("_clickClip").objectReferenceValue = click;
            serialized.FindProperty("_selectClip").objectReferenceValue = select;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            GameObject asset = PrefabUtility.SaveAsPrefabAsset(root, PREFAB_PATH);
            Object.DestroyImmediate(root);

            if (asset == null)
            {
                Debug.LogError($"{LOG}No se pudo guardar '{PREFAB_PATH}'.");
                return;
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"{LOG}Generado '{PREFAB_PATH}'. Sin verificar en el editor.");
        }

        [MenuItem("LumiKit/Audio/Montar audio en la escena abierta (LK-23)", false, 201)]
        public static void MountInScene()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PATH);
            if (prefab == null)
            {
                Debug.LogError($"{LOG}No existe '{PREFAB_PATH}'. Ejecuta antes 'Generar prefab de audio'.");
                return;
            }

            // Todas las comprobaciones antes de tocar la escena: o se monta todo o nada.
            if (Object.FindFirstObjectByType<UIAudioManager>() != null)
            {
                Debug.LogWarning($"{LOG}La escena ya tiene un UIAudioManager. No se monta nada.");
                return;
            }

            ObjectSelector selector = Object.FindFirstObjectByType<ObjectSelector>();
            if (selector == null)
            {
                Debug.LogWarning($"{LOG}No hay ObjectSelector en la escena. No se monta nada.");
                return;
            }

            if (Object.FindFirstObjectByType<UISelectionSound>() != null)
            {
                Debug.LogWarning($"{LOG}La escena ya tiene un UISelectionSound. No se monta nada.");
                return;
            }

            Scene scene = SceneManager.GetActiveScene();

            // En la raíz: DontDestroyOnLoad no funciona en un hijo.
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            Undo.RegisterCreatedObjectUndo(instance, "Montar audio LumiKit");

            UISelectionSound selectionSound = Undo.AddComponent<UISelectionSound>(selector.gameObject);
            SerializedObject serialized = new SerializedObject(selectionSound);
            serialized.FindProperty("_selector").objectReferenceValue = selector;
            serialized.ApplyModifiedProperties();

            EditorSceneManager.MarkSceneDirty(scene);
            Selection.activeGameObject = instance;

            Debug.Log($"{LOG}Audio montado en '{scene.name}'. La escena NO se ha guardado: revísala y guárdala tú.");
        }

        // ── Utilidades ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Grupo hijo directo de Master con ese nombre exacto, o null. Se filtra por nombre
        /// porque no está confirmado si FindMatchingGroups compara la ruta por prefijo.
        /// </summary>
        private static AudioMixerGroup FindGroup(AudioMixer mixer, string groupName)
        {
            AudioMixerGroup[] groups = mixer.FindMatchingGroups($"Master/{groupName}");
            for (int i = 0; i < groups.Length; i++)
            {
                if (groups[i].name == groupName)
                {
                    return groups[i];
                }
            }

            return null;
        }

        private static void AddIfMissing(List<string> missing, AudioClip clip, string path)
        {
            if (clip == null)
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
