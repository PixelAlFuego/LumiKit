using UnityEngine;

namespace LumiKit.Utils
{
    /// <summary>
    /// Único punto del pack que escribe en un MaterialPropertyBlock.
    /// </summary>
    /// <remarks>
    /// D-001: los parámetros se aplican con MaterialPropertyBlock, nunca modificando el
    /// material. Modificar un .mat en runtime dentro del editor lo sobrescribe en disco
    /// de forma permanente. Prohibido renderer.material, renderer.materials y
    /// material.SetFloat en todo Assets/LumiKit/.
    /// </remarks>
    public static class MaterialPropertyHelper
    {
        /// <summary>
        /// Propiedad que apaga el efecto sin tocar el material (D-005). Todo Shader
        /// Graph del pack la expone y termina en Lerp(base, conEfecto, _EffectEnabled).
        /// El nombre vive aquí y en ningún otro sitio.
        /// </summary>
        public const string EFFECT_ENABLED_PROPERTY = "_EffectEnabled";

        public static readonly int EFFECT_ENABLED_ID = Shader.PropertyToID(EFFECT_ENABLED_PROPERTY);

        public static void SetFloat(MaterialPropertyBlock block, int propertyId, float value)
        {
            if (block == null)
            {
                return;
            }

            block.SetFloat(propertyId, value);
        }

        public static void SetColor(MaterialPropertyBlock block, int propertyId, Color value)
        {
            if (block == null)
            {
                return;
            }

            block.SetColor(propertyId, ConvertColor(value));
        }

        /// <summary>
        /// Los shaders no tienen booleanos: un MaterialPropertyBlock no puede activar
        /// keywords, así que Boolean viaja como float 0/1 (ver .claude/rules/shaders.md).
        /// </summary>
        public static void SetBool(MaterialPropertyBlock block, int propertyId, bool value)
        {
            SetFloat(block, propertyId, value ? 1f : 0f);
        }

        /// <summary>
        /// Enum viaja como índice en un float, por el mismo motivo que Boolean.
        /// </summary>
        public static void SetEnum(MaterialPropertyBlock block, int propertyId, int index)
        {
            SetFloat(block, propertyId, index);
        }

        public static bool FloatToBool(float value)
        {
            return value >= 0.5f;
        }

        public static void Apply(Renderer renderer, MaterialPropertyBlock block)
        {
            if (renderer == null || block == null)
            {
                return;
            }

            renderer.SetPropertyBlock(block);
        }

        /// <summary>
        /// True si el material declara la propiedad. Envuelve Material.HasProperty para que la
        /// comprobación viva en el mismo sitio que las escrituras (LK-49).
        /// </summary>
        /// <remarks>
        /// Sólo mira el nombre, no el tipo: un parámetro Float apuntando a una propiedad de
        /// color pasa la comprobación. Null-safe porque se llama durante la inicialización,
        /// cuando el material puede faltar todavía.
        /// </remarks>
        public static bool HasProperty(Material material, string propertyName)
        {
            if (material == null || string.IsNullOrEmpty(propertyName))
            {
                return false;
            }

            return material.HasProperty(propertyName);
        }

        /// <summary>
        /// Punto único de conversión de color. Las propiedades Color de los grafos van
        /// en Mode = HDR.
        /// </summary>
        /// <remarks>
        /// PENDIENTE DE VERIFICACIÓN VISUAL: no está confirmado si
        /// MaterialPropertyBlock.SetColor aplica la conversión gamma→lineal que sí hace
        /// el inspector de materiales en espacio Linear. Si los colores salen lavados
        /// respecto al valor elegido en la UI, la corrección va aquí y en ningún otro
        /// sitio. Criterio diferido a LK-01.
        /// </remarks>
        private static Color ConvertColor(Color value)
        {
            return value;
        }
    }
}
