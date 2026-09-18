using System;
using System.Collections.Generic;
using LumiKit.Utils;
using UnityEngine;

namespace LumiKit.Core
{
    /// <summary>
    /// Se adjunta a cada objeto demostrable. Guarda el valor actual de cada parámetro
    /// del EffectDefinition y lo aplica al Renderer vía MaterialPropertyBlock (D-001).
    /// </summary>
    /// <remarks>
    /// Capa Core: no conoce la UI. La interfaz se entera de los cambios por
    /// OnValueChanged, nunca al revés.
    /// </remarks>
    [AddComponentMenu("LumiKit/Effect Controller")]
    [DisallowMultipleComponent]
    public class EffectController : MonoBehaviour
    {
        [SerializeField] private Renderer _targetRenderer;
        [SerializeField] private EffectDefinition _definition;

        private MaterialPropertyBlock _block;
        private readonly Dictionary<string, ParameterValue> _values =
            new Dictionary<string, ParameterValue>();
        private bool _initialized;
        private bool _effectEnabled = true;

        /// <summary>Lista reutilizada para contar materiales sin asignar basura (LK-49).</summary>
        private static readonly List<Material> _sharedMaterialBuffer = new List<Material>();

        /// <summary>Valor vivo de un parámetro. Privado: la API pública son los getters tipados.</summary>
        private struct ParameterValue
        {
            public float FloatValue;
            public Color ColorValue;
            public bool BoolValue;
            public int EnumIndex;
        }

        public EffectDefinition Definition => _definition;

        public bool IsEffectEnabled => _effectEnabled;

        /// <summary>Se dispara con el PropertyName del parámetro que acaba de cambiar.</summary>
        public event Action<string> OnValueChanged;

        private void Awake()
        {
            EnsureInitialized();
            ResetToDefaults();
            SetEffectEnabled(true);
        }

        private void EnsureInitialized()
        {
            if (_initialized)
            {
                return;
            }

            _initialized = true;
            _block = new MaterialPropertyBlock();

            if (_targetRenderer == null)
            {
                _targetRenderer = GetComponent<Renderer>();
            }

            if (_targetRenderer == null)
            {
                Debug.LogWarning($"[LumiKit] EffectController sin Renderer en '{name}'.", this);
            }
            else
            {
                // Arrastra las sobrescrituras que el Renderer ya tuviera, para no pisarlas.
                _targetRenderer.GetPropertyBlock(_block);
            }

            if (_definition == null)
            {
                Debug.LogWarning($"[LumiKit] EffectController sin EffectDefinition en '{name}'.", this);
            }

            SeedValuesFromDefaults();
            ValidateProperties();
        }

        private void SeedValuesFromDefaults()
        {
            _values.Clear();

            if (_definition == null)
            {
                return;
            }

            IReadOnlyList<EffectParameter> parameters = _definition.Parameters;
            for (int i = 0; i < parameters.Count; i++)
            {
                EffectParameter parameter = parameters[i];
                if (parameter == null || string.IsNullOrEmpty(parameter.PropertyName))
                {
                    continue;
                }

                _values[parameter.PropertyName] = new ParameterValue
                {
                    FloatValue = parameter.DefaultFloat,
                    ColorValue = parameter.DefaultColor,
                    BoolValue = parameter.DefaultBool,
                    EnumIndex = parameter.DefaultEnumIndex
                };
            }
        }

