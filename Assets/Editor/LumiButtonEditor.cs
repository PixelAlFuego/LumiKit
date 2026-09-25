using LumiKit.UI;
using UnityEditor;
using UnityEditor.UI;

namespace LumiKit.Editor
{
    /// <summary>
    /// Inspector de LumiButton: el de Button y, debajo, los cuatro campos propios (LK-50).
    /// </summary>
    /// <remarks>
    /// ButtonEditor está declarado [CustomEditor(typeof(Button), true)] (ButtonEditor.cs línea 5)
    /// y dibuja también las clases hijas, así que sin este editor _style, _fill, _border y _label
    /// no se ven. Vive en Assets/Editor/, que no se exporta: el comprador no lo tendrá
    /// (STATE > Dudas abiertas, se decide en LK-27).
    /// </remarks>
    [CustomEditor(typeof(LumiButton), true)]
    [CanEditMultipleObjects]
    public class LumiButtonEditor : ButtonEditor
    {
        private SerializedProperty _style;
        private SerializedProperty _fill;
        private SerializedProperty _border;
        private SerializedProperty _label;

        protected override void OnEnable()
        {
            base.OnEnable();
            _style = serializedObject.FindProperty("_style");
            _fill = serializedObject.FindProperty("_fill");
            _border = serializedObject.FindProperty("_border");
            _label = serializedObject.FindProperty("_label");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();
            serializedObject.Update();
            EditorGUILayout.PropertyField(_style);
            EditorGUILayout.PropertyField(_fill);
            EditorGUILayout.PropertyField(_border);
            EditorGUILayout.PropertyField(_label);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
