using UnityEngine;

namespace LumiKit.UI
{
    /// <summary>
    /// Paleta y medidas de la interfaz. Único sitio del pack donde vive un color literal:
    /// todo widget y todo generador de prefabs lee de aquí.
    /// </summary>
    /// <remarks>
    /// Los valores salen de .claude/rules/ui-style.md (GDD Parte 2, líneas 393-621). No se
    /// inventa ninguno ni se aproxima. Las dos excepciones —tamaño de etiqueta y de valor, y
    /// el ancho del campo numérico— están comentadas donde ocurren, con su motivo.
    ///
    /// Un Color no puede ser const, así que los colores van como static readonly en
    /// PascalCase con el nombre del token (CONVENTIONS.md). Color32 mantiene legible el
    /// hexadecimal del GDD y es el espacio en el que trabaja la UI de uGUI.
    ///
    /// Sin fuentes: una clase estática no guarda un TMP_FontAsset. Aquí sólo viven los tamaños;
    /// Space Grotesk, Inter y JetBrains Mono (Assets/LumiKit/Fonts/) las hornea en cada texto
    /// ParameterPanelBuilder (LK-22b). Si algún día hay que crear texto desde código sin prefab,
    /// la salida es un ScriptableObject de estilo (LumiSkin, anotado en UI_ART_BRIEF.md).
    /// </remarks>
    public static class LumiTheme
    {
        // ── Superficies ────────────────────────────────────────────────────────────────
        public static readonly Color Void = new Color32(0x0D, 0x0F, 0x14, 0xFF);
        public static readonly Color Surface = new Color32(0x15, 0x19, 0x22, 0xFF);
        public static readonly Color SurfaceElevated = new Color32(0x1E, 0x24, 0x30, 0xFF);
        public static readonly Color Border = new Color32(0x2A, 0x32, 0x42, 0xFF);
        public static readonly Color BorderStrong = new Color32(0x3D, 0x47, 0x59, 0xFF);
        // Relleno en reposo del botón secundario (GDD línea 566). Surface con alfa 0 y no negro:
        // Selectable funde RGB y alfa a la vez, y así el paso a hover no se oscurece a mitad.
        public static readonly Color Transparent = new Color32(0x15, 0x19, 0x22, 0x00);

        // ── Acentos ────────────────────────────────────────────────────────────────────
        public static readonly Color LumiCyan = new Color32(0x00, 0xE5, 0xD4, 0xFF);
        // Hover y presionado del botón primario (GDD líneas 556-557). Los usa LumiButton (LK-50).
        public static readonly Color LumiCyanHover = new Color32(0x33, 0xEB, 0xDD, 0xFF);
        public static readonly Color LumiCyanPressed = new Color32(0x00, 0xC4, 0xB6, 0xFF);
        public static readonly Color LumiMagenta = new Color32(0xFF, 0x3D, 0x9A, 0xFF);
        public static readonly Color LumiViolet = new Color32(0x8B, 0x5C, 0xF6, 0xFF);

        // ── Señales ────────────────────────────────────────────────────────────────────
        public static readonly Color SignalAmber = new Color32(0xFF, 0xB0, 0x20, 0xFF);
        public static readonly Color SignalGreen = new Color32(0x3D, 0xD6, 0x8C, 0xFF);
        public static readonly Color SignalRed = new Color32(0xFF, 0x5A, 0x5A, 0xFF);

        // ── Texto ──────────────────────────────────────────────────────────────────────
        public static readonly Color TextPrimary = new Color32(0xF2, 0xF5, 0xFA, 0xFF);
        public static readonly Color TextSecondary = new Color32(0xA0, 0xAA, 0xBA, 0xFF);
        public static readonly Color TextMuted = new Color32(0x5C, 0x66, 0x78, 0xFF);
        public static readonly Color TextOnAccent = new Color32(0x0D, 0x0F, 0x14, 0xFF);

        // ── Tamaños de texto (px) ──────────────────────────────────────────────────────
        // TEXT_LABEL y TEXT_MONO van por encima de los 13 px del GDD (líneas 451-457): a 16 px
        // se leen y a 13 no. Decisión cerrada del usuario (Sesión 05); el resto de la escala no se
        // toca. Estos números sólo significan algo con la resolución de diseño de más abajo.
        public const float TEXT_DISPLAY = 42f;
        public const float TEXT_H1 = 28f;
        public const float TEXT_H2 = 20f;
        public const float TEXT_H3 = 16f;
        public const float TEXT_BODY = 14f;
        public const float TEXT_LABEL = 16f;
        public const float TEXT_CAPTION = 12f;
        public const float TEXT_MONO = 16f;