        /// <summary>
        /// Avisa una sola vez de los propertyName que el material no declara. Un
        /// MaterialPropertyBlock que escribe una propiedad inexistente no falla: la descarta
        /// en silencio, y el parámetro parece roto sin motivo visible (LK-49).
        /// </summary>
        /// <remarks>
        /// Se llama desde EnsureInitialized, que está protegido por _initialized: una vez por
        /// componente, ni por frame ni por escritura.
        ///
        /// Lee sharedMaterial, el único acceso al material que D-001 autoriza. De ahí que sólo
        /// se valide el primer hueco del Renderer; cuando hay más, el aviso lo dice.
        ///
        /// _EffectEnabled (D-005) queda fuera a propósito: todavía no existe en ningún shader
        /// del pack y saltaría en todos los objetos, tapando lo que sí importa. Su comprobación
        /// es criterio de aceptación de cada shader.
        /// </remarks>
        private void ValidateProperties()
        {
            if (_definition == null)
            {
                return;
            }

            Material material = _targetRenderer != null ? _targetRenderer.sharedMaterial : null;
            if (material == null)
            {
                Debug.LogWarning(
                    $"[LumiKit] '{name}' con {_definition.name}: no hay material que validar, " +
                    $"así que ninguna escritura llega a un shader.{DescribeMaterialCount()}",
                    this);
                return;
            }

            List<string> missing = null;
            IReadOnlyList<EffectParameter> parameters = _definition.Parameters;

            for (int i = 0; i < parameters.Count; i++)
            {
                EffectParameter parameter = parameters[i];
                if (parameter == null || string.IsNullOrEmpty(parameter.PropertyName))
                {
                    continue;
                }

                if (MaterialPropertyHelper.HasProperty(material, parameter.PropertyName))
                {
                    continue;
                }

                if (missing == null)
                {
                    missing = new List<string>();
                }

                missing.Add(parameter.PropertyName);
            }

            if (missing == null)
            {
                return;
            }

            string shaderName = material.shader != null ? material.shader.name : "sin shader";

            Debug.LogWarning(
                $"[LumiKit] '{name}' con {_definition.name}: el material '{material.name}' " +
                $"(shader '{shaderName}') no declara {string.Join(", ", missing)}. " +
                $"Esas escrituras se descartan en silencio.{DescribeMaterialCount()}",
                this);
        }

        /// <summary>
        /// Añade al aviso cuántos materiales tiene el Renderer cuando hay más de uno, para que
        /// no quede un hueco mudo: sólo se ha validado el primero.
        /// </summary>
        /// <remarks>
        /// GetSharedMaterials sirve aquí sólo para contar, sobre una lista reutilizada, y no
        /// instancia ningún material. Validar contra todos exigiría leer sharedMaterials, que
        /// D-001 no autoriza todavía: es una enmienda pendiente, anotada en STATE.
        /// </remarks>
        private string DescribeMaterialCount()
        {
            if (_targetRenderer == null)
            {
                return string.Empty;
            }

            _sharedMaterialBuffer.Clear();
            _targetRenderer.GetSharedMaterials(_sharedMaterialBuffer);
            int count = _sharedMaterialBuffer.Count;
            _sharedMaterialBuffer.Clear();

            return count > 1 ? $" Validado 1 de {count} materiales." : string.Empty;
        }

        /// <summary>
        /// Devuelve el parámetro si existe y es del tipo esperado. Si no, avisa y
        /// devuelve null: un PropertyName mal escrito no debe fallar en silencio.
        /// </summary>
        private EffectParameter Resolve(string propertyName, ParameterType expected)
        {
            EnsureInitialized();

            if (_definition == null)
            {
                return null;
            }

            EffectParameter parameter = _definition.FindParameter(propertyName);
            if (parameter == null)
            {
                Debug.LogWarning(
                    $"[LumiKit] '{propertyName}' no existe en {_definition.name}.", this);
                return null;
            }

            if (parameter.Type != expected)
            {
                Debug.LogWarning(
                    $"[LumiKit] '{propertyName}' es {parameter.Type}, se pidió {expected}.", this);
                return null;
            }

            return parameter;
        }

        public void SetFloat(string propertyName, float value)
        {
            EffectParameter parameter = Resolve(propertyName, ParameterType.Float);
            if (parameter == null)
            {
                return;
            }

            ParameterValue stored = _values[propertyName];
            stored.FloatValue = parameter.ClampFloat(value);
            _values[propertyName] = stored;

            MaterialPropertyHelper.SetFloat(_block, parameter.PropertyId, stored.FloatValue);
            Flush(propertyName);
        }

        public void SetColor(string propertyName, Color value)
        {
            EffectParameter parameter = Resolve(propertyName, ParameterType.Color);
            if (parameter == null)
            {
                return;
            }

            ParameterValue stored = _values[propertyName];
            stored.ColorValue = value;
            _values[propertyName] = stored;

            MaterialPropertyHelper.SetColor(_block, parameter.PropertyId, value);
            Flush(propertyName);
        }

