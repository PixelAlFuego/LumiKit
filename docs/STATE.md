# Estado del proyecto
Actualizado: 2026-10-02 · Sesión 12 (en curso)

## Ahora
- **Fase 5 en curso:** LK-01 ✅ (`cp-02-outline`), LK-24 ✅ (`cp-03-tab`), Sesión 12. Fase 4 cerrada: LK-52 y LK-23 (`cp-01-audio`). Fase 7: LK-14 ✅ (`cp-04-demo`).
- Tarea activa: LK-03 🟡 (Glow 2D): implementada, pendiente de verificar en el editor; entra por decisión del usuario (2026-10-02, pasadas las 11:00). Sin demo: se prueba en TestBench.
- **Modo sprint** hasta el viernes 2026-10-02 por la tarde, último día por enfermedad. Rama `sprint/mvp`, alcance y recortes en `docs/MVP_SCOPE.md`. Punto seguro: tag `v0.3-pre-mcp` (2c5d484).

## Hoy (2026-10-02)
1. LK-03: montaje en TestBench, checklist de abajo y "verificado LK-03" o el fallo. Push con tag al cerrarla.
2. Exportar el `.unitypackage` (LK-27); la prueba de importación, la semana del 2026-10-05.

## Últimas 3 sesiones
| Sesión | Fecha | Tarea | Resultado | Commit |
|---|---|---|---|---|
| 10 | 2026-09-24 | Apagón y auditoría · LK-22b limpiador y fuentes · manija y cierre de LK-51 · LK-50 · spec de LK-52 | ✅ LK-51 · ✅ LK-22b · ✅ LK-50 · Fase 3 cerrada | 658d8fa · deef7b1 · f4b5b11 · 9b4832a · 81920ba · 37596da · 3c865e5 · cb27f92 · 6f85bb2 · f6a41dd |
| 11 | 2026-09-25, 29 y 30 | LK-52 y su cierre · modo sprint · Unity MCP, subagentes y hook · shaders del MVP en HLSL (D-010) · plan de LK-23 (D-011) | ✅ LK-52 | 9cfbb49 · 2c5d484 · 3d2f2b8 · fa4af22 · 087251a · 428efc3 · 1a9fca5 · a5ad5f4 · 0e2cd88 · ba847e5 · 64916f4 · 90c2409 |
| 12 | 2026-09-30, 10-01 y 10-02 | Sprites sin sufijo (D-012), `SH_` · assets del audio y sprites · LK-23 y su cierre · LK-01 y su cierre · alcance recortado · LK-24 y su cierre · LK-14 (D-013) y su cierre · LK-03 | ✅ LK-23 · ✅ LK-01 · ✅ LK-24 · ✅ LK-14 · 🟡 LK-03 | b4045d2 · ced99b9 · 2a43e80 · 13454a7 · a69d112 · 98ad2a1 · 83ad8a4 · a633f0f · 67564dc · 5dbfa8a · 4219e96 · 0cc38f3 · 952b864 · 96a9f7e · 1f8a619 · dd33d9f · este commit |

Sesiones 00 a 09 archivadas en `docs/archive/sesiones_2026-Q3.md`. El número de sesión cambia al cerrarla aquí; lo hecho no se renumera (usuario, Sesión 12).

## Pendiente de verificación en Unity
**LK-03** (spec > Criterios). Montaje a mano en TestBench, **sin guardar**: `SPR_RuneCoin` con `MAT_Glow2D_Default` y `_definition` = `EFF_Glow2D`. Al terminar, deshacer o cerrar sin guardar.
- [ ] Play: consola sin errores del shader ni avisos de LK-49. Halo violeta suave; huecos con borde de luz y centro más tenue; sin corte ni anillos marcados.
- [ ] Intensidad 0 quita el brillo; 5 lo ensancha y lo hace más opaco. El color cambia halo e interior.
- [ ] Pulso: el toggle lo arranca y lo detiene; la velocidad lo acelera; a 0, fijo. Arrastrando la velocidad puede saltar (aceptado).
- [ ] TAB con la runa: sin brillo mientras se mantiene. Panel "Brillo 2D" en la runa y otro en el cristal. Reset devuelve el brillo por defecto.
- [ ] Zoom: el halo escala. En Play, runa a 45° en Z, Flip X y alfa del `SpriteRenderer` a 0,5: halo uniforme, sin verse a través del cuerpo.
- [ ] Stop: ningún `.mat` modificado en `git status` (D-001).

