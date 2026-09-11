using System;
using UnityEngine;

namespace LumiKit.Core
{
    /// <summary>
    /// Describe un parámetro configurable de un efecto: a qué propiedad de shader
    /// escribe, de qué tipo es y cuál es su valor por defecto.
    /// </summary>
    /// <remarks>
    /// PropertyName debe coincidir exactamente con el Reference de la propiedad en
    /// Shader Graph. Un typo no produce error: la escritura se ignora en silencio.
    /// </remarks>
    [Serializable]
    public class EffectParameter
    {
        [Header("Etiquetas")]
        [SerializeField] private string _displayNameEs;
        [SerializeField] private string _displayNameEn;

        [Header("Vínculo con el shader")]
        [Tooltip("Reference exacto de la propiedad en Shader Graph, p. ej. _OutlineWidth")]
        [SerializeField] private string _propertyName;
        [SerializeField] private ParameterType _type = ParameterType.Float;

        [Header("Rango (sólo Float)")]
        [SerializeField] private float _minValue;
        [SerializeField] private float _maxValue = 1f;

        [Header("Valores por defecto")]
        [SerializeField] private float _defaultFloat;
        [SerializeField] private Color _defaultColor = Color.white;
        [SerializeField] private bool _defaultBool;
        [SerializeField] private int _defaultEnumIndex;

        [Header("Opciones (sólo Enum)")]
        [SerializeField] private string[] _enumOptions = new string[0];

        private int _propertyId;
        private bool _propertyIdCached;

        public string DisplayNameEs => _displayNameEs;
        public string DisplayNameEn => _displayNameEn;
        public string PropertyName => _propertyName;
        public ParameterType Type => _type;
        public float MinValue => _minValue;
        public float MaxValue => _maxValue;
        public float DefaultFloat => _defaultFloat;
        public Color DefaultColor => _defaultColor;
        public bool DefaultBool => _defaultBool;
        public int DefaultEnumIndex => _defaultEnumIndex;
        public string[] EnumOptions => _enumOptions;

        /// <summary>
        /// Identificador de la propiedad de shader, resuelto una sola vez.
        /// Nunca llamar a Shader.PropertyToID por frame.
        /// </summary>
        public int PropertyId
        {
            get
            {
                if (!_propertyIdCached)
                {
                    _propertyId = Shader.PropertyToID(_propertyName);
                    _propertyIdCached = true;
                }

                return _propertyId;
            }
        }

        public float ClampFloat(float value)
        {
            return Mathf.Clamp(value, _minValue, _maxValue);
        }

        public int ClampEnumIndex(int index)
        {
            if (_enumOptions == null || _enumOptions.Length == 0)
            {
                return 0;
            }

            return Mathf.Clamp(index, 0, _enumOptions.Length - 1);
        }
    }
}
