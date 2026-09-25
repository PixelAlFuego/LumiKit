using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LumiKit.Utils
{
    /// <summary>
    /// Regla de la demo (LK-52): mientras un campo de texto tenga el foco, ningún atajo de
    /// teclado escucha. Única comprobación del pack: los scripts la consultan, no la repiten.
    /// </summary>
    /// <remarks>
    /// Consumidores: WASD de DemoCameraController hoy; TAB (LK-24), Escape (LK-32) y H
    /// cuando lleguen. Mira el objeto seleccionado del EventSystem: un campo seleccionado
    /// pero sin editar (isFocused a false) no bloquea nada.
    /// </remarks>
    public static class TextInputFocus
    {
        /// <summary>
        /// True si el objeto seleccionado tiene un TMP_InputField, o un InputField de uGUI
        /// por si el comprador lo usa, en edición. Sin EventSystem, false.
        /// </summary>
        public static bool IsTyping
        {
            get
            {
                EventSystem eventSystem = EventSystem.current;
                if (eventSystem == null)
                {
                    return false;
                }

                // Comparación de Unity: un widget destruido cuenta como null.
                GameObject selected = eventSystem.currentSelectedGameObject;
                if (selected == null)
                {
                    return false;
                }

                if (selected.TryGetComponent(out TMP_InputField tmpField) && tmpField.isFocused)
                {
                    return true;
                }

                return selected.TryGetComponent(out InputField legacyField) && legacyField.isFocused;
            }
        }
    }
}