## Entorno confirmado
- Unity 6000.0.83f1 · URP 17.0.4 · Input System 1.19.0 · uGUI 2.0.0 · 2D Sprite 1.0.0.
- Active Input Handling = Input System Package (New) (`activeInputHandler: 1`), confirmado por el usuario. Única API y forma de lectura: D-006.
- Remoto `origin` = https://github.com/PixelAlFuego/LumiKit.git
- **Push:** sólo al cerrar una tarea en ✅, con su tag `cp-NN-nombre` (usuario, Sesión 11). Fuera de eso, requiere autorización.
- Unity MCP: `com.coplaydev.unity-mcp`, servidor 3.4.7, registrado en local como `UnityMCP` (HTTP, 127.0.0.1:8080). Reglas: CLAUDE.md. El registro stdio `unity-mcp`, que no conectaba, borrado (2026-09-30).
- Shaders del MVP: HLSL que escribe Claude (D-010). Los Shader Graph, aparcados, los construye el usuario con la especificación de Claude.
- D-001 confirmado en la práctica: tras salir de Play Mode, `MAT_Debug.mat` sin cambios en `git status` y el Mesh Renderer sin "(Instance)".

## Dudas abiertas y bloqueos
- **LK-24 cerrada con una confirmación a medias** (usuario, 2026-10-02): repitió el criterio del tester con su botón; al soltar TAB el contorno conserva el estado previo, apagado o encendido. Si TAB y el tester fallan juntos, se reabre.
- **LK-14 cerrada** (usuario, 2026-10-02). Su confirmación no nombra la esquina transparente ni el build de Windows: si fallan, se reabre.
- **LK-03, sin certeza:** el verificador sólo vio importar `SH_Glow2D`; las variantes se compilan al dibujarse. `SAMPLE_TEXTURE2D_LOD` y `[ToggleUI]`, vistos en los paquetes, no probados.
- **Audio (LK-23).** Por LFS: `Audio/SFX/` con los cinco `SFX_UI_*`, recortados por el usuario (Sesión 12), y `Audio/Music/MUS_Ambient_Loop.mp3`.
  **`SFX_UI_Error` y `SFX_UI_Transition` no entran en LK-23**: el primero espera avisos en pantalla y el segundo el cambio de escena de LK-17. Música en MP3 (usuario, Sesión 09): de WAV (64 MB) a MP3 (4 MB), Streaming, Vorbis al 70 %, sin preload; en WAV no bajaba de 5 MB y recortar
  el bucle lo rompía. `*.mp3` por LFS; nada referenciaba el `.wav`. Hueco en el bucle → el usuario la pasa a OGG. `MUS_` en CONVENTIONS (Sesión 12). Mixer del usuario (D-011): `Assets/LumiKit/Audio/Mixers/AMX_LumiKit.mixer`, grupos UI (−6 dB) y Music. Hover: al usuario le suena algo raro; probablemente no se cambia (Sesión 12).
- **`LumiButton` pinta `Selected` como reposo** (usuario, Sesión 10; con el cursor encima, como hover): así un usuario de teclado o mando no ve qué botón tiene el foco. Mismo coste en el campo del slider (LK-52), que suelta la selección al terminar de editar. El GDD no define ese estado. Se decide antes de publicar.
- **Aviso de rango ancho, para la Fase 5:** el campo del valor (LK-52) muestra 6 caracteres (`-10.00`); con 7 o más, la máscara corta. Que el validador (LK-49) avise cuando el rango de un `Float` los necesite, al llegar los `EFF_` reales.
- **El pack no tiene dónde poner un script de editor propio.** `Assets/Editor/` no se exporta (CONVENTIONS) y ningún script bajo
  `Assets/LumiKit/` puede hacer `using UnityEditor`. Sale a la luz con LK-50: su `LumiButtonEditor` funcionará aquí pero el comprador
  no verá los campos del componente en el Inspector. Segunda herramienta, `FontFeatureCleaner` (LK-22b): D-009 da los `.ttf` al comprador, pero si regenera sin él sus fuentes vuelven a pesar 20 MB o más. Haría falta un tercer asmdef sólo-editor dentro del pack. Se decide en LK-27.
