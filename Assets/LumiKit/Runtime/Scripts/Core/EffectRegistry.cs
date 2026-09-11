using System.Collections.Generic;
using UnityEngine;

namespace LumiKit.Core
{
    /// <summary>
    /// Catálogo central de todos los EffectDefinition del pack. Lo consume el contador
    /// de exploración (LK-30) y cualquier pantalla que necesite listar los efectos.
    /// </summary>
    [CreateAssetMenu(fileName = "EFF_Registry", menuName = "LumiKit/Effect Registry")]
    public class EffectRegistry : ScriptableObject
    {
        [SerializeField] private EffectDefinition[] _effects = new EffectDefinition[0];

        public IReadOnlyList<EffectDefinition> Effects => _effects;
        public int Count => _effects != null ? _effects.Length : 0;

        public EffectDefinition GetByIndex(int index)
        {
            if (_effects == null || index < 0 || index >= _effects.Length)
            {
                return null;
            }

            return _effects[index];
        }

        /// <summary>
        /// Busca por el nombre del asset (p. ej. "EFF_Outline2D"), no por el nombre
        /// de presentación, que cambia con el idioma.
        /// </summary>
        public EffectDefinition FindByName(string assetName)
        {
            if (_effects == null || string.IsNullOrEmpty(assetName))
            {
                return null;
            }

            for (int i = 0; i < _effects.Length; i++)
            {
                EffectDefinition effect = _effects[i];
                if (effect != null && effect.name == assetName)
                {
                    return effect;
                }
            }

            return null;
        }

        public int IndexOf(EffectDefinition effect)
        {
            if (_effects == null || effect == null)
            {
                return -1;
            }

            for (int i = 0; i < _effects.Length; i++)
            {
                if (_effects[i] == effect)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
