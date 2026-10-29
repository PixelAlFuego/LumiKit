# Backlog
Fuente: GDD §3.2–3.4 (líneas 653-721). Estados: ⬜ 🟠 🟡 ✅ ⛔ (ver CLAUDE.md).

## Orden de ejecución — MVP 2D
Una tarea por sesión. No adelantar fases. 3D y VFX: sin fase asignada. Ideas nuevas: una fila con fase "—".

| Fase | Tareas | Entregable |
|---|---|---|
| 0–7 | Andamiaje, LK-09, LK-12, LK-10, LK-11a, LK-11b, LK-49, LK-22a, LK-50, LK-51, LK-22b, LK-52, LK-23, LK-01, LK-24, LK-03, LK-20, LK-14, LK-13, LK-17 | MVP 2D, cerradas; detalle en `docs/archive/MVP_SCOPE_2026-10.md` y en el tag `mvp-1.0`. LK-20 sigue ⬜ (falta Lumi) |
| 8 | LK-27, LK-58, LK-53, LK-02, LK-54, LK-55, LK-57 | Post-MVP: validación y pulido. **Activa (Sesión 13)** |

## Ítems

| ID | Funcionalidad | Cat. | Fase | Estado |
|---|---|---|---|---|
| LK-01 | Outline Shader 2D: color, grosor, modo | Shader | 5 | ✅ |
| LK-02 | Dissolve Shader 2D: ruido + borde emisivo | Shader | 8 | ⬜ |
| LK-03 | Glow / Inner Glow Shader 2D: intensidad, pulso | Shader | 5 | ✅ |
| LK-04 | Toon / Cel Shader 3D con bandas | Shader | — | ⬜ fuera de alcance |
| LK-05 | Water Shader 3D: ondas + transparencia | Shader | — | ⬜ fuera de alcance |
| LK-06 | Partículas Fire & Smoke | VFX | — | ⬜ fuera de alcance |
| LK-07 | Partículas Magic Sparkles | VFX | — | ⬜ fuera de alcance |
| LK-08 | Partículas Hit Flash / Impact | VFX | — | ⬜ fuera de alcance |
| LK-09 | Arquitectura `EffectDefinition` (ScriptableObject) | Sistema | 1 | ✅ |
| LK-10 | Selección de objetos por raycast | Sistema | 2 | ✅ |
| LK-11a | Panel de parámetros + widget `Float` (slider) | UI | 3 | ✅ |
| LK-11b | Widgets `Color`, `Toggle` y `Enum` | UI | 3 | ✅ |
| LK-12 | Controlador de cámara (ortográfica 2D) | Sistema | 2 | ✅ |
| LK-13 | Escena de menú principal con navegación | UI | 7 | ✅ |
| LK-14 | Escena `02_Demo_2D` construida y poblada | Escena | 7 | ✅ |
| LK-15 | Escena `03_Demo_3D` construida y poblada | Escena | — | ⬜ fuera de alcance |
| LK-16 | Escena `04_Demo_VFX` construida y poblada | Escena | — | ⬜ fuera de alcance |
| LK-17 | Carga asíncrona entre escenas | Sistema | 7 | ✅ |
| LK-18 | Pantalla de configuración (calidad, res., idioma) | UI | — | ⬜ |
| LK-19 | Localización ES / EN | Sistema | — | ⬜ |
| LK-20 | Objetos demo 2D (Lumi, cristal, runa) | Arte | 6 | ⬜ |
| LK-21 | Objetos demo 3D low-poly en Blender | Arte | — | ⬜ fuera de alcance |
| LK-22a | Pie del panel con Reset y `ui-style.md` al día | UI | 3 | ✅ |
| LK-22b | Fuentes TMP del pack y sus licencias | Arte | 3 | ✅ |
| LK-23 | Efectos de sonido de interfaz y música | Audio | 4 | ✅ |
| LK-24 | Comparación antes / después (TAB) | Sistema | 5 | ✅ |
| LK-25 | Pantalla de créditos e info del pack | UI | — | ⬜ |
| LK-26 | Documentación técnica bilingüe | Doc | — | ⬜ |
| LK-27 | Exportación y validación del `.unitypackage` | Distrib. | 8 | 🟡 |
| LK-28 | Página del producto en itch.io | Distrib. | — | ⬜ |
| LK-29 | Copiar valores al portapapeles | Deseable | — | ⬜ |
| LK-30 | Contador de exploración (X / 8) | Deseable | — | ⬜ |
| LK-31 | Etiqueta flotante con nombre del efecto (hover) | Deseable | — | ⬜ |
| LK-32 | Menú de pausa (ESC) en escenas demo | Deseable | — | ⬜ |
| LK-33 | Presets por efecto (3 por shader) | Deseable | — | ⬜ |
| LK-34 | Toasts de confirmación | Deseable | — | ⬜ |
| LK-35 | Persistencia en `PlayerPrefs` | Deseable | — | ⬜ |
| LK-36 | Escena `_Playground` con todos los efectos | Deseable | — | ⬜ |
| LK-37 | Video de demostración para itch.io | Deseable | — | ✅ |
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
| LK-49 | Validar `propertyName` contra el shader del material | Sistema | 3 | ✅ |
| LK-50 | Componente de botón con la jerarquía del GDD §2.7 | UI | 3 | ✅ |
| LK-51 | Arte de interfaz: sprites propios del pack | Arte | 3 | ✅ |
| LK-52 | Valor editable en el slider y foco de texto para los atajos | UI | 4 | ✅ |
| LK-53 | Halo de LK-03 sin capas (fundido entre anillos o más muestras); máximo del `EFF` de 2 a 5 | Shader | 8 | ⬜ |
| LK-54 | Botón "Comparar" en pantalla e indicador de controles (GDD líneas 59 y 137); quita el texto fijo de `02_Demo_2D` | UI | 8 | ⬜ |
| LK-55 | Composición de `02_Demo_2D` a 4:3 | Escena | 8 | ⬜ |
| LK-56 | Versiones Shader Graph de LK-01, LK-02 y LK-03 (D-010) | Futura v1.1 | — | ⬜ |
| LK-57 | Foco visible de `LumiButton` y del campo del slider (teclado y mando) | UI | 8 | ⬜ |
| LK-58 | asmdef sólo-editor dentro del pack (`LumiButtonEditor`, `FontFeatureCleaner`) | Distrib. | 8 | ⬜ |
| LK-59 | `LumiSkin`: arte de UI cambiable sin regenerar (spec de LK-51, línea 78) | UI | — | ⬜ |

Estimaciones y prioridades originales: GDD §3.2 (líneas 655-686). LK-22 se partió en LK-22a y LK-22b; no queda ninguna LK-22 a secas.
Fuera del GDD: LK-49 (verificar LK-11b, Sesión 05), LK-50 (corte de LK-22, Sesión 07), LK-51 (rediseño de UI, Sesión 08), LK-52 (verificar LK-22b, Sesión 10), LK-53 a LK-59 (MVP_SCOPE, Aparcado y Fuera, Sesión 13).