- **Borrar al cerrar la Fase 5:** de `Assets/_Development/`, sólo `EffectDebugTester.cs`, `EFF_Debug.asset` y `MAT_Debug.mat`: desechables, fuera del pack, deliberadamente ausentes de CODEMAP y BACKLOG.
  El panel (LK-11) sustituyó al tester como UI de parámetros, pero no se retira: es el único disparador de `SetEffectEnabled`, que hará falta en la Fase 5 (corrección del usuario, Sesión 10).
- **`Assets/_Development/TestBench.unity` no se borra:** banco de pruebas permanente, crece con cada tarea (sección "Banco de pruebas" de cada spec). No se exporta.
- `Assets/Settings/DefaultVolumeProfile.asset` cambió solo (migración de Unity al importar) y entró en el commit `2cc98ff` sin revisión. Fuera del alcance de LK-09.
- `EffectController.cs` salió de 316 líneas, más de las ~140 estimadas en el plan.
- `Assets/TutorialInfo/` y `Assets/Readme.asset` son plantilla de Unity, fuera del pack. También `Assets/InputSystem_Actions.inputactions`, asset de acciones del proyecto: D-006 prohíbe usarlo desde el pack.
- **Issue de la Sesión 02:** sigue abierto el `OnGUI` del `EffectDebugTester`, que no pasa por `EventSystem` y no bloquea el ratón. Se cierra al retirar el tester, al cerrar la Fase 5.
- **`Assets/TextMesh Pro/` (4 MB) entra al repositorio** (usuario, Sesión 05): los prefabs la referencian por GUID. Documentar en LK-26.
- **`LiberationSans SDF - Fallback.asset` se reescribe solo:** es dinámico y guarda los glifos que le piden. Con la prueba visual de las fuentes (Sesión 10) ganó 25 caracteres. No se commitea; lo revierte el usuario. Con LK-22b el HUD ya no se lo pide: no cambió tras Play (usuario, Sesión 10).
- **`EFF_Debug.asset` tiene seis parámetros** y dos son deliberadamente distintos: `_Color` existe en el shader de los sprites y `_BaseColor` **no**. `_BaseColor` se queda como control negativo permanente de LK-49. Los marcadores usan `Sprite-Unlit-Default` (`_MainTex` y `_Color`) y el cubo, un shader URP
  que declara `_BaseColor` **y también `_Color`** en `ObsoleteProperties` (`Unlit.shader` línea 28): por eso reporta cuatro ausentes y no cinco.
- **`MAT_Debug.mat` usa `Universal Render Pipeline/Unlit`** (usuario, Sesión 06; en disco, GUID `650dd952…`): sin iluminación, lo acordado en LK-09 para poder cerrar el criterio de Linear.
- **Materiales de los sprites, para la Fase 5:** los marcadores comparten el material por defecto `Sprite-Unlit-Default`.
  Cada efecto necesitará el suyo (`MAT_` en `Assets/LumiKit/Materials/2D/`) o tocar un parámetro en uno los cambiará todos. LK-01 trae `MAT_Outline2D_Default` y LK-03, `MAT_Glow2D_Default` (LK-02, fuera del MVP).
- **Los topes no se suben** (usuario, Sesión 05): al llegar al tope se condensa (Sesión 07: `ui-style.md`, D-001 a D-003, por D-009).
- `SPR_Crystal.png` y `SPR_RuneCoin.png` **definitivos** (LK-20, D-012): en `Assets/LumiKit/Sprites/`, 1024×1024, PPU 512, sin sufijo.
  Movidos por el usuario desde `_Development/` (mismo GUID). Siguen de marcadores en TestBench. `SPR_Lumi` entra si llega, sin bloquear LK-14.
  Full Rect por LK-01 (`83ad8a4`). El PPU a 256 fue por un collider viejo, no por diseño: vuelve a 512 y D-012 se mantiene (usuario, Sesión 12).
- **Nombres y posiciones de TestBench:** la spec de LK-12 escribió `Marker_Crystal` en (-3,0,0) y `Marker_RuneCoin` en (3,0,0). El estado
  real, confirmado en la Sesión 03, es `SPR_Crystal` en (3,0,0) y `SPR_RuneCoin` en (-3,0,0). LK-10 usa los nombres reales; LK-12 no se toca.
- `ProjectSettings/TagManager.asset` entró en el cierre de LK-10 con la capa `Selectable` (índice 6) que creó el usuario. Unity aprovechó
  para migrarlo a `serializedVersion: 3` y borrar las entradas de capa vacías del final. Migración del editor, no revisada línea a línea.
