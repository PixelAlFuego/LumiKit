using UnityEngine;

namespace LumiKit.UI
{
    /// <summary>
    /// Paleta y medidas de la interfaz. Único sitio del pack donde vive un color literal:
    /// todo widget y todo generador de prefabs lee de aquí.
    /// </summary>
    /// <remarks>
    /// Los valores salen de .claude/rules/ui-style.md (GDD Parte 2, líneas 393-621). No se
    /// inventa ninguno ni se aproxima.
    ///
    /// Un Color no puede ser const, así que los colores van como static readonly en
    /// PascalCase con el nombre del token (CONVENTIONS.md). Color32 mantiene legible el
    /// hexadecimal del GDD y es el espacio en el que trabaja la UI de uGUI.
    ///
    /// Sin fuentes: los TMP_FontAsset de Space Grotesk, Inter y JetBrains Mono (GDD línea
    /// 934) no existen en disco todavía. Aquí sólo viven los tamaños. Las fuentes son LK-22.
    /// </remarks>
    public static class LumiTheme
    {
        // ── Superficies ────────────────────────────────────────────────────────────────
        public static readonly Color Void = new Color32(0x0D, 0x0F, 0x14, 0xFF);
        public static readonly Color Surface = new Color32(0x15, 0x19, 0x22, 0xFF);
        public static readonly Color SurfaceElevated = new Color32(0x1E, 0x24, 0x30, 0xFF);
        public static readonly Color Border = new Color32(0x2A, 0x32, 0x42, 0xFF);
        public static readonly Color BorderStrong = new Color32(0x3D, 0x47, 0x59, 0xFF);

        // ── Acentos ────────────────────────────────────────────────────────────────────
        public static readonly Color LumiCyan = new Color32(0x00, 0xE5, 0xD4, 0xFF);
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
        public const float TEXT_DISPLAY = 42f;
        public const float TEXT_H1 = 28f;
        public const float TEXT_H2 = 20f;
        public const float TEXT_H3 = 16f;
        public const float TEXT_BODY = 14f;
        public const float TEXT_LABEL = 13f;
        public const float TEXT_CAPTION = 12f;
        public const float TEXT_MONO = 13f;

        // ── Panel de parámetros ────────────────────────────────────────────────────────
        public const float PANEL_WIDTH = 280f;
        public const float PANEL_HEADER_HEIGHT = 48f;
        public const float PANEL_PADDING = 16f;
        public const float PANEL_BORDER = 1f;

        // ── Slider ─────────────────────────────────────────────────────────────────────
        public const float SLIDER_TRACK_HEIGHT = 4f;
        public const float SLIDER_HANDLE_SIZE = 16f;
        public const float SLIDER_HANDLE_BORDER = 2f;
        public const float SLIDER_VALUE_WIDTH = 48f;

        // ── Generales ──────────────────────────────────────────────────────────────────
        public const float SPACING = 8f;
        public const float RADIUS = 6f;
        public const float BUTTON_HEIGHT = 36f;
        public const float TRANSITION_SECONDS = 0.15f;
        public const float PRESS_SCALE = 0.98f;

        /// <summary>Resolución de referencia del CanvasScaler. GDD línea 329.</summary>
        public const float REFERENCE_WIDTH = 1920f;
        public const float REFERENCE_HEIGHT = 1080f;
    }
}
