using System.Collections.Generic;
using UnityEngine;

namespace LumiKit.Core
{
    /// <summary>
    /// Catálogo de datos de un efecto: nombre bilingüe, shader asociado y lista de
    /// parámetros configurables. La UI se construye a partir de esto (D-003), de modo
    /// que añadir un efecto nuevo no obliga a tocar código de interfaz.
    /// </summary>
    [CreateAssetMenu(fileName = "EFF_NewEffect", menuName = "LumiKit/Effect Definition")]
    public class EffectDefinition : ScriptableObject
    {
        [Header("Identidad")]
        [SerializeField] private string _displayNameEs;
        [SerializeField] private string _displayNameEn;

        [Header("Descripción")]
        [TextArea(2, 5)]
        [SerializeField] private string _descriptionEs;
        [TextArea(2, 5)]
        [SerializeField] private string _descriptionEn;

        [Header("Shader")]
        [Tooltip("Shader Graph del efecto. Debe exponer _EffectEnabled (D-005).")]
        [SerializeField] private Shader _shader;

        [Header("Parámetros expuestos a la UI")]
        [SerializeField] private EffectParameter[] _parameters = new EffectParameter[0];

        public string DisplayNameEs => _displayNameEs;
        public string DisplayNameEn => _displayNameEn;
        public string DescriptionEs => _descriptionEs;
        public string DescriptionEn => _descriptionEn;
        public Shader Shader => _shader;
        public IReadOnlyList<EffectParameter> Parameters => _parameters;
        public int ParameterCount => _parameters != null ? _parameters.Length : 0;

        /// <summary>
        /// Devuelve el parámetro cuyo PropertyName coincide, o null si no existe.
        /// </summary>
        public EffectParameter FindParameter(string propertyName)
        {
            if (_parameters == null || string.IsNullOrEmpty(propertyName))
            {
                return null;
            }

            for (int i = 0; i < _parameters.Length; i++)
            {
                EffectParameter parameter = _parameters[i];
                if (parameter != null && parameter.PropertyName == propertyName)
                {
                    return parameter;
                }
            }

            return null;
        }
    }
}
