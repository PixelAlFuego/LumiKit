namespace LumiKit.Core
{
    /// <summary>
    /// Tipo de un parámetro expuesto por un efecto. Determina qué widget genera la UI.
    /// </summary>
    /// <remarks>
    /// El orden y los valores explícitos son parte del contrato de LK-09: alterarlos
    /// después de crear el primer EffectDefinition cambiaría el valor serializado en
    /// disco y corrompería en silencio todos los .asset existentes.
    /// </remarks>
    public enum ParameterType
    {
        Float = 0,
        Color = 1,
        Boolean = 2,
        Enum = 3
    }
}