- **Enmienda pendiente a D-001:** autorizar la lectura de `sharedMaterials` (en plural) para validar todos los materiales de un `Renderer`,
  no sólo el primero. No instancia copias, que es lo que D-001 prohíbe, pero no está en su lista de permitidos. Hoy LK-49 valida el
  primero y cuenta el resto con `GetSharedMaterials`. Se decide cuando un objeto del pack lleve más de un material.
- **Referencias a "LK-22" a secas que el corte deja obsoletas**, sin tocar por estar en specs cerradas o fuera del alcance: `LK-11a` (líneas 18, 59, 78, 79), `LK-11b` (9, 39, 44, 74-76)
  y los widgets Color/Enum/Toggle. La de `ParameterPanelBuilder` la quitó LK-51. Casi todas apuntan a LK-50 o a LK-22b; se corrigen si toca abrir ese archivo.
- **Comentarios que LK-24 deja obsoletos**, sin tocar (fuera de su alcance): `ParameterPanelUI.cs:104`, `ParameterWidgetBase.cs:87` y la spec de LK-11a (45, 80)
  dicen que LK-24 llamará a `RefreshFromController`; no lo hace (Demo no conoce la UI y no hace falta). `EffectController.cs:369-371`: "ningún shader existe todavía".
- **`ENUM_OPTION_HEIGHT` (28) y `BUTTON_HEIGHT_COMPACT` (28) son el mismo número del GDD con dos nombres** en `LumiTheme`. No lo unifico sin que me lo pidas. LK-50 no lo hizo: las opciones de enum no son `LumiButton`.
- **Cerradas:** botón del pie, deuda de LK-22a (LK-51: `SPR_UI_Rect_R6_Outline` y `LumiTheme.Transparent`, vista por el usuario) · tamaño de texto sobre el GDD (Label y Mono a 16 px, GDD 13, a 1920×1080; en `LumiTheme`, D-007 y `ui-style.md`) · shaders del MVP (usuario, Sesión 11: HLSL, D-010 y `shaders.md`; `SH_` en CONVENTIONS, Sesión 12) · diferidos de LK-09 (LK-01, Sesión 12: `SetEffectEnabled` visto por el usuario; centro del trazo `#00E5D4` exacto, medido en `Temp/Captures`).
- **`02_Demo_2D` queda marcada como modificada tras generarla** (LK-14): se guarda con la raíz de `UI_Root` a escala 0 y el lienzo la dimensiona en la siguiente
  actualización del editor. Inofensivo; si la guardas tú, el `.unity` cambia según el tamaño del Game view. Regenerar también cambia el `.unity` (IDs nuevos).
- **`AudioMixer.FindMatchingGroups`, semántica sin confirmar** (LK-23): no sé si `subPath` compara por prefijo. `UIAudioBuilder` filtra además por nombre exacto del grupo.
- **Bloqueo por acción:** cerrado (2026-09-30). Hook PreToolUse `.claude/hooks/unity-mcp-guard.js`: menús sólo `LumiKit/`, `manage_scene` sin guardar/crear/borrar, `manage_editor` sólo Play, Pausa, Stop y consultas.
  **Límite aceptado para el MVP** (usuario, Sesión 12): si falta Node, el hook da error y deja pasar la llamada.
- Ramas `main` y `sprint/mvp` (Sesión 11, desde el tag `v0.3-pre-mcp`; `sprint/mvp` en `origin` desde la Sesión 12). `develop` y `feature/LK-XX-*` del GDD §4.9 aún no creadas.

## Handoff
**LK-03 🟡 (Sesión 12).** Nuevo: `SH_Glow2D.shader`; `EffectAssetBuilder` genera `MAT_Glow2D_Default` y `EFF_Glow2D` (menú `LumiKit/Efectos/Generar Glow 2D (LK-03)`). LK-01, LK-24 y LK-14 cerradas.
  Regenerar una fuente: atlas → `Import Font Features` → `FontFeatureCleaner` (spec de LK-22b). Regenerar el panel: STATE > Pendiente de verificación.
No tocar: `Core/` y `Utils/` (LK-09, LK-49, LK-52), `Demo/` (LK-10, LK-12), `Systems/` (LK-23), `SH_Outline2D.shader` (LK-01), los cuatro widgets, `EffectDebugTester.cs` hasta la Fase 5,
`Assets/LumiKit/Scenes/`, `ProjectSettings/`, `Packages/manifest.json`, nada de 3D ni VFX.