        public void SetBool(string propertyName, bool value)
        {
            EffectParameter parameter = Resolve(propertyName, ParameterType.Boolean);
            if (parameter == null)
            {
                return;
            }

            ParameterValue stored = _values[propertyName];
            stored.BoolValue = value;
            _values[propertyName] = stored;

            MaterialPropertyHelper.SetBool(_block, parameter.PropertyId, value);
            Flush(propertyName);
        }

        public void SetEnum(string propertyName, int index)
        {
            EffectParameter parameter = Resolve(propertyName, ParameterType.Enum);
            if (parameter == null)
            {
                return;
            }

            ParameterValue stored = _values[propertyName];
            stored.EnumIndex = parameter.ClampEnumIndex(index);
            _values[propertyName] = stored;

            MaterialPropertyHelper.SetEnum(_block, parameter.PropertyId, stored.EnumIndex);
            Flush(propertyName);
        }

        public float GetFloat(string propertyName)
        {
            EffectParameter parameter = Resolve(propertyName, ParameterType.Float);
            return parameter != null ? _values[propertyName].FloatValue : 0f;
        }

        public Color GetColor(string propertyName)
        {
            EffectParameter parameter = Resolve(propertyName, ParameterType.Color);
            return parameter != null ? _values[propertyName].ColorValue : Color.white;
        }

        public bool GetBool(string propertyName)
        {
            EffectParameter parameter = Resolve(propertyName, ParameterType.Boolean);
            return parameter != null && _values[propertyName].BoolValue;
        }

        public int GetEnum(string propertyName)
        {
            EffectParameter parameter = Resolve(propertyName, ParameterType.Enum);
            return parameter != null ? _values[propertyName].EnumIndex : 0;
        }

        /// <summary>
        /// Devuelve todos los parámetros a los valores por defecto del
        /// EffectDefinition. Mecánica 5 del GDD (botón Reset).
        /// </summary>
        public void ResetToDefaults()
        {
            EnsureInitialized();

            if (_definition == null)
            {
                return;
            }

            SeedValuesFromDefaults();

            IReadOnlyList<EffectParameter> parameters = _definition.Parameters;
            for (int i = 0; i < parameters.Count; i++)
            {
                WriteToBlock(parameters[i]);
            }

            MaterialPropertyHelper.Apply(_targetRenderer, _block);

            for (int i = 0; i < parameters.Count; i++)
            {
                EffectParameter parameter = parameters[i];
                if (parameter != null && !string.IsNullOrEmpty(parameter.PropertyName))
                {
                    OnValueChanged?.Invoke(parameter.PropertyName);
                }
            }
        }

        /// <summary>
        /// Apaga o enciende el efecto sin tocar el material, escribiendo la propiedad
        /// _EffectEnabled (D-005). Es lo que usa la comparación con TAB (LK-24).
        /// </summary>
        /// <remarks>
        /// Si el shader no declara _EffectEnabled, la escritura se ignora en silencio y
        /// no ocurre nada visible. Ningún shader del pack existe todavía: el criterio
        /// visual está diferido a LK-01.
        /// </remarks>
        public void SetEffectEnabled(bool enabled)
        {
            EnsureInitialized();
            _effectEnabled = enabled;

            MaterialPropertyHelper.SetBool(
                _block, MaterialPropertyHelper.EFFECT_ENABLED_ID, enabled);
            MaterialPropertyHelper.Apply(_targetRenderer, _block);
        }

        private void WriteToBlock(EffectParameter parameter)
        {
            if (parameter == null || string.IsNullOrEmpty(parameter.PropertyName))
            {
                return;
            }

            ParameterValue stored = _values[parameter.PropertyName];

            switch (parameter.Type)
            {
                case ParameterType.Float:
                    MaterialPropertyHelper.SetFloat(_block, parameter.PropertyId, stored.FloatValue);
                    break;
                case ParameterType.Color:
                    MaterialPropertyHelper.SetColor(_block, parameter.PropertyId, stored.ColorValue);
                    break;
                case ParameterType.Boolean:
                    MaterialPropertyHelper.SetBool(_block, parameter.PropertyId, stored.BoolValue);
                    break;
                case ParameterType.Enum:
                    MaterialPropertyHelper.SetEnum(_block, parameter.PropertyId, stored.EnumIndex);
                    break;
            }
        }

        private void Flush(string propertyName)
        {
            MaterialPropertyHelper.Apply(_targetRenderer, _block);
            OnValueChanged?.Invoke(propertyName);
        }
    }
}
