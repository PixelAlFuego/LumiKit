# Backlog
Fuente: GDD §3.2–3.4 (líneas 653-721). Estados: ⬜ 🟠 🟡 ✅ ⛔ (ver CLAUDE.md).

## Orden de ejecución — MVP 2D
Una tarea por sesión. No adelantar fases. 3D y VFX están fuera de alcance.

| Fase | Tareas | Entregable |
|---|---|---|
| 0 | Andamiaje | Docs + carpetas + asmdefs + git |
| 1 | LK-09 | EffectParameter, ParameterType, EffectDefinition, EffectRegistry, EffectController, MaterialPropertyHelper, Singleton |
| 2 | LK-12, LK-10 | DemoCameraController ortográfico, ObjectSelector (raycast) |
| 3 | LK-11a, LK-11b, LK-22 | ParameterWidgetBase + Slider/Color/Toggle/Enum, ParameterPanelUI, LumiTheme.cs con la paleta, prefabs de UI generados por script. El pie del panel (Reset) cae en LK-22 |
| 4 | LK-23 | UIAudioManager + AMX_LumiKit + enganche hover/click/select |
| 5 | LK-01, LK-24, LK-03, LK-02 | Outline 2D, ComparisonToggle (TAB, necesita `_EffectEnabled` de LK-01), Glow 2D, Dissolve 2D + generador procedural de texturas de ruido y rampas |
| 6 | LK-20 | Sprites Lumi, Cristal, Runa (entrega SVG para exportar a PNG 512) |
| 7 | LK-14, LK-13, LK-17 | Escena 02_Demo_2D y 01_MainMenu construidas por script, SceneLoader |

## Ítems

| ID | Funcionalidad | Cat. | Fase | Estado |
|---|---|---|---|---|
| LK-01 | Outline Shader 2D: color, grosor, modo | Shader | 5 | ⬜ |
| LK-02 | Dissolve Shader 2D: ruido + borde emisivo | Shader | 5 | ⬜ |
| LK-03 | Glow / Inner Glow Shader 2D: intensidad, pulso | Shader | 5 | ⬜ |
| LK-04 | Toon / Cel Shader 3D con bandas | Shader | — | ⬜ fuera de alcance |
| LK-05 | Water Shader 3D: ondas + transparencia | Shader | — | ⬜ fuera de alcance |
| LK-06 | Partículas Fire & Smoke | VFX | — | ⬜ fuera de alcance |
| LK-07 | Partículas Magic Sparkles | VFX | — | ⬜ fuera de alcance |
| LK-08 | Partículas Hit Flash / Impact | VFX | — | ⬜ fuera de alcance |
| LK-09 | Arquitectura `EffectDefinition` (ScriptableObject) | Sistema | 1 | ✅ |
| LK-10 | Selección de objetos por raycast | Sistema | 2 | ✅ |
| LK-11a | Panel de parámetros + widget `Float` (slider) | UI | 3 | ✅ |
| LK-11b | Widgets `Color`, `Toggle` y `Enum` | UI | 3 | 🟡 |
| LK-12 | Controlador de cámara (ortográfica 2D) | Sistema | 2 | ✅ |
| LK-13 | Escena de menú principal con navegación | UI | 7 | ⬜ |
| LK-14 | Escena `02_Demo_2D` construida y poblada | Escena | 7 | ⬜ |
| LK-15 | Escena `03_Demo_3D` construida y poblada | Escena | — | ⬜ fuera de alcance |
| LK-16 | Escena `04_Demo_VFX` construida y poblada | Escena | — | ⬜ fuera de alcance |
| LK-17 | Carga asíncrona entre escenas | Sistema | 7 | ⬜ |
| LK-18 | Pantalla de configuración (calidad, res., idioma) | UI | — | ⬜ |
| LK-19 | Localización ES / EN | Sistema | — | ⬜ |
| LK-20 | Objetos demo 2D (Lumi, cristal, runa) | Arte | 6 | ⬜ |
| LK-21 | Objetos demo 3D low-poly en Blender | Arte | — | ⬜ fuera de alcance |
| LK-22 | Identidad visual, fuentes y pie del panel (Reset) | Arte | 3 | ⬜ |
| LK-23 | Efectos de sonido de interfaz | Audio | 4 | ⬜ |
| LK-24 | Comparación antes / después (TAB) | Sistema | 5 | ⬜ |
| LK-25 | Pantalla de créditos e info del pack | UI | — | ⬜ |
| LK-26 | Documentación técnica bilingüe | Doc | — | ⬜ |
| LK-27 | Exportación y validación del `.unitypackage` | Distrib. | — | ⬜ |
| LK-28 | Página del producto en itch.io | Distrib. | — | ⬜ |
| LK-29 | Copiar valores al portapapeles | Deseable | — | ⬜ |
| LK-30 | Contador de exploración (X / 8) | Deseable | — | ⬜ |
| LK-31 | Etiqueta flotante con nombre del efecto (hover) | Deseable | — | ⬜ |
| LK-32 | Menú de pausa (ESC) en escenas demo | Deseable | — | ⬜ |
| LK-33 | Presets por efecto (3 por shader) | Deseable | — | ⬜ |
| LK-34 | Toasts de confirmación | Deseable | — | ⬜ |
| LK-35 | Persistencia en `PlayerPrefs` | Deseable | — | ⬜ |
| LK-36 | Escena `_Playground` con todos los efectos | Deseable | — | ⬜ |
| LK-37 | Video de demostración para itch.io | Deseable | — | ⬜ |
| LK-38 | Port de shaders a Godot 4.x | Futura v2.0 | — | ⬜ |
| LK-39 | Publicación en Unity Asset Store | Futura v1.1 | — | ⬜ |
| LK-40 | Publicación en Fab (Epic Games) | Futura v2.0 | — | ⬜ |
| LK-41 | Soporte Built-in Render Pipeline | Futura v1.2 | — | ⬜ |
| LK-42 | Soporte HDRP | Futura v2.1 | — | ⬜ |
| LK-43 | Ampliación a 15+ efectos | Futura v2.0 | — | ⬜ |
| LK-44 | Editor Window personalizada | Futura v2.0 | — | ⬜ |
| LK-45 | Versión WebGL de la demo | Futura v1.2 | — | ⬜ |
| LK-46 | Exportar configuraciones como `.json` | Futura v2.0 | — | ⬜ |
| LK-47 | Optimización para móviles | Futura v2.1 | — | ⬜ |
| LK-48 | Documentación en video | Futura v1.1 | — | ⬜ |

Estimaciones y prioridades originales: GDD §3.2 (líneas 655-686).