        // ── Panel de parámetros ────────────────────────────────────────────────────────
        public const float PANEL_WIDTH = 280f;
        public const float PANEL_HEADER_HEIGHT = 48f;
        public const float PANEL_PADDING = 16f;
        public const float PANEL_BORDER = 1f;
        // Derivado: el GDD manda pie fijo (línea 609) pero no dice su alto. Un botón estándar
        // con el padding del panel por arriba y por abajo.
        public const float PANEL_FOOTER_HEIGHT = BUTTON_HEIGHT + 2f * PANEL_PADDING;

        // ── Slider ─────────────────────────────────────────────────────────────────────
        public const float SLIDER_TRACK_HEIGHT = 4f;
        public const float SLIDER_HANDLE_SIZE = 16f;
        public const float SLIDER_HANDLE_BORDER = 2f;
        // 56 y no los 48 del GDD: con Mono a 16 px, "10.00" no cabe en 48 y el valor se corta.
        public const float SLIDER_VALUE_WIDTH = 56f;

        // ── Fila de un widget de parámetro ─────────────────────────────────────────────
        // Etiqueta (24) + separación (12) + slider (16). Alto y espaciado se mueven juntos:
        // subir el texto sin subir la fila deja las etiquetas pegadas al slider de arriba.
        public const float WIDGET_LABEL_HEIGHT = 24f;
        public const float WIDGET_ROW_HEIGHT = 52f;
        public const float WIDGET_ROW_SPACING = 12f;

        // ── Muestra de color y su desplegable ──────────────────────────────────────────
        public const float COLOR_SWATCH_WIDTH = 36f;
        public const float COLOR_SWATCH_HEIGHT = 24f;
        public const float COLOR_ROW_HEIGHT = 32f;
        // 24 y no 20: con la etiqueta a 16 px, una fila de 20 le recorta el trazo inferior.
        public const float COLOR_CHANNEL_HEIGHT = 24f;
        // Columna de la letra del canal (R, G, B).
        public const float COLOR_CHANNEL_LABEL_WIDTH = 16f;
        public const float COLOR_PALETTE_HEIGHT = 24f;
        // Tres canales más la fila de muestras, con SPACING entre filas y otro por encima del
        // bloque. Derivado y no literal: si cambia el alto de un canal, el total se ajusta solo.
        public const float COLOR_EXPAND_HEIGHT =
            3f * COLOR_CHANNEL_HEIGHT + COLOR_PALETTE_HEIGHT + 4f * SPACING;
        public const float COLOR_ROW_HEIGHT_EXPANDED = COLOR_ROW_HEIGHT + COLOR_EXPAND_HEIGHT;

        // ── Interruptor ────────────────────────────────────────────────────────────────
        public const float TOGGLE_TRACK_WIDTH = 36f;
        public const float TOGGLE_TRACK_HEIGHT = 20f;
        // Derivado: el GDD no fija el tamaño de la manija. 16 deja 2 px de aire por lado.
        public const float TOGGLE_HANDLE_SIZE = 16f;
        public const float TOGGLE_ROW_HEIGHT = 32f;

        // ── Botones segmentados del enum ───────────────────────────────────────────────
        public const float ENUM_OPTION_HEIGHT = 28f;
        public const float ENUM_ROW_HEIGHT = WIDGET_LABEL_HEIGHT + SPACING + ENUM_OPTION_HEIGHT;

        // ── Generales ──────────────────────────────────────────────────────────────────
        public const float SPACING = 8f;
        public const float RADIUS = 6f;
        // Muestra de color y tooltip (GDD líneas 601 y 621). Como RADIUS, es el radio que ya
        // lleva dibujado su sprite (SPR_UI_Rect_R4): cambiarlo aquí no redondea nada.
        public const float RADIUS_SMALL = 4f;
        public const float BUTTON_HEIGHT = 36f;
        // GDD líneas 582-584: la altura compacta es la de los botones dentro de un panel y
        // es la que ya usan las opciones del enum; el pie lleva la estándar.
        public const float BUTTON_HEIGHT_COMPACT = 28f;
        public const float BUTTON_PADDING = 16f;
        public const float BUTTON_PADDING_COMPACT = 12f;
        // Borde del botón secundario (GDD línea 566). Sin él, el estado Normal es invisible.
        public const float BUTTON_BORDER = 1f;
        public const float TRANSITION_SECONDS = 0.15f;
        public const float PRESS_SCALE = 0.98f;

        /// <summary>
        /// Resolución de diseño, fijada en 1920×1080 por el usuario en la Sesión 05 (D-007): el
        /// CanvasScaler usa esta referencia con match = height y el Game view se prueba igual.
        /// Cambiarla reescala el HUD y reabre la discusión del tamaño de texto. GDD línea 329.
        /// </summary>
        public const float REFERENCE_WIDTH = 1920f;
        public const float REFERENCE_HEIGHT = 1080f;
    }
}
